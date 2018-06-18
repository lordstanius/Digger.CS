namespace Digger
{
    public class Input
    {
        private readonly Game game;

        public const int KEY_LEFT = 37;
        public const int KEY_RIGHT = 39;
        public const int KEY_UP = 38;
        public const int KEY_DOWN = 40;
        public const int KEY_F1 = 112;
        public const int KEY_F10 = 121;

        public bool leftpressed, rightpressed, uppressed, downpressed, f1pressed, firepressed, minuspressed, pluspressed, f10pressed, escape, isPaused;

        public int keyPressed = 0;
        public int aKeyPressed;

        public int keydir = 0, keydir2 = 0;
        public bool firepflag = false, fire2pflag = false;

        private int dynamicdir = -1, staticdir = -1;

        public Input(Game game)
        {
            this.game = game;
        }

        public void CheckKeyBuffer()
        {
            if (pluspressed)
            {
                if (game.fps < 40)
                    game.fps += 2;
            }

            if (minuspressed)
            {
                if (game.fps > 6)
                    game.fps -= 2;
            }

            if (f10pressed)
            {
                escape = true;
                game.start = false;
                f10pressed = false;
            }
            
            /*  while (kbhit()) {
                akeypressed=getkey();
                switch (akeypressed) {
                  case 321: // F7
                    musicflag=!musicflag;
                    break;
                  case 323: // F9
                    soundflag=!soundflag;
                    break;
                  case 324: // F10
                    escape=true;
                }
            } */
        }

        public int GetAsciiKey(int make)
        {
            //int k;
            if ((make == ' ') || ((make >= 'a') && (make <= 'z')) || ((make >= '0') && (make <= '9')))
                return make;

            return 0;
            /*  if (make<2 || make>=58)
                return 0; 
              if (kbhit())
                k=getkey();
              else
                return 0;
              if (k>='a' && k<='A')
                k+='A'-'a'; */
        }

        public int GetDirection(int n)
        {
            int dir = ((n == 0) ? keydir : keydir2);
            if (n == 0)
            {
                if (game.recorder.isPlaying)
                    game.recorder.PlayGetDirection(ref dir, ref firepflag);

                game.recorder.PutDirection(dir, firepflag);
            }
            else
            {
                if (game.recorder.isPlaying)
                    game.recorder.PlayGetDirection(ref dir, ref fire2pflag);

                game.recorder.PutDirection(dir, fire2pflag);
            }

            return dir;
        }

        public void Key_DownPressed()
        {
            downpressed = true;
            dynamicdir = staticdir = 6;
        }

        public void Key_DownReleased()
        {
            downpressed = false;
            if (dynamicdir == 6)
                SetDirection();
        }

        public void Key_F1Pressed()
        {
            firepressed = true;
            f1pressed = true;
        }

        public void Key_F1Released()
        {
            f1pressed = false;
        }

        public void Key_LeftPressed()
        {
            leftpressed = true;
            dynamicdir = staticdir = 4;
        }

        public void Key_LeftReleased()
        {
            leftpressed = false;
            if (dynamicdir == 4)
                SetDirection();
        }

        public void Key_RightPressed()
        {
            rightpressed = true;
            dynamicdir = staticdir = 0;
        }

        public void Key_RightReleased()
        {
            rightpressed = false;
            if (dynamicdir == 0)
                SetDirection();
        }

        public void Key_UpPressed()
        {
            uppressed = true;
            dynamicdir = staticdir = 2;
        }

        void Key_UpReleased()
        {
            uppressed = false;
            if (dynamicdir == 2)
                SetDirection();
        }

        public void ProcessKey(int key)
        {
            if (isPaused && key == 0xa0) // space released in pause mode, ignore
                return;

            //System.Diagnostics.Debug.WriteLine("Key = 0x{0:x}", key);
            keyPressed = key;
            if (key > 0x80)
                aKeyPressed = key & 0x7f;
            switch (key)
            {
                case 0x4b: Key_LeftPressed(); break;
                case 0xcb: Key_LeftReleased(); break;
                case 0x4d: Key_RightPressed(); break;
                case 0xcd: Key_RightReleased(); break;
                case 0x48: Key_UpPressed(); break;
                case 0xc8: Key_UpReleased(); break;
                case 0x50: Key_DownPressed(); break;
                case 0xd0: Key_DownReleased(); break;
                case 0x3b: Key_F1Pressed(); break;
                case 0xbb: Key_F1Released(); break;
                //case 0x78: f10pressed = true; break;
                case 0xf8: f10pressed = true; break; // f10 keyup
                case 0x6b: pluspressed = true; break;
                case 0xeb: pluspressed = false; break;
                case 0x6d: minuspressed = true; break;
                case 0xed: minuspressed = false; break;
                case 0x20: isPaused = true; break;
            }
        }

        public void ReadDirection()
        {
            /*  int j; */
            keydir = staticdir;
            if (dynamicdir != -1)
                keydir = dynamicdir;
            staticdir = -1;
            if (f1pressed || firepressed)
                firepflag = true;
            else
                firepflag = false;
            firepressed = false;
        }

        public void SetDirection()
        {
            dynamicdir = -1;
            if (uppressed) dynamicdir = staticdir = 2;
            if (downpressed) dynamicdir = staticdir = 6;
            if (leftpressed) dynamicdir = staticdir = 4;
            if (rightpressed) dynamicdir = staticdir = 0;
        }

        public bool TestIfStarted()
        {
            bool startf = false;
            if (keyPressed != 0 && (keyPressed & 0x80) == 0 && keyPressed != 27 && keyPressed != 0x78)
            {
                startf = true;
                keyPressed = 0;
            }
            if (!startf)
                return false;
 
            return true;
        }
    }
}