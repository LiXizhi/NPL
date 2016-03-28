using System;
using System.Linq;

using ParaEngine.Tools.Lua.AST;

namespace ParaEngine.Tools.Lua
{
    public class AstDeclarationParser
    {
        private const char functionDelimiter = ':';
        private const char nameDelimiter = '.';

        private readonly TableDeclarationProvider tableDeclarationProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="AstDeclarationParser"/> class.
        /// </summary>
		/// <param name="tableDeclarationProvider">The table declaration provider to add the declarations to.</param>
        public AstDeclarationParser(TableDeclarationProvider tableDeclarationProvider)
        {
            if (tableDeclarationProvider == null)
                throw new ArgumentNullException("tableDeclarationProvider");

            this.tableDeclarationProvider = tableDeclarationProvider;
        }

        /// <summary>
        /// Adds a chunk with location information.
        /// </summary>
        public void AddChunk(Chunk chunk, int line, int column, string filename = null)
        {
            if (chunk == null)
                throw new ArgumentNullException("chunk");

			chunk.InitializeContext();
			AddNode(chunk, line, column, TableDeclarationProvider.DeclarationsTable, false, filename);
        }

    	/// <summary>
        /// Adds the full chunk.
        /// </summary>
        /// <param name="chunk"></param>
        public void AddChunk(Chunk chunk, string filename = null)
        {
            if (chunk == null)
                throw new ArgumentNullException("chunk");

			chunk.InitializeContext();
			AddNode(chunk, Int32.MaxValue, Int32.MaxValue, TableDeclarationProvider.DeclarationsTable, false, filename);
        }

		/// <summary>
		/// Adds the node.
		/// </summary>
		/// <param name="node">The node.</param>
		/// <param name="line">The line.</param>
		/// <param name="column">The column.</param>
		/// <param name="tableName">Name of the table.</param>
		/// <param name="isInScope">if set to <c>true</c> [is in scope].</param>
		private void AddNode(Node node, int line, int column, string tableName, bool isInScope, string filename)
		{
			string innerTableName = tableName;

			if (node is TableConstructor)
			{
				// Update the table name that the child nodes should add declarations to
				innerTableName = ((TableConstructor)node).Name;
			}

			bool currentlyInScope = AddDeclarations(node, line, column, innerTableName, isInScope, filename);

			if(node!=null)
			{
				foreach (var childNode in node.GetChildNodes())
				{
					AddNode(childNode, line, column, innerTableName, currentlyInScope, filename);
				}

				if (node.Next != null)
				{
					AddNode(node.Next, line, column, tableName, isInScope, filename);
				}
			}
		}

		/// <summary>
		/// Adds the declarations.
		/// </summary>
		/// <param name="node">The node.</param>
		/// <param name="line">The line.</param>
		/// <param name="column">The column.</param>
		/// <param name="tableName">Name of the table.</param>
		/// <param name="isInScope">if set to <c>true</c> [is in scope].</param>
		/// <returns></returns>
		private bool AddDeclarations(Node node, int line, int column, string tableName, bool isInScope, string filename)
		{
			bool currentlyInScope = isInScope;

			if (node is IDeclarationProvider)
			{
				var declaringNode = (IDeclarationProvider)node;

				foreach (var declaration in declaringNode.GetDeclarations())
				{
					if (declaration.IsGlobal && !node.HasLocalVariableInScope((declaration.Name)))
					{
                        declaration.FilenameDefinedIn = filename;
                        declaration.TextspanDefinedIn = node.Location;
                        AddDeclaration(tableName, declaration);
					}
					else
					{
						if (isInScope && (node.Location.eLin<=line || (node.Location.eLin==line+1 &&
                            node.Location.eCol < column)))
                        {
                            declaration.FilenameDefinedIn = filename;
                            declaration.TextspanDefinedIn = node.Location;
                            AddDeclaration(tableName, declaration);
                        }
					}
				}
			}

			if (node is IScopedDeclarationProvider)
			{
				var declaringNode = (IScopedDeclarationProvider)node;

				currentlyInScope = node.Location.Contains(line, column);

				foreach (var declaration in declaringNode.GetScopedDeclarations())
				{
					if (declaration.IsGlobal && !node.HasLocalVariableInScope((declaration.Name)))
					{
                        declaration.FilenameDefinedIn = filename;
                        declaration.TextspanDefinedIn = node.Location;
                        AddDeclaration(tableName, declaration);
					}
					else
					{
						if (currentlyInScope)
                        {
                            declaration.FilenameDefinedIn = filename;
                            declaration.TextspanDefinedIn = node.Location;
                            AddDeclaration(tableName, declaration);
                        }
					}
				}
			}

			if (node is Block)
			{
				currentlyInScope = node.Location.Contains(line, column);
			}

			return currentlyInScope;
		}

		/// <summary>
		/// Adds the declaration.
		/// </summary>
		/// <param name="tableName">Name of the table.</param>
		/// <param name="declaration">The declaration.</param>
		private void AddDeclaration(string tableName, Declaration declaration)
		{
			if (declaration.Name.Contains(functionDelimiter))
			{
				AddFunctionDeclaration(tableName, declaration);
			}
			else if (declaration.Name.Contains(nameDelimiter))
			{
				AddQualifiedDeclaration(tableName, declaration);
			}
			else
			{
				tableDeclarationProvider.AddFieldDeclaration(tableName, declaration);
			}
		}

		/// <summary>
		/// Adds the function declaration.
		/// </summary>
		/// <param name="tableName">Name of the table.</param>
		/// <param name="declaration">The declaration.</param>
        private void AddFunctionDeclaration(string tableName, Declaration declaration)
        {
            string[] parts = declaration.Name.Split(functionDelimiter);

            string qualifiedTableName = parts[0];
            string functionName = parts[1];

            // Resolve the qualified table, potentially creating new table declarations
            tableName = tableDeclarationProvider.ResolveQualifiedName(tableName, qualifiedTableName, true);

            // Update the declaration
            declaration.Name = functionName;

            // Add the field declaration
            tableDeclarationProvider.AddFieldDeclaration(tableName, declaration);
        }

		/// <summary>
		/// Adds the qualified declaration.
		/// </summary>
		/// <param name="tableName">Name of the table.</param>
		/// <param name="declaration">The declaration.</param>
        private void AddQualifiedDeclaration(string tableName, Declaration declaration)
        {
            int lastDelimiterIndex = declaration.Name.LastIndexOf(nameDelimiter);

            string qualifiedTableName = declaration.Name.Substring(0, lastDelimiterIndex);
            string declarationName = declaration.Name.Substring(lastDelimiterIndex + 1);

            // Resolve the qualified table, potentially creating new table declarations
            tableName = tableDeclarationProvider.ResolveQualifiedName(tableName, qualifiedTableName, true);

            // Update the declaration
            declaration.Name = declarationName;

            // Add the field declaration
            tableDeclarationProvider.AddFieldDeclaration(tableName, declaration);
        }
    }
}