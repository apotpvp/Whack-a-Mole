using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace AnimatedPictureBoxLibrary
{
    public class BitmapTools
    {
        public static Bitmap GetImageFromSpriteSheet(Bitmap bmSrc, Rectangle rectSrc)
        {
            //make destination bitmap as large as soure bitmap and same pixelformat
            Bitmap bmDest = new Bitmap(rectSrc.Width, rectSrc.Height, bmSrc.PixelFormat);
            Graphics graphics = Graphics.FromImage(bmDest);
            Rectangle rectDest = new Rectangle(0, 0, rectSrc.Width, rectSrc.Height);
            graphics.DrawImage(bmSrc, rectDest, rectSrc, GraphicsUnit.Pixel);
            return bmDest;
        }

        public static void CopyImage(Bitmap bmSrc, Rectangle rectSrc, Bitmap bmDest, Rectangle rectDest)
        {
            Graphics graphics = Graphics.FromImage(bmDest);
            graphics.DrawImage(bmSrc, rectDest, rectSrc, GraphicsUnit.Pixel);
        }

        public static void FillBitmapWithColor(Bitmap bitmap, Color color)
        {
            using (Graphics gfx = Graphics.FromImage(bitmap))
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(color.A, color)))
            {
                gfx.FillRectangle(brush, 0, 0, bitmap.Width, bitmap.Height);
            }
        }
    }
}
