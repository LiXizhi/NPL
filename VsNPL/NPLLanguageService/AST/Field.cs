using System.Collections.Generic;
using ParaEngine.Tools.Lua.Parser;

namespace ParaEngine.Tools.Lua.AST
{
    /// <summary>
    /// Represents a field node in a table constructor in the AST for the Lua code file.
    /// </summary>
    /// <remarks>
    /// The field can represent the following Lua language constructs:
    /// <code>[LeftExpression] = Expression</code>
    /// <code>Identifier = Expression</code>
    /// <code>Expression</code>
    /// </remarks>
    public class Field : Node, IDeclarationProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Function"/> class.
        /// </summary>
        /// <param name="location">The location of the node.</param>
        public Field(LexLocation location)
            : base(location)
        {
        }

		/// <summary>
		/// Gets or sets the name of the identifier if the field contains a simple assignemnt.
		/// </summary>
		/// <value>The identifier.</value>
        public Identifier Identifier { get; set; }

		/// <summary>
		/// Gets or sets the expression that produces the value of the field.
		/// </summary>
		/// <value>The expression.</value>
        public Node Expression { get; set; }

		/// <summary>
		/// Gets or sets the expression inside the brackets that is on the left side of the field assignment.
		/// </summary>
		/// <value>The left expression.</value>
        public Node LeftExpression { get; set; }

        /// <summary>
        /// Gets an enumeration of declarations that the node declares.
        /// </summary>
        /// <returns>An enumeration of declarations.</returns>
        public IEnumerable<Declaration> GetDeclarations()
        {
            if (Identifier != null)
            {
                // Try to deduct the declaration type and the type from the Expression
                DeclarationType declarationType = DeclarationType.Field;
                string type = null;

                if (Expression is Function)
                    declarationType = DeclarationType.Function;

                if (Expression is TableConstructor)
                {
                    declarationType = DeclarationType.Table;
                    type = ((TableConstructor)Expression).Name;
                }

                yield return DeclarationFactory.CreateDeclaration(declarationType, type, Identifier);
            }
        }

        /// <summary>
        /// Gets the child nodes.
        /// </summary>
        /// <returns>An enumerable collection of nodes.</returns>
        public override IEnumerable<Node> GetChildNodes()
        {
            yield return Identifier;
            yield return LeftExpression;
            yield return Expression;
        }
	}
}
