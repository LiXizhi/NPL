using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using EnvDTE;
using ParaEngine.Tools.Lua.CodeDom;
using ParaEngine.Tools.Lua.CodeDom.Definitions;
using ParaEngine.Tools.Lua.CodeDom.Elements;
using ParaEngine.Tools.Lua.Refactoring.UndoManager;
using ParaEngine.Tools.Services;
using ParaEngine.NPLLanguageService;

namespace ParaEngine.Tools.Lua.Refactoring.RenameService
{
	/// <summary>
	/// Renames symbols in FileCodeModel and in all referenced objects.
	/// </summary>
	public sealed class RefactorRenameAdapter
	{
		private readonly IServiceProvider provider;
		private readonly IRefactoringService refactoringService;
		private readonly LanguageService languageService;
		private List<ProjectItem> luaProjectItems;
		private readonly DTE dte;
		private readonly ILuaUndoService luaUndoService;

		/// <summary>
		/// Gets DTE instance.
		/// </summary>
		public DTE DTE
		{
			get { return dte; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RefactorRenameAdapter"/> class.
		/// </summary>
		/// <param name="serviceProvider">The <see cref="IServiceProvider"/> instance.</param>
		public RefactorRenameAdapter(IServiceProvider serviceProvider)
		{
			provider = serviceProvider;
			dte = provider.GetService(typeof (DTE)) as DTE;
			languageService = provider.GetService(typeof (ILuaLanguageService)) as LanguageService;
			refactoringService = provider.GetService(typeof (IRefactoringService)) as IRefactoringService;
			luaUndoService = provider.GetService(typeof (ILuaUndoService)) as ILuaUndoService;
		}

		/// <summary>
		/// Rename the selected code element.
		/// </summary>
		/// <returns></returns>
		public IRenameResult Rename()
		{
			IRenameResult result = null;
			LuaFileCodeModel codeModel = GetFileCodeModel();
			CodeElement element = codeModel.GetElementByEditPoint();
			if (element != null)
			{
				dte.StatusBar.Text = String.Format(Resources.RenameStartedMessage, element.Name);
				DTE.StatusBar.Highlight(true);

				//Rename element.
				result = Rename(element, string.Empty);

				dte.StatusBar.Clear();
			}
			else
			{
				dte.StatusBar.Text = Resources.RenameNotAllowedMessage;
			}

			return result;
		}

		/// <summary>
		/// Rename the selected code element.
		/// </summary>
		/// <param name="element">Element to rename.</param>
		/// <param name="newName">Element's new name.</param>
		/// <returns></returns>
		public IRenameResult Rename(CodeElement element, string newName)
		{
			IRenameResult result = Rename(element, string.Empty, true);

			if (result != null)
				luaUndoService.Add(new RenameUndoUnit(result));

			return result;
		}

		/// <summary>
		/// Rename the selected code element.
		/// </summary>
		/// <param name="element">Element to rename.</param>
		/// <param name="newName">Element's new name.</param>
		/// <param name="useToolWindow">Show/Hide rename tool window.</param>
		/// <returns></returns>
		public IRenameResult Rename(CodeElement element, string newName, bool useToolWindow)
		{
			if (element == null) return null;

			IRenameResult result = null;
			string oldName = element.Name;
			bool openAllChangedDocument = true;
			bool success = refactoringService.CanRenameSymbol(element, element.Kind, oldName);

			if (success)
			{
				//Cannot rename element without new name.
				if (!useToolWindow && string.IsNullOrEmpty(newName))
				{
					MessageBox.Show(Resources.RenameNewNameEmptyMessage,
					                Resources.RenameCommandName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
				else
				{
					//Show Rename Form if required.
					if (useToolWindow)
					{
						success = ReSolveConflicts(element, oldName,
						                           ref newName, out openAllChangedDocument) == ConflictType.None;
					}
					//Rename operation approved by user.
					if (success)
					{
						var codeModel = GetFileCodeModel();
						result = refactoringService.Rename(element, codeModel.RootElement,
						                                   element.Kind, oldName, newName);
						result.Parents.Add(codeModel);
						//If renamed element is declared in project scope then
						//rename all references in code files.
						if (result.RenameReferences)
						{
							result = RenameAllReferences(element.Kind, oldName, newName, result);
						}

						try
						{
							DTE.UndoContext.Open("Undo Code Merge", false);
							//Merge changed elements into code files from FileCodeModel.
							MergeChanges(result, openAllChangedDocument);
						}
						finally
						{
							DTE.UndoContext.Close();
						}
					}
				}
			}
			return result;
		}


		/// <summary>
		/// Solves refactoring/rename conflicts with existing element name.
		/// </summary>
		/// <param name="element">Element to rename.</param>
		/// <param name="newName">Element's new name.</param>
		/// <param name="oldName">Old name of element.</param>
		/// <param name="openAllChangedDocument">Indicates that open code files in Visual Studio Text Editor</param>
		/// <returns></returns>
		private ConflictType ReSolveConflicts(CodeElement element, string oldName, ref string newName,
		                                      out bool openAllChangedDocument)
		{
			bool success = GetNewName(element, oldName, ref newName, out openAllChangedDocument);
			if (success)
			{
				ConflictType conflictType = CheckConflicts(element, newName);
				while (conflictType != ConflictType.None && conflictType != ConflictType.Canceled)
				{
					conflictType = ReSolveConflicts(element, oldName, ref newName, out openAllChangedDocument);
				}
				return conflictType;
			}
			return ConflictType.Canceled;
		}

		/// <summary>
		/// Gets new name for Function from user.
		/// </summary>
		/// <param name="element">Element to rename.</param>
		/// <param name="newName">Element's new name.</param>
		/// <param name="oldName">Old name of element.</param>
		/// <param name="openAllChangedDocument">Indicates that open code files in Visual Studio Text Editor</param>
		/// <returns></returns>
		private static bool GetNewName(CodeElement element, string oldName, ref string newName,
		                               out bool openAllChangedDocument)
		{
			bool success;
			using (var renameSymbolForm = new RenameSymbolForm(oldName, GetElementTypeDescription(element)))
			{
				renameSymbolForm.NewName = string.IsNullOrEmpty(newName) ? oldName : newName;
				DialogResult dResult = renameSymbolForm.ShowDialog();

				success = dResult == DialogResult.OK;
				newName = renameSymbolForm.NewName;
				openAllChangedDocument = renameSymbolForm.OpenAllChangedDocument;
			}
			return success;
		}

		/// <summary>
		/// Checks the conflicts.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <param name="newName">The new name.</param>
		/// <returns></returns>
		private ConflictType CheckConflicts(CodeElement element, string newName)
		{
			IConflictResolver resolver =
				RenameStrategyFactory.CreateConflictResolver(element.Kind, GetFileCodeModel());
			if (resolver != null)
			{
				IEnumerable<CodeElement> conflictElements = resolver.FindConflicts(element, newName);
				if (resolver.HasConflict())
				{
					using (var conflictsFrom = new RenameConflictsFrom(conflictElements))
					{
						DialogResult dResult = conflictsFrom.ShowDialog();
						if (dResult == DialogResult.OK)
							return ConflictType.Function;
						if (dResult == DialogResult.Cancel)
							return ConflictType.Canceled;
					}
				}
			}
			return ConflictType.None;
		}

		/// <summary>
		/// Rename elements in all referenced lua files.
		/// </summary>
		/// <param name="elementType">Type of element.</param>
		/// <param name="oldName">Old name of element.</param>
		/// <param name="newName">New name of element.</param>
		/// <param name="result">Rename result.</param>
		private IRenameResult RenameAllReferences(vsCMElement elementType, string oldName, string newName,
		                                          IRenameResult result)
		{
			Trace.WriteLine("RenameAllReferences started...");
			try
			{
				var multiResult = new MultiRenameResult(oldName, newName);
				Project currentProject = DTE.ActiveDocument.ProjectItem.ContainingProject;
				ProjectItem activeProjectItem = DTE.ActiveDocument.ProjectItem;
				string activeProjectFileName = activeProjectItem.get_FileNames(1);
				List<ProjectItem> projectItems = GetLuaProjectItems(currentProject);

				foreach (ProjectItem projectItem in projectItems)
				{
					string fileName = projectItem.get_FileNames(1);
					if (!string.IsNullOrEmpty(fileName))
					{
						//If projectItem is the active then merge changes into MultiRenameResult
						if (activeProjectFileName == fileName)
						{
							multiResult.MergeChanges(projectItem, result);
						}
						else
						{
							if (IsLuaFile(fileName))
							{
								LuaFileCodeModel fileCodeModel = GetFileCodeModel(projectItem);
								if (fileCodeModel != null)
								{
									CodeElement root = GetRootElement(fileCodeModel);
									IRenameResult renameResult;
									//Rename references in Lua file.
									if (root != null)
									{
										renameResult = refactoringService.Rename(null, root, elementType, oldName, newName);
										renameResult.Parents.Add(fileCodeModel);
									}
									else
									{
										string message = String.Format(Resources.RootElementCannotBeFoundMessage, fileName);
										renameResult = new RenameResult(false, message);
										Trace.WriteLine(message);
									}
									multiResult.MergeChanges(projectItem, renameResult);
								}
							}
						}
					}
				}
				return multiResult;
			}
			catch (Exception e)
			{
				Trace.WriteLine(e);
				throw;
			}
		}

		/// <summary>
		/// Merges changes from IRenameResult.
		/// </summary>
		/// <param name="renameResult">IRenameResult provider.</param>
		/// <param name="openAllChangedDocument">Indicates that open code files in Visual Studio Text Editor</param>
		/// <returns></returns>
		private void MergeChanges(IRenameResult renameResult, bool openAllChangedDocument)
		{
			if (renameResult == null)
				throw new ArgumentNullException("renameResult");

			//Rename element and all refernces in other code files.
			if (renameResult is MultiRenameResult)
			{
				var multiRenameResult = renameResult as MultiRenameResult;
				Document currentWindow = DTE.ActiveDocument;
				foreach (var item in multiRenameResult)
				{
					if (item.Value.Success && item.Value.HasChanges)
					{
						if (openAllChangedDocument && !item.Key.get_IsOpen(Constants.vsViewKindCode))
						{
							//Open code file in Visual Studio Text Editor
							DTE.ItemOperations.OpenFile(item.Key.get_FileNames(1), Constants.vsViewKindPrimary);
						}
						//Merge changed elements into code file from FileCodeModel.
						MergeChanges(GetFileCodeModel(item.Key), item.Value.ChangedElements, multiRenameResult.OldName);
					}
				}
				if (currentWindow != null) currentWindow.Activate();
			}
			else if (renameResult is RenameResult)
			{
				//Rename element in actual code file.
				MergeChanges(GetFileCodeModel(), renameResult.ChangedElements, renameResult.OldName);
			}
		}

		/// <summary>
		/// Merges changes from IRenameResult into FileCodeModel.
		/// </summary>
		/// <param name="codeModel">The code model.</param>
		/// <param name="changedElements">The changed elements.</param>
		/// <param name="oldName">The old name.</param>
		private static void MergeChanges(LuaFileCodeModel codeModel, IEnumerable<CodeElement> changedElements,
		                                 string oldName)
		{
			codeModel.MergeChanges(oldName, changedElements, false);
		}

		#region Private helper functions

		/// <summary>
		/// Returns the description of the specified CodeElement.
		/// </summary>
		/// <param name="element">CodeElement instance.</param>
		/// <returns>Name of CodeElement.Kind.</returns>
		private static string GetElementTypeDescription(CodeElement element)
		{
			string name = string.Empty;
			string prefix = string.Empty;
			if (element is IDeclarationCodeElement)
			{
				vsCMAccess access = ((IDeclarationCodeElement) element).Access;
				prefix = access == vsCMAccess.vsCMAccessPrivate ? "local " : string.Empty;
			}
			if (element != null)
			{
				switch (element.Kind)
				{
					case vsCMElement.vsCMElementDefineStmt:
					case vsCMElement.vsCMElementDeclareDecl:
					case vsCMElement.vsCMElementLocalDeclStmt:
						{
							name = "variable";
							break;
						}
					case vsCMElement.vsCMElementFunctionInvokeStmt:
					case vsCMElement.vsCMElementFunction:
						{
							name = "function";
							break;
						}
					case vsCMElement.vsCMElementMap:
						{
							name = "table";
							break;
						}
				}
			}
			return String.Format("Rename {0}{1}", prefix, name);
		}

		/// <summary>
		/// Gets the root element.
		/// </summary>
		/// <param name="codeModel">LuaFileCodeModel instance.</param>
		/// <returns></returns>
		private static LuaCodeClass GetRootElement(LuaFileCodeModel codeModel)
		{
			if (codeModel != null)
				return codeModel.RootElement;

			return null;
		}

		/// <summary>
		/// Gets the LuaFileCodeModel of active ProjectItem.
		/// </summary>
		/// <returns>LuaFileCodeModel instance.</returns>
		private LuaFileCodeModel GetFileCodeModel()
		{
			LuaFileCodeModel codeModel = languageService.GetFileCodeModel();
			return codeModel;
		}

		/// <summary>
		/// Gets the FileCodeModel associated with specified ProjectItem.
		/// </summary>
		/// <param name="projectItem">Wow ProjectItem</param>
		/// <returns>LuaFileCodeModel instance.</returns>
		private LuaFileCodeModel GetFileCodeModel(ProjectItem projectItem)
		{
			LuaFileCodeModel codeModel = languageService.GetFileCodeModel(projectItem);
			return codeModel;
		}

		/// <summary>
		/// Check Wow Lua file.
		/// </summary>
		/// <param name="fileName">File name.</param>
		/// <returns></returns>
		private static bool IsLuaFile(string fileName)
		{
			return LanguageService.IsLuaFile(fileName);
		}


		/// <summary>
		/// Get all ProjectItems associated with all projects. 
		/// </summary>
		/// <returns></returns>
		private List<ProjectItem> GetLuaProjectItems()
		{
			luaProjectItems = new List<ProjectItem>();

			foreach (Project project in dte.Solution.Projects)
				WalkProject(project.ProjectItems);

			return luaProjectItems;
		}

		/// <summary>
		/// Get all ProjectItems associated with specified projects. 
		/// </summary>
		/// <returns></returns>
		private List<ProjectItem> GetLuaProjectItems(Project project)
		{
			luaProjectItems = new List<ProjectItem>();

			if (project != null)
				WalkProject(project.ProjectItems);

			return luaProjectItems;
		}

		/// <summary>
		/// Iterate through all ProjectItems in Project.
		/// </summary>
		/// <param name="projectItems">Collection of ProjectItem.</param>
		private void WalkProject(ProjectItems projectItems)
		{
			if (projectItems == null) return;

			foreach (ProjectItem projectItem in projectItems)
			{
				if (IsPhysicalFile(projectItem))
					luaProjectItems.Add(projectItem);

				WalkProject(projectItem.ProjectItems);
			}
		}

		/// <summary>
		/// Check for ProjectItem has physical file.
		/// </summary>
		/// <param name="projectItem">ProjectItem instance.</param>
		/// <returns></returns>
		private static bool IsPhysicalFile(ProjectItem projectItem)
		{
			if (projectItem != null && projectItem.Kind.ToLower() 
				== Constants.vsProjectItemKindPhysicalFile.ToLower())
				return true;

			return false;
		}

		#endregion
	}
}