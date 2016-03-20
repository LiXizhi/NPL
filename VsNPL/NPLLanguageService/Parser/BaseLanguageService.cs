using System;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace ParaEngine.Tools.Lua.Parser
{
	public abstract class BaseLanguageService : Microsoft.VisualStudio.Package.LanguageService
	{
		private IScanner scanner;
		private LanguagePreferences preferences;

		#region Custom Colors

		/// <summary>
		/// Gets the colorable item.
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="item">The item.</param>
		/// <returns></returns>
		public override int GetColorableItem(int index, out IVsColorableItem item)
		{
			if (index <= Configuration.ColorableItems.Count)
			{
				item = Configuration.ColorableItems[index - 1];
				return VSConstants.S_OK;
			}

			throw new ArgumentNullException("index");
		}

		/// <summary>
		/// Gets the item count.
		/// </summary>
		/// <param name="count">The count.</param>
		/// <returns></returns>
		public override int GetItemCount(out int count)
		{
			count = Configuration.ColorableItems.Count;
			return VSConstants.S_OK;
		}

		#endregion

		#region MPF Accessor and Factory specialisation


		/// <summary>
		/// Gets the language preferences.
		/// </summary>
		/// <returns></returns>
		public override LanguagePreferences GetLanguagePreferences()
		{
			if (preferences == null)
			{
				preferences = new LanguagePreferences(Site, typeof (LanguageService).GUID, Name);
				preferences.Init();

                // Temporarily enabled auto-outlining
                // preferences.AutoOutlining = true;
            }

			return preferences;
		}

		/// <summary>
		/// Creates the source.
		/// </summary>
		/// <param name="buffer">The buffer.</param>
		/// <returns></returns>
		public override Microsoft.VisualStudio.Package.Source CreateSource(IVsTextLines buffer)
		{
			return new Source(this, buffer, GetColorizer(buffer));
		}

		/// <summary>
		/// Gets the scanner.
		/// </summary>
		/// <param name="buffer">The buffer.</param>
		/// <returns></returns>
		public override IScanner GetScanner(IVsTextLines buffer)
		{
			if (scanner == null)
                scanner = new LineScanner();

			return scanner;
		}

		#endregion

		/// <summary>
		/// Called when [idle].
		/// </summary>
		/// <param name="periodic">if set to <c>true</c> [periodic].</param>
		public override void OnIdle(bool periodic)
		{
			base.OnIdle(periodic);
		}

		/// <summary>
		/// Gets the name.
		/// </summary>
		/// <value>The name.</value>
		public override string Name
		{
			get { return Configuration.Name; }
		}
	}
}