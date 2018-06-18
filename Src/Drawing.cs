namespace Digger
{
    public class Drawing
    {
        private readonly Game game;

        public int[] field = new int[150];
        public int[] field1 = new int[150];
        public int[] field2 = new int[150];

        short[] diggerbuf = new short[480],
        bagbuf1 = new short[480],
        bagbuf2 = new short[480],
        bagbuf3 = new short[480],
        bagbuf4 = new short[480],
        bagbuf5 = new short[480],
        bagbuf6 = new short[480],
        bagbuf7 = new short[480],
        monbuf1 = new short[480],
        monbuf2 = new short[480],
        monbuf3 = new short[480],
        monbuf4 = new short[480],
        monbuf5 = new short[480],
        monbuf6 = new short[480],
        bonusbuf = new short[480],
        firebuf = new short[128];

        int[] bitmasks = { 0xfffe, 0xfffd, 0xfffb, 0xfff7, 0xffef, 0xffdf, 0xffbf, 0xff7f, 0xfeff, 0xfdff, 0xfbff, 0xf7ff };    // [12]

        int[] monspr = { 0, 0, 0, 0, 0, 0 };    // [6]
        int[] monspd = { 0, 0, 0, 0, 0, 0 };    // [6]

        int digspr = 0, digspd = 0, firespr = 0, fireheight = 8;

        public Drawing(Game game)
        {
            this.game = game;
        }

        public void CreateDiggerBonusFireballSprites()
        {
            digspd = 1;
            digspr = 0;
            firespr = 0;
            game.sprite.CreateSprites(0, 0, diggerbuf, 4, 15, 0, 0);
            game.sprite.CreateSprites(14, 81, bonusbuf, 4, 15, 0, 0);
            game.sprite.CreateSprites(15, 82, firebuf, 2, fireheight, 0, 0);
        }

        public void CreateMonsterBagSprites()
        {
            int i;
            game.sprite.CreateSprites(1, 62, bagbuf1, 4, 15, 0, 0);
            game.sprite.CreateSprites(2, 62, bagbuf2, 4, 15, 0, 0);
            game.sprite.CreateSprites(3, 62, bagbuf3, 4, 15, 0, 0);
            game.sprite.CreateSprites(4, 62, bagbuf4, 4, 15, 0, 0);
            game.sprite.CreateSprites(5, 62, bagbuf5, 4, 15, 0, 0);
            game.sprite.CreateSprites(6, 62, bagbuf6, 4, 15, 0, 0);
            game.sprite.CreateSprites(7, 62, bagbuf7, 4, 15, 0, 0);
            game.sprite.CreateSprites(8, 71, monbuf1, 4, 15, 0, 0);
            game.sprite.CreateSprites(9, 71, monbuf2, 4, 15, 0, 0);
            game.sprite.CreateSprites(10, 71, monbuf3, 4, 15, 0, 0);
            game.sprite.CreateSprites(11, 71, monbuf4, 4, 15, 0, 0);
            game.sprite.CreateSprites(12, 71, monbuf5, 4, 15, 0, 0);
            game.sprite.CreateSprites(13, 71, monbuf6, 4, 15, 0, 0);
            CreateDiggerBonusFireballSprites();
            for (i = 0; i < 6; i++)
            {
                monspr[i] = 0;
                monspd[i] = 1;
            }
        }

        public void DrawBackground(int l)
        {
            int x, y;
            for (y = 14; y < 200; y += 4)
                for (x = 0; x < 320; x += 20)
                    game.sprite.DrawMiscSprites(x, y, 93 + l, 5, 4);
        }

        public void DrawBonus(int x, int y)
        {
            game.sprite.InitSprite(14, 81, 4, 15, 0, 0);
            game.sprite.MoveDrawSprite(14, x, y);
        }

        public void DrawBottomBlob(int x, int y)
        {
            game.sprite.InitMiscSprite(x - 4, y + 15, 6, 6);
            game.sprite.DrawMiscSprites(x - 4, y + 15, 105, 6, 6);
            game.sprite.GetSpriteImage();
        }

        public int DrawDigger(int t, int x, int y, bool f)
        {
            digspr += digspd;
            if (digspr == 2 || digspr == 0)
                digspd = -digspd;
            if (digspr > 2)
                digspr = 2;
            if (digspr < 0)
                digspr = 0;
            if (t >= 0 && t <= 6 && !((t & 1) != 0))
            {
                game.sprite.InitSprite(0, (t + (f ? 0 : 1)) * 3 + digspr + 1, 4, 15, 0, 0);
                return game.sprite.DrawSprite(0, x, y);
            }
            if (t >= 10 && t <= 15)
            {
                game.sprite.InitSprite(0, 40 - t, 4, 15, 0, 0);
                return game.sprite.DrawSprite(0, x, y);
            }
            return 0;
        }

        public void DrawEmerald(int x, int y)
        {
            game.sprite.InitMiscSprite(x, y, 4, 10);
            game.sprite.DrawMiscSprites(x, y, 108, 4, 10);
            game.sprite.GetSpriteImage();
        }

        public void DrawField()
        {
            int x, y, xp, yp;
            for (x = 0; x < 15; x++)
                for (y = 0; y < 10; y++)
                    if ((field[y * 15 + x] & 0x2000) == 0)
                    {
                        xp = x * 20 + 12;
                        yp = y * 18 + 18;
                        if ((field[y * 15 + x] & 0xfc0) != 0xfc0)
                        {
                            field[y * 15 + x] &= 0xd03f;
                            DrawBottomBlob(xp, yp - 15);
                            DrawBottomBlob(xp, yp - 12);
                            DrawBottomBlob(xp, yp - 9);
                            DrawBottomBlob(xp, yp - 6);
                            DrawBottomBlob(xp, yp - 3);
                            DrawTopBlob(xp, yp + 3);
                        }
                        if ((field[y * 15 + x] & 0x1f) != 0x1f)
                        {
                            field[y * 15 + x] &= 0xdfe0;
                            DrawRightBlob(xp - 16, yp);
                            DrawRightBlob(xp - 12, yp);
                            DrawRightBlob(xp - 8, yp);
                            DrawRightBlob(xp - 4, yp);
                            DrawLeftBlob(xp + 4, yp);
                        }
                        if (x < 14)
                            if ((field[y * 15 + x + 1] & 0xfdf) != 0xfdf)
                                DrawRightBlob(xp, yp);
                        if (y < 9)
                            if ((field[(y + 1) * 15 + x] & 0xfdf) != 0xfdf)
                                DrawBottomBlob(xp, yp);
                    }
        }

        public int DrawFire(int x, int y, int t)
        {
            if (t == 0)
            {
                firespr++;
                if (firespr > 2)
                    firespr = 0;
                game.sprite.InitSprite(15, 82 + firespr, 2, fireheight, 0, 0);
            }
            else
                game.sprite.InitSprite(15, 84 + t, 2, fireheight, 0, 0);
            return game.sprite.DrawSprite(15, x, y);
        }

        public void DrawFurryBlob(int x, int y)
        {
            game.sprite.InitMiscSprite(x - 4, y + 15, 6, 8);
            game.sprite.DrawMiscSprites(x - 4, y + 15, 107, 6, 8);
            game.sprite.GetSpriteImage();
        }

        public int DrawGold(int n, int t, int x, int y)
        {
            game.sprite.InitSprite(n, t + 62, 4, 15, 0, 0);
            return game.sprite.DrawSprite(n, x, y);
        }

        public void DrawLeftBlob(int x, int y)
        {
            game.sprite.InitMiscSprite(x - 8, y - 1, 2, 18);
            game.sprite.DrawMiscSprites(x - 8, y - 1, 104, 2, 18);
            game.sprite.GetSpriteImage();
        }

        public void DrawLife(int t, int x, int y)
        {
            game.sprite.DrawMiscSprites(x, y, t + 110, 4, 12);
        }

        public void DrawLives()
        {
            int l, n;
            n = game.GetLives(1) - 1;
            for (l = 1; l < 5; l++)
            {
                DrawLife(n > 0 ? 0 : 2, l * 20 + 60, 0);
                n--;
            }
            if (game.playerCount == 2)
            {
                n = game.GetLives(2) - 1;
                for (l = 1; l < 5; l++)
                {
                    DrawLife(n > 0 ? 1 : 2, 244 - l * 20, 0);
                    n--;
                }
            }
        }

        public int DrawMonster(int n, bool nobf, int dir, int x, int y)
        {
            monspr[n] += monspd[n];
            if (monspr[n] == 2 || monspr[n] == 0)
                monspd[n] = -monspd[n];
            if (monspr[n] > 2)
                monspr[n] = 2;
            if (monspr[n] < 0)
                monspr[n] = 0;
            if (nobf)
                game.sprite.InitSprite(n + 8, monspr[n] + 69, 4, 15, 0, 0);
            else
                switch (dir)
                {
                    case 0:
                        game.sprite.InitSprite(n + 8, monspr[n] + 73, 4, 15, 0, 0);
                        break;
                    case 4:
                        game.sprite.InitSprite(n + 8, monspr[n] + 77, 4, 15, 0, 0);
                        break;
                }
            return game.sprite.DrawSprite(n + 8, x, y);
        }

        public int DrawMonsterDie(int n, bool nobf, int dir, int x, int y)
        {
            if (nobf)
                game.sprite.InitSprite(n + 8, 72, 4, 15, 0, 0);
            else
                switch (dir)
                {
                    case 0:
                        game.sprite.InitSprite(n + 8, 76, 4, 15, 0, 0);
                        break;
                    case 4:
                        game.sprite.InitSprite(n + 8, 80, 4, 14, 0, 0);
                        break;
                }
            return game.sprite.DrawSprite(n + 8, x, y);
        }

        public void DrawRightBlob(int x, int y)
        {
            game.sprite.InitMiscSprite(x + 16, y - 1, 2, 18);
            game.sprite.DrawMiscSprites(x + 16, y - 1, 102, 2, 18);
            game.sprite.GetSpriteImage();
        }

        public void DrawSquareBlob(int x, int y)
        {
            game.sprite.InitMiscSprite(x - 4, y + 17, 6, 6);
            game.sprite.DrawMiscSprites(x - 4, y + 17, 106, 6, 6);
            game.sprite.GetSpriteImage();
        }

        public void DrawStatics()
        {
            int x, y;
            for (x = 0; x < 15; x++)
                for (y = 0; y < 10; y++)
                    if (game.GetCurrentPlayer() == 0)
                        field[y * 15 + x] = field1[y * 15 + x];
                    else
                        field[y * 15 + x] = field2[y * 15 + x];
            game.sprite.SetRetr(true);
            game.video.SetIntensity(0);
            DrawBackground(game.level.LevelPlan());
            DrawField();
            game.video.UpdateImage();
        }

        public void DrawTopBlob(int x, int y)
        {
            game.sprite.InitMiscSprite(x - 4, y - 6, 6, 6);
            game.sprite.DrawMiscSprites(x - 4, y - 6, 103, 6, 6);
            game.sprite.GetSpriteImage();
        }

        public void EatField(int x, int y, int dir)
        {
            int h = (x - 12) / 20, xr = ((x - 12) % 20) / 4, v = (y - 18) / 18, yr = ((y - 18) % 18) / 3;
            game.IncrementPenalty();
            switch (dir)
            {
                case 0:
                    h++;
                    field[v * 15 + h] &= bitmasks[xr];
                    if ((field[v * 15 + h] & 0x1f) != 0)
                        break;
                    field[v * 15 + h] &= 0xdfff;
                    break;
                case 4:
                    xr--;
                    if (xr < 0)
                    {
                        xr += 5;
                        h--;
                    }
                    field[v * 15 + h] &= bitmasks[xr];
                    if ((field[v * 15 + h] & 0x1f) != 0)
                        break;
                    field[v * 15 + h] &= 0xdfff;
                    break;
                case 2:
                    yr--;
                    if (yr < 0)
                    {
                        yr += 6;
                        v--;
                    }
                    field[v * 15 + h] &= bitmasks[6 + yr];
                    if ((field[v * 15 + h] & 0xfc0) != 0)
                        break;
                    field[v * 15 + h] &= 0xdfff;
                    break;
                case 6:
                    v++;
                    field[v * 15 + h] &= bitmasks[6 + yr];
                    if ((field[v * 15 + h] & 0xfc0) != 0)
                        break;
                    field[v * 15 + h] &= 0xdfff;
                    break;
            }
        }

        public void EraseEmerald(int x, int y)
        {
            game.sprite.InitMiscSprite(x, y, 4, 10);
            game.sprite.DrawMiscSprites(x, y, 109, 4, 10);
            game.sprite.GetSpriteImage();
        }

        public void InitDiggerSpriteBuffer()
        {
            digspd = 1;
            digspr = 0;
            firespr = 0;
            game.sprite.InitSprite(0, 0, 4, 15, 0, 0);
            game.sprite.InitSprite(14, 81, 4, 15, 0, 0);
            game.sprite.InitSprite(15, 82, 2, fireheight, 0, 0);
        }

        public void InitMonsterSpriteBuffer()
        {
            game.sprite.InitSprite(1, 62, 4, 15, 0, 0);
            game.sprite.InitSprite(2, 62, 4, 15, 0, 0);
            game.sprite.InitSprite(3, 62, 4, 15, 0, 0);
            game.sprite.InitSprite(4, 62, 4, 15, 0, 0);
            game.sprite.InitSprite(5, 62, 4, 15, 0, 0);
            game.sprite.InitSprite(6, 62, 4, 15, 0, 0);
            game.sprite.InitSprite(7, 62, 4, 15, 0, 0);
            game.sprite.InitSprite(8, 71, 4, 15, 0, 0);
            game.sprite.InitSprite(9, 71, 4, 15, 0, 0);
            game.sprite.InitSprite(10, 71, 4, 15, 0, 0);
            game.sprite.InitSprite(11, 71, 4, 15, 0, 0);
            game.sprite.InitSprite(12, 71, 4, 15, 0, 0);
            game.sprite.InitSprite(13, 71, 4, 15, 0, 0);
            InitDiggerSpriteBuffer();
        }

        public void MakeField()
        {
            int c, x, y;
            for (x = 0; x < 15; x++)
                for (y = 0; y < 10; y++)
                {
                    field[y * 15 + x] = -1;
                    c = game.level.GetChar(x, y, game.level.LevelPlan());
                    if (c == 'S' || c == 'V')
                        field[y * 15 + x] &= 0xd03f;
                    if (c == 'S' || c == 'H')
                        field[y * 15 + x] &= 0xdfe0;
                    if (game.GetCurrentPlayer() == 0)
                        field1[y * 15 + x] = field[y * 15 + x];
                    else
                        field2[y * 15 + x] = field[y * 15 + x];
                }
        }

        public void TextOut(string p, int x, int y, int c)
        {
            TextOut(p, x, y, c, false);
        }

        public void TextOut(string p, int x, int y, int c, bool b)
        {
            int i, rx = x;
            for (i = 0; i < p.Length; i++)
            {
                game.video.Write(x, y, p[i], c);
                x += 12;
            }
            if (b)
                game.video.UpdateImage();
        }

        public void SaveField()
        {
            int x, y;
            for (x = 0; x < 15; x++)
                for (y = 0; y < 10; y++)
                    if (game.GetCurrentPlayer() == 0)
                        field1[y * 15 + x] = field[y * 15 + x];
                    else
                        field2[y * 15 + x] = field[y * 15 + x];
        }
    }
}