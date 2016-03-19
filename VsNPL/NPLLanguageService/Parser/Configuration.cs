using System.Collections.Generic;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Package;

// for custom colors: 
//  references: VisualStudio.Text.[Data|Logic|UI.WPF], VisualStudio.CoreUtility, System.ComponentModel.Composition
//using System.ComponentModel.Composition;
//using Microsoft.VisualStudio.Text.Classification;
//using System.Windows.Media;
//using Microsoft.VisualStudio.Utilities;

namespace ParaEngine.Tools.Lua.Parser
{
    /// <summary>
    /// 
    /// </summary>
    public static class Configuration
    {

        //[Export(typeof(EditorFormatDefinition))]
        //[ClassificationType(ClassificationTypeNames = "NPL.GreyText")]
        //[Name("NPL.GreyText")]
        //internal sealed class GreyTextColorDefinition : ClassificationFormatDefinition
        //{
        //    public GreyTextColorDefinition()
        //    {
        //        this.ForegroundColor = Colors.Gray;
        //    }
        //}


        static readonly List<IVsColorableItem> colorableItems = new List<Microsoft.VisualStudio.TextManager.Interop.IVsColorableItem>();
        
		/// <summary>
		/// Gets the colorable items.
		/// </summary>
		/// <value>The colorable items.</value>
		public static IList<IVsColorableItem> ColorableItems
        {
            get { return colorableItems; }
        }

		/// <summary>
		/// Creates the color.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="foreground">The foreground.</param>
		/// <param name="background">The background.</param>
		/// <returns></returns>
        public static TokenColor CreateColor(string name, COLORINDEX foreground, COLORINDEX background)
        {
            return CreateColor(name, foreground, background, false, false);
        }

		/// <summary>
		/// Creates the color.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="foreground">The foreground.</param>
		/// <param name="background">The background.</param>
		/// <param name="bold">if set to <c>true</c> [bold].</param>
		/// <param name="strikethrough">if set to <c>true</c> [strikethrough].</param>
		/// <returns></returns>
        public static TokenColor CreateColor(string name, COLORINDEX foreground, COLORINDEX background, bool bold, bool strikethrough)
        {
            colorableItems.Add(new ColorableItem(name, foreground, background, bold, strikethrough));
            return (TokenColor) colorableItems.Count;
        }

		/// <summary>
		/// Colors the token.
		/// </summary>
		/// <param name="token">The token.</param>
		/// <param name="type">The type.</param>
		/// <param name="color">The color.</param>
		/// <param name="trigger">The trigger.</param>
        public static void ColorToken(int token, TokenType type, TokenColor color, TokenTriggers trigger)
        {
            definitions[token] = new TokenDefinition(type, color, trigger);
        }

		/// <summary>
		/// Gets the definition.
		/// </summary>
		/// <param name="token">The token.</param>
		/// <returns></returns>
        public static TokenDefinition GetDefinition(int token)
        {
            TokenDefinition result;
            return definitions.TryGetValue(token, out result) ? result : defaultDefinition;
        }

        private static readonly TokenDefinition defaultDefinition = new TokenDefinition(TokenType.Text, TokenColor.Text, TokenTriggers.None);
        private static readonly Dictionary<int, TokenDefinition> definitions = new Dictionary<int, TokenDefinition>();

        public struct TokenDefinition
        {
            public TokenDefinition(TokenType type, TokenColor color, TokenTriggers triggers)
            {
                this.TokenType = type;
                this.TokenColor = color;
                this.TokenTriggers = triggers;
            }

            public TokenType TokenType;
            public TokenColor TokenColor;
            public TokenTriggers TokenTriggers;
        }

        public const string Name = "Lua";
        public const string Extension = ".lua";

        static readonly CommentInfo commentInfo;
        public static CommentInfo MyCommentInfo
        {
            get
            {
                return commentInfo;
            }
        }

