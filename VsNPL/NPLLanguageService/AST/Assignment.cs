using System;
using System.Collections.Generic;
using ParaEngine.Tools.Lua.Parser;

namespace ParaEngine.Tools.Lua.AST
{
    /// <summary>
    /// Represents an assignment statement in a lua code file.
    /// </summary>
    /// <example>
    /// <code>x, y = 5, 6</code>
    /// </example>
    public class Assignment : Node, IDeclarationProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Assignment"/> class.
        /// </summary>
        /// <param name="location">The location of the node.</param>
        public Assignment(LexLocation location)
            : base(location)
        {
        }

        /// <summary>
        /// Gets or sets the linked list of variables on the left side of the assignment.
        /// </summary>
        public Node VariableList { get; set; }

        /// <summary>
        /// Gets or sets the linked list of expression on the right side of the assignment.
        /// </summary>
        public Node ExpressionList { get; set; }

        /// <summary>
        /// Gets or sets whether the assignment is local.
        /// </summary>
        public bool IsLocal { get; set; }

        /// <summary>
        /// Gets an enumeration of declarations that the node declares.
        /// </summary>
        /// <returns>An enumeration of declarations.</returns>
        public IEnumerable<Declaration> GetDeclarations()
        {
            if (VariableList != null && ExpressionList != null)
            {
                // Iterate through the two linked lists in parallel and infer the type from the expression.
                var enumerator = new ParallelEnumerator<Node, Node>(VariableList, ExpressionList);

                while (enumerator.MoveNext())
                {
                    if (enumerator.CurrentFirst is Identifier)
                    {
                        var identifier = (Identifier)enumerator.CurrentFirst;
                        DeclarationType declarationType = DeclarationType.Variable;
                        string type = null;

                        if (enumerator.CurrentSecond is Function)
                        {
                            declarationType = DeclarationType.Function;
                        }

                        if (enumerator.CurrentSecond is TableConstructor)
                        {
                            declarationType = DeclarationType.Table;
                            type = ((TableConstructor)enumerator.CurrentSecond).Name;
                        }

                        if (enumerator.CurrentSecond is Literal)
                        {
                            type = ((Literal)enumerator.CurrentSecond).Type.ToString();
                        }

                        yield return new Declaration
                                        {
                                            DeclarationType = declarationType,
                                            Name = identifier.Name,
                                            IsLocal = this.IsLocal,
                                            Type = type
                                        };
                    }
                }
            }
        }

        /// <summary>
        /// Gets the child nodes of the node.
        /// </summary>
        /// <returns>An enumerable collection of nodes.</returns>
        public override IEnumerable<Node> GetChildNodes()
        {
            yield return VariableList;
            yield return ExpressionList;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return String.Format("Assignment [IsLocal = {0}]", this.IsLocal);
        }
	}
}
