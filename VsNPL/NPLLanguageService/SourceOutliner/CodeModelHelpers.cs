/***************************************************************************

Copyright (c) 2006 Microsoft Corporation. All rights reserved.

***************************************************************************/

using System;
using System.Diagnostics;
using System.Xml;
using EnvDTE;
using EnvDTE80;

namespace ParaEngine.Tools.Lua.SourceOutliner
{
    /// <summary>
    /// Class containing helper methods for working with code elements.
    /// </summary>
    [CLSCompliant(false)]
    public static class CodeModelHelpers
    {
        /// <summary>
        /// Returns a CodeElement's access type.
        /// </summary>
        /// <param name="element">The CodeElement to examine.</param>
        /// <returns>The element's access type from the CodeAccessType enumeration.</returns>
        public static CodeAccessType ConvertCMAccessTypeToCodeAccessType(CodeElement element)
        {
        	// Determine the access by first determining the kind of code element
            // and then casting it to the appropriate strongly-typed object.

            if (element != null)
            {
                vsCMAccess rawAccess;
                switch (element.Kind)
                {
                    case vsCMElement.vsCMElementFunction:
                        rawAccess = ((CodeFunction) element).Access;
                        break;
                    case vsCMElement.vsCMElementMap: //Table variable
                    case vsCMElement.vsCMElementDeclareDecl:
                    case vsCMElement.vsCMElementVariable:
                        rawAccess = ((CodeVariable) element).Access;
                        break;
                    case vsCMElement.vsCMElementClass:
                        rawAccess = ((CodeClass) element).Access;
                        break;
                    case vsCMElement.vsCMElementEnum:
                        rawAccess = ((CodeEnum) element).Access;
                        break;
                    case vsCMElement.vsCMElementInterface:
                        rawAccess = ((CodeInterface) element).Access;
                        break;
                    case vsCMElement.vsCMElementStruct:
                        rawAccess = ((CodeStruct) element).Access;
                        break;
                    case vsCMElement.vsCMElementDelegate:
                        rawAccess = ((CodeDelegate) element).Access;
                        break;
                    case vsCMElement.vsCMElementProperty:
                        rawAccess = ((CodeProperty) element).Access;
                        break;
                    case vsCMElement.vsCMElementEvent:
                        rawAccess = ((CodeEvent) element).Access;
                        break;
                    default:
                        rawAccess = vsCMAccess.vsCMAccessDefault;
                        break;
                }

                // Convert the raw access type to the CodeAccessType enumeration.
                switch (rawAccess)
                {
                    case vsCMAccess.vsCMAccessPrivate:
                        return CodeAccessType.Private;
                    case vsCMAccess.vsCMAccessProtected:
                        return CodeAccessType.Protected;
                    case vsCMAccess.vsCMAccessPublic:
                    case vsCMAccess.vsCMAccessProject:
                        return CodeAccessType.Public;
                    case vsCMAccess.vsCMAccessAssemblyOrFamily:
                        return CodeAccessType.Friend;
                    default:
                        return CodeAccessType.Public;
                }
            }
        	return CodeAccessType.Public;
        }

    	/// <summary>
        /// Returns a CodeElement's type.
        /// </summary>
        /// <param name="element">The CodeElement to examine.</param>
        /// <returns>The element's type from the CodeElementType enumeration.</returns>
        public static CodeElementType ConvertCMElementTypeToCodeElementType(CodeElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            switch (element.Kind)
            {
                case vsCMElement.vsCMElementFunction:
                    return CodeElementType.Method;
                case vsCMElement.vsCMElementMap: //Table variable
                case vsCMElement.vsCMElementDeclareDecl:
                case vsCMElement.vsCMElementVariable:
                    return CodeElementType.Variable;
                case vsCMElement.vsCMElementClass:
                    return CodeElementType.Class;
                case vsCMElement.vsCMElementDelegate:
                    return CodeElementType.Delegate;
                case vsCMElement.vsCMElementEnum:
                    return CodeElementType.Enumeration;
                case vsCMElement.vsCMElementEvent:
                    return CodeElementType.Event;
                case vsCMElement.vsCMElementInterface:
                    return CodeElementType.Interface;
                case vsCMElement.vsCMElementModule:
                    return CodeElementType.Module;
                case vsCMElement.vsCMElementNamespace:
                    return CodeElementType.Namespace;
                case vsCMElement.vsCMElementProperty:
                    return CodeElementType.Property;
                case vsCMElement.vsCMElementStruct:
                    return CodeElementType.Structure;
                default:
                    return CodeElementType.Method;
            }
        }

