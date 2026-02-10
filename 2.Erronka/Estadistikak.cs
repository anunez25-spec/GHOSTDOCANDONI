using DocumentFormat.OpenXml.Drawing.Charts;
using Microsoft.Office.Interop.Outlook;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using Spectre.Console;

namespace Erronka
{
    /// <summary>
    /// Estadistikak kontsolan modu bisualean erakusten dituen UI klasea.
    /// MySQL datu-basetik datuak hartu eta taula/grafikoa sortzen du.
    /// </summary>
    public class EstatistikakUI
    {
        /// <summary>
        /// MySQL konexio katea.
        /// </summary>
        private const string KonexioKatea =
            "Server=localhost;Port=3306;Database=supermerkatua;Uid=root;Pwd=root;";

        /// <summary>
        /// SQL kontsulta exekutatu eta lehenengo emaitza itzultzen du.
        /// Izen bat eta totala (decimal) bueltatzen ditu.
        /// </summary>
        /// <param name="sql">Exekutatu beharreko SQL kontsulta.</param>
        /// <param name="idZutabea">Izenaren zutabearen alias-a.</param>
        /// <param name="totalZutabea">Totalaren zutabearen alias-a.</param>
        /// <returns>(izena, total) bikotea. Emaitzarik ez bada ("-", 0).</returns>
        private (string izena, decimal total) Lehenengoa(string sql, string idZutabea, string totalZutabea)
        {
            using var konexioa = new MySqlConnection(KonexioKatea);
            konexioa.Open();

            using var komandoa = new MySqlCommand(sql, konexioa);
            using var irakurgailua = komandoa.ExecuteReader();

            // Ez badago emaitzarik, balio lehenetsiak.
            if (!irakurgailua.Read()) return ("-", 0m);

            // Izenaren balioa hartu (null kontrolarekin).
            string izena = irakurgailua[idZutabea]?.ToString() ?? "-";

            // Totala hartu (DBNull bada, 0).
            decimal total = irakurgailua[totalZutabea] == DBNull.Value
                ? 0m
                : Convert.ToDecimal(irakurgailua[totalZutabea]);

            return (izena, total);
        }

