namespace Digger
{
    public class Digger
    {
        public int x, y, h, v, rx, ry, mdir, dir, 
            time, rechargetime, firex, firey, firedir, expsn,
            deathstage, deathbag, deathani, deathtime, startbonustimeleft,
            bonustimeleft, eatmsc, emocttime;

        int emmask = 0;

        private readonly byte[] emfield = new byte[150];

        public bool digonscr = false, notfiring = false, bonusvisible = false, bonusmode = false, diggervisible = false;

        private readonly int[] embox = { 8, 12, 12, 9, 16, 12, 6, 9 };  // [8]
        private readonly int[] deatharc = { 3, 5, 6, 6, 5, 3, 0 };      // [7]

        private Game game;

        public Digger(Game game)
        {
            this.game = game;
        }

        public bool IsDiggerUnderBag(int h, int v)
        {
            if (mdir == 2 || mdir == 6)
                if ((x - 12) / 20 == h)
                    if ((y - 18) / 18 == v || (y - 18) / 18 + 1 == v)
                        return true;
            return false;
        }

        public int EmeraldCount()
        {
            int x, y, n = 0;
            for (x = 0; x < 15; x++)
                for (y = 0; y < 10; y++)
                    if ((emfield[y * 15 + x] & emmask) != 0)
                        n++;
            return n;
        }

        public void CreateBonus()
        {
            bonusvisible = true;
            game.Drawing.DrawBonus(292, 18);
        }
        
        public void DiggerDie()
        {
            int clbits;
            switch (deathstage)
            {
                case 1:
                    if (game.Bags.BagY(deathbag) + 6 > y)
                        y = game.Bags.BagY(deathbag) + 6;
                    game.Drawing.DrawDigger(15, x, y, false);
                    game.IncrementPenalty();
                    if (game.Bags.GetBagDir(deathbag) + 1 == 0)
                    {
                        game.Sound.soundddie();
                        deathtime = 5;
                        deathstage = 2;
                        deathani = 0;
                        y -= 6;
                    }
                    break;
                case 2:
                    if (deathtime != 0)
                    {
                        deathtime--;
                        break;
                    }
                    if (deathani == 0)
                        game.Sound.music(2);
                    clbits = game.Drawing.DrawDigger(14 - deathani, x, y, false);
                    game.IncrementPenalty();
                    if (deathani == 0 && ((clbits & 0x3f00) != 0))
                        game.Monster.KillMonsters(clbits);
                    if (deathani < 4)
                    {
                        deathani++;
                        deathtime = 2;
                    }
                    else
                    {
                        deathstage = 4;
                        if (game.Sound.musicflag)
                            deathtime = 60;
                        else
                            deathtime = 10;
                    }
                    break;
                case 3:
                    deathstage = 5;
                    deathani = 0;
                    deathtime = 0;
                    break;
                case 5:
                    if (deathani >= 0 && deathani <= 6)
                    {
                        game.Drawing.DrawDigger(15, x, y - deatharc[deathani], false);
                        if (deathani == 6)
                            game.Sound.musicoff();
                        game.IncrementPenalty();
                        deathani++;
                        if (deathani == 1)
                            game.Sound.soundddie();
                        if (deathani == 7)
                        {
                            deathtime = 5;
                            deathani = 0;
                            deathstage = 2;
                        }
                    }
                    break;
                case 4:
                    if (deathtime != 0)
                        deathtime--;
                    else
                        game.SetDead(true);
                    break;
            }
        }

