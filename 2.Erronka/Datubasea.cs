using System.Collections.Generic;
using MySql.Data.MySqlClient;
using Spectre.Console;

namespace Erronka
{
    /// <summary>
    /// Datu-basearekin (MySQL) lan egiteko utilitate estatikoak.
    /// Baskulak/produktuak cachean kargatzen ditu eta tiketak gordetzen/eguneratzen ditu.
    /// </summary>
    public static class Datubasea
    {
        /// <summary>
        /// Baskularen izena -> baskula_id mapaketa (cachea).
        /// </summary>
        public static Dictionary<string, int> baskulaIdByName = new Dictionary<string, int>();

        /// <summary>
        /// (Produktu izena, baskula_id) -> id_produktua mapaketa (cachea).
        /// </summary>
        public static Dictionary<(string izena, int baskulaId), int> produktuaIdByName =
            new Dictionary<(string izena, int baskulaId), int>();

        /// <summary>
        /// Autosalmentaren izena (DBtik kargatua).
        /// </summary>
        public static string autosalmentaIzena;

        /// <summary>
        /// DBtik baskulen ID-ak eta izenak kargatzen ditu.
        /// Cachea betetzen du: baskulaIdByName.
        /// </summary>
        /// <param name="connStr">MySQL konexio katea.</param>
        public static void BaskulakKargatu(string connStr)
        {
            baskulaIdByName.Clear();

            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();

                using (var cmd = new MySqlCommand("SELECT baskula_id, izena FROM baskula;", conn))
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        int id = rd.GetInt32(0);
                        string izena = rd.GetString(1).Trim().ToLowerInvariant();

                        baskulaIdByName[izena] = id;
                    }
                }
            }
        }

        /// <summary>
        /// DBtik produktu guztiak kargatzen ditu.
        /// Cachea betetzen du: produktuaIdByName.
        /// </summary>
        /// <param name="connStr">MySQL konexio katea.</param>
        public static void ProduktuakKargatu(string connStr)
        {
            produktuaIdByName.Clear();

            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();

                using (var cmd = new MySqlCommand("SELECT id_produktua, produktuaren_izena, baskula_id FROM produktua;", conn))
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        int id = rd.GetInt32(0);
                        string izena = rd.GetString(1).Trim().ToLowerInvariant();
                        int baskulaId = rd.GetInt32(2);

                        produktuaIdByName[(izena, baskulaId)] = id;
                    }
                }
            }
        }

        /// <summary>
        /// Autosalmentaren izena kargatzen du (id_saltzailea = 0).
        /// Ondoren, tiketak irakurtzean autosalmenta detektatzeko erabiltzen da.
        /// </summary>
        /// <param name="connStr">MySQL konexio katea.</param>
        public static void AutoSalmentaKargatu(string connStr)
        {
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("SELECT izena FROM saltzailea WHERE id_saltzailea = 0;", conn))
                {
                    autosalmentaIzena = (cmd.ExecuteScalar()?.ToString() ?? "").Trim().ToLowerInvariant();
                }
            }
        }

        /// <summary>
        /// Tiketak MySQL datu-basean insertatzen ditu.
        /// Tarteko cacheak erabilita, id-ak jada kalkulatuta datoz.
        /// </summary>
        /// <param name="connStr">MySQL konexio katea.</param>
        /// <param name="lt">Insertatu beharreko tiket zerrenda.</param>
        public static void TiketakMysqleraSartu(string connStr, List<Tiketa> lt)
        {
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();

                string sql =
                    "INSERT INTO tiketa (id_produktua, id_saltzailea, fetxa, kantitatea_kg, prezioa_guztira) " +
                    "VALUES (@idProd, @idSaltz, @fetxa, @kg, @guztira);";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    // Parametroak behin definitu eta loop-ean balioa bakarrik aldatzen da.
                    cmd.Parameters.Add("@idProd", MySqlDbType.Int32);
                    cmd.Parameters.Add("@idSaltz", MySqlDbType.Int32);
                    cmd.Parameters.Add("@fetxa", MySqlDbType.DateTime);
                    cmd.Parameters.Add("@kg", MySqlDbType.Decimal);
                    cmd.Parameters.Add("@guztira", MySqlDbType.Decimal);

                    foreach (var t in lt)
                    {
                        cmd.Parameters["@idProd"].Value = t.Produktua;
                        cmd.Parameters["@idSaltz"].Value = t.Langilea;
                        cmd.Parameters["@fetxa"].Value = t.Data;
                        cmd.Parameters["@kg"].Value = (decimal)t.Kopurua;
                        cmd.Parameters["@guztira"].Value = (decimal)t.Prezioguztira;

                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        /// <summary>
        /// DBko tiketak pantailan erakusten ditu.
        /// Ondoren, erabiltzaileari saltzailea aldatzeko aukera ematen dio.
        /// </summary>
        public static void TicketakErakutsi()
        {
            List<Saltzailea> SaltzaileLista = Saltzailea.TiketakKargatu();

            if (SaltzaileLista.Count == 0)
            {
                Console.WriteLine("Datu basean ez dago ticketik gordeta");
            }
            else
            {
                int kontadorea = 1;
                int a = SaltzaileLista.Count;

                // Tiketak inprimatu.
                foreach (Saltzailea t in SaltzaileLista)
                {
                    Console.WriteLine(kontadorea + ". Ticketa");
                    Console.WriteLine("-Ticket ID-a: " + t.Idtiketa);
                    Console.WriteLine("-Produktua: " + t.Produktua);
                    Console.WriteLine("-Saltzailea: " + t.Langilea);
                    Console.WriteLine("-Data: " + t.Data);
                    Console.WriteLine("-Kantitatea: " + t.Kopurua);
                    Console.WriteLine("-Guztira: " + t.Prezioguztira);
                    Console.WriteLine();
                    kontadorea++;
                }

                // Saltzaileen gida.
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

                try
                {
                    Console.WriteLine("Sartu saltzailea aldatu nahi duzun ticketeko zenbakia");
                    Console.Write("Aukeraketa: ");
                    int aukeraketa = int.Parse(Console.ReadLine());

                    // Ticket zenbakia balidatu.
                    if (aukeraketa > 0 & aukeraketa <= a)
                    {
                        Console.WriteLine("Sartu saltzaile berriaren id-a");
                        int aldaketa = int.Parse(Console.ReadLine());

                        // Langile IDa balidatu.
                        if (aldaketa >= 0 & aldaketa <= 7)
                        {
                            int tiketazenbakia = SaltzaileLista[aukeraketa - 1].Idtiketa;
                            UpdateSaltzailea(tiketazenbakia, aldaketa);
                        }
                        else
                        {
                            Console.WriteLine("sartu duzun zenbakiak ez du loturarik langileekin");
                            Console.WriteLine();
                            Console.WriteLine("Menura itzultzen...");
                            Thread.Sleep(3000);
                            Console.Clear();
                        }
                    }
                    else
                    {
                        Console.WriteLine("Ez dago Ticketik zenbaki horrekin");
                        Console.WriteLine();
                        Console.WriteLine("Menura itzultzen...");
                        Thread.Sleep(3000);
                        Console.Clear();
                    }
                }
                catch
                {
                    Console.WriteLine("Ez duzu zenbaki bat sartu");
                    Console.WriteLine();
                }
            }
        }

        /// <summary>
        /// Ticket baten saltzailea DBn eguneratzen du (UPDATE).
        /// </summary>
        /// <param name="tiketaznebakia">Eguneratu beharreko ticketaren IDa.</param>
        /// <param name="aldaketa">Saltzaile berriaren IDa.</param>
        public static void UpdateSaltzailea(int tiketaznebakia, int aldaketa)
        {
            using var conn = new MySqlConnection("Server=localhost;Port=3306;Database=supermerkatua;Uid=root;Pwd=root;");
            conn.Open();

            string sql = "UPDATE tiketa SET id_saltzailea=@aldaketa WHERE id_tiketa=@id;";
            using var cmd = new MySqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@aldaketa", aldaketa);
            cmd.Parameters.AddWithValue("@id", tiketaznebakia);

            cmd.ExecuteNonQuery();
        }
    }
}
