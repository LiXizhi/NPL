using System;
using System.Collections.Generic;
using ParaEngine.Tools.Lua.Parser;

namespace ParaEngine.Tools.Lua.AST
{
    /// <summary>
    /// Represents a Loop node in the AST for the Lua code file. This class is abstract.
    /// </summary>
    /// <seealso cref="ForLoop"/>
    /// <seealso cref="WhileLoop"/>
    /// <seealso cref="RepeatUntilLoop"/>
    public abstract class Loop : Node
    {
        private string name = "~Loop_" + Guid.NewGuid();

        /// <summary>
        /// Initializes a new instance of the <see cref="Loop"/> class.
        /// </summary>
        /// <param name="location">The location of the node.</param>
		protected Loop(LexLocation location)
            : base(location)
        {
        }

		/// <summary>
		/// Gets or sets the qualifying name of the scope.
		/// </summary>
		/// <value>The name.</value>
        public virtual string Name
        {
            get { return name; }
            set { name = value; }
        }

		/// <summary>
		/// Gets whether this node denotes a scope.
		/// </summary>
		/// <value></value>
        public override bool IsScope
        {
            get { return true; }
        }

		/// <summary>
		/// Gets or sets the expression evaluated for each loop.
		/// </summary>
		/// <value>The expression.</value>
        public Node Expression { get; set; }

		/// <summary>
		/// Gets or sets the inner block of the loop.
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

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return String.Format("{0} [Name: {1}]", base.ToString(), name);
        }

    }
}
