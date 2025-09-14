namespace MuzDost
{
    static class Saver
    {
        private static List<(CodePricePair, string)> master = new List<(CodePricePair, string)>();

        public static object padlock = new object();

        public static void Save(Contractor contractor, List<CodePricePair> codes)
        {
            lock (padlock)
            {
                string fileName = Program.resultDir + contractor.name + ".csv";
                using (StreamWriter sr = new StreamWriter(fileName))
                {
                    for (int i = 0; i < codes.Count; i++)
                    {
                        string location = DetermineLocation(contractor.process.PriceToAvailType, codes[i].price);
                        sr.WriteLine($"{codes[i].code};{codes[i].price};{location}");
                        master.Add((codes[i], location));
                    }
                }
                Console.WriteLine($"{contractor.name} match result has been saved to {fileName}!");
            }
        }

        public static void SaveAll()
        {
            string fileName = Program.resultDir + "master.csv";
            using (StreamWriter sr = new StreamWriter(fileName))
            {
                for (int i = 0; i < master.Count; i++)
                {
                    sr.WriteLine($"{master[i].Item1.code};{master[i].Item1.price};{master[i].Item2}");
                }
            }
            Console.WriteLine($"All match results saved to master.xls!");
        }

        private static string DetermineLocation(int priceToAvailType, float price)
        {
            //0 => 0-999 = externí sklad4,
            //0 => 1000-4999 = externí sklad,
            //0 => 5000+ = kudihou
            //---
            //1 => 0-1999 = externí sklad4,
            //1 => 2000-7999 = externí sklad,
            //1 => 8000+ = kudihou
            return priceToAvailType switch
            {
                0 => price switch
                {
                    >= 5000 => "kudihou",
                    >= 1000 => "externí sklad",
                    _ => "externí sklad4"
                },
                1 => price switch
                {
                    >= 8000 => "kudihou",
                    >= 2000 => "externí sklad",
                    _ => "externí sklad4"
                },
                _ => "externí sklad4"
            };
        }
    }
}