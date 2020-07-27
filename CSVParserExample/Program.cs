using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;

namespace CSVParser.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-GB");

            /*Console.WriteLine("Hello World!");
            string newData = FilesManager.DownloadRemoteFile("https://stooq.pl/db/d/?d=20200603&t=d");
            List<Holding> holdings = FilesManager.ParseCsvToObject<Holding>(newData);

            string restData = FilesManager.DownloadRemoteFile("https://stooq.pl/db/l/?g=6", true);
            List<TickerNameSystem> tickerNameSystems = FilesManager.ParseCsvToObject<TickerNameSystem>(restData, ' ', StringSplitOptions.RemoveEmptyEntries);

            FilesManager filesManager = new FilesManager("<TICKER>");
            filesManager.MergeData(holdings, tickerNameSystems);

            holdings.RemoveAll(h => h.Name == null);

            Holding holding = holdings.Find(h => h.Name == "PKNORLEN");

            Console.ReadKey();*/


            Console.WriteLine("Hello World!");
            string path = @"C:\temp\stocks";
            string[] fileInfos = Directory.GetFiles(path);

            Dictionary<string, List<Holding>> holdingDictionary = new Dictionary<string, List<Holding>>();

            for (int i = 0; i < fileInfos.Length; i++)
            {
                string currentFile = fileInfos[i];
                Console.WriteLine($"{i + 1}/{fileInfos.Length} ---> {currentFile}");

                string currentFileContent = File.ReadAllText(currentFile);

                List<Holding> currentHoldings = FilesManager.ParseCsvToObject<Holding>(currentFileContent);

                foreach (Holding holding in currentHoldings)
                {
                    if (holdingDictionary.ContainsKey(holding.Date))
                    {
                        holdingDictionary[holding.Date].Add(holding);
                    }
                    else
                    {
                        List<Holding> newHoldingList = new List<Holding>
                        {
                            holding
                        };

                        holdingDictionary.Add(holding.Date, newHoldingList);
                    }
                }

                //if (i == 50) break;
            }

            int dicLength = holdingDictionary.Count;
            int current = 1;
            foreach (var holdings in holdingDictionary)
            {
                Console.WriteLine($"{current++}/{dicLength}");

                string csvContent = FilesManager.ParseObjectToCsv(holdings.Value, "<TICKER>,<PER>,<DATE>,<TIME>,<OPEN>,<HIGH>,<LOW>,<CLOSE>,<VOL>,<OPENINT>");

                File.WriteAllText($"C:/temp/stocks_converted/{holdings.Key}_d", csvContent);
            }
        }
    }
}