        public void DoDigger()
        {
            if (expsn != 0)
                DrawExplosion();
            else
                UpdateFire();
            if (diggervisible)
                if (digonscr)
                    if (time != 0)
                    {
                        game.Drawing.DrawDigger(mdir, x, y, notfiring && rechargetime == 0);
                        game.IncrementPenalty();
                        time--;
                    }
                    else
                        UpdateDigger();
                else
                    DiggerDie();
            if (bonusmode && digonscr)
            {
                if (bonustimeleft != 0)
                {
                    bonustimeleft--;
                    if (startbonustimeleft != 0 || bonustimeleft < 20)
                    {
                        startbonustimeleft--;
                        if ((bonustimeleft & 1) != 0)
                        {
                            game.Video.SetIntensity(0);
                            game.Sound.soundbonus();
                        }
                        else
                        {
                            game.Video.SetIntensity(1);
                            game.Sound.soundbonus();
                        }
                        if (startbonustimeleft == 0)
                        {
                            game.Sound.music(0);
                            game.Sound.soundbonusoff();
                            game.Video.SetIntensity(1);
                        }
                    }
                }
                else
                {
                    EndBonusMode();
                    game.Sound.soundbonusoff();
                    game.Sound.music(1);
                }
            }
            if (bonusmode && !digonscr)
            {
                EndBonusMode();
                game.Sound.soundbonusoff();
                game.Sound.music(1);
            }
            if (emocttime > 0)
                emocttime--;
        }

        public void DrawEmeralds()
        {
            int x, y;
            emmask = 1 << game.GetCurrentPlayer();
            for (x = 0; x < 15; x++)
                for (y = 0; y < 10; y++)
                    if ((emfield[y * 15 + x] & emmask) != 0)
                        game.Drawing.DrawEmerald(x * 20 + 12, y * 18 + 21);
        }

        public void DrawExplosion()
        {
            switch (expsn)
            {
                case 1:
                    game.Sound.soundexplode();
                    goto case 3;
                case 2:
                case 3:
                    game.Drawing.DrawFire(firex, firey, expsn);
                    game.IncrementPenalty();
                    expsn++;
                    break;
                default:
                    KillFire();
                    expsn = 0;
                    break;
            }
        }

        public void EndBonusMode()
        {
            bonusmode = false;
            game.Video.SetIntensity(0);
        }

        public void EraseBonus()
        {
            if (bonusvisible)
            {
                bonusvisible = false;
                game.Sprite.EraseSprite(14);
            }
        }

        public void EraseDigger()
        {
            game.Sprite.EraseSprite(0);
            diggervisible = false;
        }

        public bool IsEmeraldHit(int x, int y, int rx, int ry, int dir)
        {
            bool hit = false;
            int r;
            if (dir < 0 || dir > 6 || ((dir & 1) != 0))
                return hit;
            if (dir == 0 && rx != 0)
                x++;
            if (dir == 6 && ry != 0)
                y++;
            if (dir == 0 || dir == 4)
                r = rx;
            else
                r = ry;
            if ((emfield[y * 15 + x] & emmask) != 0)
            {
                if (r == embox[dir])
                {
                    game.Drawing.DrawEmerald(x * 20 + 12, y * 18 + 21);
                    game.IncrementPenalty();
                }
                if (r == embox[dir + 1])
                {
                    game.Drawing.EraseEmerald(x * 20 + 12, y * 18 + 21);
                    game.IncrementPenalty();
                    hit = true;
                    emfield[y * 15 + x] &= (byte)~emmask;
                }
            }
            return hit;
        }

        public void InitBonusMode()
        {
            bonusmode = true;
            EraseBonus();
            game.Video.SetIntensity(1);
            bonustimeleft = 250 - Level.LevelOf10(game.Level) * 20;
            startbonustimeleft = 20;
            eatmsc = 1;
        }

        public void InitDigger()
        {
            v = 9;
            mdir = 4;
            h = 7;
            x = h * 20 + 12;
            dir = 0;
            rx = 0;
            ry = 0;
            time = 0;
            digonscr = true;
            deathstage = 1;
            diggervisible = true;
            y = v * 18 + 18;
            game.Sprite.MoveDrawSprite(0, x, y);
            notfiring = true;
            emocttime = 0;
            bonusvisible = bonusmode = false;
            game.Input.firepressed = false;
            expsn = 0;
            rechargetime = 0;
        }

        public void KillDigger(int stage, int bag)
        {
            if (deathstage < 2 || deathstage > 4)
            {
                digonscr = false;
                deathstage = stage;
                deathbag = bag;
            }
        }

