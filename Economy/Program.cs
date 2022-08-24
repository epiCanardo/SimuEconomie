namespace Economy
{
    class Program
    {
        public static void Main(string[] args)
        {
            var stop = false;

            Task.Factory.StartNew(() =>
            {
                World.Instance.StartSimulation();
            });
            
            while (!stop)
            {
                Console.CursorVisible = false;
                Console.SetCursorPosition(0, 0);

                // affichage du timer
                var date = new TimeSpan(World.Instance.GetElapsedTicks());

                w($"Temps de simulation : {date.Hours} heures {date.Minutes} minutes {date.Seconds} secondes");
                w("");

                // affichage des stations
                w("Liste des stations", ConsoleColor.Yellow, true);
                w("**************************************************");
                foreach (var station in World.Instance.Stations)
                {
                    w(station.ToString());
                }

                // affichage des marchands
                w("Liste des marchands itinérants", ConsoleColor.Cyan, true);
                w("**************************************************");
                w($"{"Rang",-10} | {"Nom du marchand",-20} | {"Gain total",-20} | {"Crédits",-20} | {"Capacité vaisseau",-20} | {"Action",-100} | {"Contenu soute",-20}");

                var traders = World.Instance.FlyingTraders.OrderByDescending(x => x.TotalGain);
                var i = 1;
                foreach (var trader in traders)
                {
                    w($"{i,-10} | " + trader.ToString());
                    i++;
                }

                Thread.Sleep(50);
            }
        }

        private void UpdateConsole()
        {
            Console.CursorVisible = false;
            Console.SetCursorPosition(0, 0);

            // affichage du timer
            var date = new TimeSpan(World.Instance.GetElapsedTicks());

            w($"Temps de simulation : {date.Hours} heures {date.Minutes} minutes {date.Seconds} secondes");
            w("");
        }

        static void w(string text, bool isLine = true)
        {
            if (isLine)
                Console.WriteLine(text);
            else
                Console.Write(text);
        }

        static void w(string text, ConsoleColor color, bool isLine = false)
        {
            ConsoleColor previousColor = Console.ForegroundColor;
            Console.ForegroundColor = color;

            if (isLine)
                Console.WriteLine(text, color);
            else
                Console.Write(text, color);

            Console.ForegroundColor = previousColor;
        }
    }
}