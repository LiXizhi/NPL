using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TextManager.Interop;
using ParaEngine.Tools.Lua.Parser;

namespace ParaEngine.Tools.Lua
{
    /// <summary>
    /// Various extension methods used by parts of the language service.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Determines whether a LexLocation is after a caret position.
        /// </summary>
        /// <param name="location">The location to test against.</param>
        /// <param name="line">The line number of the caret.</param>
        /// <param name="column">The column number of the caret.</param>
        /// <returns>True, if the caret is before the </returns>
        public static bool After(this LexLocation location, int line, int column)
        {
            if (location == null)
                return false;

            if (location.sLin > line)
                return true;

            if (location.sLin == line && location.sCol > column)
                return true;

            return false;
        }

        /// <summary>
        /// Determines whether a LexLocation is before a caret position.
        /// </summary>
        /// <param name="location">The location to test against.</param>
        /// <param name="line">The line number of the caret.</param>
        /// <param name="column">The column number of the caret.</param>
        /// <returns>True, if the caret is before the </returns>
        public static bool Before(this LexLocation location, int line, int column)
        {
            if (location == null)
                return false;

            if (location.eLin < line)
                return true;

            if (location.eLin == line && location.eCol < column)
                return true;

            return false;
        }

        /// <summary>
        /// Determines whether a LexLocation contains a caret position.
        /// </summary>
        /// <param name="location">The location to test against.</param>
        /// <param name="line">The line number of the caret.</param>
        /// <param name="column">The column number of the caret.</param>
        /// <returns>True, if the caret is contained in the LexLocation; false otherwise.</returns>
        public static bool Contains(this LexLocation location, int line, int column)
        {
            if (location == null)
                return false;

            if (location.sLin < line && location.eLin > line)
                return true;

            if (location.sLin == line && location.sCol < column)
                return true;

            if (location.eLin == line && location.eCol > column)
                return true;

            return false;
        }

        /// <summary>
        /// Determines whether a textspan is 'dirty' and a new parsing should occur.
        /// </summary>
        /// <param name="textSpan">The text span to test.</param>
        /// <returns>True, if the textspan is dirty; False otherwise.</returns>
        public static bool IsDirty(this TextSpan textSpan)
        {
            return (textSpan.iStartIndex != textSpan.iEndIndex || textSpan.iStartLine != textSpan.iEndLine);
        }

        /// <summary>
        /// Iterates through a collection and executes the action for each item.
        /// </summary>
        /// <typeparam name="T">The type of items in the collection.</typeparam>
        /// <param name="collection">The collection to iterate.</param>
        /// <param name="action">The action to execute.</param>
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            if (collection != null)
            {
                foreach (var item in collection)
                {
                    action(item);
                }
            }
        }

    }
}
