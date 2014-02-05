using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EnvDTE;
using EnvDTE80;
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
    /// Refactor class: Renames variable elements and identifiers.
    /// </summary>
    internal sealed class VariableRenameStrategy : BaseRenameStrategy
    {
        private CodeElement variableParent;
        private bool isFunctionParameter;

        #region IRenameStrategy Members

        /// <summary>
        /// Rename element in scope of parentElement.
        /// </summary>
        /// <param name="element">Element to rename.</param>
        /// <param name="parentElement">Containing element.</param>
        /// <param name="elementType">Type of element.</param>
        /// <param name="oldName">Old name of element.</param>
        /// <param name="newName">New name of element.</param>
        public override IRenameResult RenameSymbols(CodeElement element, LuaCodeClass parentElement,
                                                    vsCMElement elementType, string oldName, string newName)
        {
            CodeElement declaration;
            renameResult = new RenameResult(oldName, newName);
            changedCodeElements = new List<CodeElement>();
            isFunctionParameter = false;

            if (element == null) //Declaration is in other lua file or not recognized by caller.
            {
                //Get declaration of the Variable.
                declaration = GetDeclaration(oldName, parentElement);
                if (declaration != null && !IsLocalDeclaration)
                {
                    RenameVariableDeclaration(declaration, oldName, newName);
                }
				//If declaration is global then rename elements in all referenced files
                if (!IsLocalDeclaration)
                {
                    //Rename all references in scope of class
                    renameResult.Success = RenameMemberVariableReferences(parentElement, elementType, oldName, newName);
                }
                renameResult.Success = true;
            }
            else
            {
                //Get declaration of the Variable.
                declaration = GetDeclaration(element, parentElement);

                //Get parent of the Variable declaration.
                if (declaration != null)
                {
                    variableParent = ((ICodeDomElement) declaration).ParentElement;
                    if (!(variableParent is LuaCodeFunction) || (variableParent is LuaCodeClass))
                    {
                        variableParent = LuaCodeDomNavigator.GetParentElement((ICodeDomElement) declaration);
                    }
                }
                else
                {
                    variableParent = ((ICodeDomElement) element).ParentElement;
                }

                //Rename CodeElements and all references.
                if (variableParent is LuaCodeClass) //CodeElement is global declared variable.
                {
                    //Rename member variable
                    if (RenameVariableDeclaration(declaration, oldName, newName))
                    {
                        //Rename all references in scope of current class.
                        renameResult.Success = RenameMemberVariableReferences(parentElement, oldName, newName);
                    }
                }
                else if (variableParent is LuaCodeFunction)//CodeElement is local declared variable.
                {
                    //Rename local variable.
                    if (RenameVariableDeclaration(declaration, oldName, newName))
                    {
                        if (IsLocalDeclaration)
                        {
                            //Rename all references in scope of Function
                            renameResult.Success = RenameMemberVariableReferencesInScope(oldName, newName);
                        }
                        else
                        {
                            //Rename all references in scope of Class.
                            renameResult.Success = RenameMemberVariableReferences(parentElement, oldName, newName);
                        }
                    }
                }
                else if (variableParent == null)
                {
                    throw new InvalidCodeElementException(
                        Resources.InvalidElementParentMessage, parentElement);
                }
                else
                {
                    Trace.WriteLine("Trace:Unrecognized variable...");
                    RenameSymbols(null, parentElement, elementType, oldName, newName);
                }
            }
            renameResult.ChangedElements = changedCodeElements;
            renameResult.RenameReferences = !IsLocalDeclaration;

			renameResult.Success = true;

            return renameResult;
        }

        #endregion

        #region Private functions

        /// <summary>
        /// Renames member variable in declaration scope.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="oldName">Old name of element.</param>
        /// <param name="newName">New name of element.</param>
        /// <returns></returns>
        private bool RenameVariableDeclaration(CodeElement element, string oldName, string newName)
        {
            if (element != null && element.Name == oldName)
                if (element is LuaCodeVariable)
                {
                    CheckLocalDeclaration(element as LuaCodeVariable);
                    ((CodeElement2) element).RenameSymbol(newName);
                    changedCodeElements.Add(element);
                    return true;
                }
                else if (isFunctionParameter)
                {
                    ((CodeElement2) element).RenameSymbol(newName);
                    changedCodeElements.Add(element);
                    return true;
                }
            return false;
        }

        /// <summary>
        /// Get global/local declaration of element, if exists.
        /// </summary>
        /// <param name="elementName">Name of CodeElement.</param>
        /// <param name="parentElement">Parent of CodeElement.</param>
        /// <returns>Return null if global declaration not found else the specified Variable.</returns>
        private CodeElement GetDeclaration(string elementName, CodeElement parentElement)
        {
            var navigator = new LuaCodeDomNavigator(parentElement);
            codeElements = new List<SimpleCodeElement>(
                navigator.WalkTopLevelMembers<LuaCodeVariable>());
            if (codeElements.Count > 0)
            {
                var declaration = codeElements.FirstOrDefault(child => child.Name == elementName);
                CheckLocalDeclaration(declaration as LuaCodeVariable);
                return declaration;
            }
            return null;
        }

        /// <summary>
        /// Get global/local declaration of element, if exists.
        /// </summary>
        /// <param name="element">CodeElement.</param>
        /// <param name="parentElement">Parent of CodeElement.</param>
        /// <returns>Return null if global declaration not found else the specified Variable.</returns>
        private CodeElement GetDeclaration(CodeElement element, CodeElement parentElement)
        {
            var codeDomElement = element as ICodeDomElement;
            if (codeDomElement != null && codeDomElement.ParentElement is LuaCodeClass)
            {
                return element;
            }
            var parent = LuaCodeDomNavigator.GetParentElement(element as ICodeDomElement);
            if (parent is LuaCodeFunction)//Check for local declaration
            {
                var navigator = new LuaCodeDomNavigator(parent);
                codeElements = new List<SimpleCodeElement>(
                    navigator.WalkMembers<LuaCodeVariable>());
                CodeElement declaration;
                if (codeElements.Count > 0)
                {
                    declaration = codeElements.FirstOrDefault(child => child.Name == element.Name);
                    if (declaration != null)
                    {
                        CheckLocalDeclaration(declaration as LuaCodeVariable);
                        return declaration;
                    }
                }
                //Check for parameter declaration
                declaration = ((LuaCodeFunction) parent).Parameters.OfType<LuaCodeElement<Identifier>>()
                    .FirstOrDefault(parameter => parameter.Name == element.Name);
                if (declaration != null)
                {
                    IsLocalDeclaration = true;
                    isFunctionParameter = true;
                    return declaration;
                }
                parent = LuaCodeDomNavigator.GetParentElement(parent as ICodeDomElement);
            }
            if (parent is LuaCodeClass)//Check for global declaration
            {
                var navigator = new LuaCodeDomNavigator(parentElement);
                codeElements = new List<SimpleCodeElement>(navigator.WalkTopLevelMembers<LuaCodeVariable>());

                if (codeElements.Count > 0)
                {
                    var declaration = codeElements.FirstOrDefault(child => child.Name == element.Name);

                    CheckLocalDeclaration(declaration as LuaCodeVariable);

                    return declaration;
                }
            }
            return null;
        }

        /// <summary>
        /// Renames all references of member variable in FileCodeModel.
        /// </summary>
        /// <param name="oldName">Old name of element.</param>
        /// <param name="newName">New name of element.</param>
        /// <returns></returns>
        private bool RenameMemberVariableReferencesInScope(string oldName, string newName)
        {
            var navigator = new LuaCodeDomNavigator(variableParent);
            codeElements = new List<SimpleCodeElement>(
                navigator.WalkMembers<LuaCodeElement<Identifier>, LuaCodeVariable>());
            codeElements.ForEach(identifier =>
                                     {
                                         if (identifier.Name == oldName)
                                         {
                                             identifier.RenameSymbol(newName);
                                             changedCodeElements.Add(identifier);
                                         }
                                     });
            return true;
        }

        /// <summary>
        /// Renames all references of member variable in FileCodeModel.
        /// </summary>
        /// <param name="parentElement">Containing element.</param>
        /// <param name="oldName">Old name of element.</param>
        /// <param name="newName">New name of element.</param>
        /// <returns></returns>
        private bool RenameMemberVariableReferences(CodeElement parentElement, string oldName, string newName)
        {
            var navigator = new LuaCodeDomNavigator(parentElement);
            codeElements = new List<SimpleCodeElement>(
                navigator.WalkMembers<LuaCodeElement<Identifier>, LuaCodeVariable>());
            codeElements.ForEach(identifier =>
                                     {
                                         if (identifier.Name == oldName)
                                         {
                                             CodeElement parent =
                                                 LuaCodeDomNavigator.GetParentElement(identifier as ICodeDomElement);
                                             if (parent != null && (parent is LuaCodeFunction || parent is LuaCodeClass))
                                             {
                                                 if (!parent.Children.OfType<LuaCodeVariable>().Any(
                                                          variable =>
                                                          variable.Name == oldName &&
                                                          variable.Access == vsCMAccess.vsCMAccessPrivate))
                                                 {
                                                     identifier.RenameSymbol(newName);
                                                     changedCodeElements.Add(identifier);
                                                 }
                                             }
                                         }
                                     });
            return true;
        }

        /// <summary>
        /// Renames all references of member variable in FileCodeModel.
        /// </summary>
        /// <param name="parentElement">Containing element.</param>
        /// <param name="elementType">Type of element.</param>
        /// <param name="oldName">Old name of element.</param>
        /// <param name="newName">New name of element.</param>
        /// <returns></returns>
        private bool RenameMemberVariableReferences(CodeElement parentElement, vsCMElement elementType, string oldName, string newName)
        {
            var navigator = new LuaCodeDomNavigator(parentElement);
            codeElements = new List<SimpleCodeElement>(
                navigator.WalkMembers<LuaCodeElement<Identifier>, LuaCodeVariable>());
            codeElements.ForEach(identifier =>
                                     {
                                         if (identifier.Name == oldName)
                                         {
                                             CodeElement parent =
                                                 LuaCodeDomNavigator.GetParentElement(identifier as ICodeDomElement);
                                             if (parent != null && (parent is LuaCodeFunction || parent is LuaCodeClass))
                                             {
                                                 if (!parent.Children.OfType<LuaCodeVariable>().Any(
                                                          variable => variable.Name == oldName))
                                                 {
                                                     identifier.RenameSymbol(newName);
                                                     changedCodeElements.Add(identifier);
                                                 }
                                             }
                                         }
                                     });
            return true;
        }

        #endregion

        #region Private Helper functions

        #endregion
    }
}