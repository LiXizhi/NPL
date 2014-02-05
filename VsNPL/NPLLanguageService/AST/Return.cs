using System.Collections.Generic;
using ParaEngine.Tools.Lua.Parser;

namespace ParaEngine.Tools.Lua.AST
{
    /// <summary>
    /// Represents a return statement node in the AST for a Lua code file.
    /// </summary>
    public class Return : Node
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Return"/> class.
        /// </summary>
        /// <param name="location">The location of the node.</param>
        public Return(LexLocation location)
            : base(location)
        {
        }

		/// <summary>
		/// Gets or sets the first node of the linked list of expressions
		/// </summary>
		/// <value>The expression list.</value>
        public Node ExpressionList { get; set; }

        /// <summary>
        /// Gets the child nodes of the node.
        /// </summary>
        /// <returns>An enumerable collection of nodes.</returns>
        public override IEnumerable<Node> GetChildNodes()
        {
            yield return ExpressionList;
        }
    }
}
