using System.Collections.Generic;
using EnvDTE;
using ParaEngine.Tools.Lua.CodeDom;

namespace ParaEngine.Tools.Lua.Refactoring
{
    /// <summary>
    /// Base class for refactoring conflict resolvers.
    /// </summary>
    public abstract class ConflictResolverBase : IConflictResolver
    {
        protected readonly LuaFileCodeModel fileCodeModel;

    	/// <summary>
        /// Initializes a new instance of the <see cref="ConflictResolverBase"/> class.
        /// </summary>
        /// <param name="fileCodeModel"></param>
		protected ConflictResolverBase(LuaFileCodeModel fileCodeModel)
        {
            this.fileCodeModel = fileCodeModel;
        }

		/// <summary>
		/// Gets or sets the type of the conflict.
		/// </summary>
		/// <value>The type of the conflict.</value>
    	public ConflictType CodeConflictType { get; set; }

    	/// <summary>
		/// Determines whether this instance has conflict.
		/// </summary>
		/// <returns>
		/// 	<c>true</c> if this instance has conflict; otherwise, <c>false</c>.
		/// </returns>
        public abstract bool HasConflict();

		/// <summary>
		/// Finds the conflicts.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <param name="newName">The new name.</param>
		/// <returns></returns>
        public abstract IEnumerable<CodeElement> FindConflicts(CodeElement element, string newName);
    }
}