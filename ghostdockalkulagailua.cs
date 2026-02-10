using System;

namespace Kalkulagailua
{
    /// <summary>
    /// Aplikazioaren sarrera puntua.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Programa abiarazten du eta kalkulagailua exekutatzen du.
        /// </summary>
        static void Main(string[] args)
        {
            Kalkulagailu kalkulagailua = new Kalkulagailu();
            kalkulagailua.Hasieratu();
        }
    }

    /// <summary>
    /// Kontsolako kalkulagailu sinplea.
    /// </summary>
    public class Kalkulagailu
    {
        /// <summary>
        /// Erabiltzaileari datuak eskatzen dizkio eta emaitza erakusten du.
        /// </summary>
        public void Hasieratu()
        {
            Console.WriteLine("Sartu lehen zenbakia:");
            double zenbaki1 = SartuZenbakia();

            Console.WriteLine("Sartu bigarren zenbakia:");
            double zenbaki2 = SartuZenbakia();

            Console.WriteLine("Hautatu eragiketa (+, -, *, /, %):");
            string eragiketa = Console.ReadLine();

            EragiketaAukera eragiketaObj = new EragiketaAukera();
            double emaitza = eragiketaObj.ErrekinEragiketa(eragiketa, zenbaki1, zenbaki2);

            Console.WriteLine("Emaitza: " + emaitza);
        }

        /// <summary>
        /// Kontsolatik zenbaki bat irakurtzen du eta double bihurtzen du.
        /// </summary>
        /// <returns>Sartutako zenbakia (double).</returns>
        private double SartuZenbakia()
        {
            return Convert.ToDouble(Console.ReadLine());
        }
    }

    /// <summary>
    /// Eragiketak aukeratu eta kalkuluak egiten ditu.
    /// </summary>
    public class EragiketaAukera
    {
        /// <summary>
        /// Eragiketaren arabera kalkulua exekutatzen du.
        /// </summary>
        /// <param name="eragiketa">Eragiketa ikurra: +, -, *, /, %.</param>
        /// <param name="zenbaki1">Lehen zenbakia.</param>
        /// <param name="zenbaki2">Bigarren zenbakia.</param>
        /// <returns>Kalkuluaren emaitza.</returns>
        public double ErrekinEragiketa(string eragiketa, double zenbaki1, double zenbaki2)
        {
            switch (eragiketa)
            {
                case "+":
                    return ZenbakiakGehitu(zenbaki1, zenbaki2);
                case "-":
                    return ZenbakiakKendu(zenbaki1, zenbaki2);
                case "*":
                    return ZenbakiakBiderkatu(zenbaki1, zenbaki2);
                case "/":
                    return Zatiketa(zenbaki1, zenbaki2);
                case "%":
                    return Moduloa(zenbaki1, zenbaki2);
                default:
                    Console.WriteLine("Errorea: eragiketa ezezaguna.");
                    return 0;
            }
        }

        /// <summary>
        /// Bi zenbaki batzen ditu.
        /// </summary>
        /// <param name="a">Lehen balioa.</param>
        /// <param name="b">Bigarren balioa.</param>
        /// <returns>Batuketaren emaitza.</returns>
        private double ZenbakiakGehitu(double a, double b)
        {
            return a + b;
        }

        /// <summary>
        /// Bigarrena lehenengotik kentzen du.
        /// </summary>
        /// <param name="a">Lehen balioa.</param>
        /// <param name="b">Bigarren balioa.</param>
        /// <returns>Kenketa emaitza.</returns>
        private double ZenbakiakKendu(double a, double b)
        {
            return a - b;
        }

        /// <summary>
        /// Bi zenbaki biderkatzen ditu.
        /// </summary>
        /// <param name="a">Lehen balioa.</param>
        /// <param name="b">Bigarren balioa.</param>
        /// <returns>Biderketaren emaitza.</returns>
        private double ZenbakiakBiderkatu(double a, double b)
        {
            return a * b;
        }

        /// <summary>
        /// Zatiketa egiten du. Zero zatitzea saihesten du.
        /// </summary>
        /// <param name="a">Zatikizuna.</param>
        /// <param name="b">Zatitzailea.</param>
        /// <returns>Zatiketaren emaitza; b=0 bada, 0 itzultzen du.</returns>
        private double Zatiketa(double a, double b)
        {
            if (b != 0)
                return a / b;

            Console.WriteLine("Errorea: zeroz zatitzea ezinezkoa da.");
            return 0;
        }

        /// <summary>
        /// Moduloa kalkulatzen du. Zero zatitzea saihesten du.
        /// </summary>
        /// <param name="a">Lehen balioa.</param>
        /// <param name="b">Bigarren balioa.</param>
        /// <returns>Moduluaren emaitza; b=0 bada, 0 itzultzen du.</returns>
        private double Moduloa(double a, double b)
        {
            if (b != 0)
                return a % b;

            Console.WriteLine("Errorea: zeroz zatitzea ezinezkoa da.");
            return 0;
        }
    }
}
