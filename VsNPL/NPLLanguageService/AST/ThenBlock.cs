using System.Collections.Generic;
using ParaEngine.Tools.Lua.Parser;

namespace ParaEngine.Tools.Lua.AST
{
    /// <summary>
    /// Represents a ThenBlock node in the AST for the Lua code file.
    /// </summary>
    public class ThenBlock : Node
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ThenBlock"/> class.
        /// </summary>
        /// <param name="location">The location of the node.</param>
        public ThenBlock(LexLocation location)
            : base(location)
        {
        }

		/// <summary>
		/// Gets or sets the block.
		/// </summary>
		/// <value>The block.</value>
        public Block Block { get; set; }

		/// <summary>
		/// Gets or sets the first node of the linked list of ElseIfBlocks.
		/// </summary>
		/// <value>The else if block list.</value>
        public ElseIfBlock ElseIfBlockList { get; set; }

		/// <summary>
		/// Gets or sets the 'else' block.
		/// </summary>
		/// <value>The else block.</value>
        public Block ElseBlock { get; set; }

        /// <summary>
        /// Gets the child nodes of the node.
        /// </summary>
        /// <returns>An enumerable collection of nodes.</returns>
        public override IEnumerable<Node> GetChildNodes()
        {
            yield return Block;
            yield return ElseIfBlockList;
            yield return ElseBlock;
        }
    }
}
