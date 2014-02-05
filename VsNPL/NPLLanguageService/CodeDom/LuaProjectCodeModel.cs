using System;
using EnvDTE;

namespace ParaEngine.Tools.Lua.CodeDom
{
    /// <summary>
    /// Represents a CodeModel for WowProjectItem.
    /// (Not supported by WowAddOnStudio)
    /// </summary>
    public sealed class LuaProjectCodeModel : CodeModel
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="LuaProjectCodeModel"/> class.
        /// </summary>
        /// <param name="project"></param>
        internal LuaProjectCodeModel(Project project)
        {
            Parent = project;
        }

        #region CodeModel interface

		/// <summary>
		/// Adds the attribute.
		/// </summary>
		/// <param name="Name">The name.</param>
		/// <param name="Location">The location.</param>
		/// <param name="Value">The value.</param>
		/// <param name="Position">The position.</param>
		/// <returns></returns>
        public CodeAttribute AddAttribute(string Name, object Location, string Value, object Position)
        {
            throw new NotImplementedException();
        }

		/// <summary>
		/// Adds the class.
		/// </summary>
		/// <param name="Name">The name.</param>
		/// <param name="Location">The location.</param>
		/// <param name="Position">The position.</param>
		/// <param name="Bases">The bases.</param>
		/// <param name="ImplementedInterfaces">The implemented interfaces.</param>
		/// <param name="Access">The access.</param>
		/// <returns></returns>
        public CodeClass AddClass(string Name, object Location, object Position, object Bases,
                                  object ImplementedInterfaces, vsCMAccess Access)
        {
            throw new NotImplementedException();
        }

		/// <summary>
		/// Adds the delegate.
		/// </summary>
		/// <param name="Name">The name.</param>
		/// <param name="Location">The location.</param>
		/// <param name="Type">The type.</param>
		/// <param name="Position">The position.</param>
		/// <param name="Access">The access.</param>
		/// <returns></returns>
        public CodeDelegate AddDelegate(string Name, object Location, object Type, object Position, vsCMAccess Access)
        {
            throw new NotImplementedException();
        }

		/// <summary>
		/// Adds the enum.
		/// </summary>
		/// <param name="Name">The name.</param>
		/// <param name="Location">The location.</param>
		/// <param name="Position">The position.</param>
		/// <param name="Bases">The bases.</param>
		/// <param name="Access">The access.</param>
		/// <returns></returns>
        public CodeEnum AddEnum(string Name, object Location, object Position, object Bases, vsCMAccess Access)
        {
            throw new NotImplementedException();
        }

		/// <summary>
		/// Adds the function.
		/// </summary>
		/// <param name="Name">The name.</param>
		/// <param name="Location">The location.</param>
		/// <param name="Kind">The kind.</param>
		/// <param name="Type">The type.</param>
		/// <param name="Position">The position.</param>
		/// <param name="Access">The access.</param>
		/// <returns></returns>
        public CodeFunction AddFunction(string Name, object Location, vsCMFunction Kind, object Type, object Position,
                                        vsCMAccess Access)
        {
            throw new NotImplementedException();
        }

		/// <summary>
		/// Adds the interface.
		/// </summary>
		/// <param name="Name">The name.</param>
		/// <param name="Location">The location.</param>
		/// <param name="Position">The position.</param>
		/// <param name="Bases">The bases.</param>
		/// <param name="Access">The access.</param>
		/// <returns></returns>
        public CodeInterface AddInterface(string Name, object Location, object Position, object Bases, vsCMAccess Access)
        {
            throw new NotImplementedException();
        }

		/// <summary>
		/// Adds the namespace.
		/// </summary>
		/// <param name="Name">The name.</param>
		/// <param name="Location">The location.</param>
		/// <param name="Position">The position.</param>
		/// <returns></returns>
        public CodeNamespace AddNamespace(string Name, object Location, object Position)
        {
            throw new NotImplementedException();
        }

		/// <summary>
		/// Adds the struct.
		/// </summary>
		/// <param name="Name">The name.</param>
		/// <param name="Location">The location.</param>
		/// <param name="Position">The position.</param>
		/// <param name="Bases">The bases.</param>
		/// <param name="ImplementedInterfaces">The implemented interfaces.</param>
		/// <param name="Access">The access.</param>
		/// <returns></returns>
        public CodeStruct AddStruct(string Name, object Location, object Position, object Bases,
                                    object ImplementedInterfaces, vsCMAccess Access)
        {
            throw new NotImplementedException();
        }

		/// <summary>
		/// Adds the variable.
		/// </summary>
		/// <param name="Name">The name.</param>
		/// <param name="Location">The location.</param>
		/// <param name="Type">The type.</param>
		/// <param name="Position">The position.</param>
		/// <param name="Access">The access.</param>
		/// <returns></returns>
        public CodeVariable AddVariable(string Name, object Location, object Type, object Position, vsCMAccess Access)
        {
            throw new NotImplementedException();
        }

		/// <summary>
		/// Codes the full name of the type from.
		/// </summary>
		/// <param name="Name">The name.</param>
		/// <returns></returns>
        public CodeType CodeTypeFromFullName(string Name)
        {
            throw new NotImplementedException();
        }

		/// <summary>
		/// Creates the code type ref.
		/// </summary>
		/// <param name="Type">The type.</param>
		/// <returns></returns>
        public CodeTypeRef CreateCodeTypeRef(object Type)
        {
            throw new NotImplementedException();
        }

		/// <summary>
		/// Determines whether [is valid ID] [the specified name].
		/// </summary>
		/// <param name="Name">The name.</param>
		/// <returns>
		/// 	<c>true</c> if [is valid ID] [the specified name]; otherwise, <c>false</c>.
		/// </returns>
        public bool IsValidID(string Name)
        {
            throw new NotImplementedException();
        }

		/// <summary>
		/// Removes the specified element.
		/// </summary>
		/// <param name="Element">The element.</param>
        public void Remove(object Element)
        {
            throw new NotImplementedException();
        }

		/// <summary>
		/// Gets the code elements.
		/// </summary>
		/// <value>The code elements.</value>
        public CodeElements CodeElements
        {
            get { throw new NotImplementedException(); }
        }

		/// <summary>
		/// Gets the DTE.
		/// </summary>
		/// <value>The DTE.</value>
        public DTE DTE
        {
            get { return Parent.DTE; }
        }

		/// <summary>
		/// Gets a value indicating whether this instance is case sensitive.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is case sensitive; otherwise, <c>false</c>.
		/// </value>
        public bool IsCaseSensitive
        {
            get { return false; }
        }

		/// <summary>
		/// Gets the language.
		/// </summary>
		/// <value>The language.</value>
        public string Language
        {
            get { return "Lua"; }
        }

		/// <summary>
		/// Gets or sets the parent.
		/// </summary>
		/// <value>The parent.</value>
        public Project Parent { get; private set; }

        #endregion
    }
}