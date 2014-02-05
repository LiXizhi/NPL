using System.Collections.Generic;

namespace ParaEngine.Tools.Lua
{
    /// <summary>
    /// AST nodes implementing this interface provide declarations for CodeSense.
    /// </summary>
    public interface IDeclarationProvider
    {
        /// <summary>
        /// Gets an enumeration of declarations that the node declares.
        /// </summary>
        /// <returns>An enumeration of declarations.</returns>
        IEnumerable<Declaration> GetDeclarations();
    }
}
