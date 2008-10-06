using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PCDimNaturalizer
{
    public class ctlFancyCheckedListBoxItem
    {
        private string _text;
        private bool _bold;

        public ctlFancyCheckedListBoxItem(string text, bool bold) 
        {
            _bold = bold;
            _text = text;
        }

        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        public bool Bold
        {
            get { return _bold; }
            set { _bold = value; }
        }

        public void DrawItem(DrawItemEventArgs e)
        {
            if (_bold)
            {
                e.Graphics.FillRectangle(new SolidBrush(e.BackColor), new Rectangle(e.Bounds.X + 17, e.Bounds.Y - 1, e.Bounds.Width - 17, e.Bounds.Height + 1));
                Font _font = new Font(e.Font, FontStyle.Bold);
                Rectangle textBounds = new Rectangle(e.Bounds.X + 17,
                                                      e.Bounds.Y - 1,
                                                      e.Bounds.Width,
                                                      _font.Height);
                e.Graphics.DrawString(_text, new Font(e.Font, FontStyle.Bold), new SolidBrush(e.ForeColor), textBounds);
                e.DrawFocusRectangle();
            }
        }

        public override string ToString()
        {
            return _text;
        }

        public override bool Equals(object obj)
        {
            return _text == obj.ToString();
        }

        public override int GetHashCode()
        {
            return _text.GetHashCode();
        }
    }

    public class ctlFancyCheckedListBox : CheckedListBox
    {

        private Size _imageSize;
        private StringFormat _fmt;
        private Font _titleFont;
        private Font _detailsFont;

        public ctlFancyCheckedListBox(Font titleFont, Font detailsFont, Size imageSize, 
                         StringAlignment aligment, StringAlignment lineAligment)
        {
            _titleFont = titleFont;
            _detailsFont = detailsFont;
            _imageSize = imageSize;
            this.ItemHeight = _imageSize.Height + this.Margin.Vertical;
            _fmt = new StringFormat();
            _fmt.Alignment = aligment;
            _fmt.LineAlignment = lineAligment;
            _titleFont = titleFont;
            _detailsFont = detailsFont;
        }

        public ctlFancyCheckedListBox()
        {
            InitializeComponent();
            _imageSize = new Size(80,60);
            this.ItemHeight = _imageSize.Height + this.Margin.Vertical;
            _fmt = new StringFormat();
            _fmt.Alignment = StringAlignment.Near;
            _fmt.LineAlignment = StringAlignment.Near;
            _titleFont = new Font(this.Font, FontStyle.Bold);
            _detailsFont = new Font(this.Font, FontStyle.Regular);
            
        }


        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            // prevent from error Visual Designer
            if (this.Items.Count > 0)            
            {
                ctlFancyCheckedListBoxItem item = (ctlFancyCheckedListBoxItem)this.Items[e.Index];
                base.OnDrawItem(e);
                item.DrawItem(e);
            }                            
        }
       
        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
        }

        #endregion
    }
}
