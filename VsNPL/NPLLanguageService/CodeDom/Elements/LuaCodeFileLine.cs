using System;

namespace ParaEngine.Tools.Lua.CodeDom.Elements
{
    /// <summary>
    /// Represents a line in LuaCodeFile.
    /// </summary>
    public class LuaCodeFileLine
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LuaCodeFileLine"/> class.
        /// </summary>
        /// <param name="codeLine">Line of code.</param>
        public LuaCodeFileLine(string codeLine)
        {
            CodeLine = codeLine;
        }

        /// <summary>
        /// Gets code line text.
        /// </summary>
        public string CodeLine { get; set; }

        /// <summary>
        /// Merge and replace new Text in an existing range.
        /// </summary>
        /// <param name="startRow"></param>
        /// <param name="endRow"></param>
        /// <param name="newText"></param>
        /// <returns></returns>
        public string MergeAndReplaceLine(int startRow, int endRow, string newText)
        {
            if (String.IsNullOrEmpty((CodeLine)))
            {
                CodeLine = newText;
            }
            else
            {
                int count = (CodeLine.Length - endRow == 1) ? 1 : (CodeLine.Length - endRow);
                CodeLine = String.Concat(
                    CodeLine.Substring(0, startRow),
                    newText,
                    CodeLine.Substring(endRow, count));
            }
            return CodeLine;
        }
    }
}