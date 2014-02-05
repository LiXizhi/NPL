using EnvDTE;
using ParaEngine.Tools.Lua.CodeDom.Elements;
using ParaEngine.Tools.Lua.Refactoring;
using ParaEngine.Tools.Lua.Refactoring.RenameService;

namespace ParaEngine.Tools.Lua.Refactoring.RenameService
{
    /// <summary>
    /// Operates with real implementation of IRenameStrategy.
    /// </summary>
    internal class CodeElementRenameContext : IRenameStrategy
    {
		/// <summary>
		/// Gets or sets the code element rename.
		/// </summary>
		/// <value>The code element rename.</value>
        internal IRenameStrategy CodeElementRename { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="CodeElementRenameContext"/> class.
		/// </summary>
		/// <param name="strategy">The strategy.</param>
        internal CodeElementRenameContext(IRenameStrategy strategy)
        {
            CodeElementRename = strategy;
        }

        #region IRenameStrategy Members

        /// <summary>
        /// Rename element in scope of parentElement.
        /// </summary>
        /// <param name="element">Element to rename.</param>
        /// <param name="parentElement">Containing element.</param>
        /// <param name="elementType">Type of element.</param>
        /// <param name="oldName">Old name of element.</param>
        /// <param name="newName">New name of element.</param>
        public IRenameResult RenameSymbols(CodeElement element, LuaCodeClass parentElement, vsCMElement elementType, string oldName, string newName)
        {
            if (CodeElementRename == null)
            {
                throw new InvalidStrategyException(CodeElementRename);
            }
            return CodeElementRename.RenameSymbols(element, parentElement, elementType, oldName, newName);
        }

        #endregion
    }
}