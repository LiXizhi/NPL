using System;
using ParaEngine.Tools.Lua.Refactoring.RenameService;

namespace ParaEngine.Tools.Lua.Refactoring.RenameService
{
    /// <summary>
    /// 
    /// </summary>
	[Serializable]
    public class InvalidStrategyException: Exception
    {
		/// <summary>
		/// 
		/// </summary>
		[NonSerialized]
        private readonly IRenameStrategy strategy;
		/// <summary>
		/// Gets the strategy.
		/// </summary>
		/// <value>The strategy.</value>
        internal IRenameStrategy Strategy
        {
            get { return strategy; }
        }
		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidStrategyException"/> class.
		/// </summary>
        public InvalidStrategyException()
		{
        }
		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidStrategyException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
        public InvalidStrategyException(string message)
            : base(message)
        {
        }
		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidStrategyException"/> class.
		/// </summary>
		/// <param name="strategy">The strategy.</param>
        internal InvalidStrategyException(IRenameStrategy strategy)
            : this(String.Empty, strategy)
        {
        }
		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidStrategyException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="strategy">The strategy.</param>
        internal InvalidStrategyException(string message, IRenameStrategy strategy)
            : base(message)
        {
            this.strategy = strategy;
        }
		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidStrategyException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="innerException">The inner exception.</param>
        public InvalidStrategyException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}