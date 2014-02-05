using System.Collections.Generic;
using ParaEngine.Tools.Lua.Parser;

namespace ParaEngine.Tools.Lua.AST
{
    /// <summary>
    /// Represents an 'elseif' block in the AST for a Lua code file.
    /// </summary>
    public class ElseIfBlock : Node
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="ElseIfBlock"/> class.
		/// </summary>
		/// <param name="location">The location of the node.</param>
        public ElseIfBlock(LexLocation location)
            : base(location)
        {
        }

		/// <summary>
		/// Gets or sets the 'elseif' expression that is evaluated.
		/// </summary>
		/// <value>The expression.</value>
        public Node Expression { get; set; }

		/// <summary>
		/// Gets or sets the block.
		/// </summary>
		/// <value>The block.</value>
        public Block Block { get; set; }

        /// <summary>
        /// Gets the child nodes of the node.
        /// </summary>
        /// <returns>An enumerable collection of nodes.</returns>
        public override IEnumerable<Node> GetChildNodes()
        {
            yield return Expression;
            yield return Block;
        }
    }
}
