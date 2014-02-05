using System;
using Microsoft.VisualStudio.TextManager.Interop;

namespace ParaEngine.Tools.Lua.Parser
{
    public class ColorableItem : IVsColorableItem
    {
        private readonly string displayName;
        private readonly COLORINDEX background;
        private readonly COLORINDEX foreground;
        private readonly uint fontFlags = (uint)FONTFLAGS.FF_DEFAULT;

		/// <summary>
		/// Initializes a new instance of the <see cref="ColorableItem"/> class.
		/// </summary>
		/// <param name="displayName">The display name.</param>
		/// <param name="foreground">The foreground.</param>
		/// <param name="background">The background.</param>
		/// <param name="bold">if set to <c>true</c> [bold].</param>
		/// <param name="strikethrough">if set to <c>true</c> [strikethrough].</param>
        public ColorableItem(string displayName, COLORINDEX foreground, COLORINDEX background, bool bold, bool strikethrough)
        {
            this.displayName = displayName;
            this.background = background;
            this.foreground = foreground;

            if (bold)
                this.fontFlags = this.fontFlags | (uint)FONTFLAGS.FF_BOLD;
            if (strikethrough)
                this.fontFlags = this.fontFlags | (uint)FONTFLAGS.FF_STRIKETHROUGH;
        }

        #region IVsColorableItem Members

		/// <summary>
		/// Defines the default background and foreground colors for a custom colorable item.
		/// </summary>
		/// <param name="piForeground">[out] Returns an integer containing the foreground color. For more information, see <see cref="T:Microsoft.VisualStudio.TextManager.Interop.COLORINDEX"/></param>
		/// <param name="piBackground">[out] Returns an integer containing the background color. For more information, see <see cref="T:Microsoft.VisualStudio.TextManager.Interop.COLORINDEX"/></param>
		/// <returns>
		/// If the method succeeds, it returns <see cref="F:Microsoft.VisualStudio.VSConstants.S_OK"/>. If it fails, it returns an error code.
		/// </returns>
        public int GetDefaultColors(COLORINDEX[] piForeground, COLORINDEX[] piBackground)
        {
            if (null == piForeground)
            {
                throw new ArgumentNullException("piForeground");
            }
            if (0 == piForeground.Length)
            {
                throw new ArgumentOutOfRangeException("piForeground");
            }
            piForeground[0] = foreground;

            if (null == piBackground)
            {
                throw new ArgumentNullException("piBackground");
            }
            if (0 == piBackground.Length)
            {
                throw new ArgumentOutOfRangeException("piBackground");
            }
            piBackground[0] = background;

            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

		/// <summary>
		/// Returns the default font flags for the custom colorable item.
		/// </summary>
		/// <param name="pdwFontFlags">[out] Font flags for the custom colorable item (that is, bold, plain text, and so on). For more information, see <see cref="T:Microsoft.VisualStudio.TextManager.Interop.FONTFLAGS"/>.</param>
		/// <returns>
		/// If the method succeeds, it returns <see cref="F:Microsoft.VisualStudio.VSConstants.S_OK"/>. If it fails, it returns an error code.
		/// </returns>
        public int GetDefaultFontFlags(out uint pdwFontFlags)
        {
            pdwFontFlags = this.fontFlags;
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

		/// <summary>
		/// Gets the display name of the custom colorable item.
		/// </summary>
		/// <param name="pbstrName">[out] Returns a localized string containing the display name for the custom colorable item.</param>
		/// <returns>
		/// If the method succeeds, it returns <see cref="F:Microsoft.VisualStudio.VSConstants.S_OK"/>. If it fails, it returns an error code.
		/// </returns>
        public int GetDisplayName(out string pbstrName)
        {
            pbstrName = displayName;
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        #endregion
    }
}
