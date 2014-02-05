using System;
using System.Collections.Generic;
namespace ParaEngine.Tools.Lua.Parser
{
    /// <summary>
    /// Helper class used by the shift-reduce parser
    /// </summary>
    public class ParserStack<T> : Stack<T>
    {
        #region Properties used from generated parts

		/// <summary>
		/// Gets the top.
		/// </summary>
		/// <value>The top.</value>
        public int top
        {
            get { return this.Count; }
        }

		/// <summary>
		/// Gets the array.
		/// </summary>
		/// <value>The array.</value>
        public T[] array
        {
            get
            {
                T[] result = ToArray();
                Array.Reverse(result);
                return result;
            }
        }

		/// <summary>
		/// Tops this instance.
		/// </summary>
		/// <returns></returns>
        public T Top()
        {
            return Peek();
        }

        #endregion
    }
}