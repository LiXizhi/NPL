using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using ParaEngine.Tools.Lua.Parser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Source = ParaEngine.Tools.Lua.Parser.Source;
using LuaParser = ParaEngine.Tools.Lua.Parser.Parser;
using ParaEngine.Tools.Lua.AST;

namespace ParaEngine.Tools.Lua
{
	/// <summary>
	/// 
	/// </summary>
    public class LuaSource : Source
    {
        private int[] indents;
        private bool[] comments;
        private bool[] longStrings;
        /// <summary>
		/// Initializes a new instance of the <see cref="LuaSource"/> class.
		/// </summary>
		/// <param name="service">The service.</param>
		/// <param name="textLines">The text lines.</param>
		/// <param name="colorizer">The colorizer.</param>
        public LuaSource(BaseLanguageService service, IVsTextLines textLines, Colorizer colorizer)
			: base(service, textLines, colorizer)
		{
            //IndentInitialize();
		}

        public override void OnIdle(bool periodic)
        {
            // LiXizhi: fixed parserequst.Check not called when file is first loaded. 
            // We're not yet doing an explicit first parse and the MPF assumes that we are. 
            if (this.LastParseTime == Int32.MaxValue)
                this.LastParseTime = this.LanguageService.Preferences.CodeSenseDelay;

            // Zhiyuan: update the indents when content of text changed
            //if (IsDirty)
                //IndentInitialize();

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

        private void IndentInitialize()
        {
            int line = GetLineCount();
            indents = new int[line];
            comments = new bool[line];
            longStrings = new bool[line];
            Chunk chunk = ParseSource(GetText());
            GetIndents(chunk, indents, comments, longStrings);
        }

        private void DoFormatting(EditArray mgr, TextSpan span)
        {
            IndentInitialize();
            IVsTextLines pBuffer = GetTextLines();
            if (pBuffer != null)
            {
                List<EditSpan> changeList = NPLFormatHelper.ReformatCode(pBuffer, indents, comments, longStrings, span);
                if (changeList != null)
                {
                    foreach (EditSpan editSpan in changeList)
                    {
                        // Add edit operation
                        mgr.Add(editSpan);
                    }
                    mgr.ApplyEdits();
                }
            }
        }

        private Chunk ParseSource(string source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            //Create a parser for the request
            LuaParser parser = CreateParser();

            // Set the source
            ((LuaScanner)parser.scanner).SetSource(source, 0);

            // Trigger the parse (hidden region and errors will be added to the AuthoringSink)
            parser.Parse();

            return parser.Chunk;
        }

        private LuaParser CreateParser()
        {
            var handler = new ParaEngine.Tools.Lua.Parser.ErrorHandler();

            LuaScanner scanner = new LuaScanner();
            LuaParser parser = new LuaParser();

            scanner.Handler = handler;

            parser.scanner = scanner;

            parser.Request = null;

            return parser;
        }

        // get indentations for each line
        private void GetIndents(Chunk chunk, int[] indents, bool[] comments, bool[] longStrings)
        {
            // start with -1
            Trace.Write(chunk.GetStringRepresentation());
            int currentIndent = -1;

            //for (int i = 0; i < chunk.Location.sLin - 1; ++i)
            //    indents[i] = 0;
            for (int i = 0; i < indents.Length; ++i)
            {
                indents[i] = -1;
                comments[i] = false;
                longStrings[i] = false;
            }
                
            SetIndent(chunk, currentIndent, indents, comments, longStrings);

            // dealed with unset lines, normally comments or blank lines
            for (int i = indents.Length - 1; i >= 0; --i)
            {
                comments[i] = indents[i] == -1 ? true : comments[i];
                if (i == indents.Length - 1)
                    indents[i] = indents[i] == -1 ? 0 : indents[i];
                else
                    indents[i] = indents[i] == -1 ? indents[i + 1] : indents[i];
            }
        }

        // recursively set indentation for each line in Ast tree
        private void SetIndent(Node node, int currentIndent, int[] indents, bool[] comments, bool[] longStrings)
        {
            int increment = 0;
            if (node is Block)
            {
                increment = 1;
                if (indents[node.Location.sLin - 1] == -1)
                    indents[node.Location.sLin - 1] = currentIndent;
                //if (indents[node.Location.eLin - 1] == -1)
                //    indents[node.Location.eLin - 1] = currentIndent;
            }
            else if (node is DefBlock)
            {
                increment = 1;
                if (indents[node.Location.sLin - 1] == -1)
                    indents[node.Location.sLin - 1] = currentIndent;
                if (indents[node.Location.eLin - 1] == -1)
                    indents[node.Location.eLin - 1] = currentIndent;
            }
            else if (node is ThenBlock || node is ElseIfBlock)
            {
                // nothing 
            }
            else if (node is Literal &&                             // Long strings [[]]
                ((Literal)node).Type == LuaType.String &&
                (node.Location.eLin != node.Location.sLin))
            {
                if (indents[node.Location.sLin - 1] == -1)
                    indents[node.Location.sLin - 1] = currentIndent;
                for (int i = node.Location.sLin+1; i <= node.Location.eLin; ++i)
                {
                    indents[i - 1] = 0;
                    longStrings[i - 1] = true;
                }
            }
            else
            {
                if (indents[node.Location.sLin - 1] == -1)
                    indents[node.Location.sLin - 1] = currentIndent;
                if (indents[node.Location.eLin - 1] == -1)
                    indents[node.Location.eLin - 1] = currentIndent;
            }

            foreach (var childNode in node.GetChildNodes())
            {
                if (childNode != null)
                    SetIndent(childNode, currentIndent + increment, indents, comments, longStrings);
            }

            if (node.Next != null)
                SetIndent(node.Next, currentIndent, indents, comments, longStrings);
        }
    }
}
