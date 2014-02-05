/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System.Collections.Generic;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Package;
using ParaEngine.Tools.Lua.Parser;
using Microsoft;
namespace ParaEngine.Tools.Lua.Parser
{
	/// <summary>
	/// 
	/// </summary>
    public class Source : Microsoft.VisualStudio.Package.Source
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="Source"/> class.
		/// </summary>
		/// <param name="service">The service.</param>
		/// <param name="textLines">The text lines.</param>
		/// <param name="colorizer">The colorizer.</param>
        public Source(BaseLanguageService service, IVsTextLines textLines, Colorizer colorizer)
            : base(service, textLines, colorizer)
        {
        }

		/// <summary>
		/// Gets or sets the parse result.
		/// </summary>
		/// <value>The parse result.</value>
		public object ParseResult { get; set; }

		/// <summary>
		/// Gets or sets the braces.
		/// </summary>
		/// <value>The braces.</value>
		public IList<TextSpan[]> Braces { get; set; }

		/// <summary>
		/// Gets information on what defines a comment in the language.
		/// </summary>
		/// <returns>
		/// A <see cref="T:Microsoft.VisualStudio.Package.CommentInfo"/> structure containing the strings that define a comment.
		/// </returns>
		public override CommentInfo GetCommentFormat()
        {
            return Configuration.MyCommentInfo;
        }
    }
}