using System.Drawing;
using System.Windows.Forms;

namespace Digger.Win32
{
    public class Window : Form
    {
        private ImageSource screen;
        private readonly Color[] npalette = { Color.Black, Color.Black, Color.Black, Color.Black };
        private readonly Color[] ipalette = { Color.Black, Color.Black, Color.Black, Color.Black };
        private readonly Color[][] palettes;

        public Window(Game game)
        {
            ClientSize = new Size(320, 200);
            Text = "Digger";
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            DoubleBuffered = true;
            game.Video.Render = Render;
            game.Video.InitVideo = InitVideo;

            palettes = new Color[][] { npalette, ipalette };

            KeyUp += (_, e) => game.KeyUp(e.KeyValue);
            KeyDown += (_, e) => game.KeyDown(e.KeyValue);
        }
        
        public void InitVideo(byte[] pixels, byte[][] npal, byte[][] ipal)
        {
            for (int i = 0; i < 3; i++)
            {
                npalette[i + 1] = Color.FromArgb(0xFF, npal[i][0], npal[i][1], npal[i][2]);
                ipalette[i + 1] = Color.FromArgb(0xFF, ipal[i][0], ipal[i][1], ipal[i][2]);
            }

            screen = new ImageSource(Video.WIDTH, Video.HEIGHT, npalette, pixels);
        }

        public void Render(byte[] pixels, int intensity)
        {
            screen.palette = palettes[intensity];
            screen.UpdateImage();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (screen != null)
                screen.Render(e.Graphics, ClientRectangle);
        }
    }
}
