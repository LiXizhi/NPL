using System.Runtime.InteropServices;
using EnvDTE;

namespace ParaEngine.Tools.Lua.Refactoring
{
    [Guid("36DDBEA5-D3EB-4400-9A3E-EA66F65AF6A8")]
    public interface IRefactoringService
    {
        /// <summary>
        /// Rename element in scope of parentElement.
        /// </summary>
        /// <param name="element">Element to rename.</param>
        /// <param name="parentElement">Containing element.</param>
        /// <param name="elementType">Type of element.</param>
        /// <param name="oldName">Old name of element.</param>
        /// <param name="newName">New name of element.</param>
        IRenameResult Rename(CodeElement element, CodeElement parentElement, vsCMElement elementType, string oldName, string newName);


        /// <summary>
        /// Check renaming element.
        /// </summary>
        /// <param name="element">Element to rename.</param>
        /// <param name="elementType">Type of element.</param>
        /// <param name="oldName">Old name of element.</param>
        /// <returns>True if success, otherwise False</returns>
        bool CanRenameSymbol(CodeElement element, vsCMElement elementType, string oldName);
    }
}