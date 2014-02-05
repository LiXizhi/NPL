namespace ParaEngine.Tools.Lua
{
    /// <summary>
    /// The types of declarations CodeSense can provide.
    /// </summary>
    public enum DeclarationType
    {
        /// <summary>
        /// A globally or locally scoped Lua variable.
        /// </summary>
        Variable,
        /// <summary>
        /// A Lua table.
        /// </summary>
        Table,
        /// <summary>
        /// A Lua table field.
        /// </summary>
        Field,
        /// <summary>
        /// A Lua function.
        /// </summary>
        Function,
        /// <summary>
        /// A Lua function parameter.
        /// </summary>
        Parameter,
        /// <summary>
        /// A Lua keyword.
        /// </summary>
        Keyword,
        /// <summary>
        /// A Lua code snippet.
        /// </summary>
        Snippet
    }
}
