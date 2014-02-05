using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using ParaEngine.Tools.Lua;
using ParaEngine.Tools.Lua.AST;
using ParaEngine.Tools.Lua.CodeDom;
using ParaEngine.Tools.Lua.CodeDom.Definitions;
using ParaEngine.Tools.Lua.CodeDom.Elements;
using ParaEngine.Tools.Lua.Refactoring;
using ParaEngine.Tools.Lua.Refactoring.RenameService;
using ParaEngine.NPLLanguageService;

namespace ParaEngine.Tools.Lua.Refactoring.RenameService
{
    /// <summary>
    /// Refactor class: Renames function elements and function calls.
    /// </summary>
    internal sealed class FunctionRenameStrategy : BaseRenameStrategy
    {

        #region IRenameStrategy Members

        /// <summary>
        /// Rename function in scope of parentElement.
        /// </summary>
        /// <param name="element">Element to rename.</param>
        /// <param name="parentElement">Containing element.</param>
        /// <param name="elementType">Type of element.</param>
        /// <param name="oldName">Old name of element.</param>
        /// <param name="newName">New name of element.</param>
        public override IRenameResult RenameSymbols(CodeElement element, LuaCodeClass parentElement, 
                                                    vsCMElement elementType, string oldName, string newName)
        {
			renameResult = new RenameResult(oldName, newName);
            changedCodeElements = new List<CodeElement>();

			//Function without parent element could not be renamed
            if (element is LuaCodeFunction && parentElement == null)
            {
                var ex = new InvalidCodeElementException(Resources.InvalidFunctionParentMessage, parentElement);
                Trace.WriteLine(ex);
                throw ex;
            }
			//Rename function, function calls or null element by its name
            if (element is LuaCodeFunction || element is LuaCodeElement<FunctionCall> || element == null)
            {
                renameResult = Rename(element, parentElement, oldName, newName);
            }
            else
            {
                throw new InvalidCodeElementException(
                    Resources.InvalidFunctionElementMessage, parentElement);
            }

			//Set RenameReferences flag to indicates that rename is local or not
            renameResult.RenameReferences = !IsLocalDeclaration;
            renameResult.ChangedElements = changedCodeElements;

			renameResult.Success = true;

            return renameResult;
        }

        #endregion

        #region Private Functions

        /// <summary>
        /// Rename LuaCodeFunction in scope of parentElement.
        /// </summary>
        /// <param name="element">Element to rename.</param>
        /// <param name="parentElement">Containing element. (LuaCodeClass)</param>
        /// <param name="oldName">Old name of element.</param>
        /// <param name="newName">New name of element.</param>
        private IRenameResult Rename(CodeElement element, LuaCodeClass parentElement, string oldName, string newName)
        {
            if (element == null)
            {
                //If function is null rename by its name.
                RenameOldFunctionOrCall(element, parentElement, oldName, newName);
                renameResult.Success = RenameFunctionCalls(parentElement, oldName, newName);
            }
            else
            {
                //Rename function/functionCall element
                if (RenameOldFunctionOrCall(element, parentElement, oldName, newName))
                {
                    if (element is LuaCodeElement<FunctionCall>)
                    {
                        CodeElement function = RenameFunctionDeclaration(element, parentElement, oldName, newName);
                        renameResult.Success = function != null;
                        if (renameResult.Success)
                        {
                            //Call Rename recursively for rename function declaration.
                            Rename(function, (LuaCodeClass)((ICodeDomElement)function).ParentElement, oldName, newName);
                        }
                    }
                    else
                    {
                        renameResult.Success = RenameFunctionCalls(parentElement, oldName, newName);
                    }
                }
                else
                {
                    throw new InvalidCodeElementException(
                        String.Format(Resources.OldFunctionNotFoundMessage, oldName), parentElement);
                }
            }
            return renameResult;
        }

        /// <summary>
        /// Rename LuaCodeFunction in scope of parentElement.
        /// </summary>
        /// <param name="element">Element to rename.</param>
        /// <param name="parentElement">Containing element. (LuaCodeClass)</param>
        /// <param name="oldName">Old name of element.</param>
        /// <param name="newName">New name of element.</param>
        private CodeElement RenameFunctionDeclaration(CodeElement element, CodeClass parentElement, string oldName, string newName)
        {
            if (element != null)
            {
                foreach (CodeElement member in parentElement.Members)
                {
                    if (member is LuaCodeFunction && member.Name == oldName
                        && ((LuaCodeFunction)member).FunctionType != vsCMFunction.vsCMFunctionTopLevel)
                    {

                        ((CodeElement2)member).RenameSymbol(newName);
                        changedCodeElements.Add(member);
                        return member;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Rename LuaCodeFunction in scope of parentElement.
        /// </summary>
        /// <param name="parentElement">Containing element. (LuaCodeClass)</param>
        /// <param name="oldName">Old name of element.</param>
        /// <param name="newName">New name of element.</param>
        private bool RenameFunctionCalls(CodeElement parentElement, string oldName, string newName)
        {
            bool result = true;
            var navigator = new LuaCodeDomNavigator(parentElement);

			codeElements = new List<SimpleCodeElement>(navigator.WalkMembers<LuaCodeElement<FunctionCall>>());
            codeElements.ForEach(funcCallElement => 
                                     {
                                         if (funcCallElement.Children != null)
                                         {
                                             LuaCodeElement<Identifier> identifier = 
                                                 funcCallElement.Children.OfType<LuaCodeElement<Identifier>>()
                                                     .FirstOrDefault();
                                             if (identifier.Name == oldName)
                                             {
                                                 identifier.RenameSymbol(newName);
                                                 changedCodeElements.Add(identifier);
                                             }
                                         }
                                         funcCallElement.Name = newName;
                                     });
            return result;
        }

        /// <summary>
        /// Rename LuaCodeFunction in scope of parentElement.
        /// </summary>
        /// <param name="element">Element to rename.</param>
        /// <param name="parentElement">Containing element. (LuaCodeClass)</param>
        /// <param name="oldName">Old name of element.</param>
        /// <param name="newName">New name of element.</param>
        private bool RenameOldFunctionOrCall(CodeElement element, CodeClass parentElement, string oldName, string newName)
        {
            bool result = false;

            //If source element is not specified then search for it.
            if (element == null)
            {
                foreach (CodeElement member in parentElement.Members)
                {
                    if (member is LuaCodeFunction && member.Name == oldName
                        && ((LuaCodeFunction)member).FunctionType != vsCMFunction.vsCMFunctionTopLevel)
                    {
                        CheckLocalDeclaration(member as LuaCodeFunction);
                        ((CodeElement2)member).RenameSymbol(newName);
                        changedCodeElements.Add(member);
                        result = true;
                        break;
                    }
                }
            }
            else
            {
                if (element is LuaCodeFunction)
                {
                    CheckLocalDeclaration(element as LuaCodeFunction);
                }
                //Rename element
                if (element.Name == oldName)
                {
                    ((CodeElement2)element).RenameSymbol(newName);
                    changedCodeElements.Add(element);
                }
                result = true;
            }
            return result;
        }


        #endregion
    }
}