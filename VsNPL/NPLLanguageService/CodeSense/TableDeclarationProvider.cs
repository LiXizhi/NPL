using System;
using System.Collections.Generic;
using System.Linq;

namespace ParaEngine.Tools.Lua
{
    /// <summary>
    /// Holds declarations that have been added to the provider and merges it dynamically with other declarations.
    /// </summary>
	public class TableDeclarationProvider : ICodeSenseDeclarationProvider
	{
		public const string DeclarationsTable = "__DECLARATIONS";

		private readonly char[] delimiters = new[] { '.', ':' };
		// private const char delimiter = '.';

		// Stores the list of declarations for each table
		private readonly Dictionary<string, Dictionary<string, Declaration>> tableDeclarations = new Dictionary<string, Dictionary<string, Declaration>>(1);

		/// <summary>
		/// Initializes a new instance of the <see cref="TableDeclarationProvider"/> class.
		/// </summary>
		public TableDeclarationProvider()
		{
			InitializeTables();
		}

		/// <summary>
		/// Clears the declarations.
		/// </summary>
		public void Clear()
		{
			tableDeclarations.Clear();
			InitializeTables();
		}

		/// <summary>
		/// Initializes the tables.
		/// </summary>
		private void InitializeTables()
		{
			// Initialize the declaration table
			tableDeclarations[DeclarationsTable] = new Dictionary<string, Declaration>();
		}

		/// <summary>
		/// Adds a field declaration to a table.
		/// </summary>
		/// <param name="tableName">The name of the table.</param>
		/// <param name="declaration">The declaration.</param>
		public void AddFieldDeclaration(string tableName, Declaration declaration)
		{
			// Check if no declarations have been added for this table
			if (!tableDeclarations.ContainsKey(tableName))
			{
				tableDeclarations[tableName] = new Dictionary<string, Declaration>(1);
			}
			else
			{
				// Remove declaration if already existed
				if (tableDeclarations[tableName].ContainsKey(declaration.Name))
					tableDeclarations[tableName].Remove(declaration.Name);
			}

			// Add declaration for the table
			tableDeclarations[tableName].Add(declaration.Name, declaration);
		}

		/// <summary>
		/// Adds a declaration.
		/// </summary>
		/// <param name="declaration">The declaration.</param>
		public void AddDeclaration(Declaration declaration)
		{
			this.AddFieldDeclaration(DeclarationsTable, declaration);
		}

		/// <summary>
		/// Gets the declarations for the given scope.
		/// </summary>
		/// <returns>
		/// An enumeration of <see cref="Declaration"/> instances.
		/// </returns>
		public IEnumerable<Declaration> GetCompleteWordDeclarations()
		{
			return tableDeclarations[DeclarationsTable].Values;
		}

		/// <summary>
		/// Gets the declarations for a MemberSelect request.
		/// </summary>
		/// <returns>
		/// An enumeration of <see cref="Declaration"/> instances.
		/// </returns>
		public IEnumerable<Declaration> GetMemberSelectDeclarations(string qualifiedName)
		{
			if (qualifiedName == null)
				throw new ArgumentNullException("qualifiedName");

			try
			{
				Declaration dec = tableDeclarations[DeclarationsTable].Values.ToList().First(declaration => declaration.Name == "something");
			}
			catch (Exception)
			{
			}
			
			// Try to resolve the qualified name starting with the global scope
			string table = ResolveQualifiedName(DeclarationsTable, qualifiedName);

			// Check that the table could be resolved and we have declarations for the table
			if (!String.IsNullOrEmpty(table) && tableDeclarations.ContainsKey(table))
				return tableDeclarations[table].Values;

			return new Declaration[0];
		}

		/// <summary>
		/// Gets the methods for a MethodTip request with the given qualified name.
		/// </summary>
		/// <param name="qualifiedName"></param>
		/// <returns></returns>
		public IEnumerable<Method> GetMethods(string qualifiedName)
		{
			if (qualifiedName == null)
				throw new ArgumentNullException("qualifiedName");

			Declaration declaration = this.ResolveDeclaration(DeclarationsTable, qualifiedName);
			if (declaration is Method)
				yield return declaration as Method;

			yield break;
		}

