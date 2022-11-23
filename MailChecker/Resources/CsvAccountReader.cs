using System.Collections.Generic;
using CsvHelper;
using System.Globalization;
using System.IO;
using CsvHelper.Configuration;

namespace MailChecker.Resources
{
    public class CsvAccountReader
    {
        public struct MailAccount
        {
            public string Mail { get; set; }
            public string Password { get; set; }
            public string Proxy { get; set; }
        }
        
        public static List<MailAccount> ReadAccount(string fileFullPath)
        {
            List<MailAccount> accounts = new();

            var csvConfig = new CsvConfiguration(CultureInfo.CurrentCulture)
            {
                HasHeaderRecord = false
            };

            using var streamReader = File.OpenText(fileFullPath);
            using var csvReader = new CsvReader(streamReader, csvConfig);

            int iter = 0;

            while (csvReader.Read())
            {
                if (iter != 0)
                {
                    for (int i = 0; csvReader.TryGetField(i, out string? value); i++)
                    {
                        var csvData = value!.Split(',');
                        var login = csvData[0];
                        if (!string.IsNullOrEmpty(csvData[0]) && !string.IsNullOrEmpty(csvData[1]))
                        {
                            accounts.Add(new() { Mail = csvData[0], Password = csvData[1], Proxy = csvData[2]});
                        }
                    }
                }
                iter++;
            }
            return accounts;
        }
    
        public static void WriteData(List<MailAccount> ramblerObjects)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false
            };

            using var writer = new StreamWriter("checkedAccounts.csv", true);
            using var csv = new CsvWriter(writer, config);
            csv.WriteRecords(ramblerObjects);
        }
    }
}