		/// <summary>
		/// Initializes the <see cref="Configuration"/> class.
		/// </summary>
        static Configuration()
        {
            commentInfo.BlockStart = "--[[";
            commentInfo.BlockEnd = "]]--";
            commentInfo.LineStart = "--";
            commentInfo.UseLineComments = true;

            // default colors - currently, these need to be declared
            CreateColor("Keyword", COLORINDEX.CI_BLUE, COLORINDEX.CI_USERTEXT_BK);
            CreateColor("Comment", COLORINDEX.CI_DARKGREEN, COLORINDEX.CI_USERTEXT_BK);
            CreateColor("Identifier", COLORINDEX.CI_SYSPLAINTEXT_FG, COLORINDEX.CI_USERTEXT_BK);
            CreateColor("String", COLORINDEX.CI_RED, COLORINDEX.CI_USERTEXT_BK);
            CreateColor("Number", COLORINDEX.CI_SYSPLAINTEXT_FG, COLORINDEX.CI_USERTEXT_BK);
            CreateColor("Text", COLORINDEX.CI_SYSPLAINTEXT_FG, COLORINDEX.CI_USERTEXT_BK);

            // custom colors: 
            TokenColor error = CreateColor("NPL.Error", COLORINDEX.CI_RED, COLORINDEX.CI_USERTEXT_BK, false, false);
            TokenColor NPLMarker = CreateColor("NPL.Marker", COLORINDEX.CI_PURPLE, COLORINDEX.CI_SYSWIDGETMGN_BK, true, false);
            TokenColor NPLGreyText = CreateColor("NPL.GreyText", COLORINDEX.CI_DARKGRAY, COLORINDEX.CI_USERTEXT_BK, false, false);
            TokenColor NPLGreyBoldText= CreateColor("NPL.NPLGreyBoldText", COLORINDEX.CI_DARKGRAY, COLORINDEX.CI_USERTEXT_BK, true, false);

            ColorToken((int)Tokens.KWAND, TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);
            ColorToken((int)Tokens.KWBREAK, TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);
            ColorToken((int)Tokens.KWDO, TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);
            ColorToken((int)Tokens.KWEND, TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);
            ColorToken((int)Tokens.KWELSE, TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);
            ColorToken((int)Tokens.KWELSEIF, TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);
            ColorToken((int)Tokens.KWFOR, TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);
            ColorToken((int)Tokens.KWFALSE, TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);
            ColorToken((int)Tokens.KWFUNCTION, TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);
            ColorToken((int)Tokens.KWIF, TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);
            ColorToken((int)Tokens.KWIN, TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);
            ColorToken((int)Tokens.KWLOCAL, TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);
            ColorToken((int)Tokens.KWNIL, TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);
            ColorToken((int)Tokens.KWNOT, TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);
            ColorToken((int)Tokens.KWOR, TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);
            ColorToken((int)Tokens.KWREPEAT, TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);
            ColorToken((int)Tokens.KWRETURN, TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);
            ColorToken((int)Tokens.KWTHEN, TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);
            ColorToken((int)Tokens.KWTRUE, TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);
            ColorToken((int)Tokens.KWUNTIL, TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);
            ColorToken((int)Tokens.KWWHILE, TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);

            ColorToken((int)Tokens.IDENTIFIER, TokenType.Identifier, TokenColor.Identifier, TokenTriggers.None);
            ColorToken((int)Tokens.NUMBER, TokenType.Literal, TokenColor.Number, TokenTriggers.None);
            ColorToken((int)Tokens.STRING, TokenType.String, TokenColor.String, TokenTriggers.None);

            ColorToken((int)Tokens.LPAREN, TokenType.Delimiter, TokenColor.Text, TokenTriggers.MatchBraces | TokenTriggers.ParameterStart);
            ColorToken((int)Tokens.RPAREN, TokenType.Delimiter, TokenColor.Text, TokenTriggers.MatchBraces | TokenTriggers.ParameterEnd);
            ColorToken((int)Tokens.LBRACE, TokenType.Delimiter, TokenColor.Text, TokenTriggers.MatchBraces);
            ColorToken((int)Tokens.RBRACE, TokenType.Delimiter, TokenColor.Text, TokenTriggers.MatchBraces);

            ColorToken((int)Tokens.COMMA, TokenType.Delimiter, TokenColor.Text, TokenTriggers.ParameterNext);

            ColorToken((int)Tokens.DOT, TokenType.Delimiter, TokenColor.Text, TokenTriggers.MemberSelect);
            ColorToken((int)Tokens.COLON, TokenType.Delimiter, TokenColor.Text, TokenTriggers.MemberSelect);
            ColorToken((int)Tokens.LBRACKET, TokenType.Delimiter, TokenColor.Text, TokenTriggers.MemberSelect | TokenTriggers.MatchBraces);

            // Extra token values internal to the scanner
            ColorToken((int)Tokens.LEX_ERROR, TokenType.Text, error, TokenTriggers.None);
            ColorToken((int)Tokens.LEX_COMMENT, TokenType.Text, TokenColor.Comment, TokenTriggers.None);

            // NPL, mixed html/npl page file related tokens
            // ColorToken((int)NPLTokens.LEX_COMMENT_LIGHT, TokenType.Comment, TokenColor.String, TokenTriggers.None);
            ColorToken((int)NPLTokens.LEX_COMMENT_LIGHT, TokenType.Comment, NPLGreyText, TokenTriggers.None);
            ColorToken((int)NPLTokens.LEX_NPL_BEGINCODE, TokenType.Comment, NPLMarker, TokenTriggers.None);
            ColorToken((int)NPLTokens.LEX_NPL_ENDCODE, TokenType.Comment, NPLMarker, TokenTriggers.None);
            ColorToken((int)NPLTokens.LEX_NPL_HTML_ATTR_VALUE, TokenType.Comment, NPLGreyBoldText, TokenTriggers.None);

        }
    }
}