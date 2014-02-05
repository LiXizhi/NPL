using EnvDTE;
using ParaEngine.Tools.Lua.CodeDom.Elements;
using ParaEngine.Tools.Lua.Refactoring;

namespace ParaEngine.Tools.Lua.Refactoring.RenameService
{
    /// <summary>
    /// Interface for implementation of RenameStrategies.
    /// </summary>
    public interface IRenameStrategy
    {
        /// <summary>
        /// Rename element in scope of parentElement.
        /// </summary>
        /// <param name="element">Element to rename.</param>
        /// <param name="parentElement">Containing element.</param>
        /// <param name="elementType">Type of element.</param>
        /// <param name="oldName">Old name of element.</param>
        /// <param name="newName">New name of element.</param>
        IRenameResult RenameSymbols(CodeElement element, LuaCodeClass parentElement, vsCMElement elementType, string oldName, string newName);
    }
}