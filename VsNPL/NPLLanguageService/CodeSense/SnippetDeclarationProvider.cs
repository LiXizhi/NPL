using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TextManager.Interop;
using ParaEngine.NPLLanguageService;

namespace ParaEngine.Tools.Lua
{
    public class SnippetDeclarationProvider : ICodeSenseDeclarationProvider
    {
        private readonly LanguageService languageService;

		/// <summary>
		/// Initializes a new instance of the <see cref="SnippetDeclarationProvider"/> class.
		/// </summary>
		/// <param name="languageService">The language service.</param>
        public SnippetDeclarationProvider(LanguageService languageService)
        {
            if (languageService == null)
                throw new ArgumentNullException("languageService");

            this.languageService = languageService;
        }

		/// <summary>
		/// Get declarations for a CompleteWord request.
		/// </summary>
		/// <returns></returns>
        public IEnumerable<Declaration> GetCompleteWordDeclarations()
        {
            List<VsExpansion> snippets = this.GetSnippets();

            return snippets.Select(snippet => new Declaration
                                                    {
                                                        DeclarationType = DeclarationType.Snippet,
                                                        Name = snippet.shortcut,
                                                        Description = snippet.description
                                                    });
        }

		/// <summary>
		/// Get declarations for a MemberSelect request for the given qualified name.
		/// </summary>
		/// <param name="qualifiedName">The qualified name of the type.</param>
		/// <returns></returns>
        public IEnumerable<Declaration> GetMemberSelectDeclarations(string qualifiedName)
        {
            yield break;
        }

		/// <summary>
		/// Gets the methods for a MethodTip request with the given qualified name.
		/// </summary>
		/// <param name="qualifiedName"></param>
		/// <returns></returns>
        public IEnumerable<Method> GetMethods(string qualifiedName)
        {
            yield break;
        }

		/// <summary>
		/// Gets the snippets.
		/// </summary>
		/// <returns></returns>
        private List<VsExpansion> GetSnippets()
        {
            var snippets = new List<VsExpansion>();

            var textManager = (IVsTextManager)languageService.GetService(typeof(SVsTextManager));
            if (textManager != null)
            {
                var textManager2 = (IVsTextManager2)textManager;
                if (textManager2 != null)
                {
                    IVsExpansionManager expansionManager = null;
                    textManager2.GetExpansionManager(out expansionManager);
                    if (expansionManager != null)
                    {
                        // Tell the environment to fetch all of our snippets.
                        IVsExpansionEnumeration expansionEnumerator = null;
                        int ret = expansionManager.EnumerateExpansions(Guids.LuaLanguageService,
                                                                       0,     // return all info
                                                                       null,    // return all types
                                                                       0,     // return all types
                                                                       1,     // include snippets without types
                                                                       0,     // do not include duplicates
                                                                       out expansionEnumerator);
                        if (expansionEnumerator != null)
                        {
                            // Cache our expansions in an array of
                            // VSExpansion structures.
                            uint count = 0;
                            uint fetched = 0;
                            VsExpansion expansionInfo = new VsExpansion();
                            IntPtr[] pExpansionInfo = new IntPtr[1];

                            // Allocate enough memory for one VSExpansion structure.
                            // This memory is filled in by the Next method.
                            pExpansionInfo[0] = Marshal.AllocCoTaskMem(Marshal.SizeOf(expansionInfo));

                            expansionEnumerator.GetCount(out count);
                            for (uint i = 0; i < count; i++)
                            {
                                expansionEnumerator.Next(1, pExpansionInfo, out fetched);
                                if (fetched > 0)
                                {
                                    // Convert the returned blob of data into a
                                    // structure that can be read in managed code.
                                    expansionInfo = (VsExpansion)
                                                    Marshal.PtrToStructure(pExpansionInfo[0],
                                                                           typeof(VsExpansion));

                                    if (!String.IsNullOrEmpty(expansionInfo.shortcut))
                                    {
                                        snippets.Add(expansionInfo);
                                    }
                                }
                            }
                            Marshal.FreeCoTaskMem(pExpansionInfo[0]);
                        }
                    }
                }
            }

            return snippets;
        
        }
    }
}
