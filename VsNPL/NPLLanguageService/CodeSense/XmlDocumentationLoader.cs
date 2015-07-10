using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Text.RegularExpressions;


namespace ParaEngine.Tools.Lua
{
    /// <summary>
    /// Loads declarations and documentation from XML files.
    /// </summary>
    public class XmlDocumentationLoader
    {
        private readonly List<XElement> docs = new List<XElement>();
        private HashSet<String> loadedDocsFileName = new HashSet<string>();
        /// <summary>
        /// Loads the XML from the given path and adds the declarations to the declaration CodeSense provider.
        /// </summary>
        /// <param name="path"></param>
        public void LoadXml(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            string filename = System.IO.Path.GetFileName(path);
            if(!loadedDocsFileName.Contains(filename))
            {
                loadedDocsFileName.Add(filename);
                // Load the XML document
                XElement doc = ValidateAndLoadXmlDocument(path);

                // Add the XML document to the list of documents
                docs.Add(doc);
            }
        }

        /// <summary>
        /// Adds the declarations from the documentation to a TableDeclarationProvider.
        /// </summary>
        /// <param name="tableDeclarationProvider"></param>
        public void AddDeclarations(TableDeclarationProvider tableDeclarationProvider)
        {
            if (tableDeclarationProvider == null)
                throw new ArgumentNullException("tableDeclarationProvider");

            foreach (XElement doc in docs)
            {
                // Query the documentation for tables and add them
                doc.XPathSelectElements("./tables/table").ForEach(table => this.AddTableDeclaration(tableDeclarationProvider, doc, table));

                if (doc.Element("globals") != null)
                {
                    // Query the documentation for global declarations and add them to the global table
                    doc.Element("globals").Elements().ForEach(element => AddDeclaration(tableDeclarationProvider, TableDeclarationProvider.DeclarationsTable, element));
                }
            }
        }

        private void AddTableDeclaration(TableDeclarationProvider tableDeclarationProvider, XElement doc, XElement table)
        {
			//Add a declaration to the global list
			var name = (string)table.Attribute("name");
			XElement variableElement = doc.XPathSelectElement(String.Format("./variables/variable[@name='{0}']", name));

            /// Added by LiXizhi. 2008.10.21. we now support namespace (ns) attribute to variables, so that a table can reside in a nested namespace. such as 
            /// <variable name="Class1" type="Class1" ns="MyCompany.MyProject.Class1"/>
            String nameSpace = (String)variableElement.Attribute("ns");
            bool bSkipRootDeclaration = false;
            if (!String.IsNullOrEmpty(nameSpace))
            {
                if(nameSpace.IndexOf('.')>0)
                {
                    bSkipRootDeclaration = true;
                    String LastTableName=null;
                    foreach (Match tableField in Regex.Matches(nameSpace, @"\w+"))
                    {
                        if(tableField.Value!=null)
                        {
                            if(LastTableName==null)
                            {
                                LastTableName = tableField.Value;
                                var d = new Declaration
                                {
                                    Name = tableField.Value,
                                    DeclarationType = DeclarationType.Table,
                                    Description = string.Empty,
                                    Type = tableField.Value,
                                };
                                tableDeclarationProvider.AddDeclaration(d);
                            }
                            else
                            {
                                var d = new Declaration
                                {
                                    Name = tableField.Value,
                                    DeclarationType = DeclarationType.Table,
                                    Description = string.Empty,
                                    Type = tableField.Value,
                                };
                                tableDeclarationProvider.AddFieldDeclaration(LastTableName, d);
                                LastTableName = tableField.Value;
                            }
                        }
                    }
                    if(LastTableName != name)
                    {
                        var d = new Declaration
                        {
                            Name = name,
                            DeclarationType = DeclarationType.Table,
                            Description = string.Empty,
                            Type = name,
                        };
                        tableDeclarationProvider.AddFieldDeclaration(LastTableName, d);
                    }
                }
            }

            if (!bSkipRootDeclaration)
            {
                var d = new Declaration
                {
                    Name = name,
                    DeclarationType = DeclarationType.Table,
                    Description = string.Empty,
                    Type = variableElement.Attribute("type").Value
                };

                tableDeclarationProvider.AddDeclaration(d);
            }

            // Check whether the table inherits declarations from another table
            XAttribute inherits = table.Attribute("inherits");
            if (inherits != null)
            {
                // inherits is a comma-delimited list of tables that this table inherits from
                string[] inheritValues = inherits.Value.Split(',');

                foreach (string inheritValue in inheritValues)
                {
                    // Query the table that the declarations should be inherited from
                    XElement baseTable = doc.XPathSelectElement(String.Format("./tables/table[@name='{0}']", inheritValue.Trim()));

                    // If the table was found, add each declaration from the base table
                    if (baseTable != null)
                        baseTable.Elements().ForEach(element => AddDeclaration(tableDeclarationProvider, name, element));
                }
            }

            // Go through all declarations and add them to the table
            table.Elements().ForEach(element => AddDeclaration(tableDeclarationProvider, name, element));
        }

