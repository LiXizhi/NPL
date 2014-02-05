using System;
using System.Collections.Generic;
using EnvDTE;
using ParaEngine.Tools.Lua.CodeDom.Definitions;
using ParaEngine.Tools.Lua.CodeDom.Elements;

namespace ParaEngine.Tools.Lua.CodeDom
{
    /// <summary>
    /// Merger class for performing merges into external Lua file.
    /// </summary>
    public class LuaExternalFileCodeMerger : IFileCodeMerger
    {
        private readonly string fileName;
        private readonly LuaCodeFile luaCodeFile;

        /// <summary>
        /// Initializes a new instance of the <see cref="LuaExternalFileCodeMerger"/> class.
        /// </summary>
        /// <param name="parent">Parent ProjectItem of LuaFileCodeModel.</param>
        public LuaExternalFileCodeMerger(ProjectItem parent)
        {
            fileName = parent.get_FileNames(1);
            luaCodeFile = new LuaCodeFile(fileName);
            luaCodeFile.Load();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LuaExternalFileCodeMerger"/> class.
        /// </summary>
        /// <param name="luaCodeFile">Instance of LuaCodeFile which represents a lua code file on disk.</param>
        public LuaExternalFileCodeMerger(LuaCodeFile luaCodeFile)
        {
            if (luaCodeFile == null)
                throw new ArgumentNullException("luaCodeFile");

            this.luaCodeFile = luaCodeFile;
        }

        #region IFileCodeMerger Members

        /// <summary>
        /// Gets count of lines in code file.
        /// </summary>
        public int LineCount
        {
            get { return luaCodeFile.Count; }
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

            luaCodeFile.InsertRange(start, lines);
        }

        /// <summary>
        /// Removes the specified range of text.
        /// </summary>
        /// <param name="start">Index of removal.</param>
        /// <param name="count">Count of chars to remove.</param>
        public void RemoveRange(int start, int count)
        {
            luaCodeFile.RemoveRange(start, count);
        }

        /// <summary>
        /// Finds a pattern in the specified range of text and replaces it with the specified text.
        /// </summary>
        /// <param name="startLine">The start line of the specified range of text.</param>
        /// <param name="startRow">The start row of the specified range of text.</param>
        /// <param name="endLine">The end line of the specified range of text.</param>
        /// <param name="endRow">The end row of the specified range of text.</param>
        /// <param name="oldText">The text to replace.</param>
        /// <param name="newText">The replacement text for pattern.</param>
        public bool Replace(int startLine, int startRow, int endLine, int endRow, string oldText, string newText)
        {
            return luaCodeFile.Replace(startLine, startRow, endLine, endRow, oldText, newText);
        }

        /// <summary>
        /// Replaces the selected text with the given text.
        /// </summary>
        /// <param name="point">The start point of the specified range of text.</param>
        /// <param name="oldText">The text to replace.</param>
        /// <param name="newText">The replacement text for pattern.</param>
        public bool Replace(TextSelection point, string oldText, string newText)
        {
            if (point == null)
                throw new ArgumentNullException("point");

            return luaCodeFile.Replace(point.AnchorPoint.Line, point.AnchorPoint.DisplayColumn,
                                       point.ActivePoint.Line,
                                       point.ActivePoint.DisplayColumn,
                                       point.Text, newText);
        }

        /// <summary>
        /// Finds a function pattern in the specified range of text and replaces it with the specified text.
        /// </summary>
        /// <param name="startLine">The start line of the specified range of text.</param>
        /// <param name="startRow">The start row of the specified range of text.</param>
        /// <param name="endLine">The end line of the specified range of text.</param>
        /// <param name="endRow">The end row of the specified range of text.</param>
        /// <param name="oldName">The text to replace.</param>
        /// <param name="newName">The replacement text for pattern.</param>
        public bool RenameFunction(int startLine, int startRow, int endLine, int endRow, string oldName, string newName)
        {
            string functionLineText = luaCodeFile[startLine - 1].CodeLine;
            int startIndex = functionLineText.ToLower()
                .IndexOf("function", StringComparison.CurrentCultureIgnoreCase);
            if (startIndex > -1)
            {
                startRow = functionLineText.IndexOf(oldName);
                endLine = startLine;
                endRow = startRow + oldName.Length + 2;
            }
            return Replace(startLine, startRow, endLine, endRow, oldName, newName);
        }

        /// <summary>
        /// Replaces the selected text with the given text.
        /// </summary>
        /// <param name="startLine">The start line of the specified range of text.</param>
        /// <param name="startRow">The start row of the specified range of text.</param>
        /// <param name="endLine">The end line of the specified range of text.</param>
        /// <param name="endRow">The end row of the specified range of text.</param>
        /// <param name="newText">The replacement text for pattern.</param>
        public void SetText(int startLine, int startRow, int endLine, int endRow, string newText)
        {
            luaCodeFile.Replace(startLine, startRow, endLine, endRow, string.Empty, newText);
        }

        /// <summary>
        /// Commits changes made by merger.
        /// </summary>
        public void Commit()
        {
            luaCodeFile.Commit();
        }

        #endregion
    }
}