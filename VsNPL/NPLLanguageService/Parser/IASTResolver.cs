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
    interface IASTResolver
    {
		/// <summary>
		/// Finds the completions.
		/// </summary>
		/// <param name="result">The result.</param>
		/// <param name="line">The line.</param>
		/// <param name="col">The col.</param>
		/// <returns></returns>
        IList<Declaration> FindCompletions(object result, int line, int col);

		/// <summary>
		/// Finds the members.
		/// </summary>
		/// <param name="result">The result.</param>
		/// <param name="line">The line.</param>
		/// <param name="col">The col.</param>
		/// <returns></returns>
        IList<Declaration> FindMembers(object result, int line, int col);

		/// <summary>
		/// Finds the quick info.
		/// </summary>
		/// <param name="result">The result.</param>
		/// <param name="line">The line.</param>
		/// <param name="col">The col.</param>
		/// <returns></returns>
        string FindQuickInfo(object result, int line, int col);

		/// <summary>
		/// Finds the methods.
		/// </summary>
		/// <param name="result">The result.</param>
		/// <param name="line">The line.</param>
		/// <param name="col">The col.</param>
		/// <param name="name">The name.</param>
		/// <returns></returns>
        IList<Method> FindMethods(object result, int line, int col, string name);
    }
}