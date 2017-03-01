using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.OLE.Interop;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using System.Drawing;

namespace NPLProject
{
    [Guid("BE2402BF-92AC-4467-9455-E9615D8F569F")]
    public class NPLPropertyPage : IPropertyPage
    {
        private readonly NPLPropertyPageControl _control;
        public NPLPropertyPage()
        {
            _control = new NPLPropertyPageControl(this);
        }

        #region IPropertyPage
        void IPropertyPage.Activate(IntPtr hWndParent, RECT[] pRect, int bModal)
        {
            NativeMethods.SetParent(_control.Handle, hWndParent);
        }

        int IPropertyPage.Apply()
        {
            return VSConstants.S_OK;
        }

        void IPropertyPage.Deactivate()
        {
            
        }

        void IPropertyPage.GetPageInfo(PROPPAGEINFO[] pPageInfo)
        {
            PROPPAGEINFO info = new PROPPAGEINFO();

            info.cb = (uint)Marshal.SizeOf(typeof(PROPPAGEINFO));
            info.dwHelpContext = 0;
            info.pszDocString = null;
            info.pszHelpFile = null;
            info.pszTitle = "General";
            info.SIZE.cx = _control.Width;
            info.SIZE.cy = _control.Height;
            pPageInfo[0] = info;
        }

        void IPropertyPage.Help(string pszHelpDir)
        {
            
        }

        int IPropertyPage.IsPageDirty()
        {
            return VSConstants.S_OK;
        }

        void IPropertyPage.Move(RECT[] pRect)
        {
            RECT r = pRect[0];

            _control.Location = new Point(r.left, r.top);
            _control.Size = new Size(r.right - r.left, r.bottom - r.top);
        }

        void IPropertyPage.SetObjects(uint cObjects, object[] ppunk)
        {
            if (ppunk == null)
            {
                return;
            }
        }

        void IPropertyPage.SetPageSite(IPropertyPageSite pPageSite)
        {
            
        }

        void IPropertyPage.Show(uint nCmdShow)
        {
            _control.Visible = true;
            _control.Show();
        }

        int IPropertyPage.TranslateAccelerator(MSG[] pMsg)
        {
            return VSConstants.S_OK;
        }
        #endregion
    }
}
