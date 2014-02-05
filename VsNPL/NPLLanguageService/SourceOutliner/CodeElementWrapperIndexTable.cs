/***************************************************************************

Copyright (c) 2006 Microsoft Corporation. All rights reserved.

***************************************************************************/

using System.Diagnostics;

namespace ParaEngine.Tools.Lua.SourceOutliner
{
    /// <summary>
    /// Class that provides an IndexTable for the CodeElementWrapperArray.
    /// </summary>
    internal class CodeElementWrapperIndexTable : IndexTable
    {
        private readonly CodeElementWrapperArray codeElementArray;

        /// <summary>
        /// Initializes a new instance of the CodeElementWrapperIndexTable class.
        /// </summary>
        /// <param name="a">The CodeElementWrapperArray object to be indexed.</param>
        public CodeElementWrapperIndexTable(CodeElementWrapperArray a)
            : base(new CodeElementWrapperArrayIndexTable(a))
        {
            codeElementArray = a;

            // Add all entries to the base.indexTable.
            for (int i = 0; i < a.Count; i++)
            {
                indexTable.Add(i);
            }
            indexTable.Sort(new SortCriteria(SortCriteria.SortType.ALPHA));
            Reset();
        }

        /// <summary>
        /// Applies a SearchCriteria to the index.
        /// </summary>
        /// <param name="crit">The SearchCriteria to apply.</param>
        /// <returns>A ResultsTable that contains the result of applying the criteria.</returns>
        public ResultsTable GenerateResultsTable(SearchCriteria crit)
        {
            Debug.Assert(crit != null);

            CodeElementWrapperArrayIndexTable idxTable;

            // If using all criteria, perform a copy.
            if (crit.ElementFilter == CodeElementType.All)
            {
                idxTable = (CodeElementWrapperArrayIndexTable) indexTable.Clone();
            }
            else
            {
                idxTable = new CodeElementWrapperArrayIndexTable(codeElementArray);

                for (int i = 0; i < codeElementArray.Count; i++)
                {
                    if (codeElementArray[i].ElementType == crit.ElementFilter)
                    {
                        idxTable.Add(i);
                    }
                }

                idxTable.Sort(new SortCriteria(SortCriteria.SortType.ALPHA));
            }

            var ret = new ResultsTable(idxTable);
            return (ret);
        }
    }
}