using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace AnnoModificationManager4.Misc
{
    public class OpenFileDialog
    {
        public System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();

        public string InitialDirectory
        {
            get
            {
                return dialog.InitialDirectory;
            }
            set
            {
                dialog.InitialDirectory = value;
            }
        }

        public bool Multiselect
        {
            get
            {
                return dialog.Multiselect;
            }
            set
            {
                dialog.Multiselect = value;
            }
        }

        public string Filter
        {
            get
            {
                return dialog.Filter;
            }
            set
            {
                dialog.Filter = value;
            }
        }

        public string FileName
        {
            get
            {
                return dialog.FileName;
            }
        }

        public List<string> FileNames
        {
            get
            {
                return Enumerable.ToList(dialog.FileNames);
            }
        }

        public bool? ShowDialog()
        {
            if (dialog.ShowDialog() == DialogResult.OK)
                return new bool?(true);
            return new bool?(false);
        }
    }
}
