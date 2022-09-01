using System.Diagnostics;
using Newtonsoft.Json;

namespace Backend
{
    public sealed class World
    {
        public static World Instance => _lazy.Value;

        private static readonly Lazy<World>
            _lazy = new(() => new World(), LazyThreadSafetyMode.ExecutionAndPublication);

        private static Random _rnd = new(DateTime.Now.Millisecond);
        private static Stopwatch _timer;
        public List<FixedTrader> FixedTraders { get; set; }
        public List<FlyingTrader> FlyingTraders { get; set; }
        public Parameters SimulationParameters { get; set; }

        private async Task UpdateDemands()
        {
            foreach (UniversalMerchendise merchendise in SimulationParameters.UniversalMerchendises)
            {
                merchendise.ApplyNewDemand();
            }

            while (true)
            {
                // une ressource est prise au hasard et on applique un modificateur de consommation / production
                Thread.Sleep(SimulationParameters.DemandsUpdateTick);
                UniversalMerchendise merchendise = SimulationParameters.UniversalMerchendises[_rnd.Next(0, SimulationParameters.UniversalMerchendises.Count)];
                merchendise.SetNewDemand(_rnd.Next(0, 2));
            }
        }

        private World()
        {
            // démarrage du temps
            InitializeTimer();

            // récupération des paramètres de simulation
            InitializeSimulationParameters();

            // les traders fixes
            InitializeFixedTraders();

            // les traders itinérants
            InitializeFlyingTraders();

            // répartition des marchandises dans les stations
            InitializeMerchendisesRepartition();
        }

        private void InitializeTimer()
        {
            _timer = new Stopwatch();
            _timer.Start();
        }

        private void InitializeSimulationParameters()
        {
            SimulationParameters = new Parameters();

            string jsonMoq;
            using (StreamReader sR = new StreamReader("SimulationParams.json"))
            {
                jsonMoq = sR.ReadToEnd();
                sR.Close();
            }

            SimulationParameters = JsonConvert.DeserializeObject<Parameters>(jsonMoq);
        }

