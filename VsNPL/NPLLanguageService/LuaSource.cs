using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using ParaEngine.Tools.Lua.Parser;
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
    }
}
