using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
namespace GMap.NET.WindowsForms.Markers
{
    public  class GMarkerGoogleExt:GMarkerGoogle
    {
        public double KiloPos { get; set; } = 0.0;
        public string StationName { get; set; } = "";
        public string TrackName { get; set; } = "";
        private bool _IsMarker = false;


        public GMarkerGoogleExt(PointLatLng p,GMarkerGoogleType MarkerType,Color b,int r,bool isCircle):base(p,MarkerType)
        {
            this.IsHitTestVisible = true;
            InRouteIndex = -1;
            this.isCircle = isCircle;
            InitialCircleParas(b, r); 
        }

        public GMarkerGoogleExt(PointLatLng p, GMarkerGoogleType MarkerType) : base(p, MarkerType)
        {
            this.IsHitTestVisible = true;
            InRouteIndex = -1;
            this.isCircle = false; 
            InitialCircleParas(Color.Red);
        }
        public GMarkerGoogleExt(PointLatLng p, Bitmap Image) : base(p, Image)
        {
            this.IsHitTestVisible = true;
            InRouteIndex = -1;
        }
        public GMarkerGoogleExt(PointLatLng p, Bitmap Image, Color b, int r, bool isCircle) : base(p, Image)
        {
            this.IsHitTestVisible = true;
            InRouteIndex = -1;
            this.isCircle = isCircle; 
            InitialCircleParas(b, r); 
        }

        public GMarkerGoogleExt(PointLatLng p, Bitmap Image, GMapRouteExt route, Color b, int r, bool isCircle) : base(p, Image)
        {
            this.BindRoute = route;
            InRouteIndex = -1;
            this.isCircle = isCircle;
            InitialCircleParas(b, r);
        }
        private void InitialCircleParas(Color b, int r)
        {
            Size = new System.Drawing.Size(2 * r, 2 * r);
            Offset = new System.Drawing.Point(-r, -r);
            OutPen = new Pen(b, 2);
        }
        private void InitialCircleParas(Color b)
        {  
            OutPen = new Pen(b, 2);
        }
        private bool isCircle; 
        public GMapRouteExt BindRoute { get; set; } 
        public int InRouteIndex { get; set; }

        public Pen Pen
        {
            get;
            set;
        }

