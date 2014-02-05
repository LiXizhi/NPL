/***************************************************************************

Copyright (c) 2006 Microsoft Corporation. All rights reserved.

***************************************************************************/

using System;
using ParaEngine.Tools.Lua.SourceOutliner;

namespace ParaEngine.Tools.Lua.SourceOutliner
{
    /// <summary>
    /// Class that provides an index table that represents the results of the search filter.
    /// </summary>
    class ResultsTable : IndexTable
    {
        /// <summary>
        /// Initializes a new instance of the ResultsTable class.
        /// </summary>
        /// <param name="indexTable">A CodeElementWrapperArrayIndexTable object to initialize from.</param>
        public ResultsTable(CodeElementWrapperArrayIndexTable indexTable) : base(indexTable)
        {
            Reset();
        }

        /// <summary>
        /// Applies a filter string to the index table.
        /// </summary>
        /// <param name="str">The user-typed filter text.</param>
        public void ApplyText(string str)
        {
            base.FilterTable(str);
        }
    }
}