        /// <summary>
        /// Returns the short name of a CodeElement from the fully-qualified name. 
        /// </summary>
        /// <param name="element">The CodeElement to extract a name from.</param>
        /// <returns>The generic element name string.</returns>
        private static string ExtractGenericNameFromFullName(CodeElement element)
        {
            int index = element.FullName.LastIndexOf('.');
            string temp;

            try
            {
                temp = element.FullName.Substring(index + 1, element.FullName.Length - index - 1);
            }
            catch (ArgumentNullException)
            {
                temp = null;
            }

            return temp;
        }

        /// <summary>
        /// Returns the parameters list from a function.
        /// </summary>
        /// <param name="element">A CodeFunction2 object.</param>
        /// <returns>A string with function parameters.</returns>
        private static string ExtractMethodParameters(CodeFunction2 element)
        {
            // Get the string holding the stub definition of this function. 
            string temp = element.get_Prototype((int) vsCMPrototype.vsCMPrototypeParamTypes);
            int len = temp.Length;
            int index = temp.LastIndexOf('(');
            string str;
            try
            {
                str = temp.Substring(index, len - index);
            }
            catch (ArgumentNullException)
            {
                str = null;
            }

            return str;
        }

        /// <summary>
        /// Returns the parent of a CodeElement.
        /// </summary>
        /// <param name="element">The CodeElement whose parent is needed.</param>
        /// <returns>The parent CodeElement object.</returns>
        public static object GetCodeElementParent(CodeElement element)
        {
            object objectParent = null;

            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            switch (element.Kind)
            {
                case vsCMElement.vsCMElementClass:
                    objectParent = ((CodeClass) element).Parent;
                    break;
                case vsCMElement.vsCMElementDelegate:
                    objectParent = ((CodeDelegate) element).Parent;
                    break;
                case vsCMElement.vsCMElementEnum:
                    objectParent = ((CodeEnum) element).Parent;
                    break;
                case vsCMElement.vsCMElementEvent:
                    objectParent = ((CodeEvent) element).Parent;
                    break;
                case vsCMElement.vsCMElementFunction:
                    objectParent = ((CodeFunction) element).Parent;
                    break;
                case vsCMElement.vsCMElementInterface:
                    objectParent = ((CodeInterface) element).Parent;
                    break;
                case vsCMElement.vsCMElementModule:
                    // VS has no CodeModule class, so a CodeClass is used instead
                    // to provide the list of children.
                    objectParent = ((CodeClass) element).Parent;
                    break;
                case vsCMElement.vsCMElementNamespace:
                    objectParent = ((CodeNamespace) element).Parent;
                    break;
                case vsCMElement.vsCMElementProperty:
                    objectParent = ((CodeProperty) element).Parent;
                    break;
                case vsCMElement.vsCMElementStruct:
                    objectParent = ((CodeStruct) element).Parent;
                    break;
                case vsCMElement.vsCMElementVariable:
                    objectParent = ((CodeVariable) element).Parent;
                    break;
                case vsCMElement.vsCMElementParameter:
                    objectParent = ((CodeParameter) element).Parent;
                    break;
                default:
                    break;
            }

            return objectParent;
        }

        /// <summary>
        /// Returns a CodeElement's display name for use in TreeViews.
        /// </summary>
        /// <param name="element">The CodeElement whose display name is needed.</param>
        /// <returns>The element's display string.</returns>
        /// <remarks>
        /// For methods, this is the function prototype;
        /// for other elements, it is simply the Name.
        /// </remarks>
        public static string GetDisplayNameFromCMElement(CodeElement element)
        {
            string strName;

            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            try
            {
                if (IsGeneric(element, out strName))
                {
                    Debug.Assert(strName != null);
                }
                else
                {
                    switch (element.Kind)
                    {
                        case vsCMElement.vsCMElementFunction:
                            var codeFunction = ((CodeFunction) element);
                            strName = codeFunction.get_Prototype((int) vsCMPrototype.vsCMPrototypeParamTypes);
                            break;
                        case vsCMElement.vsCMElementMap:
                            var codeVariable = ((CodeVariable) element);
                            strName = codeVariable.get_Prototype((int) vsCMPrototype.vsCMPrototypeParamTypes);
                            break;
                        default:
                            strName = element.Name;
                            break;
                    }
                }
            }
            catch
            {
                strName = element.Name;
            }

            return strName;
        }

