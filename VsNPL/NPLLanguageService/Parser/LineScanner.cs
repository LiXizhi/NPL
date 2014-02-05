/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using Microsoft.VisualStudio.Package;
using ParaEngine.Tools.Lua.Parser;

namespace ParaEngine.Tools.Lua.Parser
{
    /// <summary>
    /// LineScanner wraps the GPLEX scanner to provide the IScanner interface
    /// required by the Managed Package Framework. This includes mapping tokens
    /// to color definitions.
    /// </summary>
    public class LineScanner : IScanner
    {
    	readonly IColorScan lex;

		/// <summary>
		/// Initializes a new instance of the <see cref="LineScanner"/> class.
		/// </summary>
        public LineScanner()
        {
            this.lex = new ParaEngine.Tools.Lua.Lexer.Scanner();
        }

		/// <summary>
		/// Scans the token and provide info about it.
		/// </summary>
		/// <param name="tokenInfo">The token info.</param>
		/// <param name="state">The state.</param>
		/// <returns></returns>
        public bool ScanTokenAndProvideInfoAboutIt(TokenInfo tokenInfo, ref int state)
        {
            int start, end;
            int token = lex.GetNext(ref state, out start, out end);

            // !EOL and !EOF
            if (token != (int)Tokens.EOF)
            {
                Configuration.TokenDefinition definition = Configuration.GetDefinition(token);
                tokenInfo.StartIndex = start;
                tokenInfo.EndIndex = end;
                tokenInfo.Color = definition.TokenColor;
                tokenInfo.Type = definition.TokenType;
                tokenInfo.Trigger = definition.TokenTriggers;

                return true;
            }

			return false;
        }

		/// <summary>
		/// Sets the source.
		/// </summary>
		/// <param name="source">The source.</param>
		/// <param name="offset">The offset.</param>
        public void SetSource(string source, int offset)
        {
            lex.SetSource(source, offset);
        }
    }
}