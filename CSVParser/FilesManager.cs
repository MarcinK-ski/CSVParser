using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;

namespace CSVParser
{
    public class FilesManager
    {
        /// <summary>
        /// All properties in TBase object
        /// </summary>
        private PropertyInfo[] _propertiesOfBaseObject;

        /// <summary>
        /// All properties in TMergeData object
        /// </summary>
        private PropertyInfo[] _propertiesOfObjectToMerge;

        /// <summary>
        /// Property in TBase object, which contains attribute equals to key
        /// </summary>
        private PropertyInfo _keyPropertyOfBaseObject;

        /// <summary>
        /// Property in TMergeData object, which contains attribute equals to key
        /// </summary>
        private PropertyInfo _keyPropertyOfMergeDataObject;

        /// <summary>
        /// Column name, which is key to merge data
        /// </summary>
        public string Key { get; set; }

        public FilesManager(string key)
        {
            Key = key;
        }

        /// <summary>
        /// Merging properties with this same attribute in both classes
        /// </summary>
        /// <typeparam name="TBase">Main class</typeparam>
        /// <typeparam name="TMergeData">Merge from class</typeparam>
        /// <param name="baseData">Merge to this object</param>
        /// <param name="dataToMerge">Merge from this object to TBase</param>
        public void MergeData<TBase, TMergeData>(List<TBase> baseData, List<TMergeData> dataToMerge)
        {
            _propertiesOfBaseObject = typeof(TBase).GetProperties();
            _propertiesOfObjectToMerge = typeof(TMergeData).GetProperties();

            _keyPropertyOfBaseObject = GetKeyPropertyInfoByCsvName(_propertiesOfBaseObject, Key);
            _keyPropertyOfMergeDataObject = GetKeyPropertyInfoByCsvName(_propertiesOfObjectToMerge, Key);

            for (int i = 0; i < baseData.Count; i++)
            {
                TBase currentBaseRecord = baseData[i];
                object currentBaseKeyValue = _keyPropertyOfBaseObject.GetValue(currentBaseRecord);

                TMergeData data = dataToMerge.Find(dm => _keyPropertyOfMergeDataObject.GetValue(dm).Equals(currentBaseKeyValue));

                CompareTBaseAndTMergeDataAndFillEmptyProperties(data, currentBaseRecord);
            }
        }

        /// <summary>
        /// Comparation TBase and and TMergeData, to set empty properties in TBase
        /// </summary>
        /// <typeparam name="TMergeData"></typeparam>
        /// <typeparam name="TBase"></typeparam>
        /// <param name="data"></param>
        /// <param name="currentBaseRecord"></param>
        private void CompareTBaseAndTMergeDataAndFillEmptyProperties<TMergeData, TBase>(TMergeData data, TBase currentBaseRecord)
        {
            if (data != null)
            {
                foreach (PropertyInfo property in _propertiesOfBaseObject)
                {
                    if (property != _keyPropertyOfBaseObject || property.GetValue(currentBaseRecord) == default)
                    {
                        ParserAttributes attribute = Attribute.GetCustomAttribute(property, typeof(ParserAttributes)) as ParserAttributes;
                        SetBaseWithNewValueIfMergeDataContainsProperty(attribute, property, currentBaseRecord, data);
                    }
                }
            }
        }

