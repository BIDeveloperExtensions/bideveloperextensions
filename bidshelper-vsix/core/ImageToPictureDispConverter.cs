using System;
using System.Collections.Generic;
using System.Text;

namespace BIDSHelper
{
    //from http://www.mztools.com/articles/2005/MZ2005007.aspx
    public class ImageToPictureDispConverter : System.Windows.Forms.AxHost
    {
        public ImageToPictureDispConverter()
            : base("{63109182-966B-4e3c-A8B2-8BC4A88D221C}")
        {
        }

        public static stdole.IPictureDisp GetIPictureDispFromImage(System.Drawing.Image objImage)
        {
            return (stdole.IPictureDisp)System.Windows.Forms.AxHost.GetIPictureDispFromPicture(objImage);
        }

        public static stdole.IPictureDisp GetMaskIPictureDispFromBitmap(System.Drawing.Bitmap bmp)
        {
            System.Drawing.Bitmap mask = new System.Drawing.Bitmap(bmp);
            try
            {
                for (int x = 0; x < mask.Width; x++)
                {
                    for (int y = 0; y < mask.Height; y++)
                    {
                        if (mask.GetPixel(x, y).A == 0)
                            mask.SetPixel(x, y, System.Drawing.Color.White);
                        else
                            mask.SetPixel(x, y, System.Drawing.Color.Black);
                    }
                }
                return GetIPictureDispFromImage(mask);
            }
            finally
            {
                mask.Dispose();
            }
        }
    }
}
