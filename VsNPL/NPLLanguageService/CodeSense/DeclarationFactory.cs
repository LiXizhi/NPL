using System;

using ParaEngine.Tools.Lua.AST;

namespace ParaEngine.Tools.Lua
{
    /// <summary>
    /// Creates declarations based on AST nodes.
    /// </summary>
    public static class DeclarationFactory
    {
        /// <summary>
        /// Creates a declaration for an Identifier node.
        /// </summary>
        /// <param name="declarationType">The type of the declaration.</param>
        /// <param name="identifier">The Identifier node to use.</param>
        /// <returns>An instance of the <see cref="Declaration"/> class.</returns>
        public static Declaration CreateDeclaration(DeclarationType declarationType, Identifier identifier)
        {
            if (identifier == null)
                throw new ArgumentNullException("identifier");

            return new Declaration
                    {
                        DeclarationType = declarationType,
                        Name = identifier.Name
                    };
        }

        /// <summary>
        /// Creates a declaration for an Identifier node.
        /// </summary>
        /// <param name="declarationType">The type of the declaration.</param>
        /// <param name="type">The type of the declared variable/field.</param>
        /// <param name="identifier">The Identifier node to use.</param>
        /// <returns>An instance of the <see cref="Declaration"/> class.</returns>
        public static Declaration CreateDeclaration(DeclarationType declarationType, bool isLocal, Identifier identifier)
        {
            if (identifier == null)
                throw new ArgumentNullException("identifier");

            return new Declaration
                        {
                            DeclarationType = declarationType,
                            IsLocal = isLocal,
                            Name = identifier.Name
                        };
        }

        /// <summary>
        /// Creates a declaration for an Identifier node.
        /// </summary>
        /// <param name="declarationType">The type of the declaration.</param>
        /// <param name="type">The type of the declared variable/field.</param>
        /// <param name="identifier">The Identifier node to use.</param>
        /// <returns>An instance of the <see cref="Declaration"/> class.</returns>
        public static Declaration CreateDeclaration(DeclarationType declarationType, string type, Identifier identifier)
        {
            if (identifier == null)
                throw new ArgumentNullException("identifier");

            return new Declaration
            {
                DeclarationType = declarationType,
                Type = type,
                Name = identifier.Name
            };
        }
    }
}
