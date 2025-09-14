namespace MuzDost
{
    using System.Globalization;
    public struct CodePricePair
    {
        public CodePricePair(int _code, float _price)
        {
            code = _code;
            price = _price;
        }
        public int code {get; private set;}
        public float price {get; private set;}
    }

    class Matcher
    {
        public Matcher(MuzData _muzData)
        {
            muzData = _muzData;
        }
        private MuzData muzData;

        // temp synchronous sollution, MAJOR performance bottleneck
        // public async void MatchAll()
        // {
        //     Task<bool>[] matchTasks = new Task<bool>[contractors.Length];
        //     bool[] taskSuccess = new bool[contractors.Length];

        //     int successCount = 0;
        //     for (int i = 0; i < contractors.Length; i++)
        //     {
        //         matchTasks[i] = Task.Run(() => Match(contractors[i]));
        //         taskSuccess[i] = false;
        //     }

        //     try
        //     {
        //         taskSuccess = await Task.WhenAll(matchTasks);
        //     }
        //     finally
        //     {
        //         for (int i = 0; i < taskSuccess.Length; i++)
        //         {
        //             if (taskSuccess[i])
        //             {
        //                 successCount++;
        //             }
        //         }
        //         Console.WriteLine($"Finished matching contractor data to muzikant data, {successCount} out of {contractors.Length} operations have been succesfull.");
        //     }
        // }

        public async Task<bool> Match(Contractor contractor)
        {
            try
            {
                int pairsCount = contractor.process.Pairs.Length;
                List<CodePricePair>[] rawCodes = new List<CodePricePair>[pairsCount];

                await Task.Run(() => SetAvailability(contractor.GetElement(contractor.process.Availability.xmlName), contractor));
                
                for (int i = 0; i < pairsCount; i++)
                {
                    string contractorSide = contractor.process.Pairs[i].conAttribute;
                    string muzikantSide = contractor.process.Pairs[i].muzAttribute;

                    // temporary serial sollution MAJOR performance bottleneck
                    rawCodes[i] = await Task.Run(() => MatchElements(contractor.GetElement(contractorSide),
                                                                     muzData.GetElement(muzikantSide),
                                                                     contractor));
                }

                List<CodePricePair> cleanCodes = rawCodes.SelectMany(list => list)
                                                         .Distinct()
                                                         .ToList();

                Saver.Save(contractor, cleanCodes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"While matching data for contractor: {contractor.name}, the following error occured\n" + ex);
            }
            return false;
        }

        private async Task<List<CodePricePair>> MatchElements(ConElement conElement, MuzElement muzElement, Contractor contractor)
        {
            return await Task.Run(() =>
            {
                // Find all matches using the dictionary
                //var matchedKeys = new List<int>();
                List<CodePricePair> matchedKeys = new List<CodePricePair>();
                for (int i = 0; i < conElement.indexedValues.Count; i++)
                {
                    if (!string.IsNullOrEmpty(contractor.process.Price.xmlName))
                    {
                        if (i >= contractor.prices.indexedValues.Count ||
                        i >= contractor.skippedRows.Count)
                            continue;
                    }

                    if (contractor.skippedRows[i])
                        continue;

                    string conVal = conElement.indexedValues[i];
                    if (muzElement.valueToCode.TryGetValue(conVal, out int code))
                    {
                        float price = 0f;

                        if (!string.IsNullOrEmpty(contractor.process.Price.xmlName))
                        {
                            float.TryParse(ClearPrice(contractor.prices.indexedValues[i]), out price);
                        }

                        matchedKeys.Add(new CodePricePair(code, price));
                    }
                }
                return matchedKeys;
            });
        }

        private async Task SetAvailability(ConElement conElement, Contractor contractor)
        {
            await Task.Run(() =>
            {
                for (int i = 0; i < conElement.indexedValues.Count; i++)
                {
                    string conVal = conElement.indexedValues[i];

                    if (conElement.name == contractor.process.Availability.xmlName)
                    {
                        // avail code
                        switch (contractor.process.AvailType)
                        {
                            //0 => stockAmount
                            //1 => true, false
                            //2 => ANO, NE
                            //3 => delivery date
                            case 0 when int.TryParse(conVal, out int stock) && stock > 0:
                            case 1 when bool.TryParse(conVal, out bool isAvailable) && isAvailable:
                            case 2 when conVal == "ANO":
                            case 3 when int.TryParse(conVal, out int date) && date == 0:
                                contractor.skippedRows[i] = false;
                                break;
                        }
                    }
                }
            });
        }

        private string ClearPrice(string value)
        { 
            var allowedChars = "01234567890.,";
            string clearedPrice = new string(value.Where(c => allowedChars.Contains(c)).ToArray());
            try
            {
                clearedPrice = clearedPrice.Remove(clearedPrice.IndexOf(','));
            }
            catch {}

            return clearedPrice;
        }
    }
}