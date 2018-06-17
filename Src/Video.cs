namespace Digger
{
    public class Video
    {
        public delegate void RenderDelegate(byte[] pixels, int intensity);
        public delegate void InitVideoDelegate(byte[] pixels, byte[][] npalette, byte[][] ipalette);
        public RenderDelegate Render;
        public InitVideoDelegate InitVideo;
        public const int WIDTH = 320, HEIGHT = 200, SIZE = WIDTH * HEIGHT;
        public const int PIX_SIZE = 65535;

        private int currIntensity;
        private readonly byte[] pixels = new byte[PIX_SIZE];

        public byte[][][] palette = {// red  green  blue
            new byte[][] { new byte[] { 0x00, 0xAA, 0x00 }, new byte[] { 0xAA, 0x00, 0x00 }, new byte[] { 0xAA, 0x55, 0x00 } },
            new byte[][] { new byte[] { 0x55, 0xFF, 0x55 }, new byte[] { 0xFF, 0x55, 0x55 }, new byte[] { 0xFF, 0xFF, 0x55 } }
        };

        public void Init()
        {
            InitVideo?.Invoke(pixels, palette[0], palette[1]);
        }

        public void Clear()
        {
            for (int i = 0; i < SIZE; i++)
                pixels[i] = 0;
        }

        public void GetImage(int x, int y, short[] p, int w, int h)
        {

            int src = 0;
            int dest = y * WIDTH + (x & 0xfffc);

            for (int i = 0; i < h; i++)
            {
                int d = dest;
                for (int j = 0; j < w; j++)
                {
                    p[src++] = (short)((((((pixels[d] << 2) | pixels[d + 1]) << 2) | pixels[d + 2]) << 2) | pixels[d + 3]);
                    d += 4;
                    if (src == p.Length)
                        return;
                }
                dest += WIDTH;
            }

        }

        public int GetPixel(int x, int y)
        {
            int ofs = WIDTH * y + x & 0xfffc;
            return (((((pixels[ofs] << 2) | pixels[ofs + 1]) << 2) | pixels[ofs + 2]) << 2) | pixels[ofs + 3];
        }

        public void SetIntensity(int inten)
        {
            currIntensity = inten;
        }

        public void PutImage(int x, int y, short[] p, int w, int h)
        {
            PutImage(x, y, p, w, h, true);
        }

        public void PutImage(int x, int y, short[] p, int w, int h, bool b)
        {
            int src = 0;
            int dest = y * WIDTH + (x & 0xfffc);

            for (int i = 0; i < h; i++)
            {
                int d = dest;
                for (int j = 0; j < w; j++)
                {
                    short px = p[src++];
                    pixels[d + 3] = (byte)(px & 3);
                    px >>= 2;
                    pixels[d + 2] = (byte)(px & 3);
                    px >>= 2;
                    pixels[d + 1] = (byte)(px & 3);
                    px >>= 2;
                    pixels[d] = (byte)(px & 3);
                    d += 4;
                    if (src == p.Length)
                        return;
                }
                dest += WIDTH;
            }

        }

        public void PutImage(int x, int y, int ch, int w, int h)
        {
            short[] spr = CgaGrafx.cgatable[ch * 2];
            short[] msk = CgaGrafx.cgatable[ch * 2 + 1];

            int src = 0;
            int dest = y * WIDTH + (x & 0xfffc);

            for (int i = 0; i < h; i++)
            {
                int d = dest;
                for (int j = 0; j < w; j++)
                {
                    short px = spr[src];
                    short mx = msk[src];
                    src++;
                    if ((mx & 3) == 0)
                        pixels[d + 3] = (byte)(px & 3);
                    px >>= 2;
                    if ((mx & (3 << 2)) == 0)
                        pixels[d + 2] = (byte)(px & 3);
                    px >>= 2;
                    if ((mx & (3 << 4)) == 0)
                        pixels[d + 1] = (byte)(px & 3);
                    px >>= 2;
                    if ((mx & (3 << 6)) == 0)
                        pixels[d] = (byte)(px & 3);
                    d += 4;
                    if (src == spr.Length || src == msk.Length)
                    {
                        return;
                    }
                }
                dest += WIDTH;
            }

        }

        public void DrawTitleScreen()
        {
            int src = 0, dest = 0;

            while (src < CgaGrafx.cgatitledat.Length)
            {
                int b = CgaGrafx.cgatitledat[src++], l, c;

                if (b == 254)
                {
                    l = CgaGrafx.cgatitledat[src++];
                    if (l == 0)
                        l = 256;
                    c = CgaGrafx.cgatitledat[src++];
                }
                else
                {
                    l = 1;
                    c = b;
                }

                for (int i = 0; i < l; i++)
                {
                    int px = c, adst = 0;
                    if (dest < 32768)
                        adst = (dest / 320) * 640 + dest % 320;
                    else
                        adst = 320 + ((dest - 32768) / 320) * 640 + (dest - 32768) % 320;

                    pixels[adst + 3] = (byte)(px & 3);
                    px >>= 2;
                    pixels[adst + 2] = (byte)(px & 3);
                    px >>= 2;
                    pixels[adst + 1] = (byte)(px & 3);
                    px >>= 2;
                    pixels[adst + 0] = (byte)(px & 3);
                    dest += 4;
                    if (dest >= PIX_SIZE)
                        break;
                }

                if (dest >= PIX_SIZE)
                    break;
            }
        }

        public void Write(int x, int y, int ch, int c)
        {
            Write(x, y, ch, c, false);
        }

        public void Write(int x, int y, int ch, int c, bool upd)
        {
            int dest = x + y * WIDTH, ofs = 0, color = c & 3;

            ch -= 32;
            if ((ch < 0) || (ch > 0x5f))
                return;

            short[] chartab = Alpha.ascii2cga[ch];

            if (chartab == null)
                return;

            for (int i = 0; i < 12; i++)
            {
                int d = dest;
                for (int j = 0; j < 3; j++)
                {
                    int px = chartab[ofs++];
                    pixels[d + 3] = (byte)(px & color);
                    px >>= 2;
                    pixels[d + 2] = (byte)(px & color);
                    px >>= 2;
                    pixels[d + 1] = (byte)(px & color);
                    px >>= 2;
                    pixels[d] = (byte)(px & color);
                    d += 4;
                }
                dest += WIDTH;
            }

            if (upd)
                UpdateImage();
        }

        public void UpdateImage()
        {
            Render?.Invoke(pixels, currIntensity);
        }
    }
}