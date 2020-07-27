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
            if (Thread.CurrentThread.CurrentCulture.Name == "pl-PL")
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-GB");   // pl-PL has comma as delimiter instead of dot
            }
            
            Console.WriteLine("Hello World!");
            
            char decision = 'x';
            while (decision != 'E' && decision != 'e')
            {
                Console.Write("\nWhat you gonna do? [(A)ddTickerNameToHoldings / (G)enerateCSVFileByDate / (E)xit] ");
                decision = Console.ReadKey().KeyChar;

                switch (decision)
                {
                    case 'A':
                        Console.Write("\nDo you want to download data from web? [Y/N] ");
                        char download = Console.ReadKey().KeyChar;
                        Console.WriteLine();
                        AddTickerNameToHoldings(download == 'Y' || download == 'y');
                        break;
                    case 'G':
                        GenerateCSVFileByDate();
                        break;
                }
            }
        }

        /// <summary>
        /// Adds "Ticker name" from file to Holdings. If Holding has no name, will be removed.
        /// </summary>
        /// <param name="isDownloadingFromWeb">Should data be downloaded from web or get from local disc. 
        /// (You may have to change date in holdingsData URL, because stooq.pl store only few files in this way!)</param>
        private static void AddTickerNameToHoldings(bool isDownloadingFromWeb = true)
        {
            string holdingsData;
            string tickersWithNames;

            if (isDownloadingFromWeb)
            {
                holdingsData = FilesManager.DownloadRemoteFile("https://stooq.pl/db/d/?d=20200603&t=d");
                tickersWithNames = FilesManager.DownloadRemoteFile("https://stooq.pl/db/l/?g=6", true);
            }
            else
            {
                holdingsData = File.ReadAllText(@"C:\temp\20200727_d.txt");
                tickersWithNames = File.ReadAllText(@"C:\temp\TickerToName.txt");
            }

            List<Holding> holdings = FilesManager.ParseCsvToObject<Holding>(holdingsData);
            List<TickerNameSystem> tickerNameSystems = FilesManager.ParseCsvToObject<TickerNameSystem>(tickersWithNames, ' ', StringSplitOptions.RemoveEmptyEntries);

            string lookingForTicker = "PKN";    //For working check
            string lookingForName = "PKNORLEN";     //For working check

            Console.WriteLine($"I'm looking for ticker: \"{lookingForTicker}\".");
            Holding holding = holdings.Find(h => h.Ticker == lookingForTicker);
            if (holding == null)
            {
                Console.WriteLine("Ticker was not found");
                return;
            }

            Console.WriteLine($"Now, holding looks like that: \n{holding} \n\nPress any key to start merging.");
            Console.ReadKey();

            FilesManager filesManager = new FilesManager("<TICKER>");
            filesManager.MergeData(holdings, tickerNameSystems);

            holdings.RemoveAll(h => h.Name == null);

            Console.WriteLine($"Now I'm looking for name: \"{lookingForName}\" instead of ticker: \"{lookingForTicker}\".");
            Holding newHolding = holdings.Find(h => h.Name == lookingForName);
            if (newHolding == null)
            {
                Console.WriteLine("Holding by this name, was not found");
            }
            else
            {
                Console.WriteLine($"\n\nAfter merging with ticker , holding looks like that: \n{newHolding}");
            }

            Console.WriteLine("Press any key to continue. (You can set breakpoint here if you want to look at full merging result).");
            Console.ReadKey();
        }

        // Stooq store archived data divided by COMPANY but not divided by date.
        /// <summary>
        /// Gets every "COMPANY" file and creates files "divided by date"
        /// </summary>
        private static void GenerateCSVFileByDate()
        {
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
