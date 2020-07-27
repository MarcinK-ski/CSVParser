using System;

namespace CSVParser.Example
{
    public class Holding
    {
        [ParserAttributes("<NAME>")]
        public string Name { get; set; }

        [ParserAttributes("<TICKER>")]
        public string Ticker { get; set; }

        [ParserAttributes("<PER>")]
        public string Per { get; set; }

        [ParserAttributes("<DATE>")]
        public string Date { get; set; }

        [ParserAttributes("<TIME>")]
        public string Time { get; set; }

        public DateTime Timestamp { get; set; }

        [ParserAttributes("<OPEN>")]
        public float Open { get; set; }

        [ParserAttributes("<HIGH>")]
        public float High { get; set; }

        [ParserAttributes("<LOW>")]
        public float Low { get; set; }

        [ParserAttributes("<CLOSE>")]
        public float Close { get; set; }

        [ParserAttributes("<VOL>")]
        public double Vol { get; set; }

        [ParserAttributes("<OPENINT>")]
        public int Openint { get; set; }

        public Holding()
        {
        }

        public override string ToString()
        {
            return $"Name: {Name ?? "null"}\nTicker: {Ticker}\nPer: {Per}\nDate: {Date}\nTime: {Time}\nOpen: {Open}\nHigh: {High}\netc.";
        }
    }
}