        /// <summary>
        /// Returns the children of a CodeElement.
        /// </summary>
        /// <param name="element">The CodeElement parent to enumerate.</param>
        /// <returns>A CodeElements object containing the child elements.</returns>
        public static CodeElements GetMembersOf(CodeElement element)
        {
            CodeElements members = null;

            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            if (HasChildrenKind(element.Kind))
            {
            	switch (element.Kind)
            	{
            		case vsCMElement.vsCMElementNamespace:
            			members = ((CodeNamespace) element).Members;
            			break;
            		case vsCMElement.vsCMElementModule:
            			// VS has no CodeModule class, so a CodeClass is used instead
            			// to provide the list of children.
            		case vsCMElement.vsCMElementClass:
            			members = ((CodeClass) element).Members;
            			break;
            		case vsCMElement.vsCMElementEnum:
            			members = ((CodeEnum) element).Members;
            			break;
            		case vsCMElement.vsCMElementInterface:
            			members = ((CodeInterface) element).Members;
            			break;
            		case vsCMElement.vsCMElementStruct:
            			members = ((CodeStruct) element).Members;
            			break;
            		case vsCMElement.vsCMElementDelegate:
            			members = ((CodeDelegate) element).Members;
            			break;
            			//case vsCMElement.vsCMElementFunction:
            			//    members = ((CodeFunction)element).Children;
            			//    break;
            	}
            }

        	return members;
        }

