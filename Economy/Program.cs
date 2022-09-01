using Backend;

namespace Economy
{
    class Program
    {
        public static void Main(string[] args)
        {
            var stop = false;

            Task.Factory.StartNew(() => { World.Instance.StartSimulation(); });

            while (!stop)
            {
                UpdateConsoleHeader();

                // paramètres de la simulation


                // affichage des prix globaux
                w("Valeurs des biens sur le marché global", ConsoleColor.Red, true);
                w("**************************************************");
                w($"{"Bien",-10} | {"Poids",-10} | {"Valeur",-10} | {"Actualité",-50} |");
                foreach (UniversalMerchendise universalMerchendise in World.Instance.SimulationParameters.UniversalMerchendises)
                {
                    w(
                        $"{universalMerchendise.Name,-10} | {universalMerchendise.Weight,-10} | {universalMerchendise.Value.ToString("#.#"),-10} | {universalMerchendise.News,-50} |");
                }

                // affichage des biens
                w("Liste des biens au commerce", ConsoleColor.Yellow, true);
                w("**************************************************");

                List<string> text = new List<string>();

                foreach (Station station in World.Instance.SimulationParameters.Stations)
                {
                    string stationText = $"{station.Name} ({station.NegociatingTraders.Count()})";
                    text.Add($"{stationText,-49}");
                }

                w($"{string.Join("| ", text)}");

                text.Clear();

                foreach (UniversalMerchendise universalMerchendise in World.Instance.SimulationParameters.UniversalMerchendises)
                {
                    text.Clear();

                    foreach (TradingLine line in World.Instance.SimulationParameters.Stations.SelectMany(x => x.Board.TradingLines)
                                 .Where(x => x.Item.UniversalMerchendise.MerchendiseType ==
                                             universalMerchendise.MerchendiseType))
                    {
                        text.Add(line.ToString());
                    }

                    w(string.Join(" | ", text));
                }

                w("**************************************************");

                // affichage des marchands
                w("Liste des marchands itinérants", ConsoleColor.Cyan, true);
                w("**************************************************");
                w(
                    $"{"Rang",-5} | {"Nom du marchand",-20} | {"Gain total",-10} | {"Crédits",-10} | {"Trades",-8} | {"Dist. tot.",-10} | {"Dernier trade",-15} | {"Stockage actuel",-15} | {"Action",-80} | {"Contenu soute",-20}");

                var traders = World.Instance.FlyingTraders.OrderByDescending(x => x.TotalGain);
                var i = 1;
                foreach (var trader in traders)
                {
                    w($"{i,-5} | " + trader.ToString());
                    i++;
                }

                Thread.Sleep(World.Instance.SimulationParameters.ConsoleRefreshTick);
            }
        }

        private static void UpdateConsoleHeader()
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