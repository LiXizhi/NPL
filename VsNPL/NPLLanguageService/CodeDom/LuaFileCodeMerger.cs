using System;
using System.Collections.Generic;
using EnvDTE;
using ParaEngine.Tools.Lua.CodeDom;
using ParaEngine.Tools.Lua.CodeDom.Definitions;
using ParaEngine.Tools.Lua.CodeDom.Elements;

namespace ParaEngine.Tools.Lua.CodeDom
{
    /// <summary>
    /// Merger class for performing merges into VS.
    /// </summary>
    public class LuaFileCodeMerger : IFileCodeMerger
    {
        private TextDocument document;
        private readonly ProjectItem parentItem;

        /// <summary>
        /// Initializes a new instance of the <see cref="LuaFileCodeMerger"/> class.
        /// </summary>
        /// <param name="parent">Parent ProjectItem of LuaFileCodeModel.</param>
        public LuaFileCodeMerger(ProjectItem parent)
        {
            parentItem = parent;
        }

        /// <summary>
        /// Gets count of lines in code document.
        /// </summary>
        public int LineCount
        {
            get
            {
                return Document.EndPoint.Line - Document.StartPoint.Line;
            }
        }

        /// <summary>
        /// Gets TextDocument of Current ProjectItem.
        /// </summary>
        private TextDocument Document
        {
            get
            {
                if (null == document)
                {
                    if (null == parentItem.Document)
                    {
                        parentItem.Open(Guid.Empty.ToString("B"));
                    }
                    document = (TextDocument)parentItem.Document.Object("TextDocument");
                }
                return document;
            }
        }

        #region IFileCodeMerger Members

        /// <summary>
        /// Inserts the element of a collection into the buffer
        /// at the specified index.
        /// </summary>
        /// <param name="start">Index of inserting.</param>
        /// <param name="lines">Code lines.</param>
        public void InsertRange(int start, IList<string> lines)
        {
            EditPoint ep = Document.CreateEditPoint(new LuaTextPoint(Document, 1, start + 1));
            for (int i = 0; i < lines.Count; i++)
            {
                ep.Insert(lines[i] + "\r\n");
            }
        }

        /// <summary>
        /// Removes the specified range of text.
        /// </summary>
        /// <param name="start">Index of removal.</param>
        /// <param name="count">Count of chars to remove.</param>
        public void RemoveRange(int start, int count)
        {
            EditPoint ep = Document.CreateEditPoint(new LuaTextPoint(Document, 1, start + 1));
            for (int i = 0; i < count; i++)
            {
                ep.Delete(ep.LineLength + 1);
            }
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
            EditPoint ep = Document.CreateEditPoint(new LuaTextPoint(Document, startRow, startLine));
            TextRanges tr = null;
            TextPoint endTextPoint = new LuaTextPoint(Document, endRow, endLine);
            //Trace.WriteLine(string.Format("Start: {0} - End: {1}", new LuaTextPoint(Document, startRow, startLine),endTextPoint));
            return ep.ReplacePattern(endTextPoint, oldText, newText,
                              (int)vsFindOptions.vsFindOptionsMatchWholeWord, ref tr);
        }


        /// <summary>
        /// Replaces the selected text with the given text.
        /// </summary>
        /// <param name="point">The start point of the specified range of text.</param>
        /// <param name="oldText">The text to replace.</param>
        /// <param name="newText">The replacement text for pattern.</param>
        public bool Replace(TextSelection point, string oldText, string newText)
        {
            return Replace(point.AnchorPoint.Line, point.AnchorPoint.LineCharOffset,
                           point.BottomPoint.Line, point.BottomPoint.LineCharOffset, oldText, newText);
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
            var textPoint = new LuaTextPoint(Document, startRow, startLine);
            EditPoint ep = Document.CreateEditPoint(textPoint);
            ep.ReplaceText(textPoint,newText,(int)vsEPReplaceTextOptions.vsEPReplaceTextAutoformat);
        }

        /// <summary>
        /// Commits changes made by merger.
        /// </summary>
        public void Commit()
        {
            //Do nothing because TextDocument already contains modifications.
            return;
        }

        #endregion

        #region Lua Specific functions

        /// <summary>
        /// Finds a pattern in the specified range of text and replaces it with the specified text.
        /// </summary>
        /// <param name="startPoint">The start point of the specified range of text.</param>
        /// <param name="endPoint">The end point of the specified range of text.</param>
        /// <param name="oldText">The text to replace.</param>
        /// <param name="newText">The replacement text for pattern.</param>
        public bool Replace(TextPoint startPoint, TextPoint endPoint, string oldText, string newText)
        {
            EditPoint ep = Document.CreateEditPoint(startPoint);
            TextRanges tr = null;
            return ep.ReplacePattern(endPoint, oldText, newText,
                              (int)vsFindOptions.vsFindOptionsMatchWholeWord, ref tr);

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
            TextPoint textPoint = new LuaTextPoint(Document, startRow, startLine);
            TextPoint endTextPoint = new LuaTextPoint(Document, endRow, startLine);
            var functionLineText = LuaCodeDomHelper.GetCodeLineText(Document, startLine);
            if (!string.IsNullOrEmpty(functionLineText))
            {

                int startIndex = functionLineText.ToLower().IndexOf("function", StringComparison.CurrentCultureIgnoreCase);
                if (startIndex > -1)
                {
                    int nameIndex = functionLineText.IndexOf(oldName);
                    textPoint = new LuaTextPoint(Document, nameIndex, startLine);
                    endTextPoint = new LuaTextPoint(Document, nameIndex + oldName.Length + 2, startLine);
                }
            }
            return Replace(textPoint, endTextPoint, oldName, newName);
            //ep.ReplaceText(textPoint, newName, (int)vsEPReplaceTextOptions.vsEPReplaceTextAutoformat);
        }

        #endregion

    }
}