using EnvDTE;

namespace ParaEngine.Tools.Lua.CodeDom.Definitions
{
    /// <summary>
    /// Defines a declaration interface for variable and function.
    /// </summary>
    public interface IDeclarationCodeElement
    {
        /// <summary>
        /// Gets/Sets Access type of element.
        /// </summary>
        vsCMAccess Access { get; set; }
    }
}