        /// <summary>
        /// return {namespace, method}
        /// </summary>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public IEnumerable< KeyValuePair<string, Method> > FindMethods(string methodName)
        {
            if (methodName == null)
                throw new ArgumentNullException("methodName");
            // declaration from initial table
            foreach (var tableDef in tableDeclarations)
            {
                if(tableDef.Value.ContainsKey(methodName))
                {
                    var declaration = tableDef.Value[methodName];
                    if (declaration != null)
                    {
                        if (declaration is Method)
                            yield return new KeyValuePair<string, Method>(tableDef.Key != DeclarationsTable ? tableDef.Key : "", declaration as Method);
                    }
                }
            }
            yield break;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="bHasDefinition">if true, we will only return those with definition available. </param>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<string, Declaration>> FindDeclarations(string fieldName, bool bHasDefinition = true)
        {
            if (fieldName == null)
                throw new ArgumentNullException("fieldName");
            // declaration from initial table
            foreach (var tableDef in tableDeclarations)
            {
                if (tableDef.Value.ContainsKey(fieldName))
                {
                    var declaration = tableDef.Value[fieldName];
                    if (declaration != null && (!bHasDefinition || declaration.FilenameDefinedIn!=null))
                    {
                        yield return new KeyValuePair<string, Declaration>(tableDef.Key != DeclarationsTable ? tableDef.Key : "", declaration);
                    }
                }
            }
            yield break;
        }


        /// <summary>
        /// Resolves a qualified name to a declaration.
        /// </summary>
        /// <param name="table">The table to use for resolving.</param>
        /// <param name="qualifiedName">The qualified name of the declaration.</param>
        /// <returns></returns>
        public Declaration ResolveDeclaration(string table, string qualifiedName)
		{
			if (table == null)
				throw new ArgumentNullException("table");
			if (qualifiedName == null)
				throw new ArgumentNullException("qualifiedName");

			// Find the last delimiter, if any
			int lastDelimiterIndex = qualifiedName.LastIndexOfAny(delimiters);

			if (lastDelimiterIndex == -1)
			{
				// No delimiters, return declaration from initial table
				if (tableDeclarations[table] != null && tableDeclarations[table].ContainsKey(qualifiedName))
					return tableDeclarations[table][qualifiedName];
			}
			else
			{
				string qualifiedTableName = qualifiedName.Substring(0, lastDelimiterIndex);
				string declarationName = qualifiedName.Substring(lastDelimiterIndex + 1);

				// Try to resolve the table
				string tableName = this.ResolveQualifiedName(table, qualifiedTableName);

				if (tableName != null && tableDeclarations[tableName] != null && tableDeclarations[tableName].ContainsKey(declarationName))
					return tableDeclarations[tableName][declarationName];
			}

			return null;
		}

		/// <summary>
		/// Resolves a qualified name's type to a table.
		/// </summary>
		/// <param name="table">The table to use for resolving.</param>
		/// <param name="qualifiedName">The qualified name.</param>
		/// <returns></returns>
		public string ResolveQualifiedName(string table, string qualifiedName, bool addMissing)
		{
			if (table == null)
				throw new ArgumentNullException("table");
			if (qualifiedName == null)
				throw new ArgumentNullException("qualifiedName");

			// Find the first delimiter, if any, 
            int delimiterIndex = qualifiedName.LastIndexOfAny(delimiters);

			// Take the first part of the qualified name
			string name = delimiterIndex != -1 ? qualifiedName.Substring(0, delimiterIndex) : qualifiedName;

			Dictionary<string, Declaration> declarations = null;

			// Check whether we already have declarations for the table
			if (tableDeclarations.ContainsKey(table))
				declarations = tableDeclarations[table];

			// If we have no declarations but we are allowed to add missing declarations, add the dictionary
			if (addMissing && declarations == null)
				declarations = tableDeclarations[table] = new Dictionary<string, Declaration>();

			if (declarations != null)
			{
				Declaration declaration = null;

				// Check whether we already have a declaration for the name
				if (tableDeclarations[table].ContainsKey(name))
					declaration = tableDeclarations[table][name];

				// If there are no declarations yet, or the declaration is not a table and we are allowed to add missing ones, go ahead
				if (addMissing && (declaration == null || declaration.DeclarationType != DeclarationType.Table))
				{
					// Remove existing declaration, if there was one
					if (declaration != null)
						tableDeclarations[table].Remove(name);

					// Create new declaration
					declaration = new Declaration
					{
						DeclarationType = DeclarationType.Table,
						Name = name,
						Type = "~Table_" + Guid.NewGuid()
					};

					// Add the new declaration
					tableDeclarations[table].Add(name, declaration);
				}

				// Check that we have a declaration and the declaration has a type
				if (declaration != null && declaration.Type != null)
				{
					// We found the declaration that denotes the type, we either keep resolving or return the result
					if (delimiterIndex == -1)
						return declaration.Type;

					return ResolveQualifiedName(declaration.Type, qualifiedName.Substring(delimiterIndex + 1), addMissing);
				}
			}

			return null;
		}

		/// <summary>
		/// Resolves the name of the qualified.
		/// </summary>
		/// <param name="table">The table.</param>
		/// <param name="qualifiedName">Name of the qualified.</param>
		/// <returns></returns>
		public string ResolveQualifiedName(string table, string qualifiedName)
		{
			return ResolveQualifiedName(table, qualifiedName, false);
		}
	}
}
