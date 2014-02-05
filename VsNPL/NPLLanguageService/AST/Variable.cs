using System.Collections.Generic;
using ParaEngine.Tools.Lua.Parser;

namespace ParaEngine.Tools.Lua.AST
{
    /// <summary>
    /// Represents a variable node in the AST for the Lua code file.
    /// </summary>
    public class Variable : Node
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Variable"/> class.
        /// </summary>
        /// <param name="location">The location of the node.</param>
        public Variable(LexLocation location)
            : base(location)
        {
        }

		/// <summary>
		/// Gets or sets the prefix expression.
		/// </summary>
		/// <value>The prefix expression.</value>
        public Node PrefixExpression { get; set; }

		/// <summary>
		/// Gets or sets the expression.
		/// </summary>
		/// <value>The expression.</value>
        public Node Expression { get; set; }

		/// <summary>
		/// Gets or sets the identifier.
		/// </summary>
		/// <value>The identifier.</value>
        public Identifier Identifier { get; set; }

        /// <summary>
        /// Gets the child nodes of the node.
        /// </summary>
        /// <returns>An enumerable collection of nodes.</returns>
        /// <remarks>A <c>null</c> value can be yielded and should be handled.</remarks>
        public override IEnumerable<Node> GetChildNodes()
        {
            yield return PrefixExpression;
            yield return Expression;
            yield return Identifier;
        }
    }
}
