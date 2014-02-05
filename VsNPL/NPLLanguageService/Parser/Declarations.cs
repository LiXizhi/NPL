/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System.Collections.Generic;
using ParaEngine.Tools.Lua.Parser;
using Microsoft;
namespace ParaEngine.Tools.Lua.Parser
{
    public class Declarations : Microsoft.VisualStudio.Package.Declarations
    {
    	readonly IList<Declaration> declarations;

		/// <summary>
		/// Initializes a new instance of the <see cref="Declarations"/> class.
		/// </summary>
		/// <param name="declarations">The declarations.</param>
        public Declarations(IList<Declaration> declarations)
        {
            this.declarations = declarations;
        }

		/// <summary>
		/// When implemented in a derived class, gets the number of items in the list of declarations.
		/// </summary>
		/// <returns>
		/// The count of items represented by this <see cref="T:Microsoft.VisualStudio.Package.Declarations"/> class.
		/// </returns>
        public override int GetCount()
        {
            return declarations.Count;
        }

		/// <summary>
		/// When implemented in a derived class, gets a description of the specified item.
		/// </summary>
		/// <param name="index">[in] The index of the item for which to get the description.</param>
		/// <returns>
		/// If successful, returns the description; otherwise, returns null.
		/// </returns>
        public override string GetDescription(int index)
        {
            return declarations[index].Description;
        }

		/// <summary>
		/// When implemented in a derived class, gets the text to be displayed in the completion list for the specified item.
		/// </summary>
		/// <param name="index">[in] The index of the item for which to get the display text.</param>
		/// <returns>
		/// The text to be displayed, otherwise null.
		/// </returns>
        public override string GetDisplayText(int index)
        {
            return declarations[index].DisplayText;
        }

		/// <summary>
		/// When implemented in a derived class, gets the image to show next to the specified item.
		/// </summary>
		/// <param name="index">[in] The index of the item for which to get the image index.</param>
		/// <returns>
		/// The index of the image from an image list, otherwise -1.
		/// </returns>
        public override int GetGlyph(int index)
        {
            return declarations[index].Glyph;
        }

		/// <summary>
		/// When implemented in a derived class, gets the name or text to be inserted for the specified item.
		/// </summary>
		/// <param name="index">[in] The index of the item for which to get the name.</param>
		/// <returns>
		/// If successful, returns the name of the item; otherwise, returns null.
		/// </returns>
        public override string GetName(int index)
        {
            if (index >= 0)
                return declarations[index].Name;

            return null;
        }
    }
}