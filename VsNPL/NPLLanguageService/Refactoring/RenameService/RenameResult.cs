using System.Collections;
using System.Collections.Generic;
using EnvDTE;
using ParaEngine.Tools.Lua.Refactoring;

namespace ParaEngine.Tools.Lua.Refactoring.RenameService
{
    /// <summary>
    /// Holds result from rename operation.
    /// </summary>
    public class RenameResult : IRenameResult, IEnumerable<CodeElement>
    {
        private List<CodeElement> changedElements;
		private IList<FileCodeModel> parents = new List<FileCodeModel>();

        /// <summary>
        /// Indexer for RenameResult class.
        /// </summary>
        /// <param name="index">Index of CodeElement.</param>
        /// <returns></returns>
        public CodeElement this[int index]
        {
            get
            {
                if (index <= changedElements.Count)
                    return changedElements[index];
                return null;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RenameResult"/> class.
        /// </summary>
        public RenameResult()
            : this(false, string.Empty, null, string.Empty, string.Empty)
        {
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="RenameResult"/> class.
		/// </summary>
		/// <param name="oldName">The old name.</param>
		/// <param name="newName">The new name.</param>
        public RenameResult(string oldName, string newName)
            : this(false, string.Empty, null, oldName, newName)
        {
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="RenameResult"/> class.
		/// </summary>
		/// <param name="success">if set to <c>true</c> [success].</param>
        public RenameResult(bool success)
            : this(success, string.Empty, null, string.Empty, string.Empty)
        {
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="RenameResult"/> class.
		/// </summary>
		/// <param name="success">if set to <c>true</c> [success].</param>
		/// <param name="message">The message.</param>
        public RenameResult(bool success, string message)
            : this(success, message, null, string.Empty, string.Empty)
        {
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="RenameResult"/> class.
		/// </summary>
		/// <param name="success">if set to <c>true</c> [success].</param>
		/// <param name="message">The message.</param>
		/// <param name="changedElements">The changed elements.</param>
		/// <param name="oldName">The old name.</param>
		/// <param name="newName">The new name.</param>
        public RenameResult(bool success, string message, IEnumerable<CodeElement> changedElements, string oldName,
                            string newName)
        {
            Success = success;
            Message = message;
            OldName = oldName;
            NewName = newName;
			if (changedElements != null)
				ChangedElements = changedElements;
			else
				this.changedElements = new List<CodeElement>();
        }

        #region IRenameResult Members

		/// <summary>
		/// Indicates that operation successfully completed.
		/// </summary>
		/// <value></value>
        public bool Success { get; set; }

		/// <summary>
		/// Gets message about rename operation.
		/// </summary>
		/// <value></value>
        public string Message { get; set; }

		/// <summary>
		/// Indicates that RenameResult hold changed elements.
		/// </summary>
		/// <value></value>
        public bool HasChanges
        {
            get { return changedElements.Count > 0; }
        }

		/// <summary>
		/// If true all references of symbol should be renamed.
		/// </summary>
		/// <value></value>
        public bool RenameReferences { get; set; }

		/// <summary>
		/// Gets changed CodeElements.
		/// </summary>
		/// <value></value>
        public IEnumerable<CodeElement> ChangedElements
        {
            get { return changedElements; }
            set { changedElements = new List<CodeElement>(value); }
        }

        /// <summary>
        /// Merges changes from other RenameResult;
        /// </summary>
        /// <param name="success">Rename operation status.</param>
        /// <param name="changedElementList">Changed CodeElements in rename operations.</param>
        public void MergeChanges(bool success, IEnumerable<CodeElement> changedElementList)
        {
            Success = success;
            if (changedElementList != null)
            {
                changedElements.AddRange(changedElementList);
            }
        }

    	/// <summary>
		/// Gets or sets the parent.
		/// </summary>
		/// <value>The parent.</value>
		public IList<FileCodeModel> Parents
    	{
    		get { return parents; }
    		set { parents = value; }
    	}

		/// <summary>
		/// Old name of element.
		/// </summary>
		/// <value></value>
        public string OldName { get; set; }

		/// <summary>
		/// New name of element.
		/// </summary>
		/// <value></value>
        public string NewName { get; set; }

        #endregion

        #region IEnumerable<CodeElement> Members

		/// <summary>
		/// Gets IEnumerator for ResultSet.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
        public IEnumerator<CodeElement> GetEnumerator()
        {
            return changedElements.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

		/// <summary>
		/// Gets IEnumerator for ResultSet.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
		/// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}