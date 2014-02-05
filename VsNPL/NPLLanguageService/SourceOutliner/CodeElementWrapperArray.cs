/***************************************************************************

Copyright (c) 2006 Microsoft Corporation. All rights reserved.

***************************************************************************/

using System;
using System.Collections.Generic;

namespace ParaEngine.Tools.Lua.SourceOutliner
{
    /// <summary>
    /// Class that provides a list of CodeElementWrappers.
    /// </summary>
    [CLSCompliant(false)]
    public class CodeElementWrapperArray : List<CodeElementWrapper>
    {
        private static readonly CodeElementWrapperElementIDComparer comparer = new CodeElementWrapperElementIDComparer();
        private bool sorted;

        /// <summary>
        /// Adds a CodeElementWrapper object to the list.
        /// </summary>
        /// <param name="codeElementWrapper">The CodeElementWrapper to add.</param>
        public void AddCodeElementWrapper(CodeElementWrapper codeElementWrapper)
        {
            sorted = false;
            base.Add(codeElementWrapper);
        }

        /// <summary>
        /// Sorts the list alphabetically by UniqueElementID.
        /// </summary>
        public new void Sort()
        {
            if (sorted)
                return;

            base.Sort(comparer);
            sorted = true;
        }

        /// <summary>
        /// Finds a CodeElementWrapper object in the list.
        /// The object found is the one with the same UniqueElementID as the parameter.
        /// </summary>
        /// <param name="element">The CodeElementWrapper to find.</param>
        /// <returns>The found CodeElementWrapper or null if not found.</returns>
        public CodeElementWrapper FindCodeElementWrapper(CodeElementWrapper element)
        {
            Sort();

            int index = BinarySearch(element, comparer);
            if (index >= 0)
            {
                return this[index];
            }

            return null;
        }

        /// <summary>
        /// Class that compares two CodeElements for equivalence. 
        /// </summary>
        private class CodeElementWrapperElementIDComparer : Comparer<CodeElementWrapper>
        {
            /// <summary>
            /// Compares two CodeElements by UniqueElementId.
            /// </summary>
            /// <param name="X">The first CodeElementWrapper for the comparison.</param>
            /// <param name="Y">The second CodeElementWrapper for the comparison.</param>
            /// <returns>
            /// Negative if X is less than Y, else 0 if X and Y are equal, else positive if X is greater than Y.
            /// </returns>
            public override int Compare(CodeElementWrapper X, CodeElementWrapper Y)
            {
                if (X == null)
                {
                    throw new ArgumentNullException("X");
                }
                if (Y == null)
                {
                    throw new ArgumentNullException("Y");
                }

                string strElementIDX = X.UniqueElementId;
                string strElementIDY = Y.UniqueElementId;

                return string.Compare(strElementIDX, strElementIDY);
            }
        }
    }
}