        public void KillEmerald(int x, int y)
        {
            if ((emfield[y * 15 + x + 15] & emmask) != 0)
            {
                emfield[y * 15 + x + 15] &= (byte)~emmask;
                game.Drawing.EraseEmerald(x * 20 + 12, (y + 1) * 18 + 21);
            }
        }

        public void KillFire()
        {
            if (!notfiring)
            {
                notfiring = true;
                game.Sprite.EraseSprite(15);
                game.Sound.soundfireoff();
            }
        }

        public void MakeEmeraldField()
        {
            int x, y;
            emmask = 1 << game.GetCurrentPlayer();
            for (x = 0; x < 15; x++)
                for (y = 0; y < 10; y++)
                    if (Level.GetChar(x, y, Level.LevelPlan(game.Level)) == 'C')
                        emfield[y * 15 + x] |= (byte)emmask;
                    else
                        emfield[y * 15 + x] &= (byte)~emmask;
        }

        private void UpdateDigger()
        {
            int dir, ddir, clbits, diggerox, diggeroy, nmon;
            bool push = true;
            game.Input.ReadDirection();
            dir = game.Input.GetDirection(0);
            if (dir == 0 || dir == 2 || dir == 4 || dir == 6)
                ddir = dir;
            else
                ddir = -1;
            if (rx == 0 && (ddir == 2 || ddir == 6))
                this.dir = mdir = ddir;
            if (ry == 0 && (ddir == 4 || ddir == 0))
                this.dir = mdir = ddir;
            if (dir == -1)
                mdir = -1;
            else
                mdir = this.dir;
            if ((x == 292 && mdir == 0) || (x == 12 && mdir == 4) ||
            (y == 180 && mdir == 6) || (y == 18 && mdir == 2))
                mdir = -1;
            diggerox = x;
            diggeroy = y;
            if (mdir != -1)
                game.Drawing.EatField(diggerox, diggeroy, mdir);
            switch (mdir)
            {
                case 0:
                    game.Drawing.DrawRightBlob(x, y);
                    x += 4;
                    break;
                case 4:
                    game.Drawing.DrawLeftBlob(x, y);
                    x -= 4;
                    break;
                case 2:
                    game.Drawing.DrawTopBlob(x, y);
                    y -= 3;
                    break;
                case 6:
                    game.Drawing.DrawBottomBlob(x, y);
                    y += 3;
                    break;
            }

            if (IsEmeraldHit((x - 12) / 20, (y - 18) / 18, (x - 12) % 20, (y - 18) % 18, mdir))
            {
                game.Scores.ScoreEmerald();
                game.Sound.soundem();
                game.Sound.soundemerald(emocttime);
                emocttime = 9;
            }
            clbits = game.Drawing.DrawDigger(this.dir, x, y, notfiring && rechargetime == 0);
            game.IncrementPenalty();
            if ((game.Bags.BagBits() & clbits) != 0)
            {
                if (mdir == 0 || mdir == 4)
                {
                    push = game.Bags.PushBags(mdir, clbits);
                    time++;
                }
                else
                if (!game.Bags.PushUpdatedBags(clbits))
                    push = false;
                if (!push)
                { /* Strange, push not completely defined */
                    x = diggerox;
                    y = diggeroy;
                    game.Drawing.DrawDigger(mdir, x, y, notfiring && rechargetime == 0);
                    game.IncrementPenalty();
                    this.dir = game.ReverseDir(mdir);
                }
            }
            if (((clbits & 0x3f00) != 0) && bonusmode)
                for (nmon = game.Monster.KillMonsters(clbits); nmon != 0; nmon--)
                {
                    game.Sound.soundeatm();
                    game.Scores.ScoreEatMonster();
                }
            if ((clbits & 0x4000) != 0)
            {
                game.Scores.ScoreBonus();
                InitBonusMode();
            }
            h = (x - 12) / 20;
            rx = (x - 12) % 20;
            v = (y - 18) / 18;
            ry = (y - 18) % 18;
        }

