using System;
using System.Collections.Generic;
using System.Text;
using EnvDTE;
using ParaEngine.Tools.Lua.Refactoring;

namespace ParaEngine.Tools.Lua.Refactoring.RenameService
{
    /// <summary>
    /// Holds multiple results from rename operations.
    /// </summary>
    public class MultiRenameResult : IRenameResult, IEnumerable<KeyValuePair<ProjectItem, IRenameResult>>
    {
        private readonly Dictionary<ProjectItem, IRenameResult> changedProjectElements;
		private IList<FileCodeModel> parents = new List<FileCodeModel>();
        private readonly StringBuilder messageBuilder;

        /// <summary>
        /// Gets changed project elements
        /// </summary>
        public Dictionary<ProjectItem, IRenameResult> ChangedProjectElements
        {
            get { return changedProjectElements; }
        }

        /// <summary>
        /// Indexer for MultiRenameResult class.
        /// </summary>
        /// <param name="projectItem">ProjectItem reference.</param>
        /// <returns>List of associted CodeElement of ProjectItem</returns>
        public IRenameResult this[ProjectItem projectItem]
        {
            get
            {
                IRenameResult result;
                changedProjectElements.TryGetValue(projectItem, out result);
                return result;
            }
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="MultiRenameResult"/> class.
		/// </summary>
		/// <param name="oldName">The old name.</param>
		/// <param name="newName">The new name.</param>
        public MultiRenameResult(string oldName, string newName)
            : this(false, string.Empty, null, oldName, newName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiRenameResult"/> class.
        /// </summary>
        public MultiRenameResult()
            : this(false, string.Empty, null, string.Empty, string.Empty)
        {
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="MultiRenameResult"/> class.
		/// </summary>
		/// <param name="success">if set to <c>true</c> [success].</param>
        public MultiRenameResult(bool success)
            : this(success, string.Empty, null, string.Empty, string.Empty)
        {
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="MultiRenameResult"/> class.
		/// </summary>
		/// <param name="success">if set to <c>true</c> [success].</param>
		/// <param name="message">The message.</param>
        public MultiRenameResult(bool success, string message)
            : this(success, message, null, string.Empty, string.Empty)
        {
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="MultiRenameResult"/> class.
		/// </summary>
		/// <param name="success">if set to <c>true</c> [success].</param>
		/// <param name="message">The message.</param>
		/// <param name="changedElements">The changed elements.</param>
		/// <param name="oldName">The old name.</param>
		/// <param name="newName">The new name.</param>
        public MultiRenameResult(bool success, string message, Dictionary<ProjectItem, IRenameResult> changedElements,
                                 string oldName, string newName)
        {
            Success = success;
            OldName = oldName;
            NewName = newName;
            messageBuilder = new StringBuilder();
            if (!string.IsNullOrEmpty(message)) messageBuilder.Append(message);
			changedProjectElements = changedElements ?? new Dictionary<ProjectItem, IRenameResult>();
        }

        #region IRenameResult Members

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

		/// <summary>
		/// Indicates that operation successfully completed.
		/// </summary>
		/// <value></value>
        public bool Success { get; set; }

		/// <summary>
		/// Gets information about rename operation.
		/// </summary>
		/// <value></value>
        public string Message
        {
            get { return messageBuilder.ToString(); }
            set { messageBuilder.Append(value); }
        }

		/// <summary>
		/// Indicates that IRenameResult hold changed elements.
		/// </summary>
		/// <value></value>
        public bool HasChanges
        {
            get { return changedProjectElements.Count > 0; }
        }

		/// <summary>
		/// Reteurn true if change only local elements otherwise false.
		/// </summary>
		/// <value></value>
        public bool RenameReferences { get; set; }

		/// <summary>
		/// Gets changed CodeElements.
		/// </summary>
		/// <value></value>
        IEnumerable<CodeElement> IRenameResult.ChangedElements
        {
            get
            {
                var elements = new List<CodeElement>();
                changedProjectElements.Values.ForEach(
                    element => elements.AddRange(element.ChangedElements));
                return elements;
            }
            set { }
        }

        /// <summary>
        /// Gets changed CodeElements.
        /// </summary>
        public List<IRenameResult> ChangedElements
        {
            get
            {
                var elements = new List<IRenameResult>();
                changedProjectElements.Values.ForEach(element => elements.Add(element));
                return elements;
            }
		}

		/// <summary>
		/// Gets changed CodeElements by ProjectItem.
		/// </summary>
		/// <param name="projectItem">The project item.</param>
		/// <returns></returns>
        public IRenameResult GetChangedProjectElements(ProjectItem projectItem)
        {
            if (changedProjectElements.ContainsKey(projectItem))
            {
                return changedProjectElements[projectItem];
            }
            return null;
        }

        /// <summary>
        /// Merges changes from other RenameResult.
        /// </summary>
        /// <param name="success">Rename operation status.</param>
        /// <param name="changedElements">Changed CodeElements in rename operations.</param>
        public void MergeChanges(bool success, IEnumerable<CodeElement> changedElements)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Merges changes from other RenameResult.
        /// </summary>
        /// <param name="success">Rename operation status.</param>
        /// <param name="projectItem">ProjectItem associated with CodeElements</param>
        /// <param name="changedElements">Changed CodeElements in rename operations.</param>
        public void MergeChanges(bool success, ProjectItem projectItem, IEnumerable<CodeElement> changedElements)
        {
            MergeChanges(success, String.Empty, projectItem, changedElements);
        }

        /// <summary>
        /// Merges changes from other RenameResult.
        /// </summary>
        /// <param name="success">Rename operation status.</param>
        /// <param name="changedElements">Changed CodeElements in rename operations.</param>
        /// <param name="message">Information about rename operation.</param>
        /// <param name="projectItem">ProjectItem associated with CodeElements</param>
        public void MergeChanges(bool success, string message, ProjectItem projectItem,
                                 IEnumerable<CodeElement> changedElements)
        {
            Success = success;
            if (!string.IsNullOrEmpty(message)) messageBuilder.Append(message);
            if (projectItem != null && changedElements != null)
            {
                if (changedProjectElements.ContainsKey(projectItem))
                {
                    if (changedProjectElements[projectItem] == null)
                    {
                        changedProjectElements[projectItem] = new RenameResult(success, message, changedElements,
                                                                               OldName, NewName){Parents = Parents};
                    }
                    else
                    {
                        var elements = new List<CodeElement>(changedProjectElements[projectItem].ChangedElements);
                        elements.AddRange(changedElements);
                        changedProjectElements[projectItem] = new RenameResult(success, message, elements, OldName,
																			   NewName) { Parents = Parents };
                    }
                }
                else
                {
                    changedProjectElements.Add(projectItem,
											   new RenameResult(success, message, changedElements, OldName, NewName) { Parents = Parents });
                }
            }
        }

        /// <summary>
        /// Merges changes from other RenameResult.
        /// </summary>
        /// <param name="projectItem">ProjectItem associated with CodeElements</param> 
        /// <param name="result">Rename operation result.</param>
        public void MergeChanges(ProjectItem projectItem, IRenameResult result)
        {
            if (string.IsNullOrEmpty(result.OldName)) result.OldName = OldName;

			if (result.Parents != null)
				result.Parents.ForEach(parent => {if(!parents.Contains(parent)) Parents.Add(parent);});

            if (result is MultiRenameResult)
            {
                foreach (KeyValuePair<ProjectItem, IRenameResult> item 
                    in ((result as MultiRenameResult)).ChangedProjectElements)
                {
                    MergeChanges(item.Key, item.Value);
                }
            }
            else
            {
                if (!changedProjectElements.ContainsKey(projectItem))
                    changedProjectElements.Add(projectItem, result);
            }
        }

        #endregion

        #region IEnumerable<KeyValuePair<ProjectItem,List<CodeElement>>> Members

		/// <summary>
		/// Gets IEnumerator for ResultSet.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
        public IEnumerator<KeyValuePair<ProjectItem, IRenameResult>> GetEnumerator()
        {
            return ChangedProjectElements.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

		/// <summary>
		/// Gets IEnumerator for ResultSet.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
		/// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}