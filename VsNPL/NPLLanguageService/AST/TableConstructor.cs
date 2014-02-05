using System;
using System.Collections.Generic;
using ParaEngine.Tools.Lua.Parser;

namespace ParaEngine.Tools.Lua.AST
{
    /// <summary>
    /// Represents a table constructor node in the AST for the Lua code file.
    /// </summary>
    public class TableConstructor : Node
    {
        // Initialize a default name for the scope if no other name is assigned.
        private string name = "~Table_" + Guid.NewGuid();

        /// <summary>
        /// Initializes a new instance of the <see cref="Function"/> class.
        /// </summary>
        /// <param name="location">The location of the node.</param>
        public TableConstructor(LexLocation location)
            : base(location)
        {
        }

		/// <summary>
		/// Gets the qualifying name of the scope.
		/// </summary>
		/// <value>The name.</value>
		/// <remarks>
		/// This node is not aware of its scoping name and its name is set during the parsing of the tree.
		/// </remarks>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

		/// <summary>
		/// Gets the linked list of fields in the table constructor.
		/// </summary>
		/// <value>The field list.</value>
        public Field FieldList { get; set; }

		/// <summary>
		/// Gets the child nodes.
		/// </summary>
		/// <returns>An enumerable collection of nodes.</returns>
        public override IEnumerable<Node> GetChildNodes()
        {
            yield return FieldList;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return String.Format("TableConstructor [Name = {0}]", this.Name);
        }
    }
}
