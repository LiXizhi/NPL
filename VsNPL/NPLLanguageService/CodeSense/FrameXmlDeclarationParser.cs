using System;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ParaEngine.Tools.Lua
{
    public class FrameXmlDeclarationParser
    {
        private const string parentToken = "$parent";

        private readonly TableDeclarationProvider declarationCodeSenseProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlDocumentationLoader"/> class.
        /// </summary>
        /// <param name="declarationCodeSenseProvider">The declaration CodeSense provider to add the declarations to.</param>
        public FrameXmlDeclarationParser(TableDeclarationProvider declarationCodeSenseProvider)
        {
            if (declarationCodeSenseProvider == null)
                throw new ArgumentNullException("declarationCodeSenseProvider");

            this.declarationCodeSenseProvider = declarationCodeSenseProvider;
        }

		/// <summary>
		/// Adds the frame XML.
		/// </summary>
		/// <param name="path">The path.</param>
        public void AddFrameXml(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            // Load the XML from the given path
            XElement ui = XElement.Load(path);

            // Add the Frame XML
            this.AddFrameXml(ui);
        }

		/// <summary>
		/// Adds the frame XML text.
		/// </summary>
		/// <param name="text">The text.</param>
        public void AddFrameXmlText(string text)
        {
            if (text == null)
                throw new ArgumentNullException("text");

            // Parse the XML text
            XElement ui = XElement.Parse(text);

            // Add the Frame XML
            this.AddFrameXml(ui);
        }

		/// <summary>
		/// Adds the frame XML.
		/// </summary>
		/// <param name="ui">The UI.</param>
        public void AddFrameXml(XElement ui)
        {
            if (ui == null)
                throw new ArgumentNullException("ui");

            // Select all elements in the FrameXML that has a name
            ui.XPathSelectElements("/descendant::*[@name]").ForEach(element => this.AddFrameXmlElement(element));
        }

		/// <summary>
		/// Adds the frame XML element.
		/// </summary>
		/// <param name="element">The element.</param>
        private void AddFrameXmlElement(XElement element)
        {
            // Get type and name from th element
            string type = element.Name.LocalName;
            string name = GetName(element);

            if (name != null)
            {
                Declaration declaration = new Declaration
                                            {
                                                DeclarationType = DeclarationType.Table,
                                                Name = name,
                                                Type = type
                                            };

                declarationCodeSenseProvider.AddDeclaration(declaration);
            }
        }

		/// <summary>
		/// Gets the name.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <returns></returns>
        private static string GetName(XElement element)
        {
            string name = element.Attribute("name").Value;

            if (name.Contains(parentToken))
            {
                // Iterate through the parent chain until we find an element with a name property
                XElement parent = element.Parent;
                while (parent != null && parent.Attribute("name") == null)
                    parent = parent.Parent;

                if (parent == null)
                    return null;

                name = name.Replace(parentToken, parent.Attribute("name").Value);
            }

            return name;
        }
    }
}
