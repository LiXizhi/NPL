using System.Collections.Generic;
using System.Linq;
using ParaEngine.Tools.Lua.Parser;

namespace ParaEngine.Tools.Lua.AST
{
    /// <summary>
    /// Represents a for loop node in the AST for the Lua code file.
    /// </summary>
    public class ForLoop : Loop, IScopedDeclarationProvider
    {
        ///// <summary>
        /// Initializes a new instance of the <see cref="ForLoop"/> class.
        /// </summary>
        /// <param name="location">The location of the node.</param>
        public ForLoop(LexLocation location)
            : base(location)
        {
        }

		/// <summary>
		/// Gets or sets the identifier used to loop.
		/// </summary>
		/// <value>The identifier.</value>
        public Identifier Identifier { get; set; }

		/// <summary>
		/// Gets or sets the list of identifiers in an 'in' loop.
		/// </summary>
		/// <value>The identifier list.</value>
        public Identifier IdentifierList { get; set; }

        /// <summary>
        /// Gets an enumeration of declarations that should be scoped by the same node that declares it.
        /// </summary>
        /// <returns>An enumeration of declarations.</returns>
        public IEnumerable<Declaration> GetScopedDeclarations()
        {
            // Check if we have an identifier in a 'for x = 1,1,10 do <block> end' style for loop
            if (Identifier != null)
            {
                yield return DeclarationFactory.CreateDeclaration(DeclarationType.Variable, true, Identifier);
            }

            // Check if we have an identifier in a 'for x,y in <expression> do <block> end' style for loop
            if (IdentifierList != null)
            {
                IdentifierList.OfType<Identifier>()
                              .Select(identifier => DeclarationFactory.CreateDeclaration(DeclarationType.Variable, true, identifier));
            }
        }

        /// <summary>
        /// Gets the child nodes of the node.
        /// </summary>
        /// <returns>An enumerable collection of nodes.</returns>
        public override IEnumerable<Node> GetChildNodes()
        {
            yield return Identifier;
            yield return IdentifierList;
            yield return Expression;
            yield return Block;
        }
	}
}
