using System;
using System.Collections.Generic;
using ParaEngine.Tools.Lua.Parser;

namespace Babel
{
    class Resolver : IASTResolver
    {
		/// <summary>
		/// Finds the completions.
		/// </summary>
		/// <param name="result">The result.</param>
		/// <param name="line">The line.</param>
		/// <param name="col">The col.</param>
		/// <returns></returns>
        public IList<Declaration> FindCompletions(object result, int line, int col)
        {
            return new Declaration[0];
        }

		/// <summary>
		/// Finds the members.
		/// </summary>
		/// <param name="result">The result.</param>
		/// <param name="line">The line.</param>
		/// <param name="col">The col.</param>
		/// <returns></returns>
        public IList<Declaration> FindMembers(object result, int line, int col)
        {
            return new Declaration[0];
        }

		/// <summary>
		/// Finds the quick info.
		/// </summary>
		/// <param name="result">The result.</param>
		/// <param name="line">The line.</param>
		/// <param name="col">The col.</param>
		/// <returns></returns>
        public string FindQuickInfo(object result, int line, int col)
        {
            return String.Empty;
        }

		/// <summary>
		/// Finds the methods.
		/// </summary>
		/// <param name="result">The result.</param>
		/// <param name="line">The line.</param>
		/// <param name="col">The col.</param>
		/// <param name="name">The name.</param>
		/// <returns></returns>
        public IList<Method> FindMethods(object result, int line, int col, string name)
        {
            return new Method[0];
        }
    }
}
