using System;
using System.Collections.Generic;
using System.Text;

using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.IO;
using Microsoft.AnalysisServices;


public class VisualizeAttributeLattice
{
    public enum LatticeLayoutMethod { ShortRelationshipsFirst = 0, DeepestPathsFirst = 1, ShortSingleLevelRelationshipsFirst = 2 };
    public static Bitmap Render(Dimension d)
    {
        return Render(d, LatticeLayoutMethod.ShortRelationshipsFirst,false);
    }

    public static Bitmap Render(Dimension d, LatticeLayoutMethod method, bool ShowOnlyMultilevelRelationships)
    {
        if (ShowOnlyMultilevelRelationships)
        {
            if (d.Attributes.Count == 1)
            {
                //avoid an error
                ShowOnlyMultilevelRelationships = false;
            }
        }
        LatticeDrawing ld = new LatticeDrawing(d.Name);
        ld.LayoutMethod = method;
        foreach (DimensionAttribute a in d.Attributes)
        {
            ld.Nodes.Add(a.ID, new LatticeNode(a.Name));
            if (d.KeyAttribute == a)
            {
                ld.Nodes[a.ID].IsKey = true;
            }
            ld.Nodes[a.ID].Visible = a.AttributeHierarchyVisible;
            ld.Nodes[a.ID].Enabled = a.AttributeHierarchyEnabled;
        }
        foreach (DimensionAttribute a in d.Attributes)
        {
            foreach (AttributeRelationship r in a.AttributeRelationships)
            {
                ld.Nodes[a.ID].Relationships.Add(new LatticeRelationship(ld.Nodes[r.AttributeID], r.RelationshipType, r.Visible));
                if (d.KeyAttribute != a)
                {
                    ld.Nodes[r.AttributeID].NonKeyReferenceCount++;
                }
            }
        }
        if (ShowOnlyMultilevelRelationships)
        {
            foreach (DimensionAttribute a in d.Attributes)
            {
                if (a.AttributeRelationships.Count == 0 && ld.Nodes[a.ID].NonKeyReferenceCount == 0)
                {
                    ld.Nodes.Remove(a.ID);
                }
            }
            foreach (LatticeNode ln in ld.Nodes.Values)
            {
                for (int i = 0; i < ln.Relationships.Count; i++)
                {
                    LatticeRelationship lr = ln.Relationships[i];
                    if (!ld.Nodes.ContainsValue(lr.node))
                    {
                        ln.Relationships.Remove(lr);
                        i--;
                    }
                }
            }
        }


        ld.LayoutMethod = method;
        return ld.Render();
    }

    public static ImageCodecInfo GetJpegCodec()
    {
        foreach (ImageCodecInfo i in ImageCodecInfo.GetImageEncoders())
        {
            if (i.MimeType == "image/jpeg") return i;
        }
        throw new Exception("Could not find Jpeg codec!");
    }
}

class LatticeDrawing
{
    private Bitmap canvas;
    private int[] arColumnsUsed;
    private bool[,] arLayoutMatrix;
    private int maxDistanceFromKey = 0;
    private float maxNodeHeight = 50;

    private const int NODE_WIDTH = 100;
    private const int NODE_SPACING = 30;
    private Font NODE_FONT = new Font(FontFamily.GenericSansSerif, 10); //can't be a const
    private int maxNodesAcross = 1;

    public string Title;
    public System.Collections.Generic.Dictionary<string, LatticeNode> Nodes = new Dictionary<string, LatticeNode>();
    public VisualizeAttributeLattice.LatticeLayoutMethod LayoutMethod = VisualizeAttributeLattice.LatticeLayoutMethod.ShortRelationshipsFirst;
    public LatticeDrawing(string title)
    {
        this.Title = title;
    }

