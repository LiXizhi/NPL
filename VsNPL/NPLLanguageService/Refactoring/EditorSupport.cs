using System.Runtime.InteropServices;
using EnvDTE;

namespace ParaEngine.Tools.Lua.Refactoring
{
    /// <summary>
    /// 
    /// </summary>
    public class EditorSupport
    {

        /// <summary>
        /// Selects text in the code editor.
        /// </summary>
        /// <param name="dte">A DTE2 object exposing the Visual Studio automation object model.</param>
        /// <param name="element">The CodeElementWrapper object containing the selection.</param>
        /// <param name="useTryShow">true to use TryToShow to adjust the code editor window to show the selection, otherwise false.</param>
        public static void GoToCodeElementHelper(DTE dte, CodeElement element, bool useTryShow)
        {
            if (element != null)
            {
                try
                {
                    TextPoint start = element.StartPoint;
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