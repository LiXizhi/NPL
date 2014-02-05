namespace ParaEngine.Tools.Lua.Refactoring.UndoManager
{
	/// <summary>
	/// 
	/// </summary>
	public interface ILuaUndoUnit
	{
		/// <summary>
		/// Does the specified undo service.
		/// </summary>
		/// <param name="luaUndoService">The undo service.</param>
		void Do(ILuaUndoService luaUndoService);

		/// <summary>
		/// Gets the description.
		/// </summary>
		/// <value>The description.</value>
		string Description { get; }

	}
}