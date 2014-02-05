using System;
using System.Collections.Generic;

namespace ParaEngine.Tools.Lua
{
    /// <summary>
    /// Allows two enumerable collections to be enumerated in parallel.
    /// </summary>
    /// <typeparam name="TLeft"></typeparam>
    /// <typeparam name="TRight"></typeparam>
    public class ParallelEnumerator<TFirst, TSecond>
    {
        private readonly IEnumerator<TFirst> firstEnumerator;
        private readonly IEnumerator<TSecond> secondEnumerator;

		/// <summary>
		/// Initializes a new instance of the <see cref="ParallelEnumerator&lt;TFirst, TSecond&gt;"/> class.
		/// </summary>
		/// <param name="first">The first.</param>
		/// <param name="second">The second.</param>
        public ParallelEnumerator(IEnumerable<TFirst> first, IEnumerable<TSecond> second)
        {
            if (first == null)
                throw new ArgumentNullException("first");
            if (second == null)
                throw new ArgumentNullException("second");

            this.firstEnumerator = first.GetEnumerator();
            this.secondEnumerator = second.GetEnumerator();
        }

        /// <summary>
        /// Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <returns>
        /// true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
        public bool MoveNext()
        {
            return firstEnumerator.MoveNext() && secondEnumerator.MoveNext();
        }

        /// <summary>
        /// Gets the current element in the first collection.
        /// </summary>
        /// <value></value>
        /// <returns>The current element in the first collection.</returns>
        /// <exception cref="T:System.InvalidOperationException">The enumerator is positioned before the first element of the collection or after the last element.-or- The collection was modified after the enumerator was created.</exception>
        public TFirst CurrentFirst
        {
            get { return firstEnumerator.Current; }
        }

        /// <summary>
        /// Gets the current element in the second collection.
        /// </summary>
        /// <value></value>
        /// <returns>The current element in the collection.</returns>
        /// <exception cref="T:System.InvalidOperationException">The enumerator is positioned before the first element of the collection or after the last element.-or- The collection was modified after the enumerator was created.</exception>
        public TSecond CurrentSecond
        {
            get { return secondEnumerator.Current; }
        }

        /// <summary>
        /// Sets the enumerator to its initial position, which is before the first element in the collection.
        /// </summary>
        /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
        public void Reset()
        {
            firstEnumerator.Reset();
            secondEnumerator.Reset();
        }
    
    }
}
