using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.OLE.Interop;
using ParaEngine.Tools.Lua.Parser;
using System.ComponentModel.Design;
using Microsoft;
using System.Collections.Generic;

namespace ParaEngine.Tools.Lua.Parser
{
	/// <summary>
	/// Base class for packages.
	/// </summary>
    public abstract class BasePackage : Microsoft.VisualStudio.Shell.Package, IOleComponent
    {
        protected Dictionary<string, uint> m_componets = new Dictionary<string, uint>();

        /// <summary>
        /// Initializes a new instance of the <see cref="BasePackage"/> class.
        /// </summary>
        /// <!-- Failed to insert some or all of included XML -->
        /// <include file="doc\Package.uex" path="docs/doc[@for=&quot;Package.Package&quot;]"/>
        /// <devdoc>
        /// Simple constructor.
        /// </devdoc>
        protected BasePackage()
        {
            var callback = new ServiceCreatorCallback(
                delegate(IServiceContainer container, Type serviceType)
                {
                    if (typeof(LanguageService) == serviceType)
                    {
                        var language = new LanguageService();
                        language.SetSite(this);

                        // register for idle time callbacks
                        var mgr = GetService(typeof(SOleComponentManager)) as IOleComponentManager;
                        if (!HasComponent("languageservice") && mgr != null)
                        {
                            OLECRINFO[] crinfo = new OLECRINFO[1];
                            crinfo[0].cbSize = (uint)Marshal.SizeOf(typeof(OLECRINFO));
                            crinfo[0].grfcrf = (uint)_OLECRF.olecrfNeedIdleTime |
                                               (uint)_OLECRF.olecrfNeedPeriodicIdleTime;
                            crinfo[0].grfcadvf = (uint)_OLECADVF.olecadvfModal |
                                                 (uint)_OLECADVF.olecadvfRedrawOff |
                                                 (uint)_OLECADVF.olecadvfWarningsOff;
                            crinfo[0].uIdleTimeInterval = 1000;
                            uint componentID = 0;
                            int hr = mgr.FRegisterComponent(this, crinfo, out componentID);
                            AddComponentToAutoReleasePool("languageservice", componentID);
                        }

                        return language;
                    }
                    else
                    {
                        return null;
                    }
                });

            // proffer the LanguageService
            (this as IServiceContainer).AddService(typeof(LanguageService), callback, true);
        }

        public void AddComponentToAutoReleasePool(string name, uint componentID)
        {
            if(componentID!=0)
            {
                m_componets[name] = componentID;
            }
        }

        public bool HasComponent(string name)
        {
            return m_componets.ContainsKey(name);
        }

		/// <summary>
		/// </summary>
		/// <param name="disposing"></param>
		/// <!-- Failed to insert some or all of included XML -->
		/// <include file="doc\Package.uex" path="docs/doc[@for=&quot;Package.Dispose&quot;]"/>
		/// <devdoc>
		/// This method will be called by Visual Studio in reponse
		/// to a package close (disposing will be true in this
		/// case).  The default implementation revokes all
		/// services and calls Dispose() on any created services
		/// that implement IDisposable.
		/// </devdoc>
        protected override void Dispose(bool disposing)
        {
            try
            {
                foreach (var component in m_componets)
                {
                    var mgr = GetService(typeof(SOleComponentManager)) as IOleComponentManager;
                    if (mgr != null)
                    {
                        mgr.FRevokeComponent(component.Value);
                    }
                }
                m_componets.Clear();
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        #region IOleComponent Members


		/// <summary>
		/// </summary>
		/// <param name="uReason"></param>
		/// <param name="pvLoopData"></param>
		/// <param name="pMsgPeeked"></param>
		/// <returns></returns>
        public int FContinueMessageLoop(uint uReason, IntPtr pvLoopData, MSG[] pMsgPeeked)
        {
            return 1;
        }

		/// <summary>
		/// </summary>
		/// <param name="grfidlef"></param>
		/// <returns></returns>
        public int FDoIdle(uint grfidlef)
        {
            var ls = GetService(typeof(LanguageService)) as BaseLanguageService;

            if (ls != null)
            {
                ls.OnIdle((grfidlef & (uint)_OLEIDLEF.oleidlefPeriodic) != 0);
            }

            return 0;
        }

		/// <summary>
		/// </summary>
		/// <param name="pMsg"></param>
		/// <returns></returns>
        public int FPreTranslateMessage(MSG[] pMsg)
        {
            return 0;
        }

		/// <summary>
		/// </summary>
		/// <param name="fPromptUser"></param>
		/// <returns></returns>
        public int FQueryTerminate(int fPromptUser)
        {
            return 1;
        }

		/// <summary>
		/// </summary>
		/// <param name="dwReserved"></param>
		/// <param name="message"></param>
		/// <param name="wParam"></param>
		/// <param name="lParam"></param>
		/// <returns></returns>
        public int FReserved1(uint dwReserved, uint message, IntPtr wParam, IntPtr lParam)
        {
            return 1;
        }

		/// <summary>
		/// </summary>
		/// <param name="dwWhich"></param>
		/// <param name="dwReserved"></param>
		/// <returns></returns>
        public IntPtr HwndGetWindow(uint dwWhich, uint dwReserved)
        {
            return IntPtr.Zero;
        }

		/// <summary>
		/// </summary>
		/// <param name="pic"></param>
		/// <param name="fSameComponent"></param>
		/// <param name="pcrinfo"></param>
		/// <param name="fHostIsActivating"></param>
		/// <param name="pchostinfo"></param>
		/// <param name="dwReserved"></param>
        public void OnActivationChange(IOleComponent pic, int fSameComponent, OLECRINFO[] pcrinfo, int fHostIsActivating, OLECHOSTINFO[] pchostinfo, uint dwReserved)
        {
        }

		/// <summary>
		/// </summary>
		/// <param name="fActive"></param>
		/// <param name="dwOtherThreadID"></param>
        public void OnAppActivate(int fActive, uint dwOtherThreadID)
        {
        }

		/// <summary>
		/// </summary>
		/// <param name="uStateID"></param>
		/// <param name="fEnter"></param>
        public void OnEnterState(uint uStateID, int fEnter)
        {
        }

		/// <summary>
		/// </summary>
        public void OnLoseActivation()
        {
        }

		/// <summary>
		/// </summary>
        public void Terminate()
        {
        }
        #endregion
    }
}