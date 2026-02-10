using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;

namespace Erronka
{
    /// <summary>
    /// Ticket baten datuak gordetzen dituen klasea.
    /// Baskula, produktua, saltzailea, prezioa, kantitatea eta data jasotzen ditu.
    /// </summary>
    public class Tiketa
    {
        /// <summary>
        /// Prezioa, kantitatea (kg) eta prezio totala gordetzen ditu.
        /// </summary>
        private double prezioa, kopurua, prezioguztira;

        /// <summary>
        /// Ticketaren data eta ordua gordetzen du.
        /// </summary>
        private DateTime data;

        /// <summary>
        /// Baskularen IDa, produktuaren IDa eta saltzailearen IDa gordetzen ditu.
        /// </summary>
        private int baskula, produktua, langilea;

        /// <summary>
        /// Baskularen identifikatzailea.
        /// </summary>
        public int Baskula { get => baskula; set => baskula = value; }

        /// <summary>
        /// Saltzailearen (langilearen) identifikatzailea.
        /// </summary>
        public int Langilea { get => langilea; set => langilea = value; }

        /// <summary>
        /// Produktuaren identifikatzailea.
        /// </summary>
        public int Produktua { get => produktua; set => produktua = value; }

        /// <summary>
        /// Unitateko prezioa (adib. €/kg).
        /// </summary>
        public double Prezioa { get => prezioa; set => prezioa = value; }

        /// <summary>
        /// Kantitatea (kg).
        /// </summary>
        public double Kopurua { get => kopurua; set => kopurua = value; }

        /// <summary>
        /// Prezio totala (prezioa * kantitatea).
        /// </summary>
        public double Prezioguztira { get => prezioguztira; set => prezioguztira = value; }

        /// <summary>
        /// Ticketaren data.
        /// </summary>
        public DateTime Data { get => data; set => data = value; }

        /// <summary>
        /// Eraikitzaile hutsa.
        /// </summary>
        public Tiketa() { }

        /// <summary>
        /// Eraikitzailea: ticketaren datuak hasieratzen ditu.
        /// </summary>
        /// <param name="b">Baskularen IDa.</param>
        /// <param name="p">Produktuaren IDa.</param>
        /// <param name="l">Saltzailearen IDa.</param>
        /// <param name="pr">Unitateko prezioa.</param>
        /// <param name="k">Kantitatea (kg).</param>
        /// <param name="pg">Prezio totala.</param>
        /// <param name="d">Data.</param>
        public Tiketa(int b, int p, int l, double pr, double k, double pg, DateTime d)
        {
            this.Baskula = b;
            this.Produktua = p;
            this.langilea = l;
            this.prezioa = pr;
            this.kopurua = k;
            this.prezioguztira = pg;
            this.data = d;
        }
    }
}