    public Bitmap Render()
    {
        canvas = new Bitmap(NODE_WIDTH, 1000);
        Graphics g = Graphics.FromImage(canvas);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        StringFormat centered = new StringFormat();
        centered.Alignment = StringAlignment.Center;

        //find out the max node height so that all nodes can be sized the same
        //also find the key attribute
        LatticeNode key = null;
        foreach (LatticeNode ln in this.Nodes.Values)
        {
            SizeF sz = g.MeasureString(ln.Text, NODE_FONT, NODE_WIDTH, centered);
            ln.TextHeight = sz.Height;
            if (maxNodeHeight < ln.TextHeight + 10) maxNodeHeight = ln.TextHeight + 10;
            if (ln.IsKey) key = ln;
        }
        g.Dispose();
        g = null;
        canvas.Dispose();
        canvas = null;

        //mark distance from key
        TraverseRelationshipsAndMarkDistance(key, 1);
        TraverseRelationshipsAndMarkMinDistance(key, 1);

        //throw error if any nodes aren't related to the key
        foreach (LatticeNode ln in this.Nodes.Values)
        {
            if (ln.DistanceFromKey == 0 && !ln.IsKey)
            {
                throw new Exception("Node " + ln.Text + " is not related to the key directly or indirectly!");
            }
        }

        //figure out the number of nodes wide and tall
        for (int i = 1; i < Nodes.Count; i++)
        {
            int tempCount = 0;
            foreach (LatticeNode ln in this.Nodes.Values)
            {
                if (ln.DistanceFromKey == i) tempCount++;
            }
            if (maxNodesAcross < tempCount) maxNodesAcross = tempCount;
            if (tempCount == 0)
            {
                break;
            }
            else
            {
                maxDistanceFromKey = i;
            }
        }

        if (LayoutMethod == VisualizeAttributeLattice.LatticeLayoutMethod.DeepestPathsFirst)
        {
            arColumnsUsed = new int[maxDistanceFromKey];
            foreach (LatticeNode ln in this.Nodes.Values)
            {
                if (!ln.IsKey)
                {
                    maxNodesAcross = Math.Max(maxNodesAcross, ++arColumnsUsed[ln.DistanceFromKey - 1]);
                }
            }
            if (maxNodesAcross % 2 == 0) maxNodesAcross++; //make sure it's an odd number so the center column of nodes will be in center
            TraverseRelationshipsAndMarkMaxDepth(key, 1);
            arLayoutMatrix = new bool[maxDistanceFromKey, maxNodesAcross];
            for (int i = maxDistanceFromKey + 1; i >= 0; i--)
            {
                TraverseDeepestRelationshipsAndMarkColumns(key, 1, i);
            }
        }
        else
        {
            //holds the number of columns which have been used so far
            arColumnsUsed = new int[maxDistanceFromKey];
            TraverseRelationshipsAndMarkColumns(key, 1);
            for (int i = 0; i < maxDistanceFromKey; i++)
            {
                if (maxNodesAcross < arColumnsUsed[i]) maxNodesAcross = arColumnsUsed[i];
            }
        }

        canvas = new Bitmap((int)(NODE_WIDTH * maxNodesAcross + NODE_SPACING * (maxNodesAcross + 1)), (int)(maxNodeHeight * (maxDistanceFromKey + 1) + NODE_SPACING * (maxDistanceFromKey + 2) + 50));
        g = Graphics.FromImage(canvas);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.FillRectangle(new SolidBrush(Color.White), 0, 0, canvas.Width, canvas.Height);

        //draw title
        Font titleFont = new Font(FontFamily.GenericSansSerif, 14, FontStyle.Bold);
        SizeF titleSize = g.MeasureString(this.Title, titleFont, canvas.Width, centered);
        g.DrawString(this.Title, titleFont, new SolidBrush(Color.Black), new RectangleF(0, Math.Max(50 / 2 - titleSize.Height / 2, 0), canvas.Width, Math.Max(50, titleSize.Height)), centered);

        //draw the relationships
        TraverseRelationshipsAndDraw(key, g);

        //draw the nodes
        foreach (LatticeNode ln in this.Nodes.Values)
        {
            RectangleF fillRect = new RectangleF(ln.x, ln.y, NODE_WIDTH, maxNodeHeight);
            g.FillRectangle(new SolidBrush((ln.Visible ? Color.LightBlue : Color.LightGray)), fillRect);
            //Color.FromArgb(153, 204, 204) //this color is in the default palette for a GIF

            RectangleF textRect = new RectangleF(ln.x, ln.y, NODE_WIDTH, maxNodeHeight);
            textRect.Y += (maxNodeHeight - ln.TextHeight) / 2;
            g.DrawString(ln.Text, new Font(FontFamily.GenericSansSerif, 10, (ln.Enabled ? FontStyle.Regular : FontStyle.Italic)), new SolidBrush((ln.Enabled ? Color.Black : Color.Gray)), textRect, centered);
        }

        g.Dispose();
        return canvas;
    }

