namespace CSVParser.Example
{
    class TickerNameSystem
    {
        [ParserAttributes("<NAME>")]
        public string Name { get; set; }

        [ParserAttributes("<TICKER>")]
        public string Ticker { get; set; }

        public override string ToString()
        {
            return $"TickerNameSystem: T: {Ticker} - N: {Name}.";
        }
    }
}
