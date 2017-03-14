/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudioTools.Project
{
    internal class ProjectReferenceNode : ReferenceNode
    {
        #region fields
        /// <summary>
        /// The name of the assembly this refernce represents
        /// </summary>
        private Guid referencedProjectGuid;

        private string referencedProjectName = String.Empty;

        private string referencedProjectRelativePath = String.Empty;

        private string referencedProjectFullPath = String.Empty;

        private BuildDependency buildDependency;

        /// <summary>
        /// This is a reference to the automation object for the referenced project.
        /// </summary>
        private EnvDTE.Project referencedProject;

        /// <summary>
        /// This state is controlled by the solution events.
        /// The state is set to false by OnBeforeUnloadProject.
        /// The state is set to true by OnBeforeCloseProject event.
        /// </summary>
        private bool canRemoveReference = true;

        /// <summary>
        /// Possibility for solution listener to update the state on the dangling reference.
        /// It will be set in OnBeforeUnloadProject then the nopde is invalidated then it is reset to false.
        /// </summary>
        private bool isNodeValid;

        #endregion

        #region properties

        public override string Url
        {
            get
            {
                return this.referencedProjectFullPath;
            }
        }

        public override string Caption
        {
            get
            {
                return this.referencedProjectName;
            }
        }

        internal Guid ReferencedProjectGuid
        {
            get
            {
                return this.referencedProjectGuid;
            }
        }

        /// <summary>
        /// Possiblity to shortcut and set the dangling project reference icon.
        /// It is ussually manipulated by solution listsneres who handle reference updates.
        /// </summary>
        internal protected bool IsNodeValid
        {
            get
            {
                return this.isNodeValid;
            }
            set
            {
                this.isNodeValid = value;
            }
        }

        /// <summary>
        /// Controls the state whether this reference can be removed or not. Think of the project unload scenario where the project reference should not be deleted.
        /// </summary>
        internal bool CanRemoveReference
        {
            get
            {
                return this.canRemoveReference;
            }
            set
            {
                this.canRemoveReference = value;
            }
        }

        internal string ReferencedProjectName
        {
            get { return this.referencedProjectName; }
        }

        /// <summary>
        /// Gets the automation object for the referenced project.
        /// </summary>
        internal EnvDTE.Project ReferencedProjectObject
        {
            get
            {
                // If the referenced project is null then re-read.
                if (this.referencedProject == null)
                {

                    // Search for the project in the collection of the projects in the
                    // current solution.
                    EnvDTE.DTE dte = (EnvDTE.DTE)this.ProjectMgr.GetService(typeof(EnvDTE.DTE));
                    if ((null == dte) || (null == dte.Solution))
                    {
                        return null;
                    }
                    foreach (EnvDTE.Project prj in dte.Solution.Projects)
                    {
                        //Skip this project if it is an umodeled project (unloaded)
                        if (string.Compare(EnvDTE.Constants.vsProjectKindUnmodeled, prj.Kind, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            continue;
                        }

                        // Get the full path of the current project.
                        EnvDTE.Property pathProperty = null;
                        try
                        {
                            if (prj.Properties == null)
                            {
                                continue;
                            }

                            pathProperty = prj.Properties.Item("FullPath");
                            if (null == pathProperty)
                            {
                                // The full path should alway be availabe, but if this is not the
                                // case then we have to skip it.
                                continue;
                            }
                        }
                        catch (ArgumentException)
                        {
                            continue;
                        }
                        string prjPath = pathProperty.Value.ToString();
                        EnvDTE.Property fileNameProperty = null;
                        // Get the name of the project file.
                        try
                        {
                            fileNameProperty = prj.Properties.Item("FileName");
                            if (null == fileNameProperty)
                            {
                                // Again, this should never be the case, but we handle it anyway.
                                continue;
                            }
                        }
                        catch (ArgumentException)
                        {
                            continue;
                        }
                        prjPath = Path.Combine(prjPath, fileNameProperty.Value.ToString());

                        // If the full path of this project is the same as the one of this
                        // reference, then we have found the right project.
                        if (CommonUtils.IsSamePath(prjPath, referencedProjectFullPath))
                        {
                            this.referencedProject = prj;
                            break;
                        }
                    }
                }

                return this.referencedProject;
            }
            set
            {
                this.referencedProject = value;
            }
        }

        /// <summary>
        /// Gets the full path to the assembly generated by this project.
        /// </summary>
        internal string ReferencedProjectOutputPath
        {
            get
            {
                // Make sure that the referenced project implements the automation object.
                if (null == this.ReferencedProjectObject)
                {
                    return null;
                }

                // Get the configuration manager from the project.
                EnvDTE.ConfigurationManager confManager = this.ReferencedProjectObject.ConfigurationManager;
                if (null == confManager)
                {
                    return null;
                }

                // Get the active configuration.
                EnvDTE.Configuration config = confManager.ActiveConfiguration;
                if (null == config)
                {
                    return null;
                }


                if (null == config.Properties)
                {
                    return null;
                }

                // Get the output path for the current configuration.
                EnvDTE.Property outputPathProperty = config.Properties.Item("OutputPath");
                if (null == outputPathProperty || outputPathProperty.Value == null)
                {
                    return null;
                }

                // Usually the output path is relative to the project path. If it is set as an
                // absolute path, this call has no effect.
                string outputPath = CommonUtils.GetAbsoluteDirectoryPath(
                    Path.GetDirectoryName(referencedProjectFullPath),
                    outputPathProperty.Value.ToString());

                // Now get the name of the assembly from the project.
                // Some project system throw if the property does not exist. We expect an ArgumentException.
                EnvDTE.Property assemblyNameProperty = null;
                try
                {
                    assemblyNameProperty = this.ReferencedProjectObject.Properties.Item("OutputFileName");
                }
                catch (ArgumentException)
                {
                }

                if (null == assemblyNameProperty)
                {
                    return null;
                }
                // build the full path adding the name of the assembly to the output path.
                outputPath = Path.Combine(outputPath, assemblyNameProperty.Value.ToString());

                return outputPath;
            }
        }

        internal string AssemblyName
        {
            get
            {
                // Now get the name of the assembly from the project.
                // Some project system throw if the property does not exist. We expect an ArgumentException.
                EnvDTE.Property assemblyNameProperty = null;
                if (ReferencedProjectObject != null && 
                    !(ReferencedProjectObject is Automation.OAProject)) // our own projects don't have assembly names
                {
                    try
                    {
                        assemblyNameProperty = this.ReferencedProjectObject.Properties.Item(ProjectFileConstants.AssemblyName);
                    }
                    catch (ArgumentException)
                    {
                    }
                    if (assemblyNameProperty != null)
                    {
                        return assemblyNameProperty.Value.ToString();
                    }
                }
                return null;
            }
        }

        private Automation.OAProjectReference projectReference;
        internal override object Object
        {
            get
            {
                if (null == projectReference)
                {
                    projectReference = new Automation.OAProjectReference(this);
                }
                return projectReference;
            }
        }
        #endregion

        #region ctors
        /// <summary>
        /// Constructor for the ReferenceNode. It is called when the project is reloaded, when the project element representing the refernce exists. 
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings")]
        public ProjectReferenceNode(ProjectNode root, ProjectElement element)
            : base(root, element)
        {
            this.referencedProjectRelativePath = this.ItemNode.GetMetadata(ProjectFileConstants.Include);
            Debug.Assert(!String.IsNullOrEmpty(this.referencedProjectRelativePath), "Could not retrieve referenced project path form project file");

            string guidString = this.ItemNode.GetMetadata(ProjectFileConstants.Project);

            // Continue even if project setttings cannot be read.
            try
            {
                this.referencedProjectGuid = new Guid(guidString);

                this.buildDependency = new BuildDependency(this.ProjectMgr, this.referencedProjectGuid);
                this.ProjectMgr.AddBuildDependency(this.buildDependency);
            }
            finally
            {
                Debug.Assert(this.referencedProjectGuid != Guid.Empty, "Could not retrive referenced project guidproject file");

                this.referencedProjectName = this.ItemNode.GetMetadata(ProjectFileConstants.Name);

                Debug.Assert(!String.IsNullOrEmpty(this.referencedProjectName), "Could not retrive referenced project name form project file");
            }

            // TODO: Maybe referenced projects should be relative to ProjectDir?
            this.referencedProjectFullPath = CommonUtils.GetAbsoluteFilePath(this.ProjectMgr.ProjectHome, this.referencedProjectRelativePath);
        }

        /// <summary>
        /// constructor for the ProjectReferenceNode
        /// </summary>
        public ProjectReferenceNode(ProjectNode root, string referencedProjectName, string projectPath, string projectReference)
            : base(root)
        {
            Debug.Assert(root != null && !String.IsNullOrEmpty(referencedProjectName) && !String.IsNullOrEmpty(projectReference)
                && !String.IsNullOrEmpty(projectPath), "Can not add a reference because the input for adding one is invalid.");

            if (projectReference == null)
            {
                throw new ArgumentNullException("projectReference");
            }

            this.referencedProjectName = referencedProjectName;

            int indexOfSeparator = projectReference.IndexOf('|');


            string fileName = String.Empty;

            // Unfortunately we cannot use the path part of the projectReference string since it is not resolving correctly relative pathes.
            if (indexOfSeparator != -1)
            {
                string projectGuid = projectReference.Substring(0, indexOfSeparator);
                this.referencedProjectGuid = new Guid(projectGuid);
                if (indexOfSeparator + 1 < projectReference.Length)
                {
                    string remaining = projectReference.Substring(indexOfSeparator + 1);
                    indexOfSeparator = remaining.IndexOf('|');

                    if (indexOfSeparator == -1)
                    {
                        fileName = remaining;
                    }
                    else
                    {
                        fileName = remaining.Substring(0, indexOfSeparator);
                    }
                }
            }

            Debug.Assert(!String.IsNullOrEmpty(fileName), "Can not add a project reference because the input for adding one is invalid.");

            string justTheFileName = Path.GetFileName(fileName);
            this.referencedProjectFullPath = CommonUtils.GetAbsoluteFilePath(projectPath, justTheFileName);
            // TODO: Maybe referenced projects should be relative to ProjectDir?
            this.referencedProjectRelativePath = CommonUtils.GetRelativeFilePath(this.ProjectMgr.ProjectHome, this.referencedProjectFullPath);

            this.buildDependency = new BuildDependency(this.ProjectMgr, this.referencedProjectGuid);

        }
        #endregion

        #region methods
        protected override NodeProperties CreatePropertiesObject()
        {
            return new ProjectReferencesProperties(this);
        }

        /// <summary>
        /// The node is added to the hierarchy and then updates the build dependency list.
        /// </summary>
        public override void AddReference()
        {
            if (this.ProjectMgr == null)
            {
                return;
            }
            base.AddReference();
            this.ProjectMgr.AddBuildDependency(this.buildDependency);
            return;
        }

        /// <summary>
        /// Overridden method. The method updates the build dependency list before removing the node from the hierarchy.
        /// </summary>
        public override void Remove(bool removeFromStorage)
        {
            if (this.ProjectMgr == null || !this.CanRemoveReference)
            {
                return;
            }
            this.ProjectMgr.RemoveBuildDependency(this.buildDependency);
            base.Remove(removeFromStorage);

            return;
        }

        /// <summary>
        /// Links a reference node to the project file.
        /// </summary>
        protected override void BindReferenceData()
        {
            Debug.Assert(!String.IsNullOrEmpty(this.referencedProjectName), "The referencedProjectName field has not been initialized");
            Debug.Assert(this.referencedProjectGuid != Guid.Empty, "The referencedProjectName field has not been initialized");

            this.ItemNode = new MsBuildProjectElement(this.ProjectMgr, this.referencedProjectRelativePath, ProjectFileConstants.ProjectReference);

            this.ItemNode.SetMetadata(ProjectFileConstants.Name, this.referencedProjectName);
            this.ItemNode.SetMetadata(ProjectFileConstants.Project, this.referencedProjectGuid.ToString("B"));
            this.ItemNode.SetMetadata(ProjectFileConstants.Private, true.ToString());
        }

        /// <summary>
        /// Defines whether this node is valid node for painting the refererence icon.
        /// </summary>
        /// <returns></returns>
        protected override bool CanShowDefaultIcon()
        {
            if (this.referencedProjectGuid == Guid.Empty || this.ProjectMgr == null || this.ProjectMgr.IsClosed || this.isNodeValid)
            {
                return false;
            }

            IVsHierarchy hierarchy = null;

            hierarchy = VsShellUtilities.GetHierarchy(this.ProjectMgr.Site, this.referencedProjectGuid);

            if (hierarchy == null)
            {
                return false;
            }

            //If the Project is unloaded return false
            if (this.ReferencedProjectObject == null)
            {
                return false;
            }

            return File.Exists(this.referencedProjectFullPath);
        }

        /// <summary>
        /// Checks if a project reference can be added to the hierarchy. It calls base to see if the reference is not already there, then checks for circular references.
        /// </summary>
        /// <param name="errorHandler">The error handler delegate to return</param>
        /// <returns></returns>
        protected override bool CanAddReference(out CannotAddReferenceErrorMessage errorHandler)
        {
            // When this method is called this refererence has not yet been added to the hierarchy, only instantiated.
            if (!base.CanAddReference(out errorHandler))
            {
                return false;
            }

            errorHandler = null;
            if (this.IsThisProjectReferenceInCycle())
            {
                errorHandler = new CannotAddReferenceErrorMessage(ShowCircularReferenceErrorMessage);
                return false;
            }

            return true;
        }

        private bool IsThisProjectReferenceInCycle()
        {
            return IsReferenceInCycle(this.referencedProjectGuid);
        }

        private void ShowCircularReferenceErrorMessage()
        {
            string message = String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.ProjectContainsCircularReferences, CultureInfo.CurrentUICulture), this.referencedProjectName);
            string title = string.Empty;
            OLEMSGICON icon = OLEMSGICON.OLEMSGICON_CRITICAL;
            OLEMSGBUTTON buttons = OLEMSGBUTTON.OLEMSGBUTTON_OK;
            OLEMSGDEFBUTTON defaultButton = OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST;
            VsShellUtilities.ShowMessageBox(this.ProjectMgr.Site, title, message, icon, buttons, defaultButton);
        }

        /// <summary>
        /// Recursively search if this project reference guid is in cycle.
        /// </summary>
        private bool IsReferenceInCycle(Guid projectGuid)
        {
            // TODO: This has got to be wrong, it doesn't work w/ other project types.
            IVsHierarchy hierarchy = VsShellUtilities.GetHierarchy(this.ProjectMgr.Site, projectGuid);

            IReferenceContainerProvider provider = hierarchy.GetProject().GetCommonProject()  as IReferenceContainerProvider;
            if (provider != null)
            {
                IReferenceContainer referenceContainer = provider.GetReferenceContainer();

                Utilities.CheckNotNull(referenceContainer, "Could not found the References virtual node");

                foreach (ReferenceNode refNode in referenceContainer.EnumReferences())
                {
                    ProjectReferenceNode projRefNode = refNode as ProjectReferenceNode;
                    if (projRefNode != null)
                    {
                        if (projRefNode.ReferencedProjectGuid == this.ProjectMgr.ProjectIDGuid)
                        {
                            return true;
                        }

                        if (this.IsReferenceInCycle(projRefNode.ReferencedProjectGuid))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
        #endregion
    }

}
