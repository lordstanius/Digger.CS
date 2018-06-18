namespace Digger
{
    public class Sprite
    {
        private readonly Game game;

        private bool retrflag = true;
        private readonly bool[] spriteRedrawFlag = new bool[17];
        private readonly bool[] spriteRecursionFlag = new bool[17];
        private readonly bool[] spriteEnabledFlag = new bool[16];
        private readonly int[] spriteChar = new int[17];
        private readonly short[][] spriteBuffer = new short[16][];
        private readonly int[] spriteX = new int[17];
        private readonly int[] spriteY = new int[17];
        private readonly int[] spriteWidth = new int[17];
        private readonly int[] spriteHeight = new int[17];
        private readonly int[] spriteBorderWidth = new int[16];
        private readonly int[] spriteBorderHeight = new int[16];
        private readonly int[] newSpriteChar = new int[16];
        private readonly int[] newSpriteWidth = new int[16];
        private readonly int[] newSpriteHeight = new int[16];
        private readonly int[] newSpriteBorderWidth = new int[16];
        private readonly int[] newSpriteBorderHeight = new int[16];
        private readonly int[] defaultSpriteOrder = new int[16];
        private int[] spriteOrder;

        public Sprite(Game game)
        {
            this.game = game;
            spriteOrder = defaultSpriteOrder;
        }

        public bool IsInBorderCollision(int bx, int si)
        {
            if (spriteX[bx] >= spriteX[si])
            {
                if (spriteX[bx] + spriteBorderWidth[bx] > spriteWidth[si] * 4 + spriteX[si] - spriteBorderWidth[si] - 1)
                    return false;
            }
            else if (spriteX[si] + spriteBorderWidth[si] > spriteWidth[bx] * 4 + spriteX[bx] - spriteBorderWidth[bx] - 1)
                return false;

            if (spriteY[bx] >= spriteY[si])
            {
                if (spriteY[bx] + spriteBorderHeight[bx] <= spriteHeight[si] + spriteY[si] - spriteBorderHeight[si] - 1)
                    return true;
                return false;
            }

            if (spriteY[si] + spriteBorderHeight[si] <= spriteHeight[bx] + spriteY[bx] - spriteBorderHeight[bx] - 1)
                return true;

            return false;
        }

        public int CheckBorderCollision(int bx)
        {
            int si = bx, ax = 0, dx = 0;
            bx = 0;
            do
            {
                if (spriteEnabledFlag[bx] && bx != si)
                {
                    if (IsInBorderCollision(bx, si))
                        ax |= 1 << dx;
                    spriteX[bx] += 320;
                    spriteY[bx] -= 2;
                    if (IsInBorderCollision(bx, si))
                        ax |= 1 << dx;
                    spriteX[bx] -= 640;
                    spriteY[bx] += 4;
                    if (IsInBorderCollision(bx, si))
                        ax |= 1 << dx;
                    spriteX[bx] += 320;
                    spriteY[bx] -= 2;
                }
                bx++;
                dx++;
            } while (dx != 16);
            return ax;
        }

        public void ClearRedrawFlags()
        {
            ClearRecursionFlags();
            for (int i = 0; i < 17; i++)
                spriteRedrawFlag[i] = false;
        }

        public void ClearRecursionFlags()
        {
            for (int i = 0; i < 17; i++)
                spriteRecursionFlag[i] = false;
        }

        public bool CheckCollision(int bx, int si)
        {
            if (spriteX[bx] >= spriteX[si])
            {
                if (spriteX[bx] > spriteWidth[si] * 4 + spriteX[si] - 1)
                    return false;
            }
            else
              if (spriteX[si] > spriteWidth[bx] * 4 + spriteX[bx] - 1)
                return false;

            if (spriteY[bx] >= spriteY[si])
            {
                if (spriteY[bx] <= spriteHeight[si] + spriteY[si] - 1)
                    return true;
                return false;
            }

            if (spriteY[si] <= spriteHeight[bx] + spriteY[bx] - 1)
                return true;

            return false;
        }

        public void CreateSprites(int n, int ch, short[] mov, int wid, int hei, int bwid, int bhei)
        {
            newSpriteChar[n & 15] = spriteChar[n & 15] = ch;
            spriteBuffer[n & 15] = mov;
            newSpriteWidth[n & 15] = spriteWidth[n & 15] = wid;
            newSpriteHeight[n & 15] = spriteHeight[n & 15] = hei;
            newSpriteBorderWidth[n & 15] = spriteBorderWidth[n & 15] = bwid;
            newSpriteBorderHeight[n & 15] = spriteBorderHeight[n & 15] = bhei;
            spriteEnabledFlag[n & 15] = false;
        }

        public void DrawMiscSprites(int x, int y, int ch, int wid, int hei)
        {
            spriteX[16] = x & -4;
            spriteY[16] = y;
            spriteChar[16] = ch;
            spriteWidth[16] = wid;
            spriteHeight[16] = hei;
            game.video.PutImage(spriteX[16], spriteY[16], spriteChar[16], spriteWidth[16], spriteHeight[16]);
        }

        public int DrawSprite(int n, int x, int y)
        {
            int bx, t1, t2, t3, t4;
            bx = n & 15;
            x &= -4;
            ClearRedrawFlags();
            SetRedrawFlags(bx);
            t1 = spriteX[bx];
            t2 = spriteY[bx];
            t3 = spriteWidth[bx];
            t4 = spriteHeight[bx];
            spriteX[bx] = x;
            spriteY[bx] = y;
            spriteWidth[bx] = newSpriteWidth[bx];
            spriteHeight[bx] = newSpriteHeight[bx];
            ClearRecursionFlags();
            SetRedrawFlags(bx);
            spriteHeight[bx] = t4;
            spriteWidth[bx] = t3;
            spriteY[bx] = t2;
            spriteX[bx] = t1;
            spriteRedrawFlag[bx] = true;
            RedrawBackgroundImages();
            spriteX[bx] = x;
            spriteY[bx] = y;
            spriteChar[bx] = newSpriteChar[bx];
            spriteWidth[bx] = newSpriteWidth[bx];
            spriteHeight[bx] = newSpriteHeight[bx];
            spriteBorderWidth[bx] = newSpriteBorderWidth[bx];
            spriteBorderHeight[bx] = newSpriteBorderHeight[bx];
            game.video.GetImage(spriteX[bx], spriteY[bx], spriteBuffer[bx], spriteWidth[bx], spriteHeight[bx]);
            DrawActualSprites();
            return CheckBorderCollision(bx);
        }

        public void EraseSprite(int n)
        {
            int bx = n & 15;
            game.video.PutImage(spriteX[bx], spriteY[bx], spriteBuffer[bx], spriteWidth[bx], spriteHeight[bx], true);
            spriteEnabledFlag[bx] = false;
            ClearRedrawFlags();
            SetRedrawFlags(bx);
            DrawActualSprites();
        }

        public void GetSpriteImage()
        {
            for (int i = 0; i < 16; i++)
                if (spriteRedrawFlag[i])
                    game.video.GetImage(spriteX[i], spriteY[i], spriteBuffer[i], spriteWidth[i], spriteHeight[i]);

            DrawActualSprites();
        }

        public void InitMiscSprite(int x, int y, int wid, int hei)
        {
            spriteX[16] = x;
            spriteY[16] = y;
            spriteWidth[16] = wid;
            spriteHeight[16] = hei;
            ClearRedrawFlags();
            SetRedrawFlags(16);
            RedrawBackgroundImages();
        }

        public void InitSprite(int n, int ch, int wid, int hei, int bwid, int bhei)
        {
            newSpriteChar[n & 15] = ch;
            newSpriteWidth[n & 15] = wid;
            newSpriteHeight[n & 15] = hei;
            newSpriteBorderWidth[n & 15] = bwid;
            newSpriteBorderHeight[n & 15] = bhei;
        }

        public int MoveDrawSprite(int n, int x, int y)
        {
            int bx = n & 15;
            spriteX[bx] = x & -4;
            spriteY[bx] = y;
            spriteChar[bx] = newSpriteChar[bx];
            spriteWidth[bx] = newSpriteWidth[bx];
            spriteHeight[bx] = newSpriteHeight[bx];
            spriteBorderWidth[bx] = newSpriteBorderWidth[bx];
            spriteBorderHeight[bx] = newSpriteBorderHeight[bx];
            ClearRedrawFlags();
            SetRedrawFlags(bx);
            RedrawBackgroundImages();
            game.video.GetImage(spriteX[bx], spriteY[bx], spriteBuffer[bx], spriteWidth[bx], spriteHeight[bx]);
            spriteEnabledFlag[bx] = true;
            spriteRedrawFlag[bx] = true;
            DrawActualSprites();
            return CheckBorderCollision(bx);
        }

        public void DrawActualSprites()
        {
            for (int i = 0; i < 16; i++)
            {
                int j = spriteOrder[i];
                if (spriteRedrawFlag[j])
                    game.video.PutImage(spriteX[j], spriteY[j], spriteChar[j], spriteWidth[j], spriteHeight[j]);
            }
        }

        public void RedrawBackgroundImages()
        {
            for (int i = 0; i < 16; i++)
                if (spriteRedrawFlag[i])
                    game.video.PutImage(spriteX[i], spriteY[i], spriteBuffer[i], spriteWidth[i], spriteHeight[i]);
        }

        public void SetRedrawFlags(int n)
        {
            if (!spriteRecursionFlag[n])
            {
                spriteRecursionFlag[n] = true;
                for (int i = 0; i < 16; i++)
                    if (spriteEnabledFlag[i] && i != n)
                    {
                        if (CheckCollision(i, n))
                        {
                            spriteRedrawFlag[i] = true;
                            SetRedrawFlags(i);
                        }
                    }
            }
        }

        public void SetRetr(bool f)
        {
            retrflag = f;
        }

        public void SetSpriteOrder(int[] newsprorder)
        {
            if (newsprorder == null)
                spriteOrder = defaultSpriteOrder;
            else
                spriteOrder = newsprorder;
        }
    }
}