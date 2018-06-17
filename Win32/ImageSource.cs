using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Digger.Win32
{
    public class ImageSource
    {
        public Color[] palette;

        private readonly byte[] pixels;
        private readonly int[] pix;
        private readonly int w;
        private readonly int h;
        private readonly Bitmap bmp;

        public ImageSource() { }

        public ImageSource(int w, int h, Color[] palette, byte[] pixels)
        {
            this.pixels = pixels;
            this.w = w;
            this.h = h;

            this.palette = palette;
            pix = new int[pixels.Length];
            bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
        }

        public void UpdateImage()
        {
            for (int i = 0; i < pixels.Length; i++)
                pix[i] = palette[pixels[i]].ToArgb();

            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(pix, 0, bmpData.Scan0, w * h);
            bmp.UnlockBits(bmpData);
        }

        public void Render(Graphics gfx)
        {
            gfx.DrawImage(bmp, 0, 0);
        }

        public void Render(Graphics gfx, Rectangle rect)
        {
            gfx.DrawImage(bmp, rect);
        }
    }
}
