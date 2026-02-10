using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Spectre.Console;

namespace Erronka
{
    /// <summary>
    /// XMLarekin lan egiteko utilitateak.
    /// Tiketak XMLra gorde, balidatu, bidali eta backup-era mugitzen ditu.
    /// </summary>
    public class XML
    {
        /// <summary>
        /// Tiketak XML fitxategi batean gordetzen ditu.
        /// Karpeta sortzen du existitzen ez bada.
        /// </summary>
        /// <param name="lt">Tiketak dituen zerrenda.</param>
        public static void TiketakXmlraGorde(List<Tiketa> lt)
        {
            string lekua = @"\\MSIDEANDONI\CentralTicketBAI";

            // XML karpeta prestatu.
            string xmlKarpeta = Path.Combine(lekua, "XML");
            Directory.CreateDirectory(xmlKarpeta);

            // XML izena dataren arabera sortu (unikotasuna bermatzeko).
            string xmlFitx = Path.Combine(xmlKarpeta, $"tiketak_{DateTime.Now:yyyyMMdd_HHmmss}.xml");

            // Serializazioa: List<Tiketa> -> XML (root: Tiketak)
            var serializer = new XmlSerializer(typeof(List<Tiketa>), new XmlRootAttribute("Tiketak"));

            using (var fs = new FileStream(xmlFitx, FileMode.Create))
            {
                serializer.Serialize(fs, lt);
            }

            Spectre.Console.AnsiConsole.MarkupLine("[green]✓[/] Tiketak egoki prozesatu dira");
        }

        /// <summary>
        /// XML karpetako lehenengo .xml fitxategiaren bidea itzultzen du.
        /// </summary>
        /// <returns>XML fitxategiaren bidea.</returns>
        /// <exception cref="System.IO.FileNotFoundException">
        /// Karpeta horretan ez badago .xml fitxategirik.
        /// </exception>
        public static string HartuXmlBidea()
        {
            string karpeta = @"\\MSIDEANDONI\CentralTicketBAI\XML";

            string[] xmlak = Directory.GetFiles(karpeta, "*.xml");

            if (xmlak.Length == 0)
                throw new FileNotFoundException("Ez dago .xml fitxategirik karpeta honetan: " + karpeta);

            return xmlak[0];
        }

        /// <summary>
        /// XML fitxategia XSD batekin balidatzen du.
        /// Erroreak badaude, false itzultzen du.
        /// </summary>
        /// <param name="xml">Balidatu beharreko XML bidea.</param>
        /// <param name="xsd">XSD fitxategiaren bidea.</param>
        /// <returns>Balidazioa ondo: true. Bestela: false.</returns>
        public static bool Balidatu(string xml, string xsd)
        {
            bool ok = true;

            // XSD eskema kargatu eta balidazioa aktibatu.
            var s = new XmlReaderSettings { ValidationType = ValidationType.Schema };
            s.Schemas.Add(null, xsd);

            // Errore bat dagoenean ok=false.
            s.ValidationEventHandler += (_, e) =>
            {
                ok = false;
                Console.WriteLine(e.Message);
            };

            // Irakurtzen doan bitartean balidatzen da.
            using var r = XmlReader.Create(xml, s);
            while (r.Read()) { }

            return ok;
        }

        /// <summary>
        /// XML fitxategia e-postaz bidaltzen du (Outlook erabiliz).
        /// Bidalketa Excel erregistroan apuntatzen du.
        /// </summary>
        /// <exception cref="System.Exception">
        /// XML fitxategirik ez badago edo Outlook instalatuta ez badago.
        /// </exception>
        public static void BidaliXml()
        {
            string karpeta = @"\\MSIDEANDONI\CentralTicketBAI\XML";
            string jaso = "ogasunaizarraitz@gmail.com";

            // Karpeta barruko xmlak hartu.
            string[] xmlak = Directory.GetFiles(karpeta, "*.xml");

            if (xmlak.Length == 0)
                throw new Exception("Ez dago .xml fitxategirik karpeta honetan: " + karpeta);

            string xmlBidea = xmlak[0];

            // Outlook instalatuta dagoen egiaztatu.
            Type outlookType = Type.GetTypeFromProgID("Outlook.Application");
            if (outlookType == null)
                throw new Exception("Outlook ez dago instalatuta ordenagailu honetan.");

            // E-maila sortu eta bidali.
            dynamic outlookApp = Activator.CreateInstance(outlookType);
            dynamic mail = outlookApp.CreateItem(0);

            mail.To = jaso;
            mail.Subject = "Ticketak XML formatuan";
            mail.Body = "Kaixo, hemen dago XML-a gure 4 baskulek egindako ticketekin";
            mail.Attachments.Add(xmlBidea);

            mail.Send();

            // Bidalketa Excel erregistroan gorde.
            Funtzioak.ExceleraGorde(xmlBidea, jaso);
        }

        /// <summary>
        /// XML karpetako fitxategiak backup karpetara mugitzen ditu.
        /// Fitxategia existitzen bada, ez du gainidazten.
        /// </summary>
        public static void MugituXml()
        {
            string rutaOrigen = @"\\MSIDEANDONI\CentralTicketBAI\XML";
            string rutaDestino = @"\\MSIDEANDONI\CentralTicketBAI\BackUp\XML";

            try
            {
                if (Directory.Exists(rutaOrigen))
                {
                    string[] artxiboakXml = Directory.GetFiles(rutaOrigen, "*.xml");

                    foreach (string artxiboRuta in artxiboakXml)
                    {
                        string izena = Path.GetFileName(artxiboRuta);
                        string Mugitu = Path.Combine(rutaDestino, izena);

                        if (!File.Exists(Mugitu))
                        {
                            File.Move(artxiboRuta, Mugitu);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Errorea pantailan erakusten du (debug/ikuskapenerako).
                Console.WriteLine(ex.Message);
            }
        }
    }
}