		/// <summary>
		/// Adds the declaration.
		/// </summary>
		/// <param name="tableDeclarationProvider">The table declaration provider.</param>
		/// <param name="tableName">Name of the table.</param>
		/// <param name="element">The element.</param>
        private static void AddDeclaration(TableDeclarationProvider tableDeclarationProvider, string tableName, XElement element)
        {
            try
            {
                Declaration declaration = XmlDocumentationLoader.CreateDeclaration(element);

                // Add the declaration 
                if (declaration != null)
                    tableDeclarationProvider.AddFieldDeclaration(tableName, declaration);
            }
            catch (ArgumentException)
            {
                // Enum.Parse could not parse the declaration type, there's nothing we can do.
            }
        }

		/// <summary>
		/// Creates the declaration.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <returns></returns>
        private static Declaration CreateDeclaration(XElement element)
        {
            try
            {
                // Get the summary of the declaration, if available
                string summary = element.Element("summary") != null ? element.Element("summary").Value.Trim() : null;

                // Get the type of the declared variable or field, if available
                string type = element.Attribute("type") != null ? element.Attribute("type").Value : null;

                var declarationType = (DeclarationType)Enum.Parse(typeof(DeclarationType), element.Name.LocalName, true);

                if (declarationType == DeclarationType.Function)
                {
                    Parameter[] parameters = element.Elements("parameter").Select(parameter => new Parameter
                                                                                           {
                                                                                               DeclarationType = DeclarationType.Parameter,
                                                                                               Name = parameter.Attribute("name").Value,
                                                                                               Type = parameter.Element("type") != null ? parameter.Element("type").Value.Trim() : null,
                                                                                               Description = String.Format("{0}-{1}", parameter.Attribute("name").Value, parameter.Value),
                                                                                               Optional = parameter.Attribute("optional") != null ? Boolean.Parse(parameter.Attribute("optional").Value) : false
                                                                                           })
                                                                      .ToArray();
                    return new Method
                    {
                        Name = (string)element.Attribute("name"),
                        DeclarationType = declarationType,
                        Description = summary,
                        Type = type,
                        Parameters = parameters 
                    };
                }

            	// Create a declaration for the element
            	return new Declaration
            	       	{
            	       		Name = (string)element.Attribute("name"),
            	       		DeclarationType = declarationType,
            	       		Description = summary,
            	       		Type = type
            	       	};
            }
            catch (ArgumentException)
            {
                // Enum.Parse could not parse the declaration type, there's nothing we can do.
                return null;
            }
        }

		/// <summary>
		/// Validates the and load XML document.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns></returns>
        private static XElement ValidateAndLoadXmlDocument(string path)
        {
            var validator = new XmlValidator();
            using (StreamReader sr=File.OpenText(path))
            {
                string xmlContent = sr.ReadToEnd();
                using(Stream schemaStream=typeof (XmlDocumentationLoader).Assembly.GetManifestResourceStream(
                    "ParaEngine.NPLLanguageService.Documentation.LuaDoc.xsd"))
                {
                	if (validator.Validate(xmlContent, schemaStream))
                		return XElement.Load(path);
                	
					throw new ApplicationException(validator.ErrorMessage);
                }
            }
        }
    }
}