        private void UpdateFire()
        {
            int clbits, b, mon, pix = 0;
            if (notfiring)
            {
                if (rechargetime != 0)
                    rechargetime--;
                else
                if (game.Input.firepflag)
                    if (digonscr)
                    {
                        rechargetime = Level.LevelOf10(game.Level) * 3 + 60;
                        notfiring = false;
                        switch (dir)
                        {
                            case 0:
                                firex = x + 8;
                                firey = y + 4;
                                break;
                            case 4:
                                firex = x;
                                firey = y + 4;
                                break;
                            case 2:
                                firex = x + 4;
                                firey = y;
                                break;
                            case 6:
                                firex = x + 4;
                                firey = y + 8;
                                break;
                        }
                        firedir = dir;
                        game.Sprite.MoveDrawSprite(15, firex, firey);
                        game.Sound.soundfire();
                    }
            }
            else
            {
                switch (firedir)
                {
                    case 0:
                        firex += 8;
                        pix = game.Video.GetPixel(firex, firey + 4) | game.Video.GetPixel(firex + 4, firey + 4);
                        break;
                    case 4:
                        firex -= 8;
                        pix = game.Video.GetPixel(firex, firey + 4) | game.Video.GetPixel(firex + 4, firey + 4);
                        break;
                    case 2:
                        firey -= 7;
                        pix = (game.Video.GetPixel(firex + 4, firey) | game.Video.GetPixel(firex + 4, firey + 1) |
                                game.Video.GetPixel(firex + 4, firey + 2) | game.Video.GetPixel(firex + 4, firey + 3) |
                                game.Video.GetPixel(firex + 4, firey + 4) | game.Video.GetPixel(firex + 4, firey + 5) |
                                game.Video.GetPixel(firex + 4, firey + 6)) & 0xc0;
                        break;
                    case 6:
                        firey += 7;
                        pix = (game.Video.GetPixel(firex, firey) | game.Video.GetPixel(firex, firey + 1) |
                                game.Video.GetPixel(firex, firey + 2) | game.Video.GetPixel(firex, firey + 3) |
                                game.Video.GetPixel(firex, firey + 4) | game.Video.GetPixel(firex, firey + 5) |
                                game.Video.GetPixel(firex, firey + 6)) & 3;
                        break;
                }
                clbits = game.Drawing.DrawFire(firex, firey, 0);
                game.IncrementPenalty();
                if ((clbits & 0x3f00) != 0)
                    for (mon = 0, b = 256; mon < 6; mon++, b <<= 1)
                        if ((clbits & b) != 0)
                        {
                            game.Monster.KillMonster(mon);
                            game.Scores.ScoreKill();
                            expsn = 1;
                        }
                if ((clbits & 0x40fe) != 0)
                    expsn = 1;
                switch (firedir)
                {
                    case 0:
                        if (firex > 296)
                            expsn = 1;
                        else if (pix != 0 && clbits == 0)
                        {
                            expsn = 1;
                            firex -= 8;
                            game.Drawing.DrawFire(firex, firey, 0);
                        }
                        break;
                    case 4:
                        if (firex < 16)
                            expsn = 1;
                        else if (pix != 0 && clbits == 0)
                        {
                            expsn = 1;
                            firex += 8;
                            game.Drawing.DrawFire(firex, firey, 0);
                        }
                        break;
                    case 2:
                        if (firey < 15)
                            expsn = 1;
                        else if (pix != 0 && clbits == 0)
                        {
                            expsn = 1;
                            firey += 7;
                            game.Drawing.DrawFire(firex, firey, 0);
                        }
                        break;
                    case 6:
                        if (firey > 183)
                            expsn = 1;
                        else if (pix != 0 && clbits == 0)
                        {
                            expsn = 1;
                            firey -= 7;
                            game.Drawing.DrawFire(firex, firey, 0);
                        }
                        break;
                }
            }
        }
    }
}