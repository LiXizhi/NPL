using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;

namespace ParaEngine.NPLLanguageService
{
    static class CommandDoc
    {
        public static void ParseText(string path)
        {
            StreamReader reader = File.OpenText(path);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                Regex reg = new Regex("^--.*");    // need more robust regular expression
                Match m = reg.Match(line);
                if (!m.Success && line != "")
                {
                    ParseSource(line);
                }
            }
        }

        public static void ParseSource(string path)
        {
            StreamReader reader = File.OpenText(path);
            XmlDocument doc = new XmlDocument();
            XmlElement tables = doc.CreateElement("tables");
            doc.AppendChild(tables);

            Dictionary<string, XmlElement> tableList = new Dictionary<string, XmlElement>();
            XmlElement globals = doc.CreateElement("globals");
            string line;
            int linenum = 1;
            string comments = "";
            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith("--"))
                    comments += line.Substring(2) + "\n";
                else if (line.StartsWith("function "))
                {
                    Function function = new Function();
                    function.line = linenum;
                    // Parse the function
                    ParseFunction(function, line, comments);
                    // Save to the Xml Node
                    XmlElement functionNode = doc.CreateElement("function");
                    functionNode.SetAttribute("line", linenum.ToString());
                    functionNode.SetAttribute("name", function.name);
                    if (function.table != "" && !tableList.ContainsKey(function.table))
                    {
                        XmlElement currentTable = doc.CreateElement("table");
                        currentTable.SetAttribute("name", function.table);
                        currentTable.SetAttribute("src", path);
                        currentTable.AppendChild(functionNode);
                        tableList.Add(function.table, currentTable);
                        //currentTableName = function.table;
                    }
                    else if (function.table != "" && tableList.ContainsKey(function.table))
                    {
                        XmlElement currentTable;
                        tableList.TryGetValue(function.table, out currentTable);
                        currentTable.AppendChild(functionNode);
                        tableList[function.table] = currentTable;
                    }
                    else
                    {
                        globals.AppendChild(functionNode);
                    }

                    XmlElement summary = doc.CreateElement("summary");
                    XmlText summaryText = doc.CreateTextNode(function.summary);
                    summary.AppendChild(summaryText);
                    functionNode.AppendChild(summary);

                    foreach (KeyValuePair<string, string> entry in function.param)
                    {
                        XmlElement parameter = doc.CreateElement("parameter");
                        parameter.SetAttribute("name", entry.Key);
                        if (entry.Value != "")
                        {
                            XmlText description = doc.CreateTextNode(entry.Value);
                            parameter.AppendChild(description);
                        }
                        functionNode.AppendChild(parameter);
                    }

                    if (function.ret != null && function.ret != "")
                    {
                        XmlElement ret = doc.CreateElement("return");
                        XmlText retDesc = doc.CreateTextNode(function.ret);
                        ret.AppendChild(retDesc);
                        functionNode.AppendChild(ret);
                    }
                    //functions.Add(function);
                }
                else
                    comments = "";

                linenum++;
            }
            foreach (KeyValuePair<string, XmlElement> entry in tableList)
            {
                tables.AppendChild(entry.Value);
            }
            tables.AppendChild(globals);
            doc.Save("Test.xml");
        }

        public static void ParseFunction(Function function, string line, string comments)
        {
            string pattern = @".*function\s+([\w\.:]+)\s*\((.*)\)";    // need more robust regular expression
            string funcName = "";
            string tableName = "";
            string signature = "";
            Match m = Regex.Match(line, pattern);
            if (m.Success)
            {
                signature = m.Groups[0].Captures[0].Value;
                string names = m.Groups[1].Captures[0].Value;

                if (names.Contains(':'))
                {
                    tableName = names.Substring(0, names.IndexOf(':'));
                    funcName = names.Substring(names.IndexOf(':') + 1);
                }
                else if (names.Contains('.'))
                {
                    tableName = names.Substring(0, names.IndexOf('.'));
                    funcName = names.Substring(names.LastIndexOf('.') + 1);
                }
                else
                {
                    funcName = names;
                }

                string parameter = m.Groups[2].Captures[0].Value;
                string[] paramNames = new string[] { };
                if (parameter != "")
                {
                    paramNames = parameter.Split(',');
                }
                for (int i = 0; i < paramNames.Length; ++i)
                {
                    paramNames[i].Replace(" ", "");
                    paramNames[i].Replace("\t", "");
                    function.param.Add(paramNames[i], "");
                }
            }
            function.name = funcName;
            function.table = tableName;
            function.summary = signature + "\r\n" + comments;
            ParseComments(function, comments);
        }

        public static void ParseComments(Function function, string comments)
        {
            string ParamPattern = @".*@param\s+(\w+)\s*:(.*)";
            //string ReturnPattern = @"@return";
            string[] commentLines = comments.Split('\n');
            foreach (string comment in commentLines)
            {
                Match match = Regex.Match(comment, ParamPattern);
                if (match.Success)
                {
                    string paramName = match.Groups[1].Captures[0].Value;
                    string paramDesc = match.Groups[2].Captures[0].Value;
                    if (function.param.ContainsKey(paramName))
                        function.param[paramName] = paramDesc;
                    else
                        function.param.Add(paramName, paramDesc);
                }

                if (comment.Contains("@return"))
                {
                    function.ret = comment;
                }
            }
        }
    }

    internal class Function
    {
        public int line;
        public string name;
        public string summary;
        public string ret;
        public string table;
        public Dictionary<string, string> param;

        public Function()
        {
            param = new Dictionary<string, string>();
        }
    }

    internal struct Parameter
    {
        public string name;
        public string description;
    }
}
