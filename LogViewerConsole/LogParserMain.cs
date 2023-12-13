using LogViewerConsole.Models;
using System.Text;
using System.Text.RegularExpressions;
using LogViewerConsole.Helpers;
using System.Text.Json;

namespace LogViewerConsole
{
    public class LogViewerConsole
    {
        readonly string Version = "1.0.0.0";
        string outputLog = string.Empty;
        string logFile = string.Empty;
        string useConfig = string.Empty;    
        Config config;
        List<String> NextSteps = new List<String>();
        List<String> SummaryDedupe = new List<String>();

        public void Start()
        {
            try
            {
                ProcessCommandLine();

                List<string> lines = LoadFile(logFile);

                if (lines.Count == 0)
                {
                    Console.WriteLine("No data to review.");
                    return;
                }

                LogTypeIdentifier configurationToUse = IdentifyFile(lines, config);
                if (configurationToUse == null)
                {
                    Console.WriteLine("\r\nNo configuration file identified.  Exiting.");
                    return;
                }

                //If the output log exists, delete it
                if (outputLog != string.Empty)
                {
                    FileHelper.DeleteFile(outputLog);
                }

                FindResults(lines, configurationToUse);

                WriteOutput(configurationToUse, lines);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void FindResults(List<string> lines, LogTypeIdentifier configurationToUse)
        {
            //First Pass - Get details
            //Since there is a feature called relates to, results have to be stored in memory for that purpose.
            foreach (SearchTerm searchItem in configurationToUse.searchTerms)
            {
                if (searchItem != null)
                {
                    searchItem.output = new List<Row>();
                    searchItem.outputNextSteps = new List<Row>();
                    searchItem.outputSummary = new List<Row>();

                    //Find all relevant lines
                    List<int> indexes = FindAllIndexes(lines, searchItem);
                    searchItem.indexes = indexes;

                    //Populate results in memory
                    foreach (int index in indexes)
                    {
                        CreateOutputText(lines, index, searchItem);

                        if (searchItem.display == null || searchItem.display == true || !string.IsNullOrEmpty(searchItem.Summary))
                        {
                            CreateOutputTextSummary(lines, index, searchItem);
                        }

                        CreateOutputTextSummaryNextSteps(lines, index, searchItem);
                    }
                }
            }

            //This might require multiple passes in case the relates to is out of order
            bool relatesToCompleted = false;
            while (!relatesToCompleted)
            {
                relatesToCompleted = true;

                //Handle Relates to data
                foreach (SearchTerm searchItem in configurationToUse.searchTerms)
                {
                    //Handle Relates To 
                    if (searchItem.indexes.Count == 0)
                    {
                        foreach (textSelection item in searchItem.filters)
                        {
                            if (!string.IsNullOrEmpty(item.RelatesToName))
                            {
                                CreateOutputRelatesToSummary(lines, configurationToUse, searchItem, item.RelatesToName);
                                relatesToCompleted = false;
                            }
                        }
                    }
                }
            }
        }

        private void ProcessCommandLine()
        {
            Console.WriteLine("Args passed in: ");
            try
            {
                string[] args = Environment.GetCommandLineArgs();

                foreach (string arg in args)
                {
                    //Console.WriteLine("Arg:" + arg);
                    string[] param = arg.Split('=');
                    
                    if (param.Length == 1)
                    {
                        if (param[0].ToLower().Replace("-","").Trim() == "help")
                        {
                            ShowHelp();
                        }
                    
                        if (args.Length == 1)
                        {
                            if (File.Exists(param[0]) && !param[0].Contains(".dll") && !param[0].Contains(".exe"))
                            {
                                logFile = param[0];
                                Console.WriteLine("file: " + logFile);
                                if (config == null)
                                {
                                    config = new Config("LogParser.config");
                                }
                            }
                        }
                    }

                    if (param.Length == 2)
                    {
                        switch (param[0].ToLower())
                        {
                            case "file":
                                logFile = param[1];
                                Console.WriteLine("file: " + logFile);
                                break;
                            case "log":
                                outputLog = param[1];
                                Console.WriteLine("outputLog: " + outputLog);
                                break;
                            case "config":
                                config = new Config(param[1]);
                                Console.WriteLine("config: " + param[1]);
                                break;
                            case "useconfig":
                                useConfig = param[1];
                                Console.WriteLine("useconfig: " + param[1]);
                                break;
                        }
                    }
                }
                if (config == null)
                {
                    //Pull config from Github
                    config = new Config("LogParser.config");
                }

                if (string.IsNullOrEmpty(logFile))
                {
                    Console.WriteLine("\r\nMissing file to review.\r\nHELP");
                    ShowHelp();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void ShowHelp()
        {
            Console.WriteLine("Version: " + Version);
            Console.WriteLine("\r\nParameters:\r\nfile=\"<filename to be reviewed>\"");
            Console.WriteLine("outputLog=\"<output results filename>\"");
            Console.WriteLine("useconfig=\"<configuration to use in the config file>\"");
            Console.WriteLine("config=\"<offline or local configuration file to use>\"\r\n");
            Environment.Exit(0);
        }

        private void WriteOutput(LogTypeIdentifier configurationToUse, List<string> lines)
        {
            Helpers.FileHelper.WriteFile(outputLog, "Configuration Used: " + configurationToUse.name);
            Helpers.FileHelper.WriteFile(outputLog, "File Reviewed: " + logFile);
            Helpers.FileHelper.WriteFile(outputLog, "\r\nFindings directly from file");
            Helpers.FileHelper.WriteFile(outputLog, "----------------------------------------------------------");

            foreach (SearchTerm searchItem in configurationToUse.searchTerms)
            {
                if (searchItem != null)
                {
                    foreach (Row item in searchItem.output)
                    {
                        if (searchItem.display == null || searchItem.display == true)
                        {
                            WriteOutputText(item.text, searchItem);
                        }
                    }

                    if (searchItem.output.Count > 0 && searchItem.display == true)
                    {
                        Helpers.FileHelper.WriteFile(outputLog, " ");
                    }
                }
            }

            Helpers.FileHelper.WriteFile(outputLog, "\r\nSummary");
            Helpers.FileHelper.WriteFile(outputLog, "----------------------------------------------------------");

            foreach (SearchTerm searchItem in configurationToUse.searchTerms)
            {
                if (searchItem != null)
                {
                    if (searchItem.includeInSummary == true)
                    {
                        foreach (Row item in searchItem.outputSummary)
                        {
                            WriteOutputSummary(item.text, searchItem);
                        }

                        if (searchItem.outputSummary.Count > 0)
                        {
                            Helpers.FileHelper.WriteFile(outputLog, " ");
                        }
                    }
                }
            }

            Helpers.FileHelper.WriteFile(outputLog, "\r\nNext Steps");
            Helpers.FileHelper.WriteFile(outputLog, "----------------------------------------------------------");

            foreach (SearchTerm searchItem in configurationToUse.searchTerms)
            {
                if (searchItem != null)
                {
                    foreach (Row item in searchItem.outputNextSteps)
                    {
                        WriteOutputSummary(item.text, searchItem);
                    }

                    if (searchItem.outputNextSteps.Count > 0)
                    {
                        Helpers.FileHelper.WriteFile(outputLog, " ");
                    }
                }
            }

            if (configurationToUse.showFindingResultsAtBottom)
            {
                Helpers.FileHelper.WriteFile(outputLog, " ");
                Helpers.FileHelper.WriteFile(outputLog, "SUMMARY of Findings");
                Helpers.FileHelper.WriteFile(outputLog, "----------------------------------------------------------");
                foreach (SearchTerm searchItem in configurationToUse.searchTerms)
                {
                    if (searchItem != null)
                    {
                        if (!string.IsNullOrEmpty(searchItem.name))
                        {
                            Helpers.FileHelper.WriteFile(outputLog, "Filter: " + searchItem.name + "   Count: " + searchItem.indexes.Count.ToString());
                        }
                        else 
                        {
                            Helpers.FileHelper.WriteFile(outputLog, "Filter: Undefined Name   Count: " + searchItem.indexes.Count.ToString());
                        }

                    }
                }
            }
        }

        private void CreateOutputRelatesToSummary(List<string> lines, LogTypeIdentifier configurationToUse, SearchTerm searchItem, string relatesToName)
        {
            if (!string.IsNullOrEmpty(relatesToName))
            {
                if (searchItem.dictionaryResults == null)
                {
                    searchItem.dictionaryResults = new Dictionary<int, List<DictionaryItems>>();
                }

                SearchTerm relatedItem = FindRelatedSearchItem(configurationToUse, relatesToName);
                if (relatedItem != null)
                {
                    foreach (Row outputText in relatedItem.output)
                    {
                        ProcessRelatesToItem(outputText, searchItem);
                    }
                    foreach (Row outputText in relatedItem.outputSummary)
                    {
                        ProcessRelatesToItem(outputText, searchItem);
                    }
                    foreach (Row outputText in relatedItem.outputNextSteps)
                    {
                        ProcessRelatesToItem(outputText, searchItem);
                    }
                }
            }
        }

        private void ProcessRelatesToItem(Row outputText, SearchTerm searchItem)
        {
            if (containsFindInfo(outputText.text, searchItem.filters))
            {
                if (!string.IsNullOrEmpty(outputText.json))
                {
                    string[] jsonLines = outputText.json.Split("{".ToCharArray());
                    for (int i = 0; i < jsonLines.Length; i++)
                    {
                        if (containsFindInfo(jsonLines[i], searchItem.filters))
                        {
                            ProcessRelatesToItem(jsonLines[i], outputText.index, outputText, searchItem);
                        }
                    }
                }
                else
                {
                    ProcessRelatesToItem(outputText.text, outputText.index, outputText, searchItem);
                }
            }
        }
        private void ProcessRelatesToItem(string text, int index, Row outputText, SearchTerm searchItem)
        {
            if (containsFindInfo(text, searchItem.filters))
            {
                string outputSummary = searchItem.Summary;
                string outputNextSteps = searchItem.nextSteps;
                List<DictionaryItems> dictionaryItems = ProcessDictionaryItems(text, searchItem.dictionaryResults.Count, searchItem);

                foreach (DictionaryItems item in dictionaryItems)
                {
                    outputSummary = outputSummary.Replace("{{dictionary(" + item.key + ")}}", item.itemValue);
                    outputNextSteps = outputNextSteps.Replace("{{dictionary(" + item.key + ")}}", item.itemValue);
                }

                if (outputSummary != null)
                {
                    searchItem.outputSummary.Add(CreateRow(index, outputSummary, string.Empty));
                }
                if (outputNextSteps != null)
                {
                    searchItem.outputNextSteps.Add(CreateRow(index, outputNextSteps, string.Empty));
                }

                if (outputSummary == null && outputNextSteps == null)
                {
                    searchItem.output.Add(CreateRow(index, text, string.Empty));
                }

            }
        }

        private bool containsFindInfo(string text, List<textSelection> find)
        {
            if (find == null)
            {
                return false;
            }

            for (int item = 0; item < find.Count; item++)
            {
                if (find[item].RelatesToName == null)
                {
                    if (!text.ToUpper().Contains(find[item].find.ToUpper()))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private SearchTerm FindRelatedSearchItem(LogTypeIdentifier configurationToUse, string name)
        {
            foreach (SearchTerm searchItem in configurationToUse.searchTerms)
            {
                if (searchItem.name == name)
                {
                    return searchItem;
                }
            }
            return null;
        }

        private void ProcessDictionary(string text, int index, SearchTerm searchItem)
        {
            //validate that the dictionary has been init
            if (searchItem.dictionaryResults == null)
            {
                searchItem.dictionaryResults = new Dictionary<int, List<DictionaryItems>>();
            }

            if (searchItem.dictionaryItems != null)
            {
                searchItem.dictionaryResults.Add(index, ProcessDictionaryItems(text, index, searchItem));
                return;
            }
        }

        private List<DictionaryItems> ProcessDictionaryItems(string text, int index, SearchTerm searchItem)
        {
            List<DictionaryItems> dictionaryItems = new List<DictionaryItems>();

            if (searchItem.dictionaryItems != null)
            {
                //validate that the dictionary has been init
                if (searchItem.dictionaryResults == null)
                {
                    searchItem.dictionaryResults = new Dictionary<int, List<DictionaryItems>>();
                }

                foreach (DictionaryItems dictionaryItem in searchItem.dictionaryItems)
                {
                    DictionaryItems dictionaryItemtoAdd = new DictionaryItems();

                    dictionaryItemtoAdd.key = dictionaryItem.key;
                    dictionaryItemtoAdd.index = index;

                    if ((!string.IsNullOrEmpty(dictionaryItem.textStartIndexOf)) && (!string.IsNullOrEmpty(dictionaryItem.textEndIndexOf)))
                    {
                        int startIndex = text.ToLower().IndexOf(dictionaryItem.textStartIndexOf.ToLower()) + dictionaryItem.textStartIndexOf.Length;
                        int endIndex = text.ToLower().IndexOf(dictionaryItem.textEndIndexOf.ToLower(), startIndex);
                        if (endIndex == -1)
                        {
                            dictionaryItemtoAdd.itemValue = text.Substring(startIndex);
                        }
                        else
                        {
                            dictionaryItemtoAdd.itemValue = text.Substring(startIndex, endIndex - startIndex);
                        }
                    }
                    dictionaryItems.Add(dictionaryItemtoAdd);
                }
            }
            return dictionaryItems;
        }

        private void ProcessDictionary(List<string> lines, int index, SearchTerm searchItem)
        {
            ProcessDictionary(lines[index], index, searchItem);
        }

        private string CleanUpJSON(string text, SearchTerm searchItem, string indexStartOf, string indexEndOf)
        {
            string formattedJSON = text;
            try
            {
                if (!string.IsNullOrEmpty(text))
                {
                    int indexStart = text.IndexOf(indexStartOf);
                    int indexEnd = text.IndexOf(indexEndOf, indexStart);
                    string json = text.Substring(indexStart, indexEnd - indexStart);

                    formattedJSON = FormatJSON(json);
                    text = text.Replace(json, formattedJSON);
                    return text;
                }
            }
            catch (Exception ex)
            {
                //Eat the error, but share the details
                Console.WriteLine("ERROR: (" + searchItem.name+") JSON Format Error - Could not find valid JSON.  Text reviewed: " + text);
            }

            return formattedJSON;
        }

        public string FormatJSON(string json)
        {
            using JsonDocument document = JsonDocument.Parse(json);
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions() { Indented = true });
            document.WriteTo(writer);
            writer.Flush();
            return Encoding.UTF8.GetString(stream.ToArray());
        }

        private void ContainsAdvancedFind(List<string> lines, int index, SearchTerm searchItem)
        {
            int indexAbove = 0;
            int indexBelow = 0;

            foreach (textSelection filter in searchItem.filter)
            {
                if (!string.IsNullOrEmpty(filter.advancedFind?.find))
                {
                    //FindLine(List<string> lines, List<Filter> find, int startingLine, bool directionForward)
                    List<textSelection> lstTempFilter = new List<textSelection>();
                    textSelection tempFilter = new textSelection();
                    tempFilter.find = filter.advancedFind.find;
                    lstTempFilter.Add(tempFilter);

                    if (filter.advancedFind.direction.ToLower() == "down")
                    {
                        indexBelow = FindLine(lines, lstTempFilter, index, true);
                    }
                    else
                    {
                        indexAbove = FindLine(lines, lstTempFilter, index, false);
                    }

                    searchItem.outputText = string.Empty;
                    for (int i = indexAbove; i <= indexBelow; i++)
                    {
                        searchItem.outputText += "Line:" + i.ToString() + " " + lines[i] + "\r\n";
                    }
                }
            }
        }

        private void ProcessTextSelection(List<string> lines, int index, SearchTerm searchItem)
        {
            int indexAbove = 0;
            int indexBelow = 0;

            foreach (textSelectionDetails filter in searchItem.textSelection)
            {
                if (!string.IsNullOrEmpty(filter.findUp) && !string.IsNullOrEmpty(filter.findDown))
                {
                    //FindLine(List<string> lines, List<Filter> find, int startingLine, bool directionForward)
                    List<textSelection> lstTempFilter = new List<textSelection>();
                    textSelection tempFilter = new textSelection();
                    
                    tempFilter.find = filter.findUp;
                    lstTempFilter.Add(tempFilter);
                    indexAbove = FindLine(lines, lstTempFilter, index, false);

                    lstTempFilter.Clear();
                    tempFilter.find = filter.findDown;
                    lstTempFilter.Add(tempFilter);
                    indexBelow = FindLine(lines, lstTempFilter, index, true);

                    searchItem.outputText = string.Empty;
                    for (int i = indexAbove; i <= indexBelow; i++)
                    {
                        if (i == indexAbove)
                        {
                            searchItem.outputText += "Line:" + i.ToString() + " " + lines[i] + "\r\n";
                        }
                        else
                        {
                            searchItem.outputText += lines[i] + "\r\n";
                        }
                    }
                }
            }
        }

        public Row CreateRow(int index, string text)
        {
            return CreateRow(index, text, string.Empty);
        }
        public Row CreateRow(int index, string text, string json)
        {
            Row row = new Row();
            row.text = text;
            row.index = index;
            row.json = json;
            return row;
        }

        private void CreateOutputText(List<string> lines, int index, SearchTerm searchItem)
        {
            if (searchItem.filter != null)
            {
                searchItem.output.Add(CreateRow(index, "Line:" + (index + 1).ToString() + " (" + searchItem.name + ") - " + lines[index]));
                ContainsAdvancedFind(lines, index, searchItem);
                if (!string.IsNullOrEmpty(searchItem.outputText))
                {
                    searchItem.output.Add(CreateRow(index, searchItem.outputText));
                }
                return;
            }

            if (searchItem.textSelection != null) {
                //searchItem.output.Add(CreateRow(index, "Line:" + (index + 1).ToString() + " (" + searchItem.name + ") - " + lines[index]));
                ProcessTextSelection(lines, index, searchItem);
                if (!string.IsNullOrEmpty(searchItem.outputText))
                {
                    searchItem.output.Add(CreateRow(index, searchItem.outputText));
                    searchItem.outputText = string.Empty;
                }
                return;
            }


            if (!string.IsNullOrEmpty(searchItem.outputText))
            {
                searchItem.output.Add(CreateRow(index, "Line:" + (index + 1).ToString() + " (" + searchItem.name + ") - " + lines[index]));
                return;
            }
            if (string.IsNullOrEmpty(searchItem.outputText))
            {
                if (!string.IsNullOrEmpty(searchItem.name))
                {
                    searchItem.output.Add(CreateRow(index, "Line:" + (index + 1).ToString() + " (" + searchItem.name + ") - " + lines[index]));
                }
                else
                {
                    searchItem.output.Add(CreateRow(index, "Line:" + (index + 1).ToString() + " - " + lines[index]));
                }
                return;
            }
        }

        private void CreateOutputTextSummary(List<string> lines, int index, SearchTerm searchItem)
        {
            //Handle group 
            if (searchItem.filter != null)
            {
                searchItem.outputSummary.Add(CreateRow(index, "Line:" + (index + 1).ToString() + " (" + searchItem.name + ") - " + lines[index]));
                ContainsAdvancedFind(lines, index, searchItem);

                if (!string.IsNullOrEmpty(searchItem.outputText))
                {
                    searchItem.outputSummary.Add(CreateRow(index, searchItem.outputText));
                }
                return;
            }

            //Handle Line
            if (!string.IsNullOrEmpty(searchItem.Summary))
            {
                string output = searchItem.Summary;
                if (searchItem.dictionaryResults != null)
                {
                    foreach (DictionaryItems item in searchItem.dictionaryResults[index])
                    {
                        output = output.Replace("{{dictionary(" + item.key + ")}}", item.itemValue);
                    }
                }

                if (searchItem.Summary.ToUpper().Contains("{{SHOWTEXT}}"))
                {
                    string outputText = string.Empty;
                    foreach(Row item in searchItem.output)
                    {
                        outputText += item.text + "\r\n";
                    }

                    output = output.Replace("{{SHOWTEXT}}", outputText);
                }

                if (searchItem.ProcessJSON != null)
                {
                    output = CleanUpJSON(output, searchItem, searchItem.ProcessJSON.jsonStartIndexOf, searchItem.ProcessJSON.jsonEndIndexOf);
                }

                if (searchItem.dedupe)
                {
                    if (!SummaryDedupe.Contains(output))
                    {
                        searchItem.outputSummary.Add(CreateRow(index, "Line:" + (index + 1).ToString() + " (" + searchItem.name + ") - " + output, output));
                        SummaryDedupe.Add(output);
                    }
                }
                else
                {
                    searchItem.outputSummary.Add(CreateRow(index, "Line:" + (index + 1).ToString() + " (" + searchItem.name + ") - " + output, output));
                }
            }
        }

        private void CreateOutputTextSummaryNextSteps(List<string> lines, int index, SearchTerm searchItem)
        {
            //Handle Line

            if (!string.IsNullOrEmpty(searchItem.nextSteps))
            {
                string nextStep = searchItem.nextSteps;

                if (nextStep.Contains("{{dictionary("))
                {
                    foreach (DictionaryItems item in searchItem.dictionaryResults[index])
                    {
                        nextStep = nextStep.Replace("{{dictionary(" + item.key + ")}}", item.itemValue);
                    }
                }

                if (!NextSteps.Contains(nextStep))
                {
                    if (nextStep.Contains("{{TEXT}}"))
                    {
                        nextStep = nextStep.Replace("{{TEXT}}", lines[index]);
                    }

                    searchItem.outputNextSteps.Add(CreateRow(index, "Line:" + (index + 1).ToString() + " (" + searchItem.name + ") - " + nextStep));
                    NextSteps.Add(nextStep);
                }
            }
        }

        private void WriteOutputText(string text, SearchTerm searchItem)
        {
            if (searchItem.display == null || searchItem.display == true)
            {
                if (!string.IsNullOrEmpty(searchItem.outputText))
                {
                    Helpers.FileHelper.WriteFile(outputLog, searchItem.outputText);
                    Helpers.FileHelper.WriteFile(outputLog, text);
                    Helpers.FileHelper.WriteFile(outputLog, " ");
                    return;
                }

                if (string.IsNullOrEmpty(searchItem.outputText))
                {
                    Helpers.FileHelper.WriteFile(outputLog, text);
                    return;
                }
            }
        }

        private void WriteOutputSummary(string text, SearchTerm searchItem)
        {
            if (searchItem.includeInSummary == null || searchItem.includeInSummary == true)
            {
                Helpers.FileHelper.WriteFile(outputLog, text);
            }
        }

        private LogTypeIdentifier IdentifyFile(List<string> lines, Config config)
        {
            //If useconfig parameter has a value, try to find it and use it, if not, then try to find the file by file type
            if (!string.IsNullOrWhiteSpace(useConfig))
            {
                foreach (LogTypeIdentifier logTypeIdentifier in config.JSON.logTypeIdentifier)
                {
                    //This is used to select the section the user might want to foce
                    if (logTypeIdentifier.name.ToLower() == useConfig.ToLower())
                    {
                        return logTypeIdentifier;
                    }
                }
            }
            
            foreach (LogTypeIdentifier logTypeIdentifier in config.JSON.logTypeIdentifier)
            {
                if (IdentifiersExist(lines, logTypeIdentifier.identifier))
                {
                    if (useConfig.Length > 0)
                    {
                        Console.WriteLine("Useconfig parameter was used, however, none of the configuration names matched \""+useConfig+"\". \"" + logTypeIdentifier.name+"\" was auto identified as the file configuration to use. ");
                    }

                    return logTypeIdentifier;
                }
            }

            Console.WriteLine("\r\nUnable to determine what configuration to use.\r\nConsider specifying the configuration to use by using this configuration parameter - for example:\r\n   useconfig=\"UnifiedAgent\"\r\n\r\nThis is a list of configurations that are in the configuration file:");
            foreach (LogTypeIdentifier logTypeIdentifier in config.JSON.logTypeIdentifier)
            {
                Console.WriteLine(logTypeIdentifier.name);
            }

            return null;
        }

        private List<int> FindAllIndexes(List<string> lines, SearchTerm searchItem)
        {
            List<int> indexes = new List<int>();

            int lastIndex = 0;
            while (lastIndex > -1)
            {
                int indexFound = -1;

                if (searchItem.filters != null)
                {
                    indexFound = FindLine(lines, searchItem.filters, lastIndex, true);
                }

                if (indexFound >= 0)
                {
                    indexes.Add(indexFound);

                    if (searchItem.dictionaryItems != null)
                    {
                        ProcessDictionary(lines, indexFound, searchItem);
                    }
                }
                else
                {
                    break;
                }
                lastIndex = indexFound + 1;
            }

            return indexes;
        }

        private bool IdentifiersExist(List<string> lines, List<Identifier> find)
        {
            int found = 0;
            foreach (Identifier findItem in find)
            {
                string item = findItem.find.ToUpper();
                foreach (string line in lines)
                {
                    if (line.ToUpper().Contains(item))
                    {
                        found++;
                        break;
                    }
                }
            }

            if (found == find.Count)
            {
                return true;
            }
            return false;
        }

        private int FindLine(List<string> lines, List<textSelection> find, int startingLine, bool directionForward)
        {
            if (!string.IsNullOrEmpty(find[0].RelatesToName))
            {
                return -1;
            }

            if (directionForward)
            {
                for (int lineIndex = startingLine; lineIndex < lines.Count; lineIndex++)
                {
                    if (!string.IsNullOrEmpty(find[0].find))
                    {
                        if (lines[lineIndex].ToUpper().Contains(find[0].find.ToUpper()))
                        {
                            bool found = true;
                            for (int i = 1; i < find.Count; i++)
                            {
                                if (!string.IsNullOrEmpty(find[i].find))
                                {
                                    if (!lines[lineIndex].ToUpper().Contains(find[i].find.ToUpper()))
                                    {
                                        found = false; break;
                                    }
                                }
                                if (!string.IsNullOrEmpty(find[i].exclude))
                                {
                                    if (lines[lineIndex].ToUpper().Contains(find[i].exclude.ToUpper()))
                                    {
                                        found = false; break;
                                    }
                                }

                            }

                            if (found)
                            {
                                return lineIndex;
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(find[0].regex))
                    {
                        Regex rg = new Regex(find[0].regex, RegexOptions.IgnoreCase);
                        MatchCollection matchCollection = rg.Matches(lines[lineIndex].ToUpper());
                        if (rg.IsMatch(lines[lineIndex]))
                        {
                            return lineIndex;
                        }
                    }

                }
            }
            else
            {
                for (int lineIndex = startingLine; lineIndex >= 0; lineIndex--)
                {
                    if (lines[lineIndex].ToUpper().Contains(find[0].find.ToUpper()))
                    {
                        bool found = true;
                        for (int i = 1; i < find.Count; i++)
                        {
                            if (!lines[lineIndex].ToUpper().Contains(find[i].find.ToUpper()))
                            {
                                found = false; break;
                            }
                        }
                        if (found)
                        {
                            return lineIndex;
                        }
                    }
                }
            }

            return -1;
        }

        private List<string> LoadFile(string filename)
        {
            if (File.Exists(filename))
            {
                Console.WriteLine("LoadFile: " + filename);
                var data = File.ReadAllLines(filename);
                return new List<string>(data);
            }
            else
            {
                Console.WriteLine("[ERROR] - File not found: " + filename);
                Environment.Exit(0);    
            }
            return new List<string>();  
        }
    }
}
