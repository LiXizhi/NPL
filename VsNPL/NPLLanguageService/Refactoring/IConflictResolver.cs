using System.Collections.Generic;
using EnvDTE;

namespace ParaEngine.Tools.Lua.Refactoring
{
    /// <summary>
    /// 
    /// </summary>
    public interface IConflictResolver
    {
		/// <summary>
		/// Determines whether this instance has conflict.
		/// </summary>
		/// <returns>
		/// 	<c>true</c> if this instance has conflict; otherwise, <c>false</c>.
		/// </returns>
        bool HasConflict();

		/// <summary>
		/// Finds the conflicts.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <param name="newName">The new name.</param>
		/// <returns></returns>
        IEnumerable<CodeElement> FindConflicts(CodeElement element, string newName);
    }
}