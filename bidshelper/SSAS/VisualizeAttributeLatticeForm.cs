using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

namespace BIDSHelper
{
    public partial class VisualizeAttributeLatticeForm : Form
    {
        public Microsoft.AnalysisServices.Dimension dimension;
        private ToolStripMenuItem currentLayoutType;

        public VisualizeAttributeLatticeForm()
        {
            InitializeComponent();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Title = "Save As...";
                dlg.AddExtension = true;
                dlg.Filter = "JPEG files (*.jpg)|*.jpg|All files (*.*)|*.*";
                dlg.CheckFileExists = false;
                dlg.InitialDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    System.Drawing.Imaging.EncoderParameters parameters1 = GetEncoderParameters();
                    pictureBox1.Image.Save(dlg.FileName, VisualizeAttributeLattice.GetJpegCodec(), parameters1);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        public static System.Drawing.Imaging.EncoderParameters GetEncoderParameters()
        {
            System.Drawing.Imaging.EncoderParameters parameters1 = new System.Drawing.Imaging.EncoderParameters(3);
            parameters1.Param[0] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Compression, 2L);
            parameters1.Param[1] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 95L);
            parameters1.Param[2] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.ColorDepth, 24L);
            return parameters1;
        }

        private void saveAllDimensionsToFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                FolderBrowserDialog dlg = new FolderBrowserDialog();
                dlg.Description = "Save images of all dimension to directory...";
                dlg.ShowNewFolderButton = true;
                //dlg.RootFolder = System.Environment.SpecialFolder.;
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    System.Drawing.Imaging.EncoderParameters parameters1 = new System.Drawing.Imaging.EncoderParameters(3);
                    parameters1.Param[0] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Compression, 2L);
                    parameters1.Param[1] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 95L);
                    parameters1.Param[2] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.ColorDepth, 24L);
                    foreach (Microsoft.AnalysisServices.Dimension d in dimension.Parent.Dimensions)
                    {
                        Bitmap img = VisualizeAttributeLattice.Render(d, VisualizeAttributeLattice.LatticeLayoutMethod.DeepestPathsFirst, showOnlyMultilevelRelationshipsToolStripMenuItem.Checked);
                        img.Save(dlg.SelectedPath + "\\" + d.Name + ".jpg", VisualizeAttributeLattice.GetJpegCodec(), parameters1);
                        img.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void VisualizeAttributeLatticeForm_Load(object sender, EventArgs e)
        {
            try
            {
                currentLayoutType = layoutAToolStripMenuItem;
                LayoutImage();

                //load the dimensions menu
                foreach (Microsoft.AnalysisServices.Dimension d in dimension.Parent.Dimensions)
                {
                    ToolStripMenuItem item = new ToolStripMenuItem(d.Name);
                    if (d == dimension) item.Checked = true;
                    dimensionsToolStripMenuItem.DropDownItems.Add(item);
                    item.Click += new EventHandler(item_Click);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        void item_Click(object sender, EventArgs e)
        {
            try
            {
                ToolStripMenuItem item = (ToolStripMenuItem)sender;
                dimension = dimension.Parent.Dimensions.GetByName(item.Text);
                foreach (ToolStripItem item2 in dimensionsToolStripMenuItem.DropDownItems)
                {
                    if (item2 is ToolStripMenuItem)
                    {
                        ToolStripMenuItem menuItem = (ToolStripMenuItem)item2;
                        if (item2.Text == dimension.Name)
                            menuItem.Checked = true;
                        else
                            menuItem.Checked = false;
                    }
                }
                LayoutImage();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void LayoutImage()
        {
            if (pictureBox1.Image != null) pictureBox1.Image.Dispose();
            if (currentLayoutType == layoutAToolStripMenuItem)
            {
                layoutAToolStripMenuItem.Checked = true;
                layoutBToolStripMenuItem.Checked = false;
                layoutCToolStripMenuItem.Checked = false;
                pictureBox1.Image = VisualizeAttributeLattice.Render(dimension, VisualizeAttributeLattice.LatticeLayoutMethod.DeepestPathsFirst,showOnlyMultilevelRelationshipsToolStripMenuItem.Checked);
            }
            else if (currentLayoutType == layoutBToolStripMenuItem)
            {
                layoutAToolStripMenuItem.Checked = false;
                layoutBToolStripMenuItem.Checked = true;
                layoutCToolStripMenuItem.Checked = false;
                pictureBox1.Image = VisualizeAttributeLattice.Render(dimension, VisualizeAttributeLattice.LatticeLayoutMethod.ShortSingleLevelRelationshipsFirst, showOnlyMultilevelRelationshipsToolStripMenuItem.Checked);
            }
            else
            {
                layoutAToolStripMenuItem.Checked = false;
                layoutBToolStripMenuItem.Checked = false;
                layoutCToolStripMenuItem.Checked = true;
                pictureBox1.Image = VisualizeAttributeLattice.Render(dimension, VisualizeAttributeLattice.LatticeLayoutMethod.ShortRelationshipsFirst, showOnlyMultilevelRelationshipsToolStripMenuItem.Checked);
            }

            pictureBox1.Width = pictureBox1.Image.Width;
            pictureBox1.Height = pictureBox1.Image.Height;
            this.Text = dimension.Name + " - Attribute Lattice";
            if (pictureBox1.Width > 600)
            {
                if (this.WindowState != FormWindowState.Maximized) this.Width = 600;
                panel1.AutoScrollPosition = new Point(0, panel1.AutoScrollPosition.Y);
                pictureBox1.Left = 0;
                panel1.AutoScrollPosition = new Point((pictureBox1.Width - this.Width) / 2, panel1.AutoScrollPosition.Y);
            }
            else if (pictureBox1.Width < 300)
            {
                if (this.WindowState != FormWindowState.Maximized) this.Width = 300;
                panel1.AutoScrollPosition = new Point(0, panel1.AutoScrollPosition.Y);
                pictureBox1.Left = (this.Width - pictureBox1.Width) / 2;
                panel1.AutoScrollPosition = new Point(0, 0);
            }
            else
            {
                if (this.WindowState != FormWindowState.Maximized) this.Width = pictureBox1.Width + 30;
                panel1.AutoScrollPosition = new Point(0, 0);
                pictureBox1.Left = 0;
            }

            if (this.WindowState != FormWindowState.Maximized)
            {
                if (pictureBox1.Height + 100 > 600)
                {
                    this.Height = 600;
                }
                else if (pictureBox1.Height + 100 < 300)
                {
                    this.Height = 300;
                }
                else
                {
                    this.Height = pictureBox1.Height + 100;
                }
            }
        }

        private void layoutAToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                currentLayoutType = (ToolStripMenuItem)sender;
                LayoutImage();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void layoutBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                currentLayoutType = (ToolStripMenuItem)sender;
                LayoutImage();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void layoutCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                currentLayoutType = (ToolStripMenuItem)sender;
                LayoutImage();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void previousAltLeftArrowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                lock (dimensionsToolStripMenuItem)
                {
                    int oldIndex = dimension.Parent.Dimensions.IndexOf(dimension);
                    if (oldIndex == 0)
                    {
                        dimension = dimension.Parent.Dimensions[dimension.Parent.Dimensions.Count - 1];
                    }
                    else
                    {
                        dimension = dimension.Parent.Dimensions[oldIndex - 1];
                    }
                    foreach (ToolStripItem item in dimensionsToolStripMenuItem.DropDownItems)
                    {
                        if (item is ToolStripMenuItem)
                        {
                            ToolStripMenuItem menuItem = (ToolStripMenuItem)item;
                            if (item.Text == dimension.Name)
                                menuItem.Checked = true;
                            else
                                menuItem.Checked = false;
                        }
                    }
                    LayoutImage();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void nextAltRightArrowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                lock (dimensionsToolStripMenuItem)
                {
                    int oldIndex = dimension.Parent.Dimensions.IndexOf(dimension);
                    if (oldIndex == dimension.Parent.Dimensions.Count - 1)
                    {
                        dimension = dimension.Parent.Dimensions[0];
                    }
                    else
                    {
                        dimension = dimension.Parent.Dimensions[oldIndex + 1];
                    }
                    foreach (ToolStripItem item in dimensionsToolStripMenuItem.DropDownItems)
                    {
                        if (item is ToolStripMenuItem)
                        {
                            ToolStripMenuItem menuItem = (ToolStripMenuItem)item;
                            if (item.Text == dimension.Name)
                                menuItem.Checked = true;
                            else
                                menuItem.Checked = false;
                        }
                    }
                    LayoutImage();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void showOnlyMultilevelRelationshipsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                showOnlyMultilevelRelationshipsToolStripMenuItem.Checked = !showOnlyMultilevelRelationshipsToolStripMenuItem.Checked;
                LayoutImage();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void legendToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Form legend = new Form();
                legend.Icon = BIDSHelper.Resources.Common.BIDSHelper;
                legend.Text = "Visualize Attribute Lattice - Legend";
                legend.MaximizeBox = false;
                legend.MinimizeBox = false;
                PictureBox pic = new PictureBox();
                pic.Location = new System.Drawing.Point(0, 0);
                pic.Size = new System.Drawing.Size(290, 480);


                Bitmap canvas = new Bitmap(pic.Width, pic.Height);
                Graphics g = Graphics.FromImage(canvas);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.FillRectangle(new SolidBrush(Color.White), 0, 0, canvas.Width, canvas.Height);

                g.DrawString("Relationship Types", new Font(FontFamily.GenericSansSerif, 11, FontStyle.Bold | FontStyle.Underline), new SolidBrush(Color.Black), new RectangleF(10, 12, 200, 25));
                Pen pen = new Pen(Color.Green, 1.55F); //Color.Gray) //active and rigid
                g.DrawLine(pen, 10, 50, 100, 50); //active and rigid
                g.DrawString("= Active and rigid", new Font(FontFamily.GenericSansSerif, 10, FontStyle.Regular), new SolidBrush(Color.Black), new RectangleF(105, 45 - 3, 200, 25));
                pen.DashStyle = DashStyle.Dash;
                pen.DashPattern = new float[] { 1F, 1.55F };
                g.DrawLine(pen, 10, 75, 100, 75); //active and flexible
                g.DrawString("= Active and flexible", new Font(FontFamily.GenericSansSerif, 10, FontStyle.Regular), new SolidBrush(Color.Black), new RectangleF(105, 70 - 3, 200, 25));
                pen = new Pen(Color.Gray, 1.55F);
                g.DrawLine(pen, 10, 100, 100, 100); //inactive and rigid
                g.DrawString("= Inactive and rigid", new Font(FontFamily.GenericSansSerif, 10, FontStyle.Regular), new SolidBrush(Color.Black), new RectangleF(105, 95 - 3, 200, 25));
                pen.DashStyle = DashStyle.Dash;
                pen.DashPattern = new float[] { 1F, 1.55F };
                g.DrawLine(pen, 10, 125, 100, 125); //inactive and flexible
                g.DrawString("= Inactive and flexible", new Font(FontFamily.GenericSansSerif, 10, FontStyle.Regular), new SolidBrush(Color.Black), new RectangleF(105, 120 - 3, 200, 25));

                pen.Width = 5;
                pen.Color = Color.Red;
                pen.DashStyle = DashStyle.Dash;
                pen.DashPattern = new float[] { 0.4F, 1.5F };
                g.DrawLine(pen, 10, 150, 100, 150); //overlapping relationship being highlighted so it is seen
                g.DrawString("= Red ensures overlapping\r\n   relationship is seen", new Font(FontFamily.GenericSansSerif, 10, FontStyle.Italic), new SolidBrush(Color.Black), new RectangleF(105, 145 - 3, 200, 40));


                g.DrawString("Attribute Types", new Font(FontFamily.GenericSansSerif, 11, FontStyle.Bold | FontStyle.Underline), new SolidBrush(Color.Black), new RectangleF(10, 200, 200, 25));

                StringFormat centered = new StringFormat();
                centered.Alignment = StringAlignment.Center;

                RectangleF fillRect = new RectangleF(10, 230, 90, 40);
                g.FillRectangle(new SolidBrush(Color.LightBlue), fillRect);
                fillRect.Y += 12;
                g.DrawString("Attribute", new Font(FontFamily.GenericSansSerif, 10, FontStyle.Regular), new SolidBrush(Color.Black), fillRect, centered);
                g.DrawString("= Visible and enabled", new Font(FontFamily.GenericSansSerif, 10, FontStyle.Regular), new SolidBrush(Color.Black), new RectangleF(105, fillRect.Y, 200, fillRect.Height));

                fillRect = new RectangleF(10, 285, 90, 40);
                g.FillRectangle(new SolidBrush(Color.LightBlue), fillRect);
                fillRect.Y += 12;
                g.DrawString("Attribute", new Font(FontFamily.GenericSansSerif, 10, FontStyle.Italic), new SolidBrush(Color.Gray), fillRect, centered);
                g.DrawString("= Visible and disabled", new Font(FontFamily.GenericSansSerif, 10, FontStyle.Regular), new SolidBrush(Color.Black), new RectangleF(105, fillRect.Y, 200, fillRect.Height));

                fillRect = new RectangleF(10, 340, 90, 40);
                g.FillRectangle(new SolidBrush(Color.LightGray), fillRect);
                fillRect.Y += 12;
                g.DrawString("Attribute", new Font(FontFamily.GenericSansSerif, 10, FontStyle.Regular), new SolidBrush(Color.Black), fillRect, centered);
                g.DrawString("= Invisible and enabled", new Font(FontFamily.GenericSansSerif, 10, FontStyle.Regular), new SolidBrush(Color.Black), new RectangleF(105, fillRect.Y, 200, fillRect.Height));

                fillRect = new RectangleF(10, 395, 90, 40);
                g.FillRectangle(new SolidBrush(Color.LightGray), fillRect);
                fillRect.Y += 12;
                g.DrawString("Attribute", new Font(FontFamily.GenericSansSerif, 10, FontStyle.Italic), new SolidBrush(Color.Gray), fillRect, centered);
                g.DrawString("= Invisible and disabled", new Font(FontFamily.GenericSansSerif, 10, FontStyle.Regular), new SolidBrush(Color.Black), new RectangleF(105, fillRect.Y, 200, fillRect.Height));

                g.Dispose();
                pic.Image = canvas;
                legend.Controls.Add(pic);
                legend.Width = canvas.Width;
                legend.Height = canvas.Height;
                legend.MinimumSize = legend.Size;
                legend.MaximumSize = legend.Size;
                legend.SizeGripStyle = SizeGripStyle.Hide;
                pic.PerformLayout();
                this.PerformLayout();
                legend.ShowDialog();
                legend.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void VisualizeAttributeLatticeForm_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Alt && e.KeyCode == Keys.Right)
                {
                    nextAltRightArrowToolStripMenuItem_Click(null, null);
                    e.Handled = true;
                }
                else if (e.Alt && e.KeyCode == Keys.Left)
                {
                    previousAltLeftArrowToolStripMenuItem_Click(null, null);
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void VisualizeAttributeLatticeForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                pictureBox1.Image.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void openReportWithAllDimensionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                List<VisualizeAttributeLatticeImageForReport> images = new List<VisualizeAttributeLatticeImageForReport>();
                foreach (Microsoft.AnalysisServices.Dimension d in dimension.Parent.Dimensions)
                {
                    Bitmap img = VisualizeAttributeLattice.Render(d, VisualizeAttributeLattice.LatticeLayoutMethod.DeepestPathsFirst, showOnlyMultilevelRelationshipsToolStripMenuItem.Checked);
                    images.Add(new VisualizeAttributeLatticeImageForReport(d.Name, img));
                    img.Dispose();
                }

                ReportViewerForm frm = new ReportViewerForm();
                frm.ReportBindingSource.DataSource = images;
                frm.Report = "SSAS.VisualizeAttributeLattice.rdlc";
                Microsoft.Reporting.WinForms.ReportDataSource reportDataSource1 = new Microsoft.Reporting.WinForms.ReportDataSource();
                reportDataSource1.Name = "VisualizeAttributeLatticeImageForReport";
                reportDataSource1.Value = frm.ReportBindingSource;
                frm.ReportViewerControl.LocalReport.DataSources.Add(reportDataSource1);
                frm.ReportViewerControl.LocalReport.ReportEmbeddedResource = "SSAS.VisualizeAttributeLattice.rdlc";

                frm.Caption = "Visualize All Attribute Lattices";
                frm.Show();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


    }
}