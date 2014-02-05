using System;
using System.Collections.Generic;
using ParaEngine.Tools.Lua.Parser;

namespace ParaEngine.Tools.Lua.AST
{
    public class BinaryExpression : Node
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryExpression"/> class.
        /// </summary>
        /// <param name="location">The location of the node.</param>
        public BinaryExpression(LexLocation location)
            : base(location)
        {
        }

		/// <summary>
		/// Gets or sets the operator.
		/// </summary>
		/// <value>The operator.</value>
        public string Operator { get; set; }

		/// <summary>
		/// Gets or sets the expression on the left side.
		/// </summary>
		/// <value>The left expression.</value>
        public Node LeftExpression { get; set; }

		/// <summary>
		/// Gets or sets the expression on the right side.
		/// </summary>
		/// <value>The right expression.</value>
        public Node RightExpression { get; set; }

        /// <summary>
        /// Gets the child nodes of the node.
        /// </summary>
        /// <returns>An enumerable collection of nodes.</returns>
        public override IEnumerable<Node> GetChildNodes()
        {
            yield return LeftExpression;
            yield return RightExpression;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return String.Format("BinaryExpression [Operator = '{0}']", Operator);
        }
    }
}
