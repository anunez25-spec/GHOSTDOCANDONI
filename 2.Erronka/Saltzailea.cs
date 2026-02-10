using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;

namespace Erronka
{
    /// <summary>
    /// Ticketen informazioa kargatu eta saltzailea aldatzeko logika kudeatzen du.
    /// DBtik datuak irakurri eta erabiltzaileari aldaketak egiteko aukera ematen dio.
    /// </summary>
    public class Saltzailea
    {
        /// <summary>
        /// Kantitatea (kg) eta prezio totala gordetzen ditu.
        /// </summary>
        private decimal kopurua, prezioguztira;

        /// <summary>
        /// Ticketaren data gordetzen du.
        /// </summary>
        private DateTime data;

        /// <summary>
        /// Produktuaren IDa, langilearen IDa eta ticketaren IDa gordetzen ditu.
        /// </summary>
        private int produktua, langilea, idtiketa;

        /// <summary>
        /// Ticketaren identifikatzailea.
        /// </summary>
        public int Idtiketa { get => idtiketa; set => idtiketa = value; }

        /// <summary>
        /// Saltzailearen (langilearen) identifikatzailea.
        /// </summary>
        public int Langilea { get => langilea; set => langilea = value; }

        /// <summary>
        /// Produktuaren identifikatzailea.
        /// </summary>
        public int Produktua { get => produktua; set => produktua = value; }

        /// <summary>
        /// Kantitatea (kg).
        /// </summary>
        public decimal Kopurua { get => kopurua; set => kopurua = value; }

        /// <summary>
        /// Prezio totala (EUR).
        /// </summary>
        public decimal Prezioguztira { get => prezioguztira; set => prezioguztira = value; }

        /// <summary>
        /// Ticketaren data eta ordua.
        /// </summary>
        public DateTime Data { get => data; set => data = value; }

        /// <summary>
        /// Eraikitzaile hutsa.
        /// </summary>
        public Saltzailea() { }

        /// <summary>
        /// Eraikitzailea: objektua datuekin hasieratzen du.
        /// </summary>
        /// <param name="i">Ticketaren IDa.</param>
        /// <param name="p">Produktuaren IDa.</param>
        /// <param name="l">Saltzailearen IDa.</param>
        /// <param name="d">Data (fetxa).</param>
        /// <param name="k">Kantitatea (kg).</param>
        /// <param name="pg">Prezio totala.</param>
        public Saltzailea(int i, int p, int l, DateTime d, decimal k, decimal pg)
        {
            this.Idtiketa = i;
            this.Produktua = p;
            this.langilea = l;
            this.kopurua = k;
            this.prezioguztira = pg;
            this.data = d;
        }

        /// <summary>
        /// DBtik tiketen zerrenda kargatzen du.
        /// </summary>
        /// <returns>Saltzailea objektuen lista (tiketen datuekin).</returns>
        public static List<Saltzailea> TiketakKargatu()
        {
            var lista = new List<Saltzailea>();

            using (var conn = new MySqlConnection("Server=localhost;Port=3306;Database=supermerkatua;Uid=root;Pwd=root;"))
            {
                conn.Open();

                string sql = @"SELECT id_tiketa, id_produktua, id_saltzailea, fetxa, kantitatea_kg, prezioa_guztira
                       FROM tiketa
                       ORDER BY id_tiketa;";

                using (var cmd = new MySqlCommand(sql, conn))
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        int idTiketa = rd.GetInt32("id_tiketa");
                        int idProduktua = rd.GetInt32("id_produktua");
                        int idSaltzailea = rd.GetInt32("id_saltzailea");
                        DateTime fetxa = rd.GetDateTime("fetxa");

                        decimal kantitateaKg = Convert.ToDecimal(rd["kantitatea_kg"]);
                        decimal prezioaGuztira = Convert.ToDecimal(rd["prezioa_guztira"]);

                        lista.Add(new Saltzailea(idTiketa, idProduktua, idSaltzailea, fetxa, kantitateaKg, prezioaGuztira));
                    }
                }
            }

            return lista;
        }

        /// <summary>
        /// Erabiltzaileari tiketen zerrenda erakusten dio eta saltzailea aldatzeko aukera ematen dio.
        /// </summary>
        /// <param name="lt">Prozesatutako tiket zerrenda.</param>
        public static void SaltzaileaAldatu(List<Tiketa> lt)
        {
            int a = lt.Count;
            int kontadorea = 1;

            // Tiketak pantailan erakutsi.
            foreach (Tiketa t in lt)
            {
                Console.WriteLine(kontadorea + ". Ticketa");
                Console.WriteLine("-baskula: " + t.Baskula);
                Console.WriteLine("-Produktua: " + t.Produktua);
                Console.WriteLine("-Saltzailea: " + t.Langilea);
                Console.WriteLine("-Prezioa: " + t.Prezioa);
                Console.WriteLine("-Kantitatea: " + t.Kopurua);
                Console.WriteLine("-Guztira: " + t.Prezioguztira);
                Console.WriteLine("-Data: " + t.Data);
                Console.WriteLine();
                kontadorea++;
            }

            // Saltzaile posibleen gida azkarra.
            Console.WriteLine("Saltzaileak:");
            Console.WriteLine("0 = autosalmenta");
            Console.WriteLine("1 = lander");
            Console.WriteLine("2 = ander");
            Console.WriteLine("3 = mateo");
            Console.WriteLine("4 = unai");
            Console.WriteLine("5 = eber");
            Console.WriteLine("6 = igor");
            Console.WriteLine("7 = mario");
            Console.WriteLine();

            // Aldaketa eskatu.
            aldatu(a, lt);
        }

        /// <summary>
        /// Ticket baten saltzailea aldatzen du.
        /// Erabiltzaileari tiket zenbakia eta saltzaile berria eskatzen dizkio.
        /// </summary>
        /// <param name="a">Ticket kopurua.</param>
        /// <param name="lt">Tiket zerrenda.</param>
        public static void aldatu(int a, List<Tiketa> lt)
        {
            try
            {
                Console.WriteLine("Sartu saltzailea aldatu nahi duzun ticketeko zenbakia");
                Console.Write("Aukeraketa: ");
                int aukeraketa = int.Parse(Console.ReadLine());

                // Ticket zenbakia balidatu.
                if (aukeraketa > 0 & aukeraketa < a)
                {
                    Console.WriteLine("Sartu saltzaile berriaren id-a");
                    int aldaketa = int.Parse(Console.ReadLine());

                    // Langile IDa balidatu.
                    if (aldaketa >= 0 & aldaketa <= 7)
                    {
                        lt[aukeraketa - 1].Langilea = aldaketa;
                    }
                    else
                    {
                        Spectre.Console.AnsiConsole.MarkupLine("[red]✗[/] [bold][/]sartu duzun zenbakiak ez du loturarik langileekin");
                        Console.WriteLine();
                        Console.WriteLine("Menura itzultzen...");
                        Thread.Sleep(3000);
                        Console.Clear();
                    }
                }
                else
                {
                    Spectre.Console.AnsiConsole.MarkupLine("[red]✗[/] [bold][/]Ez dago Ticketik zenbaki horrekin");
                    Console.WriteLine();
                    Console.WriteLine("Menura itzultzen...");
                    Thread.Sleep(3000);
                    Console.Clear();
                }
            }
            catch
            {
                Spectre.Console.AnsiConsole.MarkupLine("[red]✗[/] [bold][/]Ez duzu zenbaki bat sartu");
                Console.WriteLine();
                Console.Clear();
            }
        }
    }
}
