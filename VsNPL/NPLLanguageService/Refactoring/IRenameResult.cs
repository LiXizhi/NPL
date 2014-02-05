using System.Collections;
using System.Collections.Generic;
using EnvDTE;

namespace ParaEngine.Tools.Lua.Refactoring
{
    /// <summary>
    /// Holds result from rename operations.
    /// </summary>
    public interface IRenameResult
    {
		/// <summary>
		/// Gets or sets the parent.
		/// </summary>
		/// <value>The parent.</value>
		IList<FileCodeModel> Parents { get; set; }

        /// <summary>
        /// Old name of element.
        /// </summary>
        string OldName { get; set; }

        /// <summary>
        /// New name of element.
        /// </summary>
        string NewName { get; set; }

        /// <summary>
        /// Indicates that operation successfully completed.
        /// </summary>
        bool Success { get; set; }

        /// <summary>
        /// If true all references of symbol should be renamed.
        /// </summary>
        bool RenameReferences { get; set; }

        /// <summary>
        /// Gets information about rename operation.
        /// </summary>
        string Message { get; set; }

        /// <summary>
        /// Indicates that IRenameResult hold changed elements.
        /// </summary>
        bool HasChanges { get; }

        /// <summary>
        /// Gets changed CodeElements.
        /// </summary>
        IEnumerable<CodeElement> ChangedElements { get; set; }

		/// <summary>
		/// Merges changes from other RenameResult.
		/// </summary>
		/// <param name="success">if set to <c>true</c> [success].</param>
		/// <param name="changedElements">The changed elements.</param>
        void MergeChanges(bool success, IEnumerable<CodeElement> changedElements);
    }
}