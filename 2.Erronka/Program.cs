using Mysqlx;
using Spectre.Console;
using System.Text;

namespace Erronka
{
    /// <summary>
    /// 
    /// </summary>
    public static class Menua
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        public static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
            bool atera = false¡
            while (!atera)
            {
                Console.Title = "TICKETBAI - Menua";
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.WriteLine("==============================================================================", 1);
                Console.WriteLine("                                 M  E  N  U  A                            ", 1);
                Console.WriteLine();
                Console.WriteLine("==============================================================================");
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("      ████████╗ ██╗ ██████╗ ██╗  ██╗███████╗████████╗██████╗  █████╗ ██╗");
                Console.WriteLine("      ╚══██╔══╝ ██║██╔════╝ ██║ ██╔╝██╔════╝╚══██╔══╝██╔══██╗██╔══██╗██║");
                Console.WriteLine("         ██║    ██║██║      █████╔╝ █████╗     ██║   ██████╔╝███████║██║");
                Console.WriteLine("         ██║    ██║██║      ██╔═██╗ ██╔══╝     ██║   ██╔══██╗██╔══██║██║");
                Console.WriteLine("         ██║    ██║╚██████╗ ██║  ██╗███████╗   ██║   ██████╔╝██║  ██║██║");
                Console.WriteLine("         ╚═╝    ╚═╝ ╚═════╝ ╚═╝  ╚═╝╚══════╝   ╚═╝   ╚═════╝ ╚═╝  ╚═╝╚═╝");
                Console.WriteLine("==============================================================================\n");
                Console.ResetColor();
                int aukeraketa = 0;
                Console.WriteLine("Zer nahi duzu egin:");
                Console.WriteLine("1. Prozesatu");
                Console.WriteLine("2. Estadistikak ikusi");
                Console.WriteLine("3. Datu basean dagoen saltzailea aldatu");
                Console.WriteLine("4. Atera");
                Console.Write("Aukeraketa: ");
                try
                {
                    aukeraketa = int.Parse(Console.ReadLine());
                    Zenbakia.Berdina = aukeraketa;
                    Console.Clear();
                    switch (aukeraketa)
                    {
                        case 1:
                            Funtzioak.Ekintzak();
                            break;

                        case 2:
                            var ui = new Erronka.EstatistikakUI();
                            ui.EstatistikakPolitEman();
                            break;
                        case 3:
                            Datubasea.TicketakErakutsi();
                            break;
                        case 4:
                            Spectre.Console.AnsiConsole.MarkupLine("[green]✓[/] [bold]Programatik egoki irten zara[/]");
                            Environment.Exit(0);
                            break;

                        default:
                            Spectre.Console.AnsiConsole.MarkupLine("[red]✗[/] [bold]Sartu duzunak ez du balio[/]");
                            break;
                    }
                }
                catch
                {
                    Console.Clear();
                    Spectre.Console.AnsiConsole.MarkupLine("[red]✗[/] [bold]Sartu duzunak ez du balio[/]");
                    Funtzioak.EnterItxaron();
                }
            }

        }
    }
    /// <summary>
    /// 
    /// </summary>
    public static class Zenbakia
    {
        /// <summary>
        /// Gets or sets the berdina.
        /// </summary>
        /// <value>
        /// The berdina.
        /// </value>
        public static int Berdina { get; set; }
    }
}
