using System;

namespace CSVParser
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ParserAttributes : Attribute
    {
        public string CsvName { get; set; }

        public ParserAttributes(string csvName)
        {
            CsvName = csvName;
        }
    }
}
