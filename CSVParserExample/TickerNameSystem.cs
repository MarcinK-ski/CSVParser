namespace CSVParser.Example
{
    class TickerNameSystem
    {
        [ParserAttributes("<NAME>")]
        public string Name { get; set; }

        [ParserAttributes("<TICKER>")]
        public string Ticker { get; set; }
    }
}
