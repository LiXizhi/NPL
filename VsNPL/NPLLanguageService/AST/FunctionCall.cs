using System.Collections.Generic;
using ParaEngine.Tools.Lua.Parser;

namespace ParaEngine.Tools.Lua.AST
{
    /// <summary>
    /// Represents a function call statement node in the AST for the Lua code file.
    /// </summary>
    public class FunctionCall : Node
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionCall"/> class.
        /// </summary>
        /// <param name="location">The location of the node.</param>
        public FunctionCall(LexLocation location)
            : base(location)
        {
        }

		/// <summary>
		/// Gets or sets the prefix expression.
		/// </summary>
		/// <value>The prefix expression.</value>
        public Node PrefixExpression { get; set; }

		/// <summary>
		/// Gets or sets the identifier after the colon.
		/// </summary>
		/// <value>The identifier.</value>
        public Identifier Identifier { get; set; }

		/// <summary>
		/// Gets or sets the arguments.
		/// </summary>
		/// <value>The arguments.</value>
        public Node Arguments { get; set; }

        /// <summary>
        /// Gets the child nodes of the node.
        /// </summary>
        /// <returns>An enumerable collection of nodes.</returns>
        public override IEnumerable<Node> GetChildNodes()
        {
            yield return PrefixExpression;
            yield return Identifier;
            yield return Arguments;
        }
    }
}
