/***************************************************************************

Copyright (c) 2006 Microsoft Corporation. All rights reserved.

***************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using EnvDTE;

namespace ParaEngine.Tools.Lua.SourceOutliner
{
    /// <summary>
    /// Class that constructs a TreeNode from a CodeElement.
    /// </summary>
    [CLSCompliant(false)]
    public class CodeElementWrapper : TreeNode
    {
        private CodeElement element;
        private List<CodeParameter> parameterList;

        /// <summary>
        /// Initializes a new instance of the CodeElementWrapper class.
        /// </summary>
        /// <param name="element">The CodeElement object to be wrapped.</param>
        public CodeElementWrapper(CodeElement element)
        {
            CodeElement = element;
        }

        /// <summary>
        /// Gets or sets the CodeElement object.
        /// </summary>
        /// <returns>A CodeElement object.</returns>
        public CodeElement CodeElement
        {
            get { return element; }
            set
            {
                element = value;

                // Push the CodeElement object properties into the TreeNode
                // properties so that they automatically get rendered when 
                // this object is added to a tree.
                Text = CodeModelHelpers.GetDisplayNameFromCMElement(element);
                ElementName = element.Name;
                UniqueElementId = CodeModelHelpers.GetUniqueElementId(element);
                ElementType = CodeModelHelpers.ConvertCMElementTypeToCodeElementType(element);
                AccessType = CodeModelHelpers.ConvertCMAccessTypeToCodeAccessType(element);
                ImageIndex = (int) ElementType + (int) AccessType;
                SelectedImageIndex = (int) ElementType + (int) AccessType;
                ToolTipText = CodeModelHelpers.GetPrototypeFromCMElement(element);

                // If the current element is a function, cache the parameterList so that a
                // notification of code model changes occurs when the signature of the function changes.
                if (element.Kind == vsCMElement.vsCMElementFunction)
                {
                    parameterList = new List<CodeParameter>();

                    var codeFunction = element as CodeFunction;
                    CodeElements parameters = codeFunction.Parameters;
                    foreach (CodeParameter parameter in parameters)
                    {
                        parameterList.Add(parameter);
                    }
                }
            }
        }

        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public bool FinishedLoading { get; set; }

        /// <summary>
        /// Gets the CodeElement name.
        /// </summary>
        /// <returns>A CodeElement name.</returns>
        public string ElementName { get; private set; }

        /// <summary>
        /// Gets a string that uniquely identifies the CodeElement.
        /// </summary>
        /// <returns>A unique id string.</returns>
        public string UniqueElementId { get; private set; }

        /// <summary>
        /// Gets the line number where the CodeElement starts in the source file.
        /// </summary>
        /// <returns>A line number.</returns>
        public int Location
        {
            get { return (element.StartPoint.Line); }
        }

        /// <summary>
        /// Gets or sets the type of the CodeElement.
        /// </summary>
        /// <returns>A CodeElementType.</returns>
        public CodeElementType ElementType { get; set; }

        /// <summary>
        /// Gets or sets the access type of the CodeElement.
        /// </summary>
        /// <returns>A CodeAccessType.</returns>
        public CodeAccessType AccessType { get; set; }

        /// <summary>
        /// Gets the starting TextPoint of the CodeElement, which identifies the 
        /// line number and character position of the start of the CodeElement.
        /// </summary>
        /// <returns>The starting TextPoint.</returns>
        public TextPoint StartPoint
        {
            get { return element.StartPoint; }
        }

        /// <summary>
        /// Gets the ending TextPoint of the CodeElement, which identifies the 
        /// line number and character position of the end of the CodeElement.
        /// </summary>
        /// <returns>The ending TextPoint.</returns>
        public TextPoint EndPoint
        {
            get { return element.EndPoint; }
        }

        #endregion Properties
    }

    /// <summary>
    /// Indicates the number of elements in CodeAccessType.
    /// </summary>
    /// <remarks> 
    /// The value of Count matches the number of elements in CodeAccessType.
    /// </remarks>
    public enum AccessTypeCount
    {
        None = 0,
        Count = 6
    }

    /// <summary>
    /// An enumeration of CodeElement types.
    /// </summary>
    public enum LuaCodeElementType
    {
        All = -1,
        Method = AccessTypeCount.Count*12,
        Variable = AccessTypeCount.Count*23,
    }

    /// <summary>
    /// An enumeration of CodeElement types.
    /// </summary>
    public enum CodeElementType
    {
        All = -1,
        Class = AccessTypeCount.Count*0,
        Delegate = AccessTypeCount.Count*2,
        Enumeration = AccessTypeCount.Count*3,
        Event = AccessTypeCount.Count*5,
        Method = AccessTypeCount.Count*12,
        Interface = AccessTypeCount.Count*8,
        Module = AccessTypeCount.Count*14,
        Namespace = AccessTypeCount.Count*15,
        Property = AccessTypeCount.Count*17,
        Structure = AccessTypeCount.Count*18,
        Variable = AccessTypeCount.Count*23,

        /* These types are currently unused.
        Constant = AccessTypeCount.Count * 1,
        EnumerationMember = AccessTypeCount.Count * 4,
        Exception = AccessTypeCount.Count * 6,
        Field = AccessTypeCount.Count * 7,
        Macro = AccessTypeCount.Count * 9,
        Map = AccessTypeCount.Count * 10,
        MapItem = AccessTypeCount.Count * 11,
        Overload = AccessTypeCount.Count * 13,
        Operator = AccessTypeCount.Count * 16,
        Template = AccessTypeCount.Count * 19,
        Typedef = AccessTypeCount.Count * 20,
        Type = AccessTypeCount.Count * 21,
        Union = AccessTypeCount.Count * 22,
        ValueType = AccessTypeCount.Count * 24,
        Intrinsic = AccessTypeCount.Count * 25,
        JSharpMethod = AccessTypeCount.Count * 26,
        JSharpField = AccessTypeCount.Count * 27,
        JSharpClass = AccessTypeCount.Count * 28,
        JSharpNamespace = AccessTypeCount.Count * 29,
        JSharpInterface = AccessTypeCount.Count * 30 
        */
    }

    /// <summary>
    /// An enumeration of CodeElement access types (private, public, etc).
    /// </summary>
    public enum CodeAccessType
    {
        Public = 0,
        Internal = 1,
        Friend = 2,
        Protected = 3,
        Private = 4,
        Shortcut = 5
    }
}