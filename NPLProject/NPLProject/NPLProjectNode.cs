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
            get { return Guids.guidSimpleProjectFactory; }
        }
        public override string ProjectType
        {
            get { return "SimpleProjectType"; }
        }

        public override void AddFileFromTemplate(
            string source, string target)
        {
            this.FileTemplateProcessor.UntokenFile(source, target);
            this.FileTemplateProcessor.Reset();
        }
    }
}