        /// <summary>
        /// Estadistika nagusiak kalkulatu eta pantailan erakusten ditu.
        /// Taula bat eta barra-grafiko bat marrazten ditu.
        /// </summary>
        public void EstatistikakPolitEman()
        {
            // Saltzaile gehien (kg)
            var (saltzGehienKgIzena, saltzGehienKg) = Lehenengoa(@"
                SELECT s.izena AS saltzailea, SUM(t.kantitatea_kg) AS guztira_kg
                FROM tiketa t
                JOIN saltzailea s ON s.id_saltzailea = t.id_saltzailea
                GROUP BY s.izena
                ORDER BY guztira_kg DESC
                LIMIT 1;", "saltzailea", "guztira_kg");

            // Saltzaile gehien (EUR)
            var (saltzGehienEurIzena, saltzGehienEur) = Lehenengoa(@"
                SELECT s.izena AS saltzailea, SUM(t.prezioa_guztira) AS guztira_euro
                FROM tiketa t
                JOIN saltzailea s ON s.id_saltzailea = t.id_saltzailea
                GROUP BY s.izena
                ORDER BY guztira_euro DESC
                LIMIT 1;", "saltzailea", "guztira_euro");

            // Saltzaile gutxien (kg)
            var (saltzGutxienKgIzena, saltzGutxienKg) = Lehenengoa(@"
                SELECT s.izena AS saltzailea, SUM(t.kantitatea_kg) AS guztira_kg
                FROM tiketa t
                JOIN saltzailea s ON s.id_saltzailea = t.id_saltzailea
                GROUP BY s.izena
                ORDER BY guztira_kg ASC
                LIMIT 1;", "saltzailea", "guztira_kg");

            // Saltzaile gutxien (EUR)
            var (saltzGutxienEurIzena, saltzGutxienEur) = Lehenengoa(@"
                SELECT s.izena AS saltzailea, SUM(t.prezioa_guztira) AS guztira_euro
                FROM tiketa t
                JOIN saltzailea s ON s.id_saltzailea = t.id_saltzailea
                GROUP BY s.izena
                ORDER BY guztira_euro ASC
                LIMIT 1;", "saltzailea", "guztira_euro");

            // Produktu gehien (kg)
            var (prodGehienIzena, prodGehienKg) = Lehenengoa(@"
                SELECT p.produktuaren_izena AS produktua, SUM(t.kantitatea_kg) AS guztira_kg
                FROM tiketa t
                JOIN produktua p ON p.id_produktua = t.id_produktua
                GROUP BY p.produktuaren_izena
                ORDER BY guztira_kg DESC
                LIMIT 1;", "produktua", "guztira_kg");

            // Produktu gutxien (kg)
            var (prodGutxienIzena, prodGutxienKg) = Lehenengoa(@"
                SELECT p.produktuaren_izena AS produktua, SUM(t.kantitatea_kg) AS guztira_kg
                FROM tiketa t
                JOIN produktua p ON p.id_produktua = t.id_produktua
                GROUP BY p.produktuaren_izena
                ORDER BY guztira_kg ASC
                LIMIT 1;", "produktua", "guztira_kg");

            // Pantaila garbitu eta titulua erakutsi.
            AnsiConsole.Clear();

            var titulua = new Panel(
                Align.Center(new Markup("[bold yellow]📊 SUPERMERKATUA — ESTADISTIKAK[/]"), VerticalAlignment.Middle))
            {
                Border = BoxBorder.Double,
                Padding = new Padding(2, 1, 2, 1)
            };
            AnsiConsole.Write(titulua);
            AnsiConsole.WriteLine();

            // Taula sortu eta datuak sartu.
            var taula = new Spectre.Console.Table()
                .Border(Spectre.Console.TableBorder.Rounded)
                .BorderColor(Spectre.Console.Color.Grey);

            taula.AddColumn("[bold]Atala[/]");
            taula.AddColumn("[bold]Izena[/]");
            taula.AddColumn("[bold]Balioa[/]");

            taula.AddRow(
                new Spectre.Console.Text("Produktu gehien saldu dituen saltzailea (kg)"),
                new Spectre.Console.Text(saltzGehienKgIzena),
                new Spectre.Console.Text($"{saltzGehienKg:0.###} kg")
            );

            taula.AddRow(
                new Spectre.Console.Text("Diru gehien ingresatu duen saltzailea (EUR)"),
                new Spectre.Console.Text(saltzGehienEurIzena),
                new Spectre.Console.Text($"{saltzGehienEur:0.00} EUR")
            );

            taula.AddRow(
                new Spectre.Console.Text("Produktu gutxien saldu dituen saltzailea (kg)"),
                new Spectre.Console.Text(saltzGutxienKgIzena),
                new Spectre.Console.Text($"{saltzGutxienKg:0.###} kg")
            );

            taula.AddRow(
                new Spectre.Console.Text("Diru gutxien ingresatu duen saltzailea (EUR)"),
                new Spectre.Console.Text(saltzGutxienEurIzena),
                new Spectre.Console.Text($"{saltzGutxienEur:0.00} EUR")
            );

            taula.AddRow(
                new Spectre.Console.Text("Gehien saldu den produktua (kg)"),
                new Spectre.Console.Text(prodGehienIzena),
                new Spectre.Console.Text($"{prodGehienKg:0.###} kg")
            );

            taula.AddRow(
                new Spectre.Console.Text("Gutxien saldu den produktua (kg)"),
                new Spectre.Console.Text(prodGutxienIzena),
                new Spectre.Console.Text($"{prodGutxienKg:0.###} kg")
            );

            Spectre.Console.AnsiConsole.Write(taula);
            Spectre.Console.AnsiConsole.WriteLine();

            // Barra-grafikoa sortu: konparazio azkarra (kg).
            var grafikoa = new Spectre.Console.BarChart()
                .Width(60)
                .Label("[bold]Konparazioa (kg)[/]")
                .CenterLabel();

            grafikoa.AddItem($"Saltzailea: {saltzGehienKgIzena}", (double)saltzGehienKg, Spectre.Console.Color.Green);
            grafikoa.AddItem($"Saltzailea: {saltzGutxienKgIzena}", (double)saltzGutxienKg, Spectre.Console.Color.Red);
            grafikoa.AddItem($"Produktua: {prodGehienIzena}", (double)prodGehienKg, Spectre.Console.Color.Yellow);
            grafikoa.AddItem($"Produktua: {prodGutxienIzena}", (double)prodGutxienKg, Spectre.Console.Color.Blue);

            Spectre.Console.AnsiConsole.Write(grafikoa);

            // Itxaron eta menura bueltatu.
            Funtzioak.EnterItxaron();
        }
    }
}
