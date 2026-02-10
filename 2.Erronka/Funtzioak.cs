using ClosedXML.Excel;
using MySql.Data.MySqlClient;
using Mysqlx;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Erronka
{
    /// <summary>
    /// Programako funtzio nagusiak biltzen dituen klasea.
    /// Tiketak irakurri, XML sortu/balidatu eta DBra bidaltzeko erabiltzen da.
    /// </summary>
    public class Funtzioak
    {
        /// <summary>
        /// Irakurritako tiket guztien zerrenda.
        /// </summary>
        static private List<Tiketa> lt = new List<Tiketa>();

        /// <summary>
        /// Autosalmentaren izena gordetzeko aldagai laguntzailea.
        /// </summary>
        static string autosalmentaIzena = "";

        /// <summary>
        /// Prozesu osoa exekutatzen du:
        /// DB kargatu, tiketak irakurri, XML sortu/balidatu, bidali eta DBra sartu.
        /// </summary>
        public static void Ekintzak()
        {
            string connStr = "Server=localhost;Port=3306;Database=supermerkatua;Uid=root;Pwd=root;";

            // DBtik datu nagusiak kargatzen ditu (baskulak, produktuak, autosalmenta).
            Datubasea.BaskulakKargatu(connStr);
            Datubasea.ProduktuakKargatu(connStr);
            Datubasea.AutoSalmentaKargatu(connStr);

            // Tiketak karpetetatik irakurri eta lt zerrendan gordetzen ditu.
            bool luzera = TiketakIrakurri();

            // 'luzera' true bada: ez dago tiketik (lt hutsik).
            if (!luzera)
            {
                Console.WriteLine("Ticketen bateko saltzailea aldatu nahi duzu?");
                Console.WriteLine("Aldatzeko Bai sartu, bestela beste edozer sartu");
                string erantzuna = (Console.ReadLine() ?? "").ToLower();

                // Saltzailea aldatzeko aukera ematen du.
                if (erantzuna == "bai")
                {
                    Saltzailea.SaltzaileaAldatu(lt);
                }

                // Tiketak XML formatuan gordetzen ditu.
                XML.TiketakXmlraGorde(lt);

                // XMLaren bidea hartu eta XSDarekin balidatzen du.
                string ruta = XML.HartuXmlBidea();
                bool ok = XML.Balidatu(ruta, @"\\MSIDEANDONI\CentralTicketBAI\XML\Tiketak.xsd");

                Spectre.Console.AnsiConsole.MarkupLine(
                    ok ? "[green]✓[/] [bold]XML balidatua izan da[/]" : "[red]✗[/] [bold]XML ezin izan da balidatu[/]"
                );

                // XMLa bidali eta gero mugitu (backup/karpeta egokira).
                XML.BidaliXml();
                XML.MugituXml();

                // Tiketak MySQL datu-basean sartzen ditu.
                Datubasea.TiketakMysqleraSartu(connStr, lt);

                Console.WriteLine();
                EnterItxaron();
            }
            else
            {
                // Ez dago tiketik prozesatzeko.
                Spectre.Console.AnsiConsole.MarkupLine("[red]✗[/] [bold]Ez dago tiketik prozesatzeko[/]");
                Console.WriteLine();
                EnterItxaron();
            }
        }

        /// <summary>
        /// Karpeta bateko .txt fitxategi guztiak backup karpetara eramaten ditu.
        /// </summary>
        /// <param name="iturriaKarpeta">Jatorrizko karpeta (tiketen karpeta).</param>
        public static void MugituTxtGuztiak(string iturriaKarpeta)
        {
            string helmugaKarpeta = @"\\MSIDEANDONI\CentralTicketBAI\BackUp\TICKET";

            // Helmuga karpeta existitzen ez bada, sortzen du.
            Directory.CreateDirectory(helmugaKarpeta);

            // Txt guztiak helmugara mugitzen ditu (berdina bada, gainidazten du).
            foreach (string fitxategia in Directory.GetFiles(iturriaKarpeta, "*.txt"))
            {
                string izena = Path.GetFileName(fitxategia);
                string helmugaBidea = Path.Combine(helmugaKarpeta, izena);

                File.Move(fitxategia, helmugaBidea, true);
            }
        }

        /// <summary>
        /// Baskula karpeta desberdinetatik tiketak irakurtzen ditu.
        /// Irakurritakoa lt zerrendan gordetzen du.
        /// </summary>
        /// <returns>lt hutsik badago true; bestela false.</returns>
        public static bool TiketakIrakurri()
        {
            // Zerrenda garbitu prozesu bakoitzean.
            lt.Clear();

            string lekua = @"\\MSIDEANDONI\CentralTicketBAI";
            string[] baskulak = { "frutategia", "harategia", "okindegia", "txarkutegia" };
            CultureInfo es = new CultureInfo("es-ES");

            foreach (string baskula in baskulak)
            {
                // Baskula bakoitzeko tiket karpeta kalkulatu.
                string karpetatiketak = Path.Combine(lekua, baskula, "Tiketak");

                // Karpeta ez badago, mezua eta hurrengora.
                if (!Directory.Exists(karpetatiketak))
                {
                    Spectre.Console.AnsiConsole.MarkupLine("[red]✗[/] [bold]Ez da existitzen[/]" + karpetatiketak);
                    continue;
                }

                foreach (string fitx in Directory.GetFiles(karpetatiketak, "*.txt"))
                {
                    // Fitxategiaren data lortu.
                    DateTime data = LortuFitxategiData(fitx);

                    using (StreamReader sr = File.OpenText(fitx))
                    {
                        // Lehen lerroa irakurri (tiketa lerro bakarrean dator).
                        string lerroa = sr.ReadLine();
                        if (lerroa == null) continue;

                        // Formatoa: produktua$saltzailea$prezioa$kopurua$guztira
                        var parts = lerroa.Split('$');

                        if (parts.Length == 5)
                        {
                            string baskKey = baskula.Trim().ToLowerInvariant();

                            // Baskula DBn existitzen dela ziurtatzen du.
                            if (!Datubasea.baskulaIdByName.TryGetValue(baskKey, out int baskula_id))
                            {
                                Spectre.Console.AnsiConsole.MarkupLine("[red]✗[/] [bold][/]Baskula ez da aurkitu DatuBasea-n" + baskula);
                                continue;
                            }

                            // Produktua DBn bilatu (baskula kontuan hartuta).
                            string produktua = parts[0].Trim();
                            string prodKey = produktua.Trim().ToLowerInvariant();

                            if (!Datubasea.produktuaIdByName.TryGetValue((prodKey, baskula_id), out int produktua_id))
                            {
                                Console.WriteLine($"Produktua ez da aurkitu Datubasea-n: {produktua} (baskula: {baskula})");
                                continue;
                            }

                            // Saltzailea kalkulatu (autosalmenta bada 0, bestela int).
                            string s = parts[1].Trim().ToLowerInvariant();
                            int saltzailea_id = (s == Datubasea.autosalmentaIzena) ? 0 : int.Parse(s);

                            // Zenbakiak parseatu (es-ES formatuan).
                            double prezioa = double.Parse(parts[2].Trim(), es);
                            double kopurua = double.Parse(parts[3].Trim(), es);
                            double prezioguztira = double.Parse(parts[4].Trim(), es);

                            // Tiketa sortu eta zerrendara gehitu.
                            Tiketa t = new Tiketa(baskula_id, produktua_id, saltzailea_id, prezioa, kopurua, prezioguztira, data);
                            lt.Add(t);
                        }
                        else
                        {
                            // Formatoa okerra bada, errorea erakutsi.
                            Spectre.Console.AnsiConsole.MarkupLine("[red]✗[/] [bold]Ez dago tiketik prozesatzeko[/]" + Path.GetFileName(fitx) + ": " + lerroa);
                        }
                    }
                }

                // Baskula horretako txt guztiak backup-era mugitu.
                MugituTxtGuztiak(karpetatiketak);
            }

            // true = ez dago tiketik (lt hutsik).
            return lt.Count == 0;
        }

        /// <summary>
        /// XML bidalketaren erregistroa Excel batean gordetzen du.
        /// Data, hartzailea, izena, bidea eta tamaina gehitzen ditu.
        /// </summary>
        /// <param name="xmlBidea">Gordetako XMLaren bidea.</param>
        /// <param name="jaso">Hartzailearen izena edo identifikatzailea.</param>
        public static void ExceleraGorde(string xmlBidea, string jaso)
        {
            string excelBidea = @"\\MSIDEANDONI\CentralTicketBAI\erregistroa.xlsx";
            string orria = "Bidalketak";

            FileInfo info = new FileInfo(xmlBidea);

            XLWorkbook wb;
            IXLWorksheet ws;

            // Excel existitzen bada, ireki; bestela, sortu.
            if (File.Exists(excelBidea))
            {
                wb = new XLWorkbook(excelBidea);

                // Orria existitzen bada hartu; bestela sortu eta goiburua jarri.
                if (wb.Worksheets.Contains(orria))
                    ws = wb.Worksheet(orria);
                else
                {
                    ws = wb.Worksheets.Add(orria);
                    ws.Cell(1, 1).Value = "Data";
                    ws.Cell(1, 2).Value = "Hartzailea";
                    ws.Cell(1, 3).Value = "XML izena";
                    ws.Cell(1, 4).Value = "XML bidea";
                    ws.Cell(1, 5).Value = "Tamaina (KB)";
                }
            }
            else
            {
                wb = new XLWorkbook();
                ws = wb.Worksheets.Add(orria);

                ws.Cell(1, 1).Value = "Data";
                ws.Cell(1, 2).Value = "Hartzailea";
                ws.Cell(1, 3).Value = "XML izena";
                ws.Cell(1, 4).Value = "XML bidea";
                ws.Cell(1, 5).Value = "Tamaina (KB)";
            }

            // Azken errenkada kalkulatu eta hurrengo lerroan idatzi.
            int azkenErrenkada = ws.LastRowUsed()?.RowNumber() ?? 1;
            int hurrengoa = azkenErrenkada + 1;

            ws.Cell(hurrengoa, 1).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            ws.Cell(hurrengoa, 2).Value = jaso;
            ws.Cell(hurrengoa, 3).Value = info.Name;
            ws.Cell(hurrengoa, 4).Value = xmlBidea;
            ws.Cell(hurrengoa, 5).Value = Math.Round(info.Length / 1024.0, 2);

            // Zutabeak doitu eta fitxategia gorde.
            ws.Columns().AdjustToContents();
            wb.SaveAs(excelBidea);
        }

        /// <summary>
        /// Fitxategiaren sorrera data hartzen du (segundorik gabe).
        /// </summary>
        /// <param name="fitxPath">Fitxategiaren bidea.</param>
        /// <returns>Fitxategiaren data/ordua (segundoak 0).</returns>
        public static DateTime LortuFitxategiData(string fitxPath)
        {
            DateTime dt = File.GetCreationTime(fitxPath);
            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0);
        }

        /// <summary>
        /// Erabiltzaileari ENTER sakatzeko eskatzen dio eta pantaila garbitzen du.
        /// </summary>
        public static void EnterItxaron()
        {
            Console.WriteLine("\n[ENTER] zapaldu jarraitzeko...");
            while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }
            Console.Clear();
        }
    }
}
