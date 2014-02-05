using System;
using System.Diagnostics;
using System.IO;
using EnvDTE;
using ParaEngine.Tools.Lua.AST;
using ParaEngine.Tools.Lua.CodeDom.Definitions;
using ParaEngine.Tools.Lua.CodeDom.Elements;
using ParaEngine.Tools.Lua.Parser;

namespace ParaEngine.Tools.Lua.CodeDom
{
    /// <summary>
    /// Lua CodeDom static helper class.
    /// </summary>
    public static class LuaCodeDomHelper
    {

        /// <summary>
        /// Converts a LexLocation type to TextPoint.
        /// </summary>
        /// <param name="location">LexLocation instance.</param>
        /// <param name="document">TextDocument</param>
        /// <returns>new LexLocation instance</returns>
        public static TextPoint LexLocationToTextPoint(TextDocument document, LexLocation location)
        {
            TextPoint point = new LuaTextPoint(document, location.sCol, location.sLin);
            return point;
        }

        /// <summary>
        /// Converts a LexLocation type to TextPoint.
        /// </summary>
        /// <param name="location">LexLocation instance.</param>
        /// <returns>new LexLocation instance</returns>
        public static TextPoint LexLocationToTextPoint(LexLocation location)
        {
            return LexLocationToTextPoint(null, location);
        }


		/// <summary>
		/// Returns function declaration line from source code.
		/// </summary>
		/// <param name="node">The node.</param>
		/// <param name="document">The document.</param>
		/// <returns></returns>
        public static TextPoint GetFunctionDeclarationFromSource(
            FunctionDeclaration node, TextDocument document)
        {
            TextPoint endPoint = null;
            string name = node.Name;
            var functionLineText = GetCodeLineText(document, node.Location.sLin);
            if (!string.IsNullOrEmpty(functionLineText))
            {
                int startIndex = functionLineText.ToLower().IndexOf("function", StringComparison.CurrentCultureIgnoreCase);
                if (startIndex > -1)
                {
                    int nameIndex = functionLineText.IndexOf(name);
                    endPoint = new LuaTextPoint(document, nameIndex + name.Length + 2, node.Location.sLin);
                }
            }
            return endPoint;
        }


		/// <summary>
		/// Returns function declaration line from source code.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="document">The document.</param>
		/// <param name="startPoint">The start point.</param>
		/// <returns></returns>
        public static TextPoint GetFunctionDeclarationFromSource(string name, TextDocument document, TextPoint startPoint)
        {
            TextPoint endPoint = null;
            var functionLineText = GetCodeLineText(document, startPoint.Line);
            if (!string.IsNullOrEmpty(functionLineText))
            {
                int startIndex = functionLineText.ToLower().IndexOf("function", StringComparison.CurrentCultureIgnoreCase);
                if (startIndex > -1)
                {
                    int nameIndex = functionLineText.IndexOf(name);
                    endPoint = new LuaTextPoint(document, nameIndex + name.Length + 2, startPoint.Line);
                }
            }
            return endPoint;
        }

		/// <summary>
		/// Gets text from source code.
		/// </summary>
		/// <param name="document">The document.</param>
		/// <param name="startLine">The start line.</param>
		/// <returns></returns>
        public static string GetCodeLineText(TextDocument document, int startLine)
        {
            string result = string.Empty;
        	try
        	{
        		if (startLine > 0)
        		{
        			EditPoint ep = document.CreateEditPoint(new LuaTextPoint(document, 1, startLine));
        			result = ep.GetText(1000);
        			if (!string.IsNullOrEmpty(result))
        			{
        				using (var reader = new StringReader(result))
        				{
        					result = reader.ReadLine();
        				}
        			}
        		}
        	}
        	catch (Exception e)
        	{
        		Trace.WriteLine(e);
        	}
            return result;
        }


		/// <summary>
		/// Gets parent element of ICodeDomElement.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <returns></returns>
        public static CodeElement GetParentElement(ICodeDomElement element)
        {
            if (element.ParentElement is LuaCodeFunction || element.ParentElement is LuaCodeClass)
            {
                return element.ParentElement;
            }
            CodeElement parent = GetParent(element.ParentElement as ICodeDomElement);
            return parent;
        }


		/// <summary>
		/// Gets parent of the specified element.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <returns></returns>
        private static CodeElement GetParent(ICodeDomElement element)
        {
            if (element == null) return null;
            if (element.ParentElement is LuaCodeFunction
            || element.ParentElement is LuaCodeClass || element.ParentElement == null)
            {
                return element.ParentElement;
            }
            CodeElement parent = GetParent(element.ParentElement as ICodeDomElement);
            return parent;
        }
    }
}
