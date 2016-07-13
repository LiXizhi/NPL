using System;
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
        internal static List<EditSpan> ReformatCode(IVsTextLines pBuffer, TextSpan span, int tabSize)
        {
            return null;
        }
    }
}