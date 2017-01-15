using System;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using System.Collections.Generic;

namespace ParaEngine.Tools.Lua
{
    internal class NPLFormatHelper
    {
        /// <summary>
        /// @see also: https://github.com/Trass3r/AsmHighlighter/blob/master/AsmHighlighter/AsmHighlighterFormatHelper.cs
        /// @see also: https://github.com/samizzo/nshader/blob/master/NShaderVS/NShaderFormatHelper.cs
        /// https://msdn.microsoft.com/en-us/library/bb164633.aspx
        /// </summary>
        /// <param name="pBuffer"></param>
        /// <param name="span"></param>
        /// <param name="tabSize"></param>
        /// <returns></returns>
        internal static List<EditSpan> ReformatCode(IVsTextLines pBuffer, int[] indents, TextSpan span, int tabSize)
        {
            List<EditSpan> changeList = new List<EditSpan>();
            string line = "";
            for (int i = span.iStartLine; i <= span.iEndLine; ++i)
            {
                TextSpan editTextSpan = new TextSpan();
                editTextSpan.iStartLine = i;
                editTextSpan.iEndLine = i;
                editTextSpan.iStartIndex = 0;

                int startIndex = 0;
                int endIndex = 0;
                pBuffer.GetLengthOfLine(i, out endIndex);
                editTextSpan.iEndIndex = endIndex;
                pBuffer.GetLineText(i, startIndex, i, endIndex, out line);

                string pat = @"^[' '\t]*";
                Regex regex = new Regex(pat);
                Match m = regex.Match(line);
                if (m.Success)
                {
                    Capture c = m.Groups[0].Captures[0];
                    line = line.Substring(c.Index + c.Length);
                }

                for (int j = 0; j < tabSize * indents[i]; ++j)
                    line = " " + line;

                changeList.Add(new EditSpan(editTextSpan, line));
            }

            return changeList;
        }
    }
}