        /// <summary>
        /// Returns a CodeElement's documentation comment if it has 
        /// one, otherwise returns relevant information from the prototype.
        /// </summary>
        /// <param name="element">A CodeElement object.</param>
        /// <returns>A string representing the element's definition.</returns>
        public static string GetPrototypeFromCMElement(CodeElement element)
        {
            var docComment = new XmlDocument();

            try
            {
                if (element == null)
                {
                    throw new ArgumentNullException("element");
                }

                switch (element.Kind)
                {
                    case vsCMElement.vsCMElementMap:
                        {
                            return ((CodeVariable2) element).get_Prototype((int) vsCMPrototype.vsCMPrototypeType |
                                                                           (int) vsCMPrototype.vsCMPrototypeParamNames |
                                                                           (int) vsCMPrototype.vsCMPrototypeParamTypes);
                        }
                    case vsCMElement.vsCMElementFunction:
                        var codeFunction = ((CodeFunction) element);
                        if (!string.IsNullOrEmpty(codeFunction.DocComment))
                        {
                            docComment.LoadXml(String.Format("<docComment>{0}</docComment>", codeFunction.DocComment));
                            XmlNode node = docComment.SelectSingleNode("/docComment/summary");
                            if (node != null)
                            {
                                return String.Format("{0}\n{1}", node.InnerText.Trim(),
                                                     codeFunction.get_Prototype(
                                                         (int) vsCMPrototype.vsCMPrototypeType |
                                                         (int) vsCMPrototype.vsCMPrototypeParamNames |
                                                         (int) vsCMPrototype.vsCMPrototypeParamTypes));
                            }
                            return string.Empty;
                        }
                        return codeFunction.get_Prototype((int) vsCMPrototype.vsCMPrototypeType |
                                                          (int) vsCMPrototype.vsCMPrototypeParamNames |
                                                          (int) vsCMPrototype.vsCMPrototypeParamTypes);

                    case vsCMElement.vsCMElementProperty:
                        var codeProperty = ((CodeProperty) element);
                        if (!string.IsNullOrEmpty(codeProperty.DocComment))
                        {
                            docComment.LoadXml(String.Format("<docComment>{0}</docComment>", codeProperty.DocComment));
                            XmlNode node = docComment.SelectSingleNode("/docComment/summary");
                            if (node != null)
                            {
                                return String.Format("{0}\n{1}", node.InnerText.Trim(),
                                                     codeProperty.get_Prototype((int) vsCMPrototype.vsCMPrototypeType |
                                                                                (int)
                                                                                vsCMPrototype.vsCMPrototypeParamNames |
                                                                                (int)
                                                                                vsCMPrototype.vsCMPrototypeParamTypes));
                            }
                            return string.Empty;
                        }
                        return codeProperty.get_Prototype((int) vsCMPrototype.vsCMPrototypeType |
                                                          (int) vsCMPrototype.vsCMPrototypeParamNames |
                                                          (int) vsCMPrototype.vsCMPrototypeParamTypes);

                    case vsCMElement.vsCMElementVariable:
                        var codeVariable = ((CodeVariable) element);
                        return codeVariable.get_Prototype((int) vsCMPrototype.vsCMPrototypeType);

                    case vsCMElement.vsCMElementEvent:
                        var codeEvent = ((CodeEvent) element);
                        if (!string.IsNullOrEmpty(codeEvent.DocComment))
                        {
                            docComment.LoadXml(String.Format("<docComment>{0}</docComment>", codeEvent.DocComment));
                            XmlNode node = docComment.SelectSingleNode("/docComment/summary");
                            if (node != null)
                            {
                                return String.Format("{0}\n{1}", node.InnerText.Trim(),
                                                     codeEvent.get_Prototype((int) vsCMPrototype.vsCMPrototypeType |
                                                                             (int) vsCMPrototype.vsCMPrototypeParamNames |
                                                                             (int) vsCMPrototype.vsCMPrototypeParamTypes));
                            }
                            return string.Empty;
                        }
                        return codeEvent.get_Prototype((int) vsCMPrototype.vsCMPrototypeType |
                                                       (int) vsCMPrototype.vsCMPrototypeParamNames |
                                                       (int) vsCMPrototype.vsCMPrototypeParamTypes);

                    case vsCMElement.vsCMElementDelegate:
                        var codeDelegate = ((CodeDelegate) element);
                        if (!string.IsNullOrEmpty(codeDelegate.DocComment))
                        {
                            docComment.LoadXml(String.Format("<docComment>{0}</docComment>", codeDelegate.DocComment));
                            XmlNode node = docComment.SelectSingleNode("/docComment/summary");
                            if (node != null)
                            {
                                return String.Format("{0}\n{1}", node.InnerText.Trim(),
                                                     codeDelegate.get_Prototype((int) vsCMPrototype.vsCMPrototypeType |
                                                                                (int)
                                                                                vsCMPrototype.vsCMPrototypeParamNames |
                                                                                (int)
                                                                                vsCMPrototype.vsCMPrototypeParamTypes));
                            }
                            return string.Empty;
                        }
                        return codeDelegate.get_Prototype((int) vsCMPrototype.vsCMPrototypeType |
                                                          (int) vsCMPrototype.vsCMPrototypeParamNames |
                                                          (int) vsCMPrototype.vsCMPrototypeParamTypes);

                    case vsCMElement.vsCMElementClass:
                        var codeClass = ((CodeClass) element);
                        if (!string.IsNullOrEmpty(codeClass.DocComment))
                        {
                            docComment.LoadXml(String.Format("<docComment>{0}</docComment>", codeClass.DocComment));
                            XmlNode node = docComment.SelectSingleNode("/docComment/summary");
                            if (node != null)
                            {
                                return node.InnerText.Trim();
                            }
                            return string.Empty;
                        }
                        return string.Empty;

                    case vsCMElement.vsCMElementStruct:
                        var codeStruct = ((CodeStruct) element);
                        if (!string.IsNullOrEmpty(codeStruct.DocComment))
                        {
                            docComment.LoadXml(String.Format("<docComment>{0}</docComment>", codeStruct.DocComment));
                            XmlNode node = docComment.SelectSingleNode("/docComment/summary");
                            if (node != null)
                            {
                                return node.InnerText.Trim();
                            }
                            return string.Empty;
                        }
                        return string.Empty;

                    case vsCMElement.vsCMElementInterface:
                        var codeInterface = ((CodeInterface) element);
                        if (!string.IsNullOrEmpty(codeInterface.DocComment))
                        {
                            docComment.LoadXml(String.Format("<docComment>{0}</docComment>", codeInterface.DocComment));
                            XmlNode node = docComment.SelectSingleNode("/docComment/summary");
                            if (node != null)
                            {
                                return node.InnerText.Trim();
                            }
                            return string.Empty;
                        }
                        return string.Empty;

                    default:
                        return string.Empty;
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Returns an identifier to use for a unique CodeElement.
        /// </summary>
        /// <param name="element">The CodeElement to ID.</param>
        /// <returns>A string that identifies the element.</returns>
        /// <remarks>
        /// Each language has limitations when creating a 'unique' identifier.
        /// If the language is VB, a method identifier may not actually 
        /// remain unique, because VB creates the identifier based on the original
        /// method signature and never updates it if the signature changes.
        /// For example, if a method is copied and pasted to create a similar method, the new 
        /// method initially has the same signature as the old one and thus VB's ElementID
        /// will return the same identifier for the clone as it returned for the copied method.
        /// If the language is C#, the code element throws and exception when accessing the 
        /// ElementID, so the function prototype is used here to generate an ID instead.
        /// </remarks>
        public static string GetUniqueElementId(CodeElement element)
        {
            string strElementID = null;

            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            try
            {
                // VB will return an ElementID for the element.
                if (element.Language == CodeModelLanguageConstants.vsCMLanguageVB)
                {
                    var codeElement2 = element as CodeElement2;
                    if (codeElement2 != null)
                    {
                        strElementID = codeElement2.ElementID;
                    }
                }

                // For non-VB languages, construct an ID from the best available signature.
                if (strElementID == null)
                {
                    switch (element.Kind)
                    {
                        case vsCMElement.vsCMElementFunction:
                            var codeFunction = ((CodeFunction) element);
                            strElementID = codeFunction.get_Prototype((int) vsCMPrototype.vsCMPrototypeParamTypes);
                            break;
                        default:
                            strElementID = element.FullName;
                            break;
                    }
                }
            }
            catch (Exception)
            {
                strElementID = element.Name;
            }

            return strElementID;
        }

        /// <summary>
        /// Determines whether a particular CodeElement type is one that can have child elements.
        /// </summary>
        /// <param name="kind">An object type identifier from the CodeElement.Kind enumeration.</param>
        /// <returns>true if the CodeElement type can have children, otherwise false.</returns>
        public static bool HasChildrenKind(vsCMElement kind)
        {
            switch (kind)
            {
                case vsCMElement.vsCMElementClass:
                case vsCMElement.vsCMElementDelegate:
                case vsCMElement.vsCMElementEnum:
                case vsCMElement.vsCMElementInterface:
                case vsCMElement.vsCMElementModule:
                case vsCMElement.vsCMElementNamespace:
                case vsCMElement.vsCMElementStruct:
                case vsCMElement.vsCMElementFunction:
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Returns a flag indicating whether a CodeElement is a generic, 
        /// and also returns the element's generic name.
        /// </summary>
        /// <param name="element">The CodeElement to test.</param>
        /// <param name="name">The returned generic name.</param>
        /// <returns>true if the delegate is a generic, otherwise false.</returns>
        public static bool IsGeneric(CodeElement element, out string name)
        {
            bool isGen = false;
            string postfix = null;

            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            switch (element.Kind)
            {
                case vsCMElement.vsCMElementClass:
                    var codeClass = element as CodeClass2;

                    if ((codeClass != null) && codeClass.IsGeneric)
                    {
                        isGen = true;
                    }
                    break;

                case vsCMElement.vsCMElementInterface:
                    var codeInterface = element as CodeInterface2;

                    if ((codeInterface != null) && codeInterface.IsGeneric)
                    {
                        isGen = true;
                    }
                    break;


                case vsCMElement.vsCMElementFunction:
                    var codeFunction = element as CodeFunction2;

                    if ((codeFunction != null) && codeFunction.IsGeneric)
                    {
                        // Get information about the parameters, which is appended to the function name later.
                        postfix = ExtractMethodParameters(codeFunction);
                        isGen = true;
                    }
                    break;

                case vsCMElement.vsCMElementProperty:
                    var codeProperty = element as CodeProperty2;

                    if ((codeProperty != null) && codeProperty.IsGeneric)
                    {
                        isGen = true;
                    }
                    break;

                case vsCMElement.vsCMElementVariable:
                    var codeVariable = element as CodeVariable2;

                    if ((codeVariable != null) && codeVariable.IsGeneric)
                    {
                        isGen = true;
                    }
                    break;

                case vsCMElement.vsCMElementDelegate:
                    var codeDelegate = element as CodeDelegate2;

                    if ((codeDelegate != null) && codeDelegate.IsGeneric)
                    {
                        isGen = true;
                    }
                    break;
            }

            if (isGen)
            {
                // postfix is not null if the CodeElement is a generic function.
                name = ExtractGenericNameFromFullName(element) + postfix;
            }
            else
            {
                name = null;
            }

            return isGen;
        }

        /// <summary>
        /// Determines whether a particular CodeElement type is one that will be diagrammed.
        /// </summary>
        /// <param name="kind">An object type identifier from the CodeElement.Kind enumeration.</param>
        /// <returns>true if Source Outliner is interested in the type, otherwise false.</returns>
        public static bool IsInterestingKind(vsCMElement kind)
        {
            switch (kind)
            {
                case vsCMElement.vsCMElementFunction:
                case vsCMElement.vsCMElementDeclareDecl:
                case vsCMElement.vsCMElementMap:
                case vsCMElement.vsCMElementClass:
                case vsCMElement.vsCMElementDelegate:
                case vsCMElement.vsCMElementEnum:
                case vsCMElement.vsCMElementEvent:
                case vsCMElement.vsCMElementInterface:
                case vsCMElement.vsCMElementModule:
                case vsCMElement.vsCMElementNamespace:
                case vsCMElement.vsCMElementProperty:
                case vsCMElement.vsCMElementStruct:
                case vsCMElement.vsCMElementVariable:
                case vsCMElement.vsCMElementParameter:
                    return true;

                default:
                    return false;
            }
        }
    }
}