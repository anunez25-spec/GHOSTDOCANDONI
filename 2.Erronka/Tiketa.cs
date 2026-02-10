using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;

namespace Erronka
{
    public class Tiketa
    {
        private double prezioa, kopurua, prezioguztira;
        private DateTime data;
        private int baskula, produktua, langilea;

        public int Baskula { get => baskula; set => baskula = value; }
        public int Langilea { get => langilea; set => langilea = value; }

        public int Produktua { get => produktua; set => produktua = value; }
        public double Prezioa { get => prezioa; set => prezioa = value; }
        public double Kopurua { get => kopurua; set => kopurua = value; }
        public double Prezioguztira { get => prezioguztira; set => prezioguztira = value; }
        public DateTime Data { get => data; set => data = value; }
        public Tiketa() { }
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
