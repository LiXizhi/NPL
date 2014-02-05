using System;
using System.Runtime.InteropServices;
using EnvDTE;
using EnvDTE80;
using ParaEngine.Tools.Lua.AST;
using ParaEngine.Tools.Lua.CodeDom.Elements;

namespace ParaEngine.Tools.Lua.CodeDom.Elements
{
    /// <summary>
    /// An object defining the type of a construct in a Lua source file.
    /// </summary>
    [ComVisible(true)]
    public class LuaCodeTypeRef : LuaCodeElement<LuaDeclaredType>, CodeTypeRef2
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LuaCodeTypeRef"/> class.
        /// </summary>
        /// <param name="dte"></param>
        /// <param name="luaDeclaredType"></param>
        public LuaCodeTypeRef(DTE dte, LuaDeclaredType luaDeclaredType)
            : base(dte, luaDeclaredType.Name, luaDeclaredType)
        {
        }


        /// <summary>
        /// Converts LuaDeclaredType to LuaCodeTypeRef.
        /// </summary>
        /// <param name="typeRef">LuaDeclaredType instance.</param>
        /// <returns></returns>
        public static LuaCodeTypeRef FromCodeTypeReference(LuaDeclaredType typeRef)
        {
            if (null == typeRef)
            {
                throw new ArgumentNullException("typeRef");
            }
            return new LuaCodeTypeRef(null, typeRef);
        }

        #region CodeTypeRef Members

        /// <summary>
        /// Gets a fully-qualified name of the specified code element.
        /// </summary>
        public string AsFullName
        {
            get { return AsString; }
        }

        /// <summary>
        /// Gets a string to use for displaying the <see cref="T:EnvDTE.CodeTypeRef" /> object.
        /// </summary>
        public string AsString
        {
            get { return Name; }
        }

        /// <summary>
        /// Sets or gets information describing this item's kind of <see cref="T:EnvDTE.CodeTypeRef" /> object.
        /// </summary>
        public CodeType CodeType
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Creates an array of a specified type, and inserts it into the code in the correct location.
        /// </summary>
        /// <param name="rank">Optional. Default value = 1. The number of dimensions in the type array.</param>
        /// <returns>A <see cref="T:EnvDTE.CodeTypeRef" /> object.</returns>
        public CodeTypeRef CreateArrayType(int rank)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets or gets an object representing the programmatic type.
        /// </summary>
        public CodeTypeRef ElementType
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the immediate parent object of a <see cref="T:EnvDTE.CodeTypeRef" /> object.
        /// </summary>
        public object Parent
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// If this item is an array, sets or gets the number of dimensions in this array.
        /// </summary>
        public int Rank
        {
            get { return 1; }
            set {  }
        }

        /// <summary>
        /// Gets the base type of the <see cref="T:EnvDTE.CodeTypeRef" />.
        /// </summary>
        public vsCMTypeRef TypeKind
        {
            get { return vsCMTypeRef.vsCMTypeRefOther; }
        }

        #region CodeTypeRef2 Members

        /// <summary>
        /// Gets a value indicating whether the current class is a generic.
        /// </summary>
        public bool IsGeneric
        {
            get { return false; }
        }

        #endregion
        
        #endregion

        #region Overriden Members

        /// <summary>
        /// Gets the immediate parent object of a <see cref="T:EnvDTE.CodeVariable" /> object.
        /// </summary>
        public override CodeElement ParentElement
        {
            get { return null; }
        }

        /// <summary>
        /// Returns a collection of objects contained within this <see cref="T:EnvDTE.CodeElement" />.
        /// </summary>
        public override CodeElements Children
        {
            get { return null; }
        }

        /// <summary>
        /// Gets the <see cref="T:EnvDTE.CodeElements" /> collection containing the CodeElement 
        /// that supports this property.
        /// </summary>
        public override CodeElements Collection
        {
            get { return null; }
        }

        /// <summary>
        /// Gets a fully-qualified name of the specified code element.
        /// </summary>
        public override string FullName
        {
            get { return LuaASTNodeObject.Name; }
        }

        /// <summary>
        /// Gets the capabilities of the code model.
        /// </summary>
        public override vsCMInfoLocation InfoLocation
        {
            get { return vsCMInfoLocation.vsCMInfoLocationNone; }
        }

        /// <summary>
        /// Gets an enumeration that defines the type of object.
        /// </summary>
        public override vsCMElement Kind
        {
            get { return vsCMElement.vsCMElementTypeDef; }
        }

        /// <summary>
        /// Gets the <see cref="T:EnvDTE.ProjectItem" /> object associated with the 
        /// <see cref="T:EnvDTE.CodeVariable" /> object.
        /// </summary>
        public override ProjectItem ProjectItem
        {
            get { return null; }
        }

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
        public override string ToString()
        {
            return AsString;
        }

        #endregion
    }
}