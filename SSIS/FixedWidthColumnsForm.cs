using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;

namespace BIDSHelper.SSIS
{
    public partial class FixedWidthColumnsForm : Form
    {
        public FixedWidthColumnsForm()
        {
            InitializeComponent();
            this.Icon = BIDSHelper.Resources.Common.BIDSHelper;
            this.cboRaggedRightDelimiter.SelectedItem = "[None]";
        }

        private const string ClipboardFormat = "XML Spreadsheet";

        private void btnClipboard_Click(object sender, EventArgs e)
        {
            try
            {
                if (Clipboard.ContainsData(ClipboardFormat))
                {
                    object clipData = Clipboard.GetData(ClipboardFormat);
                    MemoryStream ms = clipData as MemoryStream;
                    if (ms != null)
                    {
                        XmlDocument xml = new XmlDocument();
                        xml.Load(ms);
                        XmlNodeList table = xml.GetElementsByTagName("Table");
                        if (table.Count > 0)
                        {
                            this.dataGridView1.Rows.Clear();
                            int iRow = 0;
                            foreach (XmlNode row in table[0].ChildNodes)
                            {
                                if (row.ChildNodes.Count >= 2)
                                {
                                    iRow++;
                                    XmlNode nodeColumnName = row.ChildNodes[0];
                                    XmlNode nodeWidth = row.ChildNodes[1];
                                    int width = 0;
                                    if (!Int32.TryParse(nodeWidth.InnerText, out width) || width == 0)
                                    {
                                        MessageBox.Show("There is a problem with the Width from row " + iRow + ":\r\n\r\n" + nodeWidth.InnerText);
                                        this.dataGridView1.Rows.Clear();
                                        return;
                                    }

                                    this.dataGridView1.Rows.Add(new object[] { nodeColumnName.InnerText, nodeWidth.InnerText });
                                }
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Please copy data from your clipboard from Excel 2003 or later.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}