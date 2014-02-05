using System.Collections.Generic;
using ParaEngine.Tools.Lua.Parser;

namespace ParaEngine.Tools.Lua.AST
{
    /// <summary>
    /// Represents a ParameterList node in the AST for the Lua code file.
    /// </summary>
    /// <seealso cref="FunctionDeclaration"/>
    /// <seealso cref="Function"/>
    public class ParameterList : Node
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterList"/> class.
        /// </summary>
        /// <param name="location">The location of the node.</param>
        public ParameterList(LexLocation location)
            : base(location)
        {
        }

		/// <summary>
		/// Gets or sets the list of identifiers.
		/// </summary>
		/// <value>The identifier list.</value>
        public Identifier IdentifierList { get; set; }

        /// <summary>
        /// Gets the child nodes of the node.
        /// </summary>
        /// <returns>An enumerable collection of nodes.</returns>
        public override IEnumerable<Node> GetChildNodes()
        {
            yield return IdentifierList;
        }

    }
}
