using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace ParaEngine.Tools.Lua
{
    public class LuaDeclarations : Declarations
    {
        private readonly List<Declaration> declarations;
        private readonly Microsoft.VisualStudio.Package.LanguageService languageService;
        private TextSpan        commitSpan;

		/// <summary>
		/// Initializes a new instance of the <see cref="LuaDeclarations"/> class.
		/// </summary>
		/// <param name="languageService">The language service.</param>
        public LuaDeclarations(Microsoft.VisualStudio.Package.LanguageService languageService)
        {
            if (languageService == null)
                throw new ArgumentNullException("languageService");

            this.languageService = languageService;
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="LuaDeclarations"/> class.
		/// </summary>
		/// <param name="languageService">The language service.</param>
		/// <param name="declarations">The declarations.</param>
        public LuaDeclarations(Microsoft.VisualStudio.Package.LanguageService languageService, IEnumerable<Declaration> declarations)
        {
            if (languageService == null)
                throw new ArgumentNullException("languageService");
            if (declarations == null)
                throw new ArgumentNullException("declarations");

            this.declarations = new List<Declaration>(declarations);
            this.languageService = languageService;
        }

        // This method is called to get the string to commit to the source buffer.
        // Note that the initial extent is only what the user has typed so far.
		/// <summary>
		/// Called to commit the specified item to the source file.
		/// </summary>
		/// <param name="textView">[in] An <see cref="T:Microsoft.VisualStudio.TextManager.Interop.IVsTextView"/> object representing the view that displays the source file.</param>
		/// <param name="textSoFar">[in] A string containing the text that has been typed by the user.</param>
		/// <param name="commitCharacter">[in] The character used to commit the text.</param>
		/// <param name="index">[in] The index of the item to commit to the source file.</param>
		/// <param name="initialExtent">[in, out] A <see cref="T:Microsoft.VisualStudio.TextManager.Interop.TextSpan"/> object specifying the text typed so far. Returns the span of the committed text.</param>
		/// <returns>
		/// If successful, returns a string containing the text to commit to the source file; otherwise, returns null.
		/// </returns>
        public override string OnCommit(IVsTextView textView,
                                        string textSoFar,
                                        char commitCharacter,
                                        int index,
                                        ref TextSpan initialExtent)
        {
            // We intercept this call only to get the initial extent
            // of what was committed to the source buffer.
            commitSpan = initialExtent;

            return base.OnCommit(textView,
                                 textSoFar,
                                 commitCharacter,
                                 index,
                                 ref initialExtent);
        }

        // This method is called after the string has been committed to the source buffer.
		/// <summary>
		/// Called after the declaration has been committed to the source file. When implemented in a derived class, it provides a completion character which may itself be a trigger for another round of IntelliSense.
		/// </summary>
		/// <param name="textView">[in] An <see cref="T:Microsoft.VisualStudio.TextManager.Interop.IVsTextView"/> object representing the view that displays the source file.</param>
		/// <param name="committedText">[in] A string containing the text that was inserted as part of the completion process.</param>
		/// <param name="commitCharacter">[in] The character that was used to commit the text to the source file.</param>
		/// <param name="index">[in] The index of the item that was committed to the source file.</param>
		/// <returns>
		/// Returns a character to be inserted after the committed text. If nothing is to be inserted, returns 0.
		/// </returns>
        public override char OnAutoComplete(IVsTextView textView,
                                            string committedText,
                                            char commitCharacter,
                                            int index)
        {
/*
            // Do not replace when committed with \13 (enter)
            if (commitCharacter == 13)
                return '\0';
*/

            Declaration declaration = declarations[index];
            if (declaration != null)
            {
                // In this example, MyDeclaration identifies types with a string.
                // You can choose a different approach.
                if (declaration.DeclarationType == DeclarationType.Snippet)
                {
                    Source source = languageService.GetSource(textView);
                    if (source != null)
                    {
                        ExpansionProvider expansionProvider = source.GetExpansionProvider();
                        if (expansionProvider != null)
                        {
                            string title;
                            string path;
                            int commitLength = commitSpan.iEndIndex - commitSpan.iStartIndex;
                            if (commitLength < committedText.Length)
                            {
                                // Replace everything that was inserted
                                // so calculate the span of the full
                                // insertion, taking into account what
                                // was inserted when the commitSpan
                                // was obtained in the first place.
                                commitSpan.iEndIndex += (committedText.Length - commitLength);
                            }

                            if (expansionProvider.FindExpansionByShortcut(textView,
                                                           committedText,
                                                           commitSpan,
                                                           true,
                                                           out title,
                                                           out path) == 0)
                            {
                                expansionProvider.InsertNamedExpansion(textView,
                                                        title,
                                                        path,
                                                        commitSpan,
                                                        false);
                            }
                        }
                    }
                }
            }
            return '\0';
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
            return declarations[index].Name;
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

		/// <summary>
		/// When implemented in a derived class, gets the image to show next to the specified item.
		/// </summary>
		/// <param name="index">[in] The index of the item for which to get the image index.</param>
		/// <returns>
		/// The index of the image from an image list, otherwise -1.
		/// </returns>
        public override int GetGlyph(int index)
        {
            return GetGlyphImageIndex(declarations[index]);
        }

		/// <summary>
		/// Gets the index of the glyph image.
		/// </summary>
		/// <param name="declaration">The declaration.</param>
		/// <returns></returns>
        private static int GetGlyphImageIndex(Declaration declaration)
        {
            int glyph = declaration.IsLocal ? (int)GlyphImageIndex.AccessPrivate : (int)GlyphImageIndex.AccessPublic;
            
            switch (declaration.DeclarationType)
            {
                case DeclarationType.Variable:
                case DeclarationType.Field:
                    glyph += (int)GlyphImageIndex.Variable;
                    break;
                case DeclarationType.Parameter:
                    glyph = (int)GlyphImageIndex.Variable;
                    break;
                case DeclarationType.Function:
                    glyph += (int)GlyphImageIndex.Method;
                    break;
                case DeclarationType.Table:
                    glyph += (int)GlyphImageIndex.Type;
                    break;
                case DeclarationType.Keyword:
                    glyph += (int)GlyphImageIndex.Keyword;
                    break;
                case DeclarationType.Snippet:
                    glyph += (int)GlyphImageIndex.Snippet;
                    break;
            }

            return glyph;
        }
    }
}
