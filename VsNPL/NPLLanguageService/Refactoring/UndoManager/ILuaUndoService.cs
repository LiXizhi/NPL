using System;

namespace ParaEngine.Tools.Lua.Refactoring.UndoManager
{
	/// <summary>
	/// 
	/// </summary>
	public interface ILuaUndoService : IDisposable
	{
		/// <summary>
		/// Gets the service provider.
		/// </summary>
		/// <value>The service provider.</value>
		IServiceProvider ServiceProvider { get; }

		/// <summary>
		/// Adds the specified unit.
		/// </summary>
		/// <param name="unit">The unit.</param>
		void Add(ILuaUndoUnit unit);

		/// <summary>
		/// Clears this instance.
		/// </summary>
		void Clear();

		/// <summary>
		/// Redothis instance.
		/// </summary>
		void Redo();

		/// <summary>
		/// Undo this instance.
		/// </summary>
		void Undo();
	}
}