using System;
using System.Collections.Generic;
using System.Linq;
using ParaEngine.Tools.Lua.Parser;

namespace ParaEngine.Tools.Lua.AST
{
    /// <summary>
    /// Represents a local declaration node in the AST for the Lua code file.
    /// </summary>
    public class LocalDeclaration : Node, IDeclarationProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Declaration"/> class.
        /// </summary>
        /// <param name="location">The location of the node.</param>
        public LocalDeclaration(LexLocation location)
            : base(location)
        {
        }

		/// <summary>
		/// Gets or sets the list of identifiers being declared.
		/// </summary>
		/// <value>The identifier list.</value>
        public Identifier IdentifierList { get; set; }

		/// <summary>
		/// Gets an enumeration of declarations that the node declares.
		/// </summary>
		/// <returns>An enumeration of declarations.</returns>
        public IEnumerable<Declaration> GetDeclarations()
        {
            // Iterate through the identifier list and for each Identifier, return a Variable declaration
            return IdentifierList.OfType<Identifier>()
                                 .Select(identifier => DeclarationFactory.CreateDeclaration(DeclarationType.Variable, true, identifier));
        }

		/// <summary>
		/// Gets the child nodes of the node.
		/// </summary>
		/// <returns>An enumerable collection of nodes.</returns>
        public override IEnumerable<Node> GetChildNodes()
        {
            yield return IdentifierList;            
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return String.Format("LocalDeclaration");
        }
	}
}
