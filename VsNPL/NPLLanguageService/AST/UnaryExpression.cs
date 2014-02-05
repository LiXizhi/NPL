using System;
using System.Collections.Generic;
using ParaEngine.Tools.Lua.Parser;

namespace ParaEngine.Tools.Lua.AST
{
    /// <summary>
    /// Represents a UnaryExpression node in the AST of a Lua code file.
    /// </summary>
    public class UnaryExpression : Node
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnaryExpression"/> class.
        /// </summary>
        /// <param name="location">The location of the node.</param>
        public UnaryExpression(LexLocation location)
            : base(location)
        {
        }

		/// <summary>
		/// Gets or sets the operator.
		/// </summary>
		/// <value>The operator.</value>
        public string Operator { get; set; }

		/// <summary>
		/// Gets or sets the expression.
		/// </summary>
		/// <value>The expression.</value>
        public Node Expression { get; set; }

        /// <summary>
        /// Gets the child nodes of the node.
        /// </summary>
        /// <returns>An enumerable collection of nodes.</returns>
        public override IEnumerable<Node> GetChildNodes()
        {
            yield return Expression;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return String.Format("UnaryExpression [Operator = '{0}']", Operator);
        }
    }
}