    private void TraverseRelationshipsAndMarkDistance(LatticeNode ln, int DistanceFromKey)
    {
        foreach (LatticeRelationship lr in ln.Relationships)
        {
            LatticeNode r = lr.node;
            if (r.DistanceFromKey < DistanceFromKey) r.DistanceFromKey = DistanceFromKey;
            if (DistanceFromKey + 1 < Nodes.Count) //prevent circular references from causing an infinite loop
            {
                TraverseRelationshipsAndMarkDistance(r, DistanceFromKey + 1);
            }
        }
    }

    private void TraverseRelationshipsAndMarkMinDistance(LatticeNode ln, int DistanceFromKey)
    {
        foreach (LatticeRelationship lr in ln.Relationships)
        {
            LatticeNode r = lr.node;
            if (r.DistanceFromKey == DistanceFromKey)
            {
                TraverseRelationshipsAndMarkMinDistance(r, DistanceFromKey + 1);
            }
            if (ln.MinRelationshipDistance < r.DistanceFromKey - ln.DistanceFromKey)
            {
                ln.MinRelationshipDistance = r.DistanceFromKey - ln.DistanceFromKey;
            }
        }
    }

    private int TraverseRelationshipsAndMarkMaxDepth(LatticeNode ln, int DistanceFromKey)
    {
        int depth = DistanceFromKey;
        foreach (LatticeRelationship lr in ln.Relationships)
        {
            LatticeNode r = lr.node;
            if (r.DistanceFromKey >= DistanceFromKey)
            {
                depth = Math.Max(depth, TraverseRelationshipsAndMarkMaxDepth(r, DistanceFromKey + 1));
            }
        }
        ln.MaxRelationshipDepth = depth;
        return depth;
    }

    private void TraverseDeepestRelationshipsAndMarkColumns(LatticeNode ln, int DistanceFromKey, int MaxRelationshipDepth)
    {
        foreach (LatticeRelationship lr in ln.Relationships)
        {
            LatticeNode r = lr.node;
            if (r.MaxRelationshipDepth == MaxRelationshipDepth && r.MinColumnPosition == 0)
            {
                int pos = (int)Math.Round(maxNodesAcross / 2.0, MidpointRounding.AwayFromZero);
                bool bColumnSet = false;

                //start by going the only the direction (left or right) that the child node is going
                if (DistanceFromKey > 1)
                {
                    for (int j = 0; j < maxNodesAcross / 2.0; j++)
                    {
                        if (ln.MaxColumnPosition > pos && !arLayoutMatrix[DistanceFromKey - 1, pos + j - 1])
                        {
                            bColumnSet = true;
                            arLayoutMatrix[DistanceFromKey - 1, pos + j - 1] = true;
                            r.MinColumnPosition = pos + j;
                            r.MaxColumnPosition = pos + j;
                            break;
                        }
                        else if (ln.MinColumnPosition < pos && !arLayoutMatrix[DistanceFromKey - 1, pos - j - 1])
                        {
                            bColumnSet = true;
                            arLayoutMatrix[DistanceFromKey - 1, pos - j - 1] = true;
                            r.MinColumnPosition = pos - j;
                            r.MaxColumnPosition = pos - j;
                            break;
                        }
                    }
                }

                //if there's no room going the way that the child node went, then go the other way
                if (!bColumnSet)
                {
                    for (int j = 0; j < maxNodesAcross / 2.0; j++)
                    {
                        if (!arLayoutMatrix[DistanceFromKey - 1, pos + j - 1])
                        {
                            bColumnSet = true;
                            arLayoutMatrix[DistanceFromKey - 1, pos + j - 1] = true;
                            r.MinColumnPosition = pos + j;
                            r.MaxColumnPosition = pos + j;
                            break;
                        }
                        else if (!arLayoutMatrix[DistanceFromKey - 1, pos - j - 1])
                        {
                            bColumnSet = true;
                            arLayoutMatrix[DistanceFromKey - 1, pos - j - 1] = true;
                            r.MinColumnPosition = pos - j;
                            r.MaxColumnPosition = pos - j;
                            break;
                        }
                    }
                }
            }
            TraverseDeepestRelationshipsAndMarkColumns(r, DistanceFromKey + 1, MaxRelationshipDepth);
        }
    }

