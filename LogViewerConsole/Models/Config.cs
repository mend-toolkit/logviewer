using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Reflection.Metadata.Ecma335;
using System.Net;
using LogViewerConsole.Helpers;
 
namespace LogViewerConsole.Models
{
    public class Config
    {
        public Root JSON { get; set; }
        public Config(String filename)
        {
            try
            {
                if (string.IsNullOrEmpty(filename))
                {
                    filename = @"LogParser.config";
                }

                if (!File.Exists(filename))
                {
                    GetConfigFromWebclient("https://raw.githubusercontent.com/mend-toolkit/LogParser/main/LogParser.config");
                    Console.WriteLine("Configuration file source - Github");
                    return;
                }

                string json = File.ReadAllText(filename);

                var options = new JsonSerializerOptions { AllowTrailingCommas = true };
                var result = JsonSerializer.Deserialize<Root>(json, options);
                if (result == null)
                {
                    throw new Exception("Configuration File issue has been identified.");
                }

                JSON = result;

                if (JSON == null)
                {
                    throw new Exception("Configuration File is invalid");
                }
            }
            catch (Exception ex)
            {
                //Throw Error Back
                throw;
            }
        
        }

        public void GetConfigFromWebclient(string url)
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    var json = wc.DownloadString(url);

                    var options = new JsonSerializerOptions { AllowTrailingCommas = true };
                    var result = JsonSerializer.Deserialize<Root>(json, options);
                    if (result == null)
                    {
                        throw new Exception("Configuration File issue has been identified.");
                    }

                    JSON = result;

                    if (JSON == null)
                    {
                        throw new Exception("Configuration File is invalid");
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }

        public class selectionFind
    {
        public string find { get; set; }
        public string regEx { get; set; }
        public string direction { get; set; }
    }

    public class ProcessJSON
    {
        public string jsonStartIndexOf { get; set; }
        public string jsonEndIndexOf { get; set; }
    }

    public class DictionaryItems
    {
        public string key { get; set; }
        public string textStartIndexOf { get; set; }
        public string textEndIndexOf { get; set; }
        public object value { get; set; }
        public string text { get; set; }
        public string itemValue { get; set; }
        public int index { get; set; }

    }

    public class textSelectionDetails
    {
        public string findUp { get; set; }
        public string findDown { get; set; }
    }

    public class textSelection
    {
        public string find { get; set; }
        public string regex { get; set; }
        public string exclude { get; set; }
        public selectionFind? advancedFind { get; set; }
        public string RelatesToName { get; set; }

    }

    public class Identifier
    {
        public string find { get; set; }
    }

    public class LogTypeIdentifier
    {
        public string name { get; set; }
        public List<Identifier> identifier { get; set; }
        public List<SearchTerm> searchTerms { get; set; }
        public SummaryLayout summaryLayout { get; set; }
        public bool showFindingResultsAtBottom { get; set; }
    }

    public class Root
    {
        public List<LogTypeIdentifier> logTypeIdentifier { get; set; }
    }

    public class Row
    {
        public string text { get; set; }
        public int index { get; set; }
        public string json { get; set; }
    }

    public class SearchTerm
    {
        public bool groupResults { get; set; }
        public string name { get; set; }
        public List<textSelection> filters { get; set; }
        public List<textSelectionDetails> textSelection { get; set; }
        public string outputText { get; set; }
        public bool? includeInSummary { get; set; }

        public bool dedupe { get; set; }
        public bool? display { get; set; }
        public List<DictionaryItems> dictionaryItems { get; set; }
        public string Summary { get; set; }
        public string nextSteps { get; set; }
        public List<textSelection> filter { get; set; }
        public List<int> indexes { get; set; }
        public Dictionary<int, List<DictionaryItems>> dictionaryResults { get; set; }
        public ProcessJSON ProcessJSON { get; set; }
        public List<Row> output { get; set; }
        public List<Row> outputSummary { get; set; }
        public List<Row> outputNextSteps { get; set; }
        public List<Row> tempLines { get; set; }
    }

    public class SummaryLayout
    {
        public List<Row> rows { get; set; }
    }

}