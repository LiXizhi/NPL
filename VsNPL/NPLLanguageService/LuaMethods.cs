using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Package;

namespace ParaEngine.Tools.Lua
{
	/// <summary>
	/// 
	/// </summary>
    public class LuaMethods : Methods
    {
        private readonly IList<Method> methods;

		/// <summary>
		/// Initializes a new instance of the <see cref="LuaMethods"/> class.
		/// </summary>
		/// <param name="methods">The methods.</param>
        public LuaMethods(IList<Method> methods)
        {
            if (methods == null)
                throw new ArgumentNullException("methods");

            this.methods = methods;
        }

		/// <summary>
		/// When implemented in a derived class, gets the number of overloaded method signatures represented in this collection.
		/// </summary>
		/// <returns>
		/// The number of signatures in the collection.
		/// </returns>
        public override int GetCount()
        {
            return methods.Count;
        }

		/// <summary>
		/// When implemented in a derived class, gets the description of the specified method signature.
		/// </summary>
		/// <param name="index">[in] An index into the internal list to the desired method signature.</param>
		/// <returns>
		/// The description of the specified method signature, or null if the method signature does not exist.
		/// </returns>
        public override string GetDescription(int index)
        {
            return methods[index].Description;
        }

		/// <summary>
		/// When implemented in a derived class, gets the name of the specified method signature.
		/// </summary>
		/// <param name="index">[in] The index of the method whose name is to be returned.</param>
		/// <returns>
		/// The name of the specified method, or null.
		/// </returns>
        public override string GetName(int index)
        {
            return methods[index].Name;
        }

		/// <summary>
		/// When implemented in a derived class, gets the return type of the specified method signature.
		/// </summary>
		/// <param name="index">[in] An index into the list of method signatures.</param>
		/// <returns>
		/// The return type of the specified method signature, or null.
		/// </returns>
        public override string GetType(int index)
        {
            return methods[index].Type;
        }

		/// <summary>
		/// When implemented in a derived class, gets the number of parameters on the specified method signature.
		/// </summary>
		/// <param name="index">[in] An index into the list of method signatures.</param>
		/// <returns>
		/// The number of parameters on the specified method signature, or -1.
		/// </returns>
        public override int GetParameterCount(int index)
        {
            return methods[index].Parameters != null ? methods[index].Parameters.Count : 0;
        }

		/// <summary>
		/// Gets the parameter info.
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="parameterIndex">Index of the parameter.</param>
		/// <param name="name">The name.</param>
		/// <param name="display">The display.</param>
		/// <param name="description">The description.</param>
        public override void GetParameterInfo(int index, int parameterIndex, out string name, out string display, out string description)
        {
            Parameter parameter = methods[index].Parameters[parameterIndex];

            name = parameter.Name;

            display = parameter.Display;

            // Add leading space if not first parameter
            if (parameterIndex > 0)
                display = display.Insert(0, " ");

            description = parameter.Description;
        }
    }
}
