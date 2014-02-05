using System.Collections.Generic;
using ParaEngine.Tools.Lua.Parser;

namespace ParaEngine.Tools.Lua.AST
{
    /// <summary>
    /// Represents an If node in the AST for a Lua code file.
    /// </summary>
    public class If : Node
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="If"/> class.
        /// </summary>
        /// <param name="location">The location.</param>
        public If(LexLocation location)
            : base(location)
        {
        }

		/// <summary>
		/// Gets or sets the expression evaluated.
		/// </summary>
		/// <value>The expression.</value>
        public Node Expression { get; set; }

		/// <summary>
		/// Gets or sets the 'then' block.
		/// </summary>
		/// <value>The then block.</value>
        public ThenBlock ThenBlock { get; set; }

        /// <summary>
        /// Gets the child nodes of the node.
        /// </summary>
        /// <returns>An enumerable collection of nodes.</returns>
        public override IEnumerable<Node> GetChildNodes()
        {
            yield return Expression;
            yield return ThenBlock;
        }

    }
}