    private void TraverseRelationshipsAndMarkColumns(LatticeNode ln, int DistanceFromKey)
    {
        for (int i = ln.DistanceFromKey + 1; i <= maxDistanceFromKey; i++) //enumerate the levels above this node
        {
            List<int> loopOrder = new List<int>();
            if (LayoutMethod == VisualizeAttributeLattice.LatticeLayoutMethod.ShortSingleLevelRelationshipsFirst)
            {
                for (int j = i; j <= maxDistanceFromKey; j++) //enumerate the distance from the next node to the top... helps to start with a bunch of short relationships
                {
                    loopOrder.Add(j);
                }
            }
            else
            {
                int lastJ = -1;
                for (int j = i + 1; lastJ != i; j = (j >= maxDistanceFromKey ? i : j + 1)) //enumerate the distance from the next node to the top... helps to start with a bunch of short relationships
                {
                    lastJ = j;
                    loopOrder.Add(j);
                }
            }
            foreach (int j in loopOrder) //enumerate the distance from the next node to the top... helps to start with a bunch of short relationships
            {
                foreach (LatticeRelationship lr in ln.Relationships)
                {
                    LatticeNode r = lr.node;
                    if (r.DistanceFromKey == i && r.DistanceFromKey + r.MinRelationshipDistance == j)
                    {
                        if (ln.MinRelationshipColumnDistance < Math.Abs(ln.MinColumnPosition - r.MinColumnPosition))
                        {
                            ln.MinRelationshipColumnDistance = Math.Abs(ln.MinColumnPosition - r.MinColumnPosition);
                        }
                        if (r.DistanceFromKey >= DistanceFromKey)
                        {
                            if (r.MaxColumnPosition == arColumnsUsed[DistanceFromKey - 1] && ln.MaxColumnPosition > r.MaxColumnPosition)
                            {
                                if (r.MinColumnPosition == 0) r.MinColumnPosition = ln.MinColumnPosition;
                                arColumnsUsed[DistanceFromKey - 1] = ln.MaxColumnPosition;
                                r.MaxColumnPosition = arColumnsUsed[DistanceFromKey - 1];
                                TraverseRelationshipsAndMarkColumns(r, DistanceFromKey + 1);
                            }
                            else if (r.MinColumnPosition == 0)
                            {
                                r.MinColumnPosition = Math.Max(arColumnsUsed[DistanceFromKey - 1] + 1, ln.MinColumnPosition);
                                r.MaxColumnPosition = Math.Max(arColumnsUsed[DistanceFromKey - 1] + 1, ln.MaxColumnPosition);
                                arColumnsUsed[DistanceFromKey - 1] = r.MaxColumnPosition;
                                TraverseRelationshipsAndMarkColumns(r, DistanceFromKey + 1);
                            }
                        }
                    }
                }
            }
        }
    }

