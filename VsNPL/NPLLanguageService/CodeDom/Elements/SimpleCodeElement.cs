using System;
using System.Runtime.InteropServices;
using EnvDTE;
using EnvDTE80;
using ParaEngine.Tools.Lua.AST;
using ParaEngine.Tools.Lua.CodeDom.Elements;
using Microsoft.VisualStudio.TextManager.Interop;

namespace ParaEngine.Tools.Lua.CodeDom.Elements
{
    /// <summary>
    /// Base class for LuaCodeElement.
    /// </summary>
    [ComVisible(true)]
    public abstract class SimpleCodeElement : CodeElement2
    {
        protected string id;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleCodeElement"/> class.
        /// </summary>
        /// <param name="dte"></param>
        /// <param name="name"></param>
        protected SimpleCodeElement(DTE dte, string name)
        {
            DTE = dte;
            Name = name;
        }

        virtual public TextSpan GetTextSpan()
        {
            return new TextSpan();
        }

        #region CodeElement Members

            /// <summary>
            /// Returns a collection of objects contained within this <see cref="T:EnvDTE.CodeElement" />.
            /// </summary>
        public abstract CodeElements Children { get; }

        /// <summary>
        /// Gets the <see cref="T:EnvDTE.CodeElements" /> collection containing the CodeElement 
        /// that supports this property.
        /// </summary>
        public abstract CodeElements Collection { get; }

        /// <summary>
        /// Gets an enumeration that defines the type of object.
        /// </summary>
        public abstract vsCMElement Kind { get; }

        /// <summary>
        /// Gets the text point that is the location of the end of the code item.
        /// </summary>
        public abstract TextPoint EndPoint { get; }

        /// <summary>
        /// Gets a fully-qualified name of the specified code element.
        /// </summary>
        public abstract string FullName { get; }

        /// <summary>
        /// Returns a <see cref="T:EnvDTE.TextPoint" /> object that marks the beginning 
        /// of the code element definition.
        /// </summary>
        /// <param name="part">Optional. A <see cref="T:EnvDTE.vsCMPart" /> constant specifying 
        /// the portion of the code to retrieve.</param>
        /// <returns>A <see cref="T:EnvDTE.TextPoint" /> object.</returns>
        public abstract TextPoint GetEndPoint(vsCMPart part);

        /// <summary>
        /// Returns a <see cref="T:EnvDTE.TextPoint" /> object that marks the end 
        /// of the code element definition.
        /// </summary>
        /// <param name="part">Optional. A <see cref="T:EnvDTE.vsCMPart" /> constant specifying 
        /// the portion of the code to retrieve.</param>
        /// <returns>A <see cref="T:EnvDTE.TextPoint" /> object.</returns>
        public abstract TextPoint GetStartPoint(vsCMPart part);

        /// <summary>
        /// Gets the <see cref="T:EnvDTE.ProjectItem" /> object associated 
        /// with the <see cref="T:EnvDTE.CodeElement" /> object.
        /// </summary>
        public abstract ProjectItem ProjectItem { get; }

        /// <summary>
        /// Gets a text point object that defines the beginning of the code item.
        /// </summary>
        public abstract TextPoint StartPoint { get; }

        /// <summary>
        /// Gets the top-level extensibility object.
        /// </summary>
        public DTE DTE { get; private set; }

        /// <summary>
        /// Gets the Extender category ID (CATID) for the object.
        /// </summary>
        public string ExtenderCATID
        {
            get { return String.Empty; }
        }

        /// <summary>
        /// Gets a list of available Extenders for the object.
        /// </summary>
        public object ExtenderNames
        {
            get { return new object[0]; }
        }

        /// <summary>
        /// Gets the capabilities of the code model.
        /// </summary>
        public virtual vsCMInfoLocation InfoLocation
        {
            get { return vsCMInfoLocation.vsCMInfoLocationProject; }
        }

        /// <summary>
        /// Indicates whether or not a <see cref="T:EnvDTE.CodeType" /> object can be 
        /// obtained from the <see cref="T:EnvDTE.CodeElement" /> object.
        /// </summary>
        public virtual bool IsCodeType
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the programming language that is used to author the code.
        /// </summary>
        public string Language
        {
            get { return "Lua"; }
        }

        /// <summary>
        /// Sets or gets the name of the object.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the available extender for the object by name.
        /// </summary>
        public object get_Extender(string extenderName)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Common protected helpers

		/// <summary>
		/// Objects to type ref.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns></returns>
        protected CodeTypeRef ObjectToTypeRef(object type)
        {
			if (null == type)
				throw new ArgumentNullException("type");

			var ctr = type as CodeTypeRef;
            if (ctr != null) return ctr;

			if (type is int)
				type = (LuaType) (int) type;

			if (type is LuaDeclaredType)
				return new LuaCodeTypeRef(DTE, type as LuaDeclaredType);

			var stringType = type as string;

			if (stringType != null)
				return new LuaCodeTypeRef(DTE, LuaDeclaredType.Find(stringType));

			throw new InvalidOperationException(String.Format("Unknown type to get type from: {0} ({1})", type.GetType(),
                                                              type));
        }

        #endregion

        #region CodeElement2 Members

        /// <summary>
        /// Gets a value that uniquely identifies the element.
        /// </summary>
        public string ElementID
        {
            get
            {
                if (id == null) id = new Guid().ToString();
                return id;
            }
        }

        /// <summary>
        /// Changes the declared name of an object and updates all code references 
        /// to the object within the scope of the current project.
        /// </summary>
        /// <param name="newName">Required. The name of the symbol to rename.</param>
        public virtual void RenameSymbol(string newName)
        {
            Name = newName;
        }

        #endregion
    }
}