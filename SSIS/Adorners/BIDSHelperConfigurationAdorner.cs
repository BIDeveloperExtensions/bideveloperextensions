using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.IntegrationServices.Designer.View;
using System.Windows;
using System.Windows.Media;
using Microsoft.SqlServer.Graph.Model;
using System.Windows.Documents;

namespace BIDSHelper.SSIS.Adorners
{
    class BIDSHelperConfigurationAdorner : AdornerBase
    {

        private readonly List<Tuple<Point, ImageSource>> m_images;

        private BIDSHelperConfigurationAdorner m_adorner;
        private AdornerLayer m_adornerLayer;
        private object m_tag;
        private FrameworkElement m_view;
        private Type m_type;

        public BIDSHelperConfigurationAdorner(FrameworkElement adornedElement, Type type)
            : base(adornedElement)
        {
            this.m_images = new List<Tuple<Point, ImageSource>>();

            if (adornedElement == null)
            {
                throw new ArgumentNullException("adornedElement");
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            this.m_type = type;
            this.m_tag = this.GetType().FullName;
            this.m_view = adornedElement;
            this.m_adornerLayer = AdornerLayer.GetAdornerLayer(adornedElement);
            if (this.m_adornerLayer == null)
            {
                return;
            }
            System.Windows.Documents.Adorner[] adorners = this.m_adornerLayer.GetAdorners(adornedElement);
            if (adorners != null)
            {
                foreach (System.Windows.Documents.Adorner adorner in adorners)
                {
                    if (Convert.ToString(adorner.Tag) == Convert.ToString(m_tag))
                    {
                        this.m_adorner = (BIDSHelperConfigurationAdorner)adorner;
                        break;
                    }
                }
            }
        }


        public void UpdateAdorner(bool bHasExpression, bool bHasConfiguration)
        {
            if (!bHasExpression && !bHasConfiguration)
            {
                DetachAdorner();
                return;
            }

            if (this.m_adornerLayer != null)
            {
                if (this.m_adorner == null)
                {
                    this.m_adorner = this;
                    m_adorner.Tag = this.m_tag;
                    this.m_adornerLayer.Add(this.m_adorner);
                }

                m_adorner.Images.Clear();
                System.Windows.Media.Imaging.BitmapSource s_Icon;
                if (bHasExpression && bHasConfiguration)
                {
                    s_Icon = HighlightingToDo.GetBitmapSource(ExpressionHighlighterPlugin.ExpressionColor, ExpressionHighlighterPlugin.ConfigurationColor);
                    m_adorner.ToolTip = "Controlled by expression and configuration";
                }
                else if (bHasExpression)
                {
                    s_Icon = HighlightingToDo.GetBitmapSource(ExpressionHighlighterPlugin.ExpressionColor);
                    m_adorner.ToolTip = "Controlled by expression";
                }
                else
                {
                    s_Icon = HighlightingToDo.GetBitmapSource(ExpressionHighlighterPlugin.ConfigurationColor);
                    m_adorner.ToolTip = "Controlled by configuration";
                }

                m_adorner.Images.Add(new Tuple<Point, System.Windows.Media.ImageSource>(GetImagePosition(), s_Icon));
                m_adorner.InvalidateVisual();
                this.m_adornerLayer.UpdateLayout();
            }
        }

        private void DetachAdorner()
        {
            if ((this.m_adornerLayer != null) && (this.m_adorner != null))
            {
                this.m_adornerLayer.Remove(this.m_adorner);
                this.m_adornerLayer.Update();
                this.m_adorner = null;
            }
        }

        private Point GetImagePosition()
        {
            double x = 0;
            double y = 0;
            if (m_type == typeof(Microsoft.SqlServer.Graph.Model.ModelElement)) //if this is a control flow task or a data flow component
            {
                x = 2;
                y = m_view.ActualHeight / 2.0 - 5;
            }
            else if (m_type == typeof(Microsoft.SqlServer.IntegrationServices.Designer.ConnectionManagers.ConnectionManagerModelElement))
            {
                x = 4;
                y = 0;
            }
            return new Point(x, y);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            foreach (Tuple<Point, ImageSource> tuple in this.m_images)
            {
                Point location = tuple.Item1;
                ImageSource imageSource = tuple.Item2;
                drawingContext.DrawImage(imageSource, new Rect(location, new Size(imageSource.Width, imageSource.Height)));
            }
        }

        protected override void OnSizeChangedOfAdornedElement(object sender, EventArgs args)
        {
            if (m_view != null && m_adorner != null && m_adorner.Images.Count > 0)
            {
                for (int i = 0; i < m_adorner.Images.Count; i++)
                {
                    m_adorner.Images[i] = new Tuple<Point, ImageSource>(GetImagePosition(), m_adorner.Images[i].Item2);
                }
            }
        }

        public List<Tuple<Point, ImageSource>> Images
        {
            get
            {
                return this.m_images;
            }
        }

    
    }
}
