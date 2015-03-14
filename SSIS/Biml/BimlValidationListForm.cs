namespace BIDSHelper.SSIS.Biml
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Windows.Forms;
    using Varigence.Flow.FlowFramework.Validation;
    
    public partial class BimlValidationListForm : Form
    {
        public BimlValidationListForm(ValidationReporter validationReporter, bool showWarnings)
        {
            InitializeComponent();
            this.Icon = Resources.Common.Biml;

            // TODO: Get better icons, as per VS Error List, flatter, smoother, no resize requried.
            // Load icons into imge list, with indexes matching the enum Varigence.Flow.FlowFramework.Severity
            this.imageList.Images.Add(Resources.Common.Stop);
            this.imageList.Images.Add(Resources.Common.Stop);
            this.imageList.Images.Add(ResizeIcon(SystemIcons.Warning));
            this.imageList.Images.Add(ResizeIcon(SystemIcons.Information));
            this.imageList.Images.Add(ResizeIcon(SystemIcons.Question));

            // Set font that is readable, and appears to match VS Error List font, but probably just conincidence since VS fonts can be reset in VS itself, rather than using system defaults
            this.dataGridView.DefaultCellStyle.Font = SystemFonts.MenuFont;

            // Ensure that the Image column doesn't try and render a misisng image icon, we just want it to be empty if we pass in null
            ((DataGridViewImageColumn)this.dataGridView.Columns[0]).DefaultCellStyle.NullValue = null;

            // Enumerate validation items and add them to the 
            foreach (var validationItem in validationReporter.Errors)
            {
                this.dataGridView.Rows.Add(this.imageList.Images[(int)validationItem.Severity], validationItem.Message, validationItem.Recommendation, validationItem.Line, validationItem.Offset, Path.GetFileName(validationItem.FileName), validationItem.FilePath);
            }

            // Add a final row that tells people to go and look in the Output window, coz it is cool!
            this.dataGridView.Rows.Add(null, "Please see the Output window for more information.");
        }

        private System.Drawing.Icon ResizeIcon(Icon icon)
        {
            Size iconSize = SystemInformation.SmallIconSize;
            Bitmap bitmap = new Bitmap(iconSize.Width, iconSize.Height);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(icon.ToBitmap(), new Rectangle(Point.Empty, iconSize));
            }

            Icon smallerErrorIcon = System.Drawing.Icon.FromHandle(bitmap.GetHicon());
            return smallerErrorIcon;
        }

        private void helpButton_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(BIDSHelper.Resources.Common.BimlValidationHelpUrl);
        }

        private void buttonClose_Click(object sender, EventArgs e)
        { 
        }
    }
}
