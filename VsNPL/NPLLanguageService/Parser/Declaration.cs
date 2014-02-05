/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Collections.Generic;

namespace ParaEngine.Tools.Lua.Parser
{
    public struct Declaration
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="Declaration"/> struct.
		/// </summary>
		/// <param name="description">The description.</param>
		/// <param name="displayText">The display text.</param>
		/// <param name="glyph">The glyph.</param>
		/// <param name="name">The name.</param>
        public Declaration(string description, string displayText, int glyph, string name)
        {
            this.Description = description;
            this.DisplayText = displayText;
            this.Glyph = glyph;
            this.Name = name;
        }

        public string Description;
        public string DisplayText;
        public int Glyph;
        public string Name;
    }
}