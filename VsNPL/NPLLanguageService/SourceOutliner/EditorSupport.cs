/***************************************************************************

Copyright (c) 2006 Microsoft Corporation. All rights reserved.

***************************************************************************/

using EnvDTE;
using EnvDTE80;
using System;
using System.Runtime.InteropServices;
using ParaEngine.Tools.Lua.SourceOutliner;

namespace ParaEngine.Tools.Lua.SourceOutliner
{
    /// <summary>
    /// Class containing helper methods for the editor.
    /// </summary>
    public class EditorSupport
    {
        /// <summary>
        /// Highlights the source code that corresponds to a code element.
        /// </summary>
        /// <param name="obj">The CodeElementWrapper object for the code element.</param>
        /// <param name="application">A DTE object exposing the Visual Studio automation object model.</param>
        public void GoToCodeElement(Object obj, Object application)
        {
            var dte = (DTE2)application;
            var cew = (CodeElementWrapper)obj;
            GoToCodeElementHelper(dte, cew, false);
        }

        /// <summary>
        /// Activates the current text window.
        /// </summary>
        /// <param name="obj">The CodeElementWrapper object for the code element.</param>
        /// <param name="application">A DTE object exposing the Visual Studio automation object model.</param>
        public void ActivateCodeWindow(Object obj, Object application)
        {
            var dte = (DTE2)application;
            var cew = (CodeElementWrapper)obj;
            GoToCodeElementHelper(dte, cew, false);
        }

        /// <summary>
        /// Selects text in the code editor.
        /// </summary>
        /// <param name="dte">A DTE2 object exposing the Visual Studio automation object model.</param>
        /// <param name="cew">The CodeElementWrapper object containing the selection.</param>
        /// <param name="useTryShow">true to use TryToShow to adjust the code editor window to show the selection, otherwise false.</param>
        private static void GoToCodeElementHelper(DTE2 dte, CodeElementWrapper cew, bool useTryShow)
        {
            if (cew != null)
            {
                try
                {
                    TextPoint start = cew.StartPoint;
                    var tx = (TextDocument)dte.ActiveDocument.Object("TextDocument");
                    int line = start.Line;
                    int offset = start.LineCharOffset;

                    if (!useTryShow)
                    {
                        tx.Selection.MoveToLineAndOffset(line, offset, false);
                    }
                    else
                    {
                        start.TryToShow(vsPaneShowHow.vsPaneShowCentered, start);
                    }

                    if (!useTryShow)
                    {
                        tx.Selection.SelectLine();
                    }
                }
                catch (COMException)
                {
                    // Discard the exception that gets thrown when accessing 
                    // a non-code TextDocument, for example a Windows form.
                }
            }
        }
    }
}