using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ParaEngine.Tools.Lua.CodeDom.Elements
{
    /// <summary>
    /// This class Represents a Lua code document.
    /// </summary>
    public class LuaCodeFile : List<LuaCodeFileLine>
    {
        private string fileName;

        /// <summary>
        /// Initializes a new instance of the <see cref="LuaCodeFile"/> class.
        /// </summary>
        /// <param name="sourceFileName">Source file path.</param>
        public LuaCodeFile(string sourceFileName)
        {
            fileName = sourceFileName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LuaCodeFile"/> class.
        /// </summary>
        /// <param name="capacity">Capacity of lines buffer.</param>
        /// <param name="sourceFileName">Source file path.</param>
        public LuaCodeFile(int capacity, string sourceFileName)
            : base(capacity)
        {
            fileName = sourceFileName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LuaCodeFile"/> class.
        /// </summary>
        /// <param name="collection">Collection of LuaCodeFileLine.</param>
        /// <param name="sourceFileName">Source file path.</param>
        public LuaCodeFile(IEnumerable<LuaCodeFileLine> collection, string sourceFileName)
            : base(collection)
        {
            fileName = sourceFileName;
        }

        /// <summary>
        /// Load Lua Source file into context.
        /// </summary>
        public void Load()
        {
            Load(fileName);
        }

        /// <summary>
        /// Load Lua Source file into context.
        /// </summary>
        /// <param name="sourceFileName">Source file path.</param>
        public void Load(string sourceFileName)
        {
            if (String.IsNullOrEmpty(sourceFileName))
                throw new ArgumentNullException("sourceFileName");

            fileName = sourceFileName;
            CheckFile(fileName);

            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                Load(stream);
                stream.Close();
            }
        }

        /// <summary>
        ///Load Lua Source file into context.
        /// </summary>
        /// <param name="stream"></param>
        public void Load(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            using (var reader = new StreamReader(stream))
            {
                while (reader.Peek() > -1)
                {
                    Add(new LuaCodeFileLine(reader.ReadLine()));
                }
                reader.Close();
            }
        }

        /// <summary>
        /// Gets source code text.
        /// </summary>
        public string SourceCode
        {
            get
            {
                var builder = new StringBuilder();
                ForEach(item => builder.AppendLine(item.CodeLine));
                return builder.ToString();
            }
        }

        /// <summary>
        /// Inserts the element of a collection into the LuaCodeFile
        /// at the specified index.
        /// </summary>
        /// <param name="start">Index of inserting.</param>
        /// <param name="lines">Code lines.</param>
        public void InsertRange(int start, IList<string> lines)
        {
            if (lines == null)
                throw new ArgumentNullException("lines");

            var codeFileLines = new List<LuaCodeFileLine>();
            lines.ForEach(line => codeFileLines.Add(new LuaCodeFileLine(line)));
            InsertRange(--start, codeFileLines);
        }

        /// <summary>
        /// Finds a pattern in the specified range of LuaCode file and replaces it with the specified text.
        /// </summary>
        /// <param name="startLine">The start line of the specified range of text.</param>
        /// <param name="startRow">The start row of the specified range of text.</param>
        /// <param name="endLine">The end line of the specified range of text.</param>
        /// <param name="endRow">The end row of the specified range of text.</param>
        /// <param name="oldText">The text to replace.</param>
        /// <param name="newText">The replacement text for pattern.</param>
        public bool Replace(int startLine, int startRow, int endLine, int endRow, string oldText, string newText)
        {
            Debug.WriteLine(String.Format("LuaCodeFile:Replace([{0},{1}],[{2},{3}]) {4}->{5}",
                                          startLine, startRow, endLine, endRow, oldText, newText));
            bool result = true;
            //decraease points because points from TextDocument are 1-based.
            startLine--;
            endLine--;
            startRow--;
            endRow--;
            CheckParameters(startLine, startRow, endLine, endRow);
            for (int index = startLine; index <= endLine; index++)
            {
                string codeLine = this[startLine].CodeLine;
                string tempCodeLine = codeLine.Substring(startRow, endRow - startRow);
                if (string.IsNullOrEmpty(oldText) || tempCodeLine.Contains(oldText))
                {
                    tempCodeLine = string.IsNullOrEmpty(oldText) ? newText : tempCodeLine.Replace(oldText, newText);
					Debug.WriteLine(String.Format("LuaCodeFile:MergeAndReplaceLine([{0}],[{1}]) {2}->{3}",
                                                  startRow, endRow, oldText, newText));
                    this[startLine].MergeAndReplaceLine(startRow, endRow, tempCodeLine);
                    result = result && true;
                }
                else
                {
                    result = false;
                }
            }
            return result;
        }

        /// <summary>
        /// Checks value of parameters.
        /// </summary>
        /// <param name="startLine">The start line of the specified range of text.</param>
        /// <param name="startRow">The start row of the specified range of text.</param>
        /// <param name="endLine">The end line of the specified range of text.</param>
        /// <param name="endRow">The end row of the specified range of text.</param>
        private static void CheckParameters(int startLine, int startRow, int endLine, int endRow)
        {
            CheckRange(startLine, "startLine");
            CheckRange(startRow, "startRow");
            CheckRange(endLine, "endLine");
            CheckRange(endRow, "endRow");
        }

        /// <summary>
        /// Check the specified file.
        /// </summary>
        /// <param name="sourceFileName">File name.</param>
        private static void CheckFile(string sourceFileName)
        {
            if (!File.Exists(sourceFileName))
            {
                throw new FileNotFoundException("Source file does not exist.", sourceFileName);
            }
        }

        /// <summary>
        /// Check the specified value.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="parameterName"></param>
        private static void CheckRange(int position, string parameterName)
        {
            if (position < 0)
                throw new ArgumentOutOfRangeException(parameterName);
        }


        /// <summary>
        /// Save changes into code file.
        /// </summary>
        public void Commit()
        {
            Commit(fileName);
        }

        /// <summary>
        ///Load Lua Source file into context.
        /// </summary>
        /// <param name="stream"></param>
        public void Commit(Stream stream)
        {
            using (var writer = new StreamWriter(stream))
            {
                ForEach(line => writer.WriteLine(line.CodeLine));
                writer.Flush();
                writer.Close();
            }
        }

        /// <summary>
        /// Save changes into code file.
        /// </summary>
        /// <param name="sourceFileName">Source file path.</param>
        public void Commit(string sourceFileName)
        {
            Commit(sourceFileName, false);
        }

        /// <summary>
        /// Save changes into code file.
        /// </summary>
        /// <param name="sourceFileName">Source file path.</param>
        /// <param name="appendLines">Append or create lines.</param>
        internal void Commit(string sourceFileName, bool appendLines)
        {
            fileName = sourceFileName;
            CheckFile(fileName);
            using (var stream = new FileStream(fileName, appendLines ? FileMode.Append : FileMode.Open,
                                                      FileAccess.Write, FileShare.Read))
            {
                Commit(stream);
                stream.Close();
            }
        }


    }
}