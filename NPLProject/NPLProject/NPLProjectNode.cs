using Microsoft.VisualStudio.Project;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPLProject
{
    class NPLProjectNode : ProjectNode
    {
        private NPLProjectPackage package;
        private static ImageList imageList;
        internal static int imageIndex;
        public override int ImageIndex
        {
            get { return imageIndex; }
        }

        static NPLProjectNode()
        {
            imageList = Utilities.GetImageList(typeof(NPLProjectNode).Assembly.GetManifestResourceStream("NPLProject.Resources.NPLProjectNode.bmp"));
        }

        public NPLProjectNode(NPLProjectPackage package)
        {
            this.package = package;

            imageIndex = this.ImageHandler.ImageList.Images.Count;

            foreach (Image img in imageList.Images)
            {
                this.ImageHandler.AddImage(img);
            }
        }
        public override Guid ProjectGuid
        {
            get { return Guids.guidNPLProjectFactory; }
        }
        public override string ProjectType
        {
            get { return "NPLProjectType"; }
        }

        protected override Guid[] GetConfigurationIndependentPropertyPages()
        {
            Guid[] result = new Guid[1];
            result[0] = typeof(NPLPropertyPage).GUID;
            return result;
        }
        protected override Guid[] GetPriorityProjectDesignerPages()
        {
            Guid[] result = new Guid[1];
            result[0] = typeof(NPLPropertyPage).GUID;
            return result;
        }

        public override void AddFileFromTemplate(
            string source, string target)
        {
            this.FileTemplateProcessor.UntokenFile(source, target);
            this.FileTemplateProcessor.Reset();
        }
    }
}
