using System;
using System.Runtime.InteropServices;
using EnvDTE;
using EnvDTE80;
using ParaEngine.Tools.Lua.AST;

namespace ParaEngine.Tools.Lua.CodeDom.Elements
{
	/// <summary>
	/// Root element for Lua parsed code file and CodeDom tree.
	/// </summary>
    [ComVisible(true)]
    public class LuaCodeClass : LuaCodeElement<Chunk>, CodeClass2
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LuaCodeClass"/> class.
        /// </summary>
        /// <param name="dte"></param>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <param name="access"></param>
        /// <param name="chunk"></param>
        public LuaCodeClass(DTE dte, CodeElement parent, string name, vsCMAccess access, Chunk chunk)
            : this(dte, parent, name, chunk)
        {
            Access = access;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LuaCodeClass"/> class.
        /// </summary>
        /// <param name="dte"></param>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <param name="chunk"></param>
        public LuaCodeClass(DTE dte, CodeElement parent, string name, Chunk chunk)
            : base(dte, name, parent, chunk)
        {
            LuaASTNodeObject = chunk;
            childObjects = new LuaCodeElements(DTE, this);
        }

        #region Private Members


        #endregion

        #region CodeClass Members

        /// <summary>
        /// Creates a new variable code construct and inserts the element in the CodeDom tree. 
        /// </summary>
        /// <param name="variableName">Required. The name of the new variable.</param>
        /// <param name="variableType">Required. A vsCMTypeRef constant indicating the data type that the function returns. This can be a CodeTypeRef object, a vsCMTypeRef constant, or a fully qualified type name.</param>
        /// <param name="position">Optional. Default = 0. The code element after which to add the new element. If the value is a CodeElement, then the new element is added immediately after it.
        /// If the value is a Long, then AddVariable indicates the element after which to add the new element.
        /// Because collections begin their count at 1, passing 0 indicates that the new element should be placed at the beginning of the collection. A value of -1 means the element should be placed at the end. 
        /// </param>
        /// <param name="variableAccess">A vsCMAccess constant.</param>
        /// <param name="variablNode">The Node AST object of Variable.</param>
        /// <returns></returns>
        public CodeVariable AddVariable(string variableName, object variableType, object position, vsCMAccess variableAccess, object variablNode)
        {
            var codeVar = new LuaCodeVariable(DTE, this, variableName, ObjectToTypeRef(variableType), variableAccess, variablNode as Variable);
            AddVariable(codeVar);
            return codeVar;
        }


        /// <summary>
        /// Adds a function to the Lua class.
        /// </summary>
        /// <param name="functionName"></param>
        /// <param name="kind"></param>
        /// <param name="returnType"></param>
        /// <param name="position"></param>
        /// <param name="functionAccess"></param>
        /// <param name="functionNode"></param>
        /// <returns></returns>
        public CodeFunction AddFunction(string functionName, vsCMFunction kind, object returnType, object position,
                                        vsCMAccess functionAccess, object functionNode)
        {
            var codeFunction = new LuaCodeFunction(
                DTE, this, functionName, kind, returnType, Access, functionNode as FunctionDeclaration);
            AddFunction(codeFunction);
            return codeFunction;
        }

        /// <summary>
        /// Adds a function to the Lua class.
        /// </summary>
        /// <param name="function">LuaCodeFunction instance</param>
        public void AddFunction(LuaCodeFunction function)
        {
            childObjects.AddElement(function);
            CommitChanges();
        }

        /// <summary>
        /// Adds a member variable to the Lua class.
        /// </summary>
        /// <param name="variable"></param>
        public void AddVariable(LuaCodeVariable variable)
        {
            childObjects.AddElement(variable);
            CommitChanges();
        }

        /// <summary>
        /// Sets or gets whether or not an item is declared as abstract.
        /// </summary>
        public bool IsAbstract
        {
            get { return false; }
            set {  }
        }

        #region CodeClass2 Members

        /// <summary>
        /// Gets an enumeration that defines the kind of class.
        /// </summary>
        public vsCMClassKind ClassKind
        {
            get { return vsCMClassKind.vsCMClassKindMainClass; }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets partial class elements.
        /// </summary>
        public CodeElements PartialClasses
        {
            get { return null; }
        }

        /// <summary>
        /// Sets of gets the relationship of this class with other classes.
        /// </summary>
        public vsCMDataTypeKind DataTypeKind
        {
            get { return vsCMDataTypeKind.vsCMDataTypeKindMain; }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets parts of a class.
        /// </summary>
        public CodeElements Parts
        {
            get { return null; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether a class may be used to create a new class.
        /// </summary>
        public vsCMInheritanceKind InheritanceKind
        {
            get { return vsCMInheritanceKind.vsCMInheritanceKindNone; }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets a value indicating whether the current class is a generic.
        /// </summary>
        public bool IsGeneric
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether the current class is shared.
        /// </summary>
        public bool IsShared
        {
            get { return false; }
            set { throw new NotImplementedException(); }
        }

        #endregion

        #region Not Supported Members

		/// <summary>
		/// Adds the class.
		/// </summary>
		/// <param name="objectName">Name of the object.</param>
		/// <param name="Position">The position.</param>
		/// <param name="objectBases">The object bases.</param>
		/// <param name="objectImplementedInterfaces">The object implemented interfaces.</param>
		/// <param name="objectAccess">The object access.</param>
		/// <returns></returns>
        public CodeClass AddClass(string objectName, object Position, object objectBases,
                                  object objectImplementedInterfaces, vsCMAccess objectAccess)
        {
            throw new NotSupportedException("Lua does not support this feature.");
        }

		/// <summary>
		/// Adds the delegate.
		/// </summary>
		/// <param name="objectName">Name of the object.</param>
		/// <param name="Type">The type.</param>
		/// <param name="Position">The position.</param>
		/// <param name="objectAccess">The object access.</param>
		/// <returns></returns>
        public CodeDelegate AddDelegate(string objectName, object Type, object Position, vsCMAccess objectAccess)
        {
            throw new NotSupportedException("Lua does not support this feature.");
        }

		/// <summary>
		/// Adds the enum.
		/// </summary>
		/// <param name="objectName">Name of the object.</param>
		/// <param name="Position">The position.</param>
		/// <param name="objectBases">The object bases.</param>
		/// <param name="objectAccess">The object access.</param>
		/// <returns></returns>
        public CodeEnum AddEnum(string objectName, object Position, object objectBases, vsCMAccess objectAccess)
        {
            throw new NotSupportedException("Lua does not support this feature.");
        }

		/// <summary>
		/// Adds the implemented interface.
		/// </summary>
		/// <param name="objectBase">The object base.</param>
		/// <param name="position">The position.</param>
		/// <returns></returns>
        public CodeInterface AddImplementedInterface(object objectBase, object position)
        {
            throw new NotSupportedException("Lua does not support this feature.");
        }

		/// <summary>
		/// Adds the property.
		/// </summary>
		/// <param name="getterName">Name of the getter.</param>
		/// <param name="putterName">Name of the putter.</param>
		/// <param name="Type">The type.</param>
		/// <param name="position">The position.</param>
		/// <param name="objectAccess">The object access.</param>
		/// <param name="objectLocation">The object location.</param>
		/// <returns></returns>
        public CodeProperty AddProperty(string getterName, string putterName, object Type, object position,
                                        vsCMAccess objectAccess, object objectLocation)
        {
            throw new NotSupportedException("Lua does not support this feature.");
        }

		/// <summary>
		/// Adds the struct.
		/// </summary>
		/// <param name="objectName">Name of the object.</param>
		/// <param name="position">The position.</param>
		/// <param name="objectBases">The object bases.</param>
		/// <param name="objectImplementedInterfaces">The object implemented interfaces.</param>
		/// <param name="objectAccess">The object access.</param>
		/// <returns></returns>
        public CodeStruct AddStruct(string objectName, object position, object objectBases,
                                    object objectImplementedInterfaces, vsCMAccess objectAccess)
        {
            throw new NotSupportedException("Lua does not support this feature.");
        }

		/// <summary>
		/// Removes the interface.
		/// </summary>
		/// <param name="Element">The element.</param>
        public void RemoveInterface(object Element)
        {
            throw new NotSupportedException("Lua does not support this feature.");
        }

		/// <summary>
		/// Adds the event.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="FullDelegateName">Full name of the delegate.</param>
		/// <param name="CreatePropertyStyleEvent">if set to <c>true</c> [create property style event].</param>
		/// <param name="Location">The location.</param>
		/// <param name="access">The access.</param>
		/// <returns></returns>
        public CodeEvent AddEvent(string name, string FullDelegateName, bool CreatePropertyStyleEvent, object Location,
                                  vsCMAccess access)
        {
            throw new NotSupportedException("Lua does not support this feature.");
        }

		/// <summary>
		/// Adds the attribute.
		/// </summary>
		/// <param name="attributeName">Name of the attribute.</param>
		/// <param name="Value">The value.</param>
		/// <param name="Position">The position.</param>
		/// <returns></returns>
        public CodeAttribute AddAttribute(string attributeName, string Value, object Position)
        {
            throw new NotSupportedException("Lua does not support this feature.");
        }

		/// <summary>
		/// Adds the base.
		/// </summary>
		/// <param name="Base">The base.</param>
		/// <param name="Position">The position.</param>
		/// <returns></returns>
        public CodeElement AddBase(object Base, object Position)
        {
            throw new NotSupportedException("Lua does not support this feature.");
        }

		/// <summary>
		/// Gets the attributes.
		/// </summary>
		/// <value>The attributes.</value>
        public CodeElements Attributes
        {
            get { throw new NotSupportedException("Lua does not support this feature."); }
        }

		/// <summary>
		/// Gets the bases.
		/// </summary>
		/// <value>The bases.</value>
        public CodeElements Bases
        {
            get { throw new NotSupportedException("Lua does not support this feature."); }
        }

		/// <summary>
		/// Gets the derived types.
		/// </summary>
		/// <value>The derived types.</value>
        public CodeElements DerivedTypes
        {
            get { throw new NotSupportedException("Lua does not support this feature."); }
        }

		/// <summary>
		/// Gets the implemented interfaces.
		/// </summary>
		/// <value>The implemented interfaces.</value>
        public CodeElements ImplementedInterfaces
        {
            get { throw new NotSupportedException("Lua does not support this feature."); }
        }

		/// <summary>
		/// Gets the namespace.
		/// </summary>
		/// <value>The namespace.</value>
        public CodeNamespace Namespace
        {
            get { throw new NotSupportedException("Lua does not support this feature."); }
        }

		/// <summary>
		/// Removes the base.
		/// </summary>
		/// <param name="element">The element.</param>
        public void RemoveBase(object element)
        {
            throw new NotSupportedException("Lua does not support this feature.");
        }

		/// <summary>
		/// Get_s the is derived from.
		/// </summary>
		/// <param name="fullName">The full name.</param>
		/// <returns></returns>
        public bool get_IsDerivedFrom(string fullName)
        {
            throw new NotSupportedException("Lua does not support this feature.");
        }

        #endregion


        /// <summary>
        /// Gets a collection of code elements contained by the class.
        /// </summary>
        public CodeElements Members
        {
            get { return childObjects; }
        }

        /// <summary>
        /// Sets or gets the access attributes of this code class.
        /// </summary>
        public vsCMAccess Access
        {
            get { return vsCMAccess.vsCMAccessPublic; }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets or sets the comment associated with the code element.
        /// </summary>
        public string Comment
        {
            get { return String.Empty; }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets or sets the document comment for the current code model element.
        /// </summary>
        public string DocComment
        {
            get { return String.Empty; }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets an enumeration that defines the type of object.
        /// </summary>
        public override vsCMElement Kind
        {
            get { return vsCMElement.vsCMElementClass; }
        }

        /// <summary>
        /// Removes a member of the class.
        /// </summary>
        /// <param name="element"></param>
        public void RemoveMember(object element)
        {
            childObjects.RemoveElement(element as CodeElement);
        }

        /// <summary>
        /// Gets the immediate parent object of the class.
        /// </summary>
        public object Parent
        {
            get { return parent; }
            internal set { parent = (CodeElement)value; }
        }

        #endregion

        #region Overriden Members

        /// <summary>
        /// Gets the immediate parent object of a <see cref="T:EnvDTE.CodeVariable" /> object.
        /// </summary>
        public override CodeElement ParentElement
        {
            get { return parent; }
        }

        /// <summary>
        /// Returns a collection of objects contained within this <see cref="T:EnvDTE.CodeElement" />.
        /// </summary>
        public override CodeElements Children
        {
            get { return childObjects; }
        }

        /// <summary>
        /// Gets the <see cref="T:EnvDTE.CodeElements" /> collection containing the CodeElement 
        /// that supports this property.
        /// </summary>
        public override CodeElements Collection
        {
            get { return Children; }
        }


        /// <summary>
        /// Gets the <see cref="T:EnvDTE.ProjectItem" /> associated with the given object.
        /// </summary>
        public override ProjectItem ProjectItem
        {
            get { return parent.ProjectItem; }
        }

        #endregion
    }
}