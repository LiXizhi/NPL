using System;
using System.Text;

namespace ParaEngine.Tools.Lua
{
	public class Parameter : Declaration
	{
		/// <summary>
		/// Gets or sets whether the parameter is optional.
		/// </summary>
		public bool Optional { get; set; }

		/// <summary>
		/// Gets the text that should be displayed for the parameter.
		/// </summary>
		public string Display
		{
			get
			{
				var sb = new StringBuilder();

				if (!String.IsNullOrEmpty(this.Type))
					sb.Append(this.Type + " ");

				sb.Append(this.Name);


				if (Optional)
					sb.Append(" (optional)");

				return sb.ToString();
			}
		}
	}
}