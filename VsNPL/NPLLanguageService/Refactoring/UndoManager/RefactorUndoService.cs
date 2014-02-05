using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ParaEngine.Tools.Lua.Refactoring.UndoManager
{
	/// <summary>
	/// 
	/// </summary>
	public class RefactorUndoService : ILuaUndoService
	{
		private readonly IServiceProvider provider;
		private readonly List<ILuaUndoUnit> undoUnits = new List<ILuaUndoUnit>();

		/// <summary>
		/// Initializes a new instance of the <see cref="RefactorUndoService"/> class.
		/// </summary>
		/// <param name="provider">The provider.</param>
		public RefactorUndoService(IServiceProvider provider)
		{
			if (provider == null)
				throw new ArgumentNullException("provider");

			this.provider = provider;

			AttachCommandEvents();
		}

		/// <summary>
		/// Gets the service provider.
		/// </summary>
		/// <value>The service provider.</value>
		public IServiceProvider ServiceProvider
		{
			get { return provider; }
		}

		#region ILuaUndoService Members

		/// <summary>
		/// Adds the specified unit.
		/// </summary>
		/// <param name="unit">The unit.</param>
		public virtual void Add(ILuaUndoUnit unit)
		{
			if (unit == null)
				throw new ArgumentNullException("unit");

			undoUnits.Add(unit);
		}

		/// <summary>
		/// Clears this instance.
		/// </summary>
		public virtual void Clear()
		{
			undoUnits.Clear();
		}

		/// <summary>
		/// Redo this instance.
		/// </summary>
		public virtual void Redo()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Undo this instance.
		/// </summary>
		public virtual void Undo()
		{
			Debug.WriteLine("Undo called!");

			try
			{
				foreach (ILuaUndoUnit unit in undoUnits)
				{
					unit.Do(this);
				}
			}
			finally
			{
				Clear();
			}
		}

		#endregion

		/// <summary>
		/// Called when [after execute undo event].
		/// </summary>
		/// <param name="Guid">The GUID.</param>
		/// <param name="ID">The ID.</param>
		/// <param name="CustomIn">The custom in.</param>
		/// <param name="CustomOut">The custom out.</param>
		protected virtual void OnAfterExecuteUndoEvent(string Guid, int ID, object CustomIn, object CustomOut)
		{
			Debug.WriteLine("OnAfterExecuteUndoEvent");
		}


		/// <summary>
		/// Attaches the command events.
		/// </summary>
		private static void AttachCommandEvents()
		{
			//var dte = provider.GetService(typeof(DTE)) as DTE;

			//if (dte != null)
			//{
			//    Command command = dte.Commands.Item("Edit.Undo", 0);
			//    CommandEvents events = dte.Events.get_CommandEvents(command.Guid, command.ID);
			//    events.AfterExecute += OnAfterExecuteUndoEvent;
			//}
		}

		#region IDisposable Members

		/// <summary>
		/// Performs application-defined tasks associated with freeing,
		/// releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing,
		/// releasing, or resetting unmanaged resources.
		/// </summary>
		protected virtual void Dispose(bool disposing)
		{
			try
			{
				//var dte = provider.GetService(typeof(DTE)) as DTE2;

				//if (dte != null)
				//{
				//    Command command = dte.Commands.Item("Edit.Undo", 0);
				//    CommandEvents events = dte.Events.get_CommandEvents(command.Guid, command.ID);
				//    events.AfterExecute -= OnAfterExecuteUndoEvent;
				//}
			}
			catch (Exception e)
			{
				Trace.WriteLine(e);
			}
		}

		#endregion
	}
}