        private void InitializeMerchendisesRepartition()
        {
            foreach (var station in SimulationParameters.Stations)
            {
                station.Merchendises = new List<Merchendise>
                {
                    new()
                    {
                        UniversalMerchendise =
                            SimulationParameters.UniversalMerchendises.First(x => x.MerchendiseType == MerchendiseType.Gold),
                        Name = "Or",
                        Quantity = (_rnd.Next(0, 2) == 1)
                            ? _rnd.Next(0, GetShortageMaxBound(20, station.StorageCapacity))
                            : _rnd.Next(GetExcessMinBound(20, station.StorageCapacity),
                                GetMaxBound(20, station.StorageCapacity))
                    },
                    new()
                    {
                        UniversalMerchendise =
                            SimulationParameters.UniversalMerchendises.First(x => x.MerchendiseType == MerchendiseType.Orionum),
                        Name = "Orionum",

                        Quantity = (_rnd.Next(0, 2) == 1)
                            ? _rnd.Next(0, GetShortageMaxBound(50, station.StorageCapacity))
                            : _rnd.Next(GetExcessMinBound(50, station.StorageCapacity),
                                GetMaxBound(50, station.StorageCapacity))
                    },
                    new()
                    {
                        UniversalMerchendise =
                            SimulationParameters.UniversalMerchendises.First(x => x.MerchendiseType == MerchendiseType.Deuterium),
                        Name = "H2",
                        Quantity = (_rnd.Next(0, 2) == 1)
                            ? _rnd.Next(0, GetShortageMaxBound(1, station.StorageCapacity))
                            : _rnd.Next(GetExcessMinBound(1, station.StorageCapacity),
                                GetMaxBound(1, station.StorageCapacity))
                    },
                    new()
                    {
                        UniversalMerchendise =
                            SimulationParameters.UniversalMerchendises.First(x => x.MerchendiseType == MerchendiseType.Iron),
                        Name = "Fer",
                        Quantity = (_rnd.Next(0, 2) == 1)
                            ? _rnd.Next(0, GetShortageMaxBound(10, station.StorageCapacity))
                            : _rnd.Next(GetExcessMinBound(10, station.StorageCapacity),
                                GetMaxBound(10, station.StorageCapacity))
                    }
                    //,
                    //{
                    //    new()
                    //    {
                    //        MerchendiseType = MerchendiseType.Silicon, Name = "Silicone", Weight = 3,
                    //        UniversalValue = 80,
                    //        Quantity = (_rnd.Next(0, 2) == 1)
                    //            ? _rnd.Next(0, GetShortageMaxBound(3, Stations[i].StorageCapacity))
                    //            : _rnd.Next(GetExcessMinBound(3, Stations[i].StorageCapacity),
                    //                GetMaxBound(3, Stations[i].StorageCapacity))
                    //    }
                    //},
                    //{
                    //    new()
                    //    {
                    //        MerchendiseType = MerchendiseType.Water, Name = "Eau pure", Weight = 1, UniversalValue = 10,
                    //        Quantity = (_rnd.Next(0, 2) == 1)
                    //            ? _rnd.Next(0, GetShortageMaxBound(1, Stations[i].StorageCapacity))
                    //            : _rnd.Next(GetExcessMinBound(1, Stations[i].StorageCapacity),
                    //                GetMaxBound(1, Stations[i].StorageCapacity))
                    //    }
                    //},
                    //{
                    //    new()
                    //    {
                    //        MerchendiseType = MerchendiseType.Wood, Name = "Bois", Weight = 2, UniversalValue = 1,
                    //        Quantity = (_rnd.Next(0, 2) == 1)
                    //            ? _rnd.Next(0, GetShortageMaxBound(2, Stations[i].StorageCapacity))
                    //            : _rnd.Next(GetExcessMinBound(2, Stations[i].StorageCapacity),
                    //                GetMaxBound(2, Stations[i].StorageCapacity))
                    //    }
                    //},
                };
                station.InitializeTradingBoard();
            }
        }

