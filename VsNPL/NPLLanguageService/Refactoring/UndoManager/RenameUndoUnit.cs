using EnvDTE;
using ParaEngine.Tools.Lua.CodeDom;
using ParaEngine.Tools.Lua.CodeDom.Elements;

namespace ParaEngine.Tools.Lua.Refactoring.UndoManager
{
	/// <summary>
	/// 
	/// </summary>
	public class RenameUndoUnit : ILuaUndoUnit
	{
		private readonly IRenameResult renameResult;

		/// <summary>
		/// Initializes a new instance of the <see cref="RenameUndoUnit"/> class.
		/// </summary>
		/// <param name="renameResult">The rename result.</param>
		public RenameUndoUnit(IRenameResult renameResult)
		{
			this.renameResult = renameResult;
		}

		#region ILuaUndoUnit Members

		/// <summary>
		/// Does the specified undo service.
		/// </summary>
		/// <param name="luaUndoService">The undo service.</param>
		public void Do(ILuaUndoService luaUndoService)
		{
			if(renameResult != null && renameResult.Parents != null)
			{
				foreach (SimpleCodeElement element in renameResult.ChangedElements)
				{
					element.RenameSymbol(renameResult.OldName);
				}
				foreach (FileCodeModel codeModel in renameResult.Parents)
				{
					((LuaFileCodeModel)codeModel).Dirty = true;
				}
			}
		}

		/// <summary>
		/// Gets the description.
		/// </summary>
		/// <value>The description.</value>
		public string Description
		{
			get { return "Undo Rename"; }
		}

		#endregion
	}
}