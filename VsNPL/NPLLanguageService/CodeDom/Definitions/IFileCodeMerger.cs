using System.Collections.Generic;
using EnvDTE;

namespace ParaEngine.Tools.Lua.CodeDom.Definitions
{
    /// <summary>
    /// Interface for FileCodeMerger implementations.
    /// </summary>
    public interface IFileCodeMerger
    {
        /// <summary>
        /// Count of line in the code file.
        /// </summary>
        int LineCount { get; }

        /// <summary>
        /// Inserts the element of a collection into the buffer
        /// at the specified index.
        /// </summary>
        /// <param name="start">Index of inserting.</param>
        /// <param name="lines">Code lines.</param>
        void InsertRange(int start, IList<string> lines);

        /// <summary>
        /// Removes the specified range of text.
        /// </summary>
        /// <param name="start">Index of removal.</param>
        /// <param name="count">Count of chars to remove.</param>
        void RemoveRange(int start, int count);

        /// <summary>
        /// Finds a pattern in the specified range of text and replaces it with the specified text.
        /// </summary>
        /// <param name="startLine">The start line of the specified range of text.</param>
        /// <param name="startRow">The start row of the specified range of text.</param>
        /// <param name="endLine">The end line of the specified range of text.</param>
        /// <param name="endRow">The end row of the specified range of text.</param>
        /// <param name="oldText">The text to replace.</param>
        /// <param name="newText">The replacement text for pattern.</param>
        bool Replace(int startLine, int startRow, int endLine, int endRow, string oldText, string newText);

        /// <summary>
        /// Replaces the selected text with the given text.
        /// </summary>
        /// <param name="point">The start point of the specified range of text.</param>
        /// <param name="oldText">The text to replace.</param>
        /// <param name="newText">The replacement text for pattern.</param>
        bool Replace(TextSelection point, string oldText, string newText);

        /// <summary>
        /// Finds a function pattern in the specified range of text and replaces it with the specified text.
        /// </summary>
        /// <param name="startLine">The start line of the specified range of text.</param>
        /// <param name="startRow">The start row of the specified range of text.</param>
        /// <param name="endLine">The end line of the specified range of text.</param>
        /// <param name="endRow">The end row of the specified range of text.</param>
        /// <param name="oldName">The text to replace.</param>
        /// <param name="newName">The replacement text for pattern.</param>
        bool RenameFunction(int startLine, int startRow, int endLine, int endRow, string oldName, string newName);

        /// <summary>
        /// Replaces the selected text with the given text.
        /// </summary>
        /// <param name="startLine">The start line of the specified range of text.</param>
        /// <param name="startRow">The start row of the specified range of text.</param>
        /// <param name="endLine">The end line of the specified range of text.</param>
        /// <param name="endRow">The end row of the specified range of text.</param>
        /// <param name="newText">The replacement text for pattern.</param>
        void SetText(int startLine, int startRow, int endLine, int endRow, string newText);

        /// <summary>
        /// Commits changes made by merger.
        /// </summary>
        void Commit();
    }
}