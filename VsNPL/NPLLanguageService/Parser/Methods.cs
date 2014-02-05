/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System.Collections.Generic;
using ParaEngine.Tools.Lua.Parser;

namespace ParaEngine.Tools.Lua.Parser
{
    public class Methods : Microsoft.VisualStudio.Package.Methods
    {
    	readonly IList<Method> methods;

		/// <summary>
		/// Initializes a new instance of the <see cref="Methods"/> class.
		/// </summary>
		/// <param name="methods">The methods.</param>
        public Methods(IList<Method> methods)
        {
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
            return (methods[index].Parameters == null) ? 0 : methods[index].Parameters.Count;
        }

		/// <summary>
		/// Gets the parameter info.
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="paramIndex">Index of the param.</param>
		/// <param name="name">The name.</param>
		/// <param name="display">The display.</param>
		/// <param name="description">The description.</param>
        public override void GetParameterInfo(int index, int paramIndex, out string name, out string display, out string description)
        {
            Parameter parameter = methods[index].Parameters[paramIndex];
            name = parameter.Name;
            display = parameter.Display;
            description = parameter.Description;
        }
    }
}