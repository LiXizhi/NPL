using System.Collections.Generic;
using ParaEngine.Tools.Lua.Parser;

namespace ParaEngine.Tools.Lua.AST
{
    /// <summary>
    /// Represents a block of statements in a Lua code file.
    /// </summary>
    public class Block : Node
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Block"/> class.
        /// </summary>
        /// <param name="location">The location of the node.</param>
        public Block(LexLocation location)
            : base(location)
        {
        }

		/// <summary>
		/// Gets or sets the first node in the linked list of statements in the block.
		/// </summary>
		/// <value>The statement list.</value>
        public Node StatementList { get; set; }

        /// <summary>
        /// Gets the child nodes of the node.
        /// </summary>
        /// <returns>An enumerable collection of nodes.</returns>
        public override IEnumerable<Node> GetChildNodes()
        {
            yield return StatementList;
        }
    }
}