    private void TraverseRelationshipsAndDraw(LatticeNode ln, Graphics g)
    {
        if (ln.IsKey)
        {
            ln.x = canvas.Width / 2 - NODE_WIDTH / 2;
        }
        else if (ln.MinColumnPosition == ln.MaxColumnPosition)
        {
            ln.x = ln.MinColumnPosition * NODE_SPACING + (ln.MinColumnPosition - 1) * NODE_WIDTH;
        }
        else
        {
            ln.x = (((float)ln.MaxColumnPosition - ln.MinColumnPosition) / 2 + ln.MinColumnPosition) * NODE_SPACING + ((((float)ln.MaxColumnPosition - ln.MinColumnPosition) / 2 + ln.MinColumnPosition) - 1) * NODE_WIDTH;
        }
        ln.y = canvas.Height - (maxNodeHeight + NODE_SPACING) * (ln.DistanceFromKey + 1);

        ln.Rendered = true;

        foreach (LatticeRelationship lr in ln.Relationships)
        {
            LatticeNode r = lr.node;
            if (!r.Rendered)
            {
                TraverseRelationshipsAndDraw(r, g);
            }
            Pen pen = new Pen((lr.Visible ? Color.Green : Color.Gray), 1.55F);
            pen.DashStyle = (lr.RelationshipType == RelationshipType.Rigid ? DashStyle.Solid : DashStyle.Dash);
            if (lr.RelationshipType != RelationshipType.Rigid)
            {
                pen.DashPattern = new float[] { 1F, 1.55F };
                //pen.DashOffset = (int)((new Random()).NextDouble()*10);
            }
            if (r.DistanceFromKey - ln.DistanceFromKey > 1 && ln.x == r.x)
            {
                //multilevel relationships which are completely vertical could never be seen
                pen.Width = 5;
                pen.Color = Color.Red;
                pen.DashStyle = DashStyle.Dash;
                pen.DashPattern = new float[] { 0.4F, 1.5F };
            }

            //purposefully don't put an end cap on it... we could note the cardinality, but according to http://msdn2.microsoft.com/en-us/library/ms176124.aspx that has no impact
            //GraphicsPath hPath = new GraphicsPath();
            //hPath.AddLine(new Point(-2, -2), new Point(2, -2));
            //CustomLineCap cap = new CustomLineCap(null, hPath);
            //pen.CustomEndCap = cap;

            //g.DrawLine(new Pen(Color.White, 7), ln.x + NODE_WIDTH / 2, ln.y, r.x + NODE_WIDTH / 2, r.y + maxNodeHeight); //if we get a bunch of redundant relationships with crossing lines, we might want to turn this on
            g.DrawLine(pen, ln.x + NODE_WIDTH / 2, ln.y, r.x + NODE_WIDTH / 2, r.y + maxNodeHeight);
        }
    }
}

class LatticeNode
{
    public string Text;
    public bool IsKey = false;
    public int DistanceFromKey = 0;
    public int MinColumnPosition = 0;
    public int MaxColumnPosition = 0;
    public float TextHeight;
    public float x;
    public float y;
    public bool Rendered = false;
    public bool Visible = true;
    public bool Enabled = true;
    public int MinRelationshipDistance = 0;
    public int MinRelationshipColumnDistance = 0;
    public int NonKeyReferenceCount = 0;
    public int MaxRelationshipDepth = 0;
    public System.Collections.Generic.List<LatticeRelationship> Relationships = new List<LatticeRelationship>();
    public LatticeNode(string text)
    {
        this.Text = text;
    }
}

class LatticeRelationship
{
    public LatticeNode node;
    public bool Visible = true;
    public RelationshipType RelationshipType = RelationshipType.Rigid;

    public LatticeRelationship(LatticeNode ln, RelationshipType rt, bool visible)
    {
        this.node = ln;
        this.RelationshipType = rt;
        this.Visible = visible;
    }
}

public class VisualizeAttributeLatticeImageForReport
{
    public VisualizeAttributeLatticeImageForReport(string DimensionName, Bitmap image)
    {
        double reportWidthInches = 10.25;
        if (image.Width < reportWidthInches * image.HorizontalResolution)
        {
            Bitmap newimg = new Bitmap(Convert.ToInt32(reportWidthInches * image.HorizontalResolution), image.Height);
            Graphics g = Graphics.FromImage(newimg);
            g.FillRectangle(new SolidBrush(Color.White), 0, 0, newimg.Width, newimg.Height);
            g.DrawImage(image, new Point(newimg.Width / 2 - image.Width / 2, 0));
            image.Dispose();
            image = newimg;
        }
        MemoryStream mem = new MemoryStream();
        System.Drawing.Imaging.EncoderParameters parameters1 = BIDSHelper.VisualizeAttributeLatticeForm.GetEncoderParameters();
        image.Save(mem, VisualizeAttributeLattice.GetJpegCodec(), parameters1);
        _ImageBase64 = Convert.ToBase64String(mem.ToArray());
        _DimensionName = DimensionName;
    }
    
    private string _DimensionName;
    public string DimensionName
    {
        get { return _DimensionName; }
        set { _DimensionName = value; }
    }

    public string _ImageBase64;
    public string ImageBase64
    {
        get { return _ImageBase64; }
    }
}