        public Pen OutPen
        {
            get;
            set;
        }
        public bool AddRect { get; set; }
        public override void OnRender(Graphics g)
        {
            try
            {
                if (g == null)
                    return;
                if(_IsMarker)
                {
                    var bitmap = RotateImage(this.Bitmap, _angle);
                    var offsetX = LocalPosition.X - ((LocalPosition.X - 30) + bitmap.Width / 2);
                    var offsetY = LocalPosition.Y - ((LocalPosition.Y - 60) + bitmap.Height / 2);

                    //g.DrawImageUnscaled(bitmap, LocalPosition.X + offsetX, LocalPosition.Y + offsetY);
                    g.DrawImageUnscaled(bitmap, LocalPosition.X, LocalPosition.Y);
                    return;
                }
                Rectangle rect = new Rectangle(LocalPosition.X, LocalPosition.Y, Size.Width, Size.Height);
                if (!isCircle)
                {
                    base.OnRender(g);
                    if (this.AddRect)
                    {
                        g.DrawRectangle(new Pen(Color.Red, this.OutPen.Width), rect);
                    }
                }
                else
                {

                    if (OutPen != null)
                    {
                        g.DrawEllipse(OutPen, rect);
                    }
                    if (this.AddRect)
                    {
                        g.DrawRectangle(new Pen(Color.Red, this.OutPen.Width), rect);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        #region  旋转
        private static Bitmap RotateImage(Image image, float angle)
        {
            if (image == null)
                throw new ArgumentNullException("image");
            const double pi2 = Math.PI / 2.0;
            // Why can't C# allow these to be const, or at least readonly
            // *sigh*  I'm starting to talk like Christian Graus :omg:
            double oldWidth = (double)image.Width;
            double oldHeight = (double)image.Height;
            // Convert degrees to radians
            double theta = ((double)angle) * Math.PI / 180.0;
            double locked_theta = theta;
            // Ensure theta is now [0, 2pi)
            while (locked_theta < 0.0)
                locked_theta += 2 * Math.PI;
            double newWidth, newHeight;
            int nWidth, nHeight; // The newWidth/newHeight expressed as ints
            #region Explaination of the calculations
            /*
			 * The trig involved in calculating the new width and height
			 * is fairly simple; the hard part was remembering that when 
			 * PI/2 <= theta <= PI and 3PI/2 <= theta < 2PI the width and 
			 * height are switched.
			 * 
			 * When you rotate a rectangle, r, the bounding box surrounding r
			 * contains for right-triangles of empty space.  Each of the 
			 * triangles hypotenuse's are a known length, either the width or
			 * the height of r.  Because we know the length of the hypotenuse
			 * and we have a known angle of rotation, we can use the trig
			 * function identities to find the length of the other two sides.
			 * 
			 * sine = opposite/hypotenuse
			 * cosine = adjacent/hypotenuse
			 * 
			 * solving for the unknown we get
			 * 
			 * opposite = sine * hypotenuse
			 * adjacent = cosine * hypotenuse
			 * 
			 * Another interesting point about these triangles is that there
			 * are only two different triangles. The proof for which is easy
			 * to see, but its been too long since I've written a proof that
			 * I can't explain it well enough to want to publish it.  
			 * 
			 * Just trust me when I say the triangles formed by the lengths 
			 * width are always the same (for a given theta) and the same 
			 * goes for the height of r.
			 * 
			 * Rather than associate the opposite/adjacent sides with the
			 * width and height of the original bitmap, I'll associate them
			 * based on their position.
			 * 
			 * adjacent/oppositeTop will refer to the triangles making up the 
			 * upper right and lower left corners
			 * 
			 * adjacent/oppositeBottom will refer to the triangles making up 
			 * the upper left and lower right corners
			 * 
			 * The names are based on the right side corners, because thats 
			 * where I did my work on paper (the right side).
			 * 
			 * Now if you draw this out, you will see that the width of the 
			 * bounding box is calculated by adding together adjacentTop and 
			 * oppositeBottom while the height is calculate by adding 
			 * together adjacentBottom and oppositeTop.
			 */
            #endregion
            double adjacentTop, oppositeTop;
            double adjacentBottom, oppositeBottom;
            // We need to calculate the sides of the triangles based
            // on how much rotation is being done to the bitmap.
            //   Refer to the first paragraph in the explaination above for 
            //   reasons why.
            if ((locked_theta >= 0.0 && locked_theta < pi2) ||
                (locked_theta >= Math.PI && locked_theta < (Math.PI + pi2)))
            {
                adjacentTop = Math.Abs(Math.Cos(locked_theta)) * oldWidth;
                oppositeTop = Math.Abs(Math.Sin(locked_theta)) * oldWidth;
                adjacentBottom = Math.Abs(Math.Cos(locked_theta)) * oldHeight;
                oppositeBottom = Math.Abs(Math.Sin(locked_theta)) * oldHeight;
            }
            else
            {
                adjacentTop = Math.Abs(Math.Sin(locked_theta)) * oldHeight;
                oppositeTop = Math.Abs(Math.Cos(locked_theta)) * oldHeight;
                adjacentBottom = Math.Abs(Math.Sin(locked_theta)) * oldWidth;
                oppositeBottom = Math.Abs(Math.Cos(locked_theta)) * oldWidth;
            }
            newWidth = adjacentTop + oppositeBottom;
            newHeight = adjacentBottom + oppositeTop;
            nWidth = (int)Math.Ceiling(newWidth);
            nHeight = (int)Math.Ceiling(newHeight);
            Bitmap rotatedBmp = new Bitmap(nWidth, nHeight);
            using (Graphics g = Graphics.FromImage(rotatedBmp))
            {
                // This array will be used to pass in the three points that 
                // make up the rotated image
                Point[] points;
                /*
                 * The values of opposite/adjacentTop/Bottom are referring to 
                 * fixed locations instead of in relation to the
                 * rotating image so I need to change which values are used
                 * based on the how much the image is rotating.
                 * 
                 * For each point, one of the coordinates will always be 0, 
                 * nWidth, or nHeight.  This because the Bitmap we are drawing on
                 * is the bounding box for the rotated bitmap.  If both of the 
                 * corrdinates for any of the given points wasn't in the set above
                 * then the bitmap we are drawing on WOULDN'T be the bounding box
                 * as required.
                 */
                if (locked_theta >= 0.0 && locked_theta < pi2)
                {
                    points = new Point[] {
                                             new Point( (int) oppositeBottom, 0 ),
                                             new Point( nWidth, (int) oppositeTop ),
                                             new Point( 0, (int) adjacentBottom )
                                         };
                }
                else if (locked_theta >= pi2 && locked_theta < Math.PI)
                {
                    points = new Point[] {
                                             new Point( nWidth, (int) oppositeTop ),
                                             new Point( (int) adjacentTop, nHeight ),
                                             new Point( (int) oppositeBottom, 0 )
                                         };
                }
                else if (locked_theta >= Math.PI && locked_theta < (Math.PI + pi2))
                {
                    points = new Point[] {
                                             new Point( (int) adjacentTop, nHeight ),
                                             new Point( 0, (int) adjacentBottom ),
                                             new Point( nWidth, (int) oppositeTop )
                                         };
                }
                else
                {
                    points = new Point[] {
                                             new Point( 0, (int) adjacentBottom ),
                                             new Point( (int) oppositeBottom, 0 ),
                                             new Point( (int) adjacentTop, nHeight )
                                         };
                }
                g.DrawImage(image, points);
            }
            return rotatedBmp;
        }

        public float _angle;
        #endregion
        //public override void OnRender(Graphics g)
        //{
        //    try
        //    {
        //        if (g == null)
        //            return;
        //        //var bitmap = RotateImage(_bitmap, _angle);
        //        var bitmap = RotateImage(this.Bitmap, _angle);
        //        var offsetX = LocalPosition.X - ((LocalPosition.X - 30) + bitmap.Width / 2);
        //        var offsetY = LocalPosition.Y - ((LocalPosition.Y - 60) + bitmap.Height / 2);

        //        //g.DrawImageUnscaled(bitmap, LocalPosition.X + offsetX, LocalPosition.Y + offsetY);
        //        g.DrawImageUnscaled(bitmap, LocalPosition.X , LocalPosition.Y);

        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}

        public override void Dispose()
        {
            if (Pen != null)
            {
                Pen.Dispose();
                Pen = null;
            }

            if (OutPen != null)
            {
                OutPen.Dispose();
                OutPen = null;
            }

            base.Dispose();
        }
    } 
}
