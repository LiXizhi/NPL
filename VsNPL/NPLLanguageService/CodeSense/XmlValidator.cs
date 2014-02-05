using System.IO;
using System.Xml;
using System.Xml.Schema;

namespace ParaEngine.Tools.Lua
{
	/// <summary>
	/// Validates XML instance against an XSD definition.
	/// </summary>
	public class XmlValidator
	{
		#region Private members

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="XmlValidator"/> class.
		/// </summary>
		public XmlValidator()
		{
			ErrorsCount = 0;
			ErrorMessage = "";
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets number of errors during validation.
		/// </summary>
		public int ErrorsCount { get; private set; }

		/// <summary>
		/// Gets error message of validation.
		/// </summary>
		public string ErrorMessage { get; private set; }

		#endregion

		#region Public member functions

		/// <summary>
		/// Validates the specified XML document.
		/// </summary>
		/// <param name="xmlDocument">The XML document.</param>
		/// <param name="schema">The schema.</param>
		/// <returns></returns>
		public bool Validate(string xmlDocument, Stream schema)
		{
			bool result = true;

			ErrorsCount = 0;
			ErrorMessage = "";

			var validationSettings = new XmlReaderSettings();
			validationSettings.ValidationEventHandler += ValidationHandler;

			validationSettings.ValidationType = ValidationType.Schema;
			validationSettings.Schemas.Add(null, XmlReader.Create(schema));

			XmlReader validatingReader = XmlReader.Create(new StringReader(xmlDocument), validationSettings);

			while (validatingReader.Read())
			{
			}

			if (ErrorsCount > 0)
			{
				result = false;
			}

			return result;
		}

		#endregion

		#region Private member functions

		/// <summary>
		/// Validations the handler.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="args">The <see cref="System.Xml.Schema.ValidationEventArgs"/> instance containing the event data.</param>
		private void ValidationHandler(object sender,
		                               ValidationEventArgs args)
		{
			ErrorMessage = ErrorMessage + args.Message + "\r\n";
			ErrorsCount++;
		}

		#endregion
	}
}