        private void InitializeFlyingTraders()
        {
            FlyingTraders = new List<FlyingTrader>
            {
                new()
                {
                    ID = 1000, Account = 10000,
                    Name = "Golan Trevize",
                    OwnedShip = new Ship {ID = 10000, StorageCapacity = 100, Localisation = SimulationParameters.Stations[0]},
                    CurrentState = Trader.TraderState.InStation
                },
                new()
                {
                    ID = 1001,
                    Account = 10000,
                    Name = "Salvor Hardin",
                    OwnedShip = new Ship {ID = 10001, StorageCapacity = 100, Localisation = SimulationParameters.Stations[1]},
                    CurrentState = Trader.TraderState.InStation
                },
                new()
                {
                    ID = 1002, Account = 10000,
                    Name = "Donald J. Trump",
                    OwnedShip = new Ship {ID = 10002, StorageCapacity = 100, Localisation = SimulationParameters.Stations[2]},
                    CurrentState = Trader.TraderState.InStation
                },
                new()
                {
                    ID = 1003,
                    Account = 10000,
                    Name = "Elon Musk",
                    OwnedShip = new Ship {ID = 10003, StorageCapacity = 100, Localisation = SimulationParameters.Stations[3]},
                    CurrentState = Trader.TraderState.InStation
                },
                new()
                {
                    ID = 1004, Account = 10000,
                    Name = "Ultra Doux",
                    OwnedShip = new Ship {ID = 10004, StorageCapacity = 200, Localisation = SimulationParameters.Stations[0]},
                    CurrentState = Trader.TraderState.InStation
                },
                new()
                {
                    ID = 1005,
                    Account = 10000,
                    Name = "Médiocre UI",
                    OwnedShip = new Ship {ID = 10005, StorageCapacity = 200, Localisation = SimulationParameters.Stations[1]},
                    CurrentState = Trader.TraderState.InStation
                },
                new()
                {
                    ID = 1006, Account = 10000,
                    Name = "Marie Curie",
                    OwnedShip = new Ship {ID = 10006, StorageCapacity = 200, Localisation = SimulationParameters.Stations[2]},
                    CurrentState = Trader.TraderState.InStation
                },
                new()
                {
                    ID = 1007,
                    Account = 10000,
                    Name = "Hiroko",
                    OwnedShip = new Ship {ID = 10007, StorageCapacity = 200, Localisation = SimulationParameters.Stations[3]},
                    CurrentState = Trader.TraderState.InStation
                },
                new()
                {
                    ID = 1008, Account = 10000,
                    Name = "Jeff Bezos",
                    OwnedShip = new Ship {ID = 10004, StorageCapacity = 500, Localisation = SimulationParameters.Stations[0]},
                    CurrentState = Trader.TraderState.InStation
                },
                new()
                {
                    ID = 1009,
                    Account = 10000,
                    Name = "X71-F",
                    OwnedShip = new Ship {ID = 10005, StorageCapacity = 500, Localisation = SimulationParameters.Stations[1]},
                    CurrentState = Trader.TraderState.InStation
                },
                new()
                {
                    ID = 1010, Account = 10000,
                    Name = "Cléon XXII",
                    OwnedShip = new Ship {ID = 10006, StorageCapacity = 500, Localisation = SimulationParameters.Stations[2]},
                    CurrentState = Trader.TraderState.InStation
                },
                new()
                {
                    ID = 1011,
                    Account = 10000,
                    Name = "Caliméro",
                    OwnedShip = new Ship {ID = 10007, StorageCapacity = 500, Localisation = SimulationParameters.Stations[3]},
                    CurrentState = Trader.TraderState.InStation
                }
            };
        }
        private void InitializeFixedTraders()
        {
            FixedTraders = new List<FixedTrader>
            {
                new()
                {
                    ID = 100, Account = 10000, Name = "", WorkingStation = SimulationParameters.Stations.First(x => x.ID == 0),
                    CurrentState = Trader.TraderState.InStation
                },
                new()
                {
                    ID = 101, Account = 10000, Name = "", WorkingStation = SimulationParameters.Stations.First(x => x.ID == 1),
                    CurrentState = Trader.TraderState.InStation
                },
                new()
                {
                    ID = 102, Account = 10000, Name = "", WorkingStation = SimulationParameters.Stations.First(x => x.ID == 2),
                    CurrentState = Trader.TraderState.InStation
                },
                new()
                {
                    ID = 103, Account = 10000, Name = "", WorkingStation = SimulationParameters.Stations.First(x => x.ID == 3),
                    CurrentState = Trader.TraderState.InStation
                }
            };
        }

        private int GetShortageMaxBound(int weight, int capacity)
        {
            return (int) Math.Floor(capacity / weight * 0.25);
        }

        private int GetExcessMinBound(int weight, int capacity)
        {
            return (int) Math.Floor(capacity / weight * 0.75);
        }

        private int GetMaxBound(int weight, int capacity)
        {
            return (int) Math.Floor((double) (capacity / weight));
        }

        public void StartSimulation()
        {
            List<Task> tasks = new List<Task>();

            // instanciation des task par marchand fixe
            foreach (var fixedTrader in FixedTraders)
            {
                tasks.Add(Task.Factory.StartNew(() => { fixedTrader.StorageCleanup().Wait(); }));
            }

            // instanciation des task par marchand volant
            foreach (var flyingTrader in FlyingTraders)
            {
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(_rnd.Next(1000, 5000));
                    flyingTrader.MainLoop().Wait();
                }));
            }

            // instanciation des task par station
            foreach (var station in SimulationParameters.Stations)
            {
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    tasks.Add(Task.Factory.StartNew(() => { station.UpdateProdAndConso().Wait(); }));
                }));
            }

            // instanciation de la tâche du marché global
            tasks.Add(Task.Factory.StartNew(() => { UpdateDemands().Wait(); }));

            Task.WaitAll(tasks.ToArray());
        }

        public long GetElapsedTicks()
        {
            return _timer.ElapsedTicks;
        }
    }
}