        private void SetBaseWithNewValueIfMergeDataContainsProperty<TBase, TMergeData>(ParserAttributes attribute, PropertyInfo property, TBase currentBaseRecord, TMergeData data)
        {
            if (attribute != null)
            {
                foreach (PropertyInfo mergeProperty in _propertiesOfObjectToMerge)
                {
                    if (mergeProperty != _keyPropertyOfMergeDataObject)
                    {
                        ParserAttributes mergeAttribute = Attribute.GetCustomAttribute(mergeProperty, typeof(ParserAttributes)) as ParserAttributes;
                        if (mergeAttribute != null && attribute.CsvName.Equals(mergeAttribute.CsvName))
                        {
                            var mergePropertyValue = mergeProperty.GetValue(data);
                            property.SetValue(currentBaseRecord, mergePropertyValue);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get PropertyInfo, which has attribute name equals to key
        /// </summary>
        /// <param name="propertyInfos">All PropertiesInfo in class</param>
        /// <param name="key">Attribute name</param>
        /// <returns>PropertyInfo for property with key attribute</returns>
        private PropertyInfo GetKeyPropertyInfoByCsvName(PropertyInfo[] propertyInfos, string key)
        {
            foreach (PropertyInfo property in propertyInfos)
            {
                ParserAttributes attribute = Attribute.GetCustomAttribute(property, typeof(ParserAttributes)) as ParserAttributes;
                if (attribute != null && attribute.CsvName.Equals(key))
                {
                    return property;
                }
            }

            return null;
        }

        /// <summary>
        /// Parsing CSV file contetnt to object T
        /// </summary>
        /// <typeparam name="T">Generic object</typeparam>
        /// <param name="data">CSV file content</param>
        /// <param name="delimiter">Delimiter char, used in CSV file</param>
        /// <param name="stringSplitOptions">Specifies whether applicable Overload:System.String.Split method overloads include or omit empty substrings from the return value.</param>
        /// <returns>List with T objects parsed from CSV</returns>
        public static List<T> ParseCsvToObject<T>(string data, char delimiter = ',', StringSplitOptions stringSplitOptions = StringSplitOptions.None) where T : new()
        {
            List<T> listOfObjects = new List<T>();

            char[] separator = { delimiter };

            using (StringReader reader = new StringReader(data))
            {
                int lineNo = 0;
                string line = string.Empty;
                Dictionary<string, int> headerSet = new Dictionary<string, int>();

                while ((line = reader.ReadLine()) != null)
                {
                    string[] splittedLine = line.Split(separator, stringSplitOptions);

                    if (++lineNo == 1)
                    {
                        for (int i = 0; i < splittedLine.Length; i++)
                        {
                            string currentColumnName = splittedLine[i];
                            headerSet.Add(currentColumnName, i);
                        }
                    }
                    else
                    {
                        T parsedObject = new T();

                        PropertyInfo[] properties = typeof(T).GetProperties();

                        foreach (PropertyInfo property in properties)
                        {
                            ParserAttributes attribute = Attribute.GetCustomAttribute(property, typeof(ParserAttributes)) as ParserAttributes;

                            if (attribute != null)
                            {
                                var currentColumn = headerSet.FirstOrDefault(header => header.Key == attribute.CsvName);
                                if (currentColumn.Key != null)
                                {
                                    int numberOfCurrentColumn = currentColumn.Value;
                                    string currentColumnValue = splittedLine[numberOfCurrentColumn];

                                    Type propType = property.PropertyType;
                                    object convertedColumnValue = Convert.ChangeType(currentColumnValue, propType, CultureInfo.InvariantCulture);

                                    property.SetValue(parsedObject, convertedColumnValue);
                                }
                            }
                        }

                        listOfObjects.Add(parsedObject);
                    }
                }
            }

            return listOfObjects;
        }

        /// <summary>
        /// Parsing List with objects T to string with CSV format
        /// </summary>
        /// <typeparam name="T">Generic object</typeparam>
        /// <param name="data">List with T-Objects</param>
        /// <param name="headerWithColumnNames">Header with column names (ex. "<Column1>,<Column2>,<Column3>,<Column4>")</param>
        /// <param name="headerDelimiter">Delimiter for header line. By default is ','.</param>
        /// <returns></returns>
        public static string ParseObjectToCsv<T>(List<T> data, string headerWithColumnNames, char headerDelimiter = ',')
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(headerWithColumnNames);
            Dictionary<string, int> headerSet = new Dictionary<string, int>();

            string[] columnName = headerWithColumnNames.Split(headerDelimiter);
            for (int i = 0; i < columnName.Length; i++)     // DUPLICATE with line ~158!
            {
                headerSet.Add(columnName[i], i);
            }
            stringBuilder.Append("\n");

            for (int i = 0; i < data.Count; i++)
            {
                //Console.WriteLine($"{i + 1}/{data.Count} ---> {data}");

                PropertyInfo[] properties = typeof(T).GetProperties();

                int propertyCounter = 0;
                foreach (PropertyInfo property in properties)
                {
                    ParserAttributes attribute = Attribute.GetCustomAttribute(property, typeof(ParserAttributes)) as ParserAttributes;

                    if (attribute != null)
                    {
                        var currentColumn = headerSet.FirstOrDefault(header => header.Key == attribute.CsvName);
                        if (currentColumn.Key != null)
                        {
                            if (propertyCounter++ != 0)
                            {
                                stringBuilder.Append(",");
                            }

                            string currentColumnValue = property.GetValue(data[i]).ToString();
                            stringBuilder.Append(currentColumnValue);
                        }
                    }
                }
                stringBuilder.Append("\n");
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Downloading string of file from remote server 
        /// </summary>
        /// <param name="fileUrl">Url to file on remote server</param>
        /// <param name="isResultGzipped">Set true, if url returns gzip encoding</param>
        /// <returns>Content of downloaded file</returns>
        public static string DownloadRemoteFile(string fileUrl, bool isResultGzipped = false)
        {
            using (WebClient client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                if (isResultGzipped)
                {
                    client.Headers[HttpRequestHeader.AcceptEncoding] = "gzip";
                    GZipStream responseStream = new GZipStream(client.OpenRead(fileUrl), CompressionMode.Decompress);
                    StreamReader reader = new StreamReader(responseStream);
                    string result = reader.ReadToEnd();
                    return result;
                }
                else
                {
                    return client.DownloadString(fileUrl);
                }
            }
        }
    }
}
