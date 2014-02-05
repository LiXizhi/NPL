using System.Collections.Generic;

namespace ParaEngine.Tools.Lua
{
    /// <summary>
    /// AST nodes implementing this interface provide scoped declarations for CodeSense.
    /// </summary>
    public interface IScopedDeclarationProvider
    {
        /// <summary>
        /// Gets an enumeration of declarations that should be scoped by the same node that declares it.
        /// </summary>
        /// <returns>An enumeration of declarations.</returns>
        IEnumerable<Declaration> GetScopedDeclarations();
    }
}
