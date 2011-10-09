using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace BIDSHelper.SSIS.PerformanceVisualization
{
    public class DtsGanttGrid : DtsGrid
    {
        private const double PIXELS_PER_SECOND = 8;
        private Font FONT_DIAMOND_SUPERSCRIPT = new Font("Arial", 6F);
        private DateTime _DataBindingDate;

        public DtsGanttGrid()
        {
            InitializeComponent();
        }

        public DtsGanttGrid(IContainer container)
        {
            container.Add(this);
            InitializeComponent();
        }

        private void SetupComponent()
        {
            System.Windows.Forms.DataGridViewTextBoxColumn barDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            barDataGridViewTextBoxColumn.HeaderText = "Name";
            barDataGridViewTextBoxColumn.Name = "barDataGridViewTextBoxColumn";
            barDataGridViewTextBoxColumn.ReadOnly = true;
            barDataGridViewTextBoxColumn.Width = 0;
            this.Columns.Add(barDataGridViewTextBoxColumn);

            this.BackgroundColor = Color.White;
        }

        protected override void OnDataBindingComplete(DataGridViewBindingCompleteEventArgs e)
        {
            base.OnDataBindingComplete(e);
            SetupComponent();
            listRowBitmaps = new List<Bitmap>(new Bitmap[this.Rows.Count]);
            _DataBindingDate = DateTime.Now;
        }

        protected override void OnCellPainting(DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex == 1)
            {
                e.Paint(e.ClipBounds, DataGridViewPaintParts.Background | DataGridViewPaintParts.Border);
                DrawGanttBar(e, e.RowIndex);
                e.Handled = true;
            }
            else
            {
                base.OnCellPainting(e);
            }
        }


        List<Bitmap> listRowBitmaps = null;

        private void DrawGanttBar(DataGridViewCellPaintingEventArgs e, int RowIndex)
        {
            Graphics graphics = e.Graphics;
            if (listRowBitmaps == null) return;
            if (listRowBitmaps.Count > RowIndex && listRowBitmaps[RowIndex] != null)
            {
                RectangleF clipRectangle = base.GetCellDisplayRectangle(1, RowIndex, false);
                graphics.DrawImage(listRowBitmaps[RowIndex], e.CellBounds.X, e.CellBounds.Y);
                return;
            }

            IDtsGridRowData dataPackage = (IDtsGridRowData)((BindingSource)DataSource)[0];
            if (dataPackage.DateRanges.Count > 0)
            {
                TimeSpan tsTotal;
                if (dataPackage.DateRanges[0].EndDate > DateTime.MinValue)
                    tsTotal = dataPackage.DateRanges[0].EndDate.Subtract(dataPackage.DateRanges[0].StartDate);
                else
                    tsTotal = _DataBindingDate.Subtract(dataPackage.DateRanges[0].StartDate);
                if (Columns[1].Width != ((int)(tsTotal.TotalSeconds * PIXELS_PER_SECOND + 2)))
                {
                    Columns[1].Width = ((int)(tsTotal.TotalSeconds * PIXELS_PER_SECOND + 2));
                }

                RectangleF clipRectangle = base.GetCellDisplayRectangle(1, RowIndex, false);
                graphics.SetClip(clipRectangle);
                float height = clipRectangle.Height;
                int num4 = this.Columns[1].Width - (int)clipRectangle.Width;

                if (this.IsRowVisible(RowIndex))
                {
                    int iRowHeight = Rows[RowIndex].Height;
                    int iBarHeight = iRowHeight - 6;

                    Bitmap canvas = new Bitmap((int)clipRectangle.Width, (int)clipRectangle.Height, e.Graphics);
                    Graphics gBitmap = Graphics.FromImage(canvas);
                    gBitmap.SmoothingMode = SmoothingMode.AntiAlias;
                    gBitmap.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                    IDtsGridRowData data = (IDtsGridRowData)((BindingSource)DataSource)[RowIndex];
                    DateRange rangePrev = null;
                    int iOverlappingDiamonds = 1;
                    for (int iRange = 0; iRange < data.DateRanges.Count; iRange++)
                    {
                        DateRange range = data.DateRanges[iRange];
                        TimeSpan tsLeft = range.StartDate.Subtract(dataPackage.DateRanges[0].StartDate);
                        if (data.Type == typeof(PipelinePath))
                        {
                            int x = (int)(PIXELS_PER_SECOND * tsLeft.TotalSeconds);
                            if (x > 0)
                                x -= BIDSHelper.Resources.Common.SmallBlueDiamond.Width / 2;
                            else
                                x -= 4;

                            if (rangePrev != null && rangePrev.StartDate == range.StartDate)
                            {
                                iOverlappingDiamonds++;
                                PointF pointDot = new PointF(x, 0);
                                if (iRange == data.DateRanges.Count - 1 || data.DateRanges[iRange + 1].StartDate != range.StartDate)
                                {
                                    string s = (iOverlappingDiamonds > 9 ? "+" : iOverlappingDiamonds.ToString());
                                    gBitmap.DrawString(s, FONT_DIAMOND_SUPERSCRIPT, Brushes.DarkBlue, new PointF(x + 7F, -1F));
                                    gBitmap.DrawIcon(BIDSHelper.Resources.Common.SmallBlueDiamond, x, (int)3);
                                }
                            }
                            else
                            {
                                iOverlappingDiamonds = 1;
                                gBitmap.DrawIcon(BIDSHelper.Resources.Common.SmallBlueDiamond, x, (int)3);
                            }
                        }
                        else
                        {
                            TimeSpan tsBar;
                            if (range.EndDate > DateTime.MinValue)
                                tsBar = range.EndDate.Subtract(range.StartDate);
                            else
                                tsBar = _DataBindingDate.Subtract(range.StartDate);
                            float x = (float)(PIXELS_PER_SECOND * tsLeft.TotalSeconds);
                            float width = (float)(PIXELS_PER_SECOND * tsBar.TotalSeconds);
                            if (width < (float)(iBarHeight) / 2f)
                            {
                                width = (float)(iBarHeight) / 2f;
                                if (x > 0)
                                    x -= width / 2f;
                            }

                            GraphicsPath path = GetRoundedRectanglePath(x, 2, width, (float)(iBarHeight), (float)((iBarHeight) / 5f));
                            Color colorDark = (data.IsError ? Color.DarkRed : Color.DarkBlue);
                            Color colorLight = (data.IsError ? Color.Red : Color.Blue);
                            gBitmap.FillPath(new LinearGradientBrush(new Point(0, 2), new Point(0, 2 + iBarHeight), Color.FromArgb(0x80, colorLight), Color.FromArgb(0xe0, colorDark)), path);
                            gBitmap.DrawPath(Pens.Black, path);
                        }
                        rangePrev = range;
                    }

                    //cache the bitmap so the expense of recreating it won't have to happen on scroll of the grid
                    if (listRowBitmaps.Count > RowIndex)
                    {
                        listRowBitmaps[RowIndex] = canvas;
                        clipRectangle = base.GetCellDisplayRectangle(1, RowIndex, false);
                        graphics.DrawImage(listRowBitmaps[RowIndex], e.CellBounds.X, e.CellBounds.Y);
                    }
                }
            }
            graphics.ResetClip();
        }




        private static GraphicsPath GetRoundedRectanglePath(float x, float y, float width, float height, float radius)
        {
            float num = Math.Min(radius * 2f, Math.Max(width, height));
            RectangleF rect = new RectangleF(x, y, num, num);
            GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(rect, 180f, 90f);
            rect.X = (x + width) - num;
            path.AddArc(rect, 270f, 90f);
            rect.Y = (y + height) - num;
            path.AddArc(rect, 0f, 90f);
            rect.X = x;
            path.AddArc(rect, 90f, 90f);
            path.CloseFigure();
            return path;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (listRowBitmaps != null)
                {
                    for (int i = 0; i < listRowBitmaps.Count; i++)
                    {
                        if (listRowBitmaps[i] != null)
                        {
                            listRowBitmaps[i].Dispose();
                            listRowBitmaps[i] = null;
                        }
                    }
                }
                if (components != null)
                {
                    components.Dispose();
                }
            }

            base.Dispose(disposing);
        }

    }
}
