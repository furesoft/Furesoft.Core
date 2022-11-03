using System;
using System.Windows.Forms;
using System.Drawing;

namespace DDPanBox;

class DDPanBox : Control
{
    public DDPanBox()
    {
        //Set up double buffering and a little extra.
        this.SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer |
        ControlStyles.AllPaintingInWmPaint | ControlStyles.SupportsTransparentBackColor,
        true);

      //set the part of the source image to be drawn.
      //DrawRect = DeflateRect(ClientRectangle, Padding);
        DrawRect = ClientRectangle;

      //Subscribe to our event handlers.
      this.MouseDown += new MouseEventHandler(panBox_MouseDown);
      this.MouseMove += new MouseEventHandler(panBox_MouseMove);
      this.MouseUp += new MouseEventHandler(panBox_MouseUp);
      this.Resize += new EventHandler(panBox_Resize);

    }
    //if true were at 2:1 false then 1:1
    bool zoom = false;

    bool dragging = false; //Tells us if our image has been clicked on.

    Point start = new(); //Keep initial click for accurate panning.

    void panBox_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            dragging = true;
            //offset new point by original one so we know where in the image we are.
            start = new Point(e.Location.X + DrawRect.Location.X, e.Location.Y + DrawRect.Location.Y);
            Cursor = Cursors.SizeAll; //just for looks.
        }
        else if (e.Button == MouseButtons.Right)
        {
            zoom = !zoom;
            if (zoom)
            {
                // modify the drawrect to a smaller rectangle to zoom, and center it in previous drawrect
                DrawRect = new Rectangle(DrawRect.X + ClientRectangle.Width / 4, DrawRect.Y + ClientRectangle.Height / 4, DrawRect.Width / 2, DrawRect.Height / 2);
            }
            else
            {
                //Do the reverse of above
                DrawRect = new Rectangle(DrawRect.X - ClientRectangle.Width/4, DrawRect.Y - ClientRectangle.Height/4, ClientRectangle.Width, ClientRectangle.Height);
            }
            //Calculate Draw Rectangle by calling the resize event.
            panBox_Resize(null, EventArgs.Empty);
        }
    }

    void panBox_MouseMove(object sender, MouseEventArgs e)
    {
        if (dragging)
        {
            DrawRect.Location = new Point(start.X - e.Location.X, start.Y - e.Location.Y);

            if (DrawRect.Location.X < 0 -Padding.Left)
                DrawRect.Location = new Point(0 - Padding.Left, DrawRect.Location.Y);

            if (DrawRect.Location.Y < 0 - Padding.Top)
                DrawRect.Location = new Point(DrawRect.Location.X, 0 - Padding.Top);

            if (DrawRect.Location.X > _Image.Width - DrawRect.Width + Padding.Right)
                DrawRect.Location = new Point(_Image.Width - DrawRect.Width + Padding.Right, DrawRect.Location.Y);

            if (DrawRect.Location.Y > _Image.Height - DrawRect.Height + Padding.Bottom)
                DrawRect.Location = new Point(DrawRect.Location.X, _Image.Height - DrawRect.Height + Padding.Bottom);

            this.Refresh();
        }
    }

    void panBox_MouseUp(object sender, MouseEventArgs e)
    {
        dragging = false;
        Cursor = Cursors.Default;
    }

    void panBox_Resize(object sender, EventArgs e)
    {
        if (_Image != null)
        {
            if (zoom)
            {
                DrawRect = new Rectangle(DrawRect.Location.X, DrawRect.Location.Y, ClientRectangle.Width / 2, ClientRectangle.Height / 2);
            }
            else
                DrawRect = new Rectangle(DrawRect.Location.X, DrawRect.Location.Y, ClientRectangle.Width, ClientRectangle.Height);

            if (DrawRect.Location.X < 0 - Padding.Left)
                DrawRect.Location = new Point(0 - Padding.Left, DrawRect.Location.Y);

            if (DrawRect.Location.Y < 0 - Padding.Top)
                DrawRect.Location = new Point(DrawRect.Location.X, 0 - Padding.Top);

            if (DrawRect.Location.X > _Image.Width - DrawRect.Width + Padding.Right)
                DrawRect.Location = new Point(_Image.Width - DrawRect.Width + Padding.Right, DrawRect.Location.Y);

            if (DrawRect.Location.Y > _Image.Height - DrawRect.Height + Padding.Bottom)
                DrawRect.Location = new Point(DrawRect.Location.X, _Image.Height - DrawRect.Height + Padding.Bottom);

            this.Refresh();
        }
    }

    private Image _Image;

    public Image Image
    {
        get
        {
            return _Image;
        }
        set
        {
            _Image = value;
            //Calculate Draw Rectangle by calling the resize event.
            panBox_Resize(this, EventArgs.Empty);
        }
    }

    private Rectangle DrawRect;


    protected override void OnPaint(PaintEventArgs e)
    {
        if (_Image != null)
        {
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            e.Graphics.DrawImage(_Image, ClientRectangle, DrawRect, GraphicsUnit.Pixel);

            if(zoom)
                e.Graphics.DrawString("Zoom 2:1", this.Font, Brushes.White, new PointF(15F, 15F));
            else
                e.Graphics.DrawString("Zoom 1:1", this.Font, Brushes.White, new PointF(15F, 15F));
        }

        base.OnPaint(e);
    }

    public static Rectangle DeflateRect(Rectangle rect, Padding padding)
    {
        rect.X += padding.Left;
        rect.Y += padding.Top;
        rect.Width -= padding.Horizontal;
        rect.Height -= padding.Vertical;
        return rect;

    }

    

    
}
