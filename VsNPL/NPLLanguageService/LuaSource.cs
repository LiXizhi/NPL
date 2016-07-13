using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using ParaEngine.Tools.Lua.Parser;
using System;
using System.Collections.Generic;
using Source = ParaEngine.Tools.Lua.Parser.Source;

namespace ParaEngine.Tools.Lua
{
	/// <summary>
	/// 
	/// </summary>
    public class LuaSource : Source
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="LuaSource"/> class.
		/// </summary>
		/// <param name="service">The service.</param>
		/// <param name="textLines">The text lines.</param>
		/// <param name="colorizer">The colorizer.</param>
        public LuaSource(BaseLanguageService service, IVsTextLines textLines, Colorizer colorizer)
			: base(service, textLines, colorizer)
		{
		}

        public override void OnIdle(bool periodic)
        {
            // LiXizhi: fixed parserequst.Check not called when file is first loaded. 
            // We're not yet doing an explicit first parse and the MPF assumes that we are. 
            if (this.LastParseTime == Int32.MaxValue)
                this.LastParseTime = this.LanguageService.Preferences.CodeSenseDelay;

            base.OnIdle(periodic);
        }

        public override void OnCommand(IVsTextView textView, VSConstants.VSStd2KCmdID command, char ch)
        {
            base.OnCommand(textView, command, ch);
            //if (command == VSConstants.VSStd2KCmdID.ECMD_RENAMESYMBOL)
            //{
            //    System.Diagnostics.Debug.WriteLine(command);
            //}
/*
            if (command == VSConstants.VSStd2KCmdID.TYPECHAR)
            {
                int line, col;
                textView.GetCaretPos(out line, out col);

                TokenInfo tokenInfo = this.GetTokenInfo(line, col);

                if (tokenInfo.Type == TokenType.Identifier && !this.IsCompletorActive)
                    this.Completion(textView, tokenInfo, ParseReason.CompleteWord);
            }
*/
        }

        public override void ReformatSpan(EditArray mgr, TextSpan span)
        {
            string description = "Reformat code";
            CompoundAction ca = new CompoundAction(this, description);
            using (ca)
            {
                ca.FlushEditActions();      // Flush any pending edits
                DoFormatting(mgr, span);    // Format the span
            }
        }

        private void DoFormatting(EditArray mgr, TextSpan span)
        {
            // Make sure there is one space after every comma unless followed
            // by a tab or comma is at end of line.
            IVsTextLines pBuffer = GetTextLines();
            if (pBuffer != null)
            {
                List<EditSpan> changeList = NPLFormatHelper.ReformatCode(pBuffer, span, LanguageService.GetLanguagePreferences().TabSize);
                if (changeList != null)
                {
                    foreach (EditSpan editSpan in changeList)
                    {
                        // Add edit operation
                        mgr.Add(editSpan);
                    }
                }
            }
        }
    }
}
