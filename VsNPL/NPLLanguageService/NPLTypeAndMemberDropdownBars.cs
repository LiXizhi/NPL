using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using ParaEngine.Tools.Lua.CodeDom;
using EnvDTE;
using ParaEngine.Tools.Lua.CodeDom.Elements;

namespace ParaEngine.Tools.Lua
{
    /// <summary>
    /// Manages the dropdown lists in the code editor navigation bar
    /// </summary>
    /// <remarks>
    /// The lists are rebuilt in response to the recompiled event fired by the Source object.
    /// The list of types is a flat list of all type/struct/enum definitions present in the source file
    /// The member list includes members of all types from the type list as well as members of the module
    /// Selection of a type/type member from the list positions caret on the appropriate point in the source
    /// The members of the currently selected type are shown in regular font. Members of all other types are shown in grey
    /// Currently selected member is shown in bold.
    /// As caret is moved through the source code, the highlighting of the member list is modified accordingly
    /// The selected type is the last type on the type list containing the current caret position. This implies
    /// that the nested types are listed with outer type first
    /// </remarks>

    public class NPLTypeAndMemberDropdownBars : TypeAndMemberDropdownBars
    {
        private readonly LanguageService service;
        private bool isDirty;
        // private readonly DropdownBarsManager barManager;

        public NPLTypeAndMemberDropdownBars(LanguageService service)
            : base(service)
        {
            this.service = service;
            isDirty = true;
        }

        void SourceRecompiled(object sender, EventArgs e)
        {
            isDirty = true;
            service.Invoke(new Action(service.SynchronizeDropdowns), new object[] { });
        }
        
        public override bool OnSynchronizeDropdowns(Microsoft.VisualStudio.Package.LanguageService languageService, IVsTextView textView, int line, int col, ArrayList dropDownTypes, ArrayList dropDownMembers, ref int selectedType, ref int selectedMember)
        {
            if (isDirty)
            {
                isDirty = false;
                dropDownTypes.Clear();
                dropDownMembers.Clear();

                Source source = service.GetSource(textView);
                if (source != null)
                {
                    string filename = source.GetFilePath();
                    LuaFileCodeModel codeModel = service.GetFileCodeModel();
                    if(codeModel!=null)
                    {
                        foreach (CodeElement elem_ in codeModel.LuaCodeElements)
                        {
                            SimpleCodeElement elem = elem_ as SimpleCodeElement;
                            if(elem!=null)
                            {
                                if(elem is LuaCodeFunction)
                                {
                                    dropDownMembers.Add(new DropDownMember(
                                        elem.FullName,
                                        elem.GetTextSpan(),
                                        18, // 18 for IconImageIndex.Method
                                        DROPDOWNFONTATTR.FONTATTR_GRAY
                                        ));
                                }
                                else
                                {
                                    dropDownTypes.Add(new DropDownMember(
                                        elem.FullName,
                                        elem.GetTextSpan(),
                                        29, // 29 for IconImageIndex.Variable 
                                        DROPDOWNFONTATTR.FONTATTR_PLAIN
                                        ));
                                }
                            }
                        }
                        // create dummy item to show as title
                        if(dropDownMembers.Count > 0)
                        {
                            dropDownMembers.Add(new DropDownMember(
                                        String.Format("{0} function(s)", dropDownMembers.Count),
                                        new TextSpan(),
                                        0, // 18 for IconImageIndex.Method
                                        DROPDOWNFONTATTR.FONTATTR_GRAY
                                        ));
                            selectedMember = dropDownMembers.Count - 1;
                        }
                        if (dropDownTypes.Count > 0)
                        {
                            dropDownTypes.Add(new DropDownMember(
                                        String.Format("{0} variable(s)", dropDownTypes.Count),
                                        new TextSpan(),
                                        0, // 29 for IconImageIndex.Variable
                                        DROPDOWNFONTATTR.FONTATTR_GRAY
                                        ));
                            selectedType = dropDownTypes.Count - 1;
                        }
                    }
                }
                // TODO: set current selection to selectedType and selectedMember
                // for performance reason, I did not implement it. 

            }


            return true;
        }

    }
}
