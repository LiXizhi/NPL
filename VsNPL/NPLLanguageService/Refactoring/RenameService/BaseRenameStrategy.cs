using System.Collections.Generic;
using EnvDTE;
using ParaEngine.Tools.Lua.CodeDom.Elements;
using ParaEngine.Tools.Lua.Refactoring;

namespace ParaEngine.Tools.Lua.Refactoring.RenameService
{
    /// <summary>
    /// Base class for various Refactor-Rename strategies.
    /// </summary>
    public abstract class BaseRenameStrategy : IRenameStrategy
    {
        #region Protected members

        protected List<SimpleCodeElement> codeElements;
        protected List<CodeElement> changedCodeElements;
        protected IRenameResult renameResult;

        #endregion

        #region IRenameStrategy Members

        /// <summary>
        /// Rename function in scope of parentElement.
        /// </summary>
        /// <param name="element">Element to rename.</param>
        /// <param name="parentElement">Containing element.</param>
        /// <param name="elementType">Type of element.</param>
        /// <param name="oldName">Old name of element.</param>
        /// <param name="newName">New name of element.</param>
        public abstract IRenameResult RenameSymbols(CodeElement element, LuaCodeClass parentElement, vsCMElement elementType, string oldName, string newName);

        #endregion

        /// <summary>
        /// Indicates that element is declared for local usage.
        /// </summary>
        protected bool IsLocalDeclaration { get; set; }

        /// <summary>
        /// 
        /// </summChecks element for local declaration scope.ary>
        /// <param name="function">CodeFunction instance.</param>
        protected void CheckLocalDeclaration(CodeFunction function)
        {
            if (function != null)
                IsLocalDeclaration = function.Access == vsCMAccess.vsCMAccessPrivate;
        }

        /// <summary>
        /// Checks element for local declaration scope.
        /// </summary>
        /// <param name="variable">CodeVariable instance.</param>
        protected void CheckLocalDeclaration(CodeVariable variable)
        {
            if (variable != null)
                IsLocalDeclaration = variable.Access == vsCMAccess.vsCMAccessPrivate;
        }
    }
}