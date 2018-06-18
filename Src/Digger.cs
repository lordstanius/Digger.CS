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
            game.drawing.DrawBonus(292, 18);
        }
        
        public void DiggerDie()
        {
            int clbits;
            switch (deathstage)
            {
                case 1:
                    if (game.bags.BagY(deathbag) + 6 > y)
                        y = game.bags.BagY(deathbag) + 6;
                    game.drawing.DrawDigger(15, x, y, false);
                    game.IncrementPenalty();
                    if (game.bags.GetBagDir(deathbag) + 1 == 0)
                    {
                        game.sound.soundddie();
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
                        game.sound.music(2);
                    clbits = game.drawing.DrawDigger(14 - deathani, x, y, false);
                    game.IncrementPenalty();
                    if (deathani == 0 && ((clbits & 0x3f00) != 0))
                        game.monster.KillMonsters(clbits);
                    if (deathani < 4)
                    {
                        deathani++;
                        deathtime = 2;
                    }
                    else
                    {
                        deathstage = 4;
                        if (game.sound.musicflag)
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
                        game.drawing.DrawDigger(15, x, y - deatharc[deathani], false);
                        if (deathani == 6)
                            game.sound.musicoff();
                        game.IncrementPenalty();
                        deathani++;
                        if (deathani == 1)
                            game.sound.soundddie();
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
                        game.drawing.DrawDigger(mdir, x, y, notfiring && rechargetime == 0);
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
                            game.video.SetIntensity(0);
                            game.sound.soundbonus();
                        }
                        else
                        {
                            game.video.SetIntensity(1);
                            game.sound.soundbonus();
                        }
                        if (startbonustimeleft == 0)
                        {
                            game.sound.music(0);
                            game.sound.soundbonusoff();
                            game.video.SetIntensity(1);
                        }
                    }
                }
                else
                {
                    EndBonusMode();
                    game.sound.soundbonusoff();
                    game.sound.music(1);
                }
            }
            if (bonusmode && !digonscr)
            {
                EndBonusMode();
                game.sound.soundbonusoff();
                game.sound.music(1);
            }
            if (emocttime > 0)
                emocttime--;
        }

        public void DrawEmeralds()
        {
            int x, y;
            emmask = 1 << game.currentPlayer;
            for (x = 0; x < 15; x++)
                for (y = 0; y < 10; y++)
                    if ((emfield[y * 15 + x] & emmask) != 0)
                        game.drawing.DrawEmerald(x * 20 + 12, y * 18 + 21);
        }

        public void DrawExplosion()
        {
            switch (expsn)
            {
                case 1:
                    game.sound.soundexplode();
                    goto case 3;
                case 2:
                case 3:
                    game.drawing.DrawFire(firex, firey, expsn);
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
            game.video.SetIntensity(0);
        }

        public void EraseBonus()
        {
            if (bonusvisible)
            {
                bonusvisible = false;
                game.sprite.EraseSprite(14);
            }
        }

        public void EraseDigger()
        {
            game.sprite.EraseSprite(0);
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
                    game.drawing.DrawEmerald(x * 20 + 12, y * 18 + 21);
                    game.IncrementPenalty();
                }
                if (r == embox[dir + 1])
                {
                    game.drawing.EraseEmerald(x * 20 + 12, y * 18 + 21);
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
            game.video.SetIntensity(1);
            bonustimeleft = 250 - game.level.LevelOf10() * 20;
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
            game.sprite.MoveDrawSprite(0, x, y);
            notfiring = true;
            emocttime = 0;
            bonusvisible = bonusmode = false;
            game.input.firepressed = false;
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
                game.drawing.EraseEmerald(x * 20 + 12, (y + 1) * 18 + 21);
            }
        }

        public void KillFire()
        {
            if (!notfiring)
            {
                notfiring = true;
                game.sprite.EraseSprite(15);
                game.sound.soundfireoff();
            }
        }

        public void MakeEmeraldField()
        {
            int x, y;
            emmask = 1 << game.currentPlayer;
            for (x = 0; x < 15; x++)
                for (y = 0; y < 10; y++)
                    if (game.level.GetChar(x, y, game.level.LevelPlan()) == 'C')
                        emfield[y * 15 + x] |= (byte)emmask;
                    else
                        emfield[y * 15 + x] &= (byte)~emmask;
        }

        private void UpdateDigger()
        {
            int dir, ddir, clbits, diggerox, diggeroy, nmon;
            bool push = true;
            game.input.ReadDirection();
            dir = game.input.GetDirection(0);
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
                game.drawing.EatField(diggerox, diggeroy, mdir);
            switch (mdir)
            {
                case 0:
                    game.drawing.DrawRightBlob(x, y);
                    x += 4;
                    break;
                case 4:
                    game.drawing.DrawLeftBlob(x, y);
                    x -= 4;
                    break;
                case 2:
                    game.drawing.DrawTopBlob(x, y);
                    y -= 3;
                    break;
                case 6:
                    game.drawing.DrawBottomBlob(x, y);
                    y += 3;
                    break;
            }

            if (IsEmeraldHit((x - 12) / 20, (y - 18) / 18, (x - 12) % 20, (y - 18) % 18, mdir))
            {
                game.scores.ScoreEmerald();
                game.sound.soundem();
                game.sound.soundemerald(emocttime);
                emocttime = 9;
            }
            clbits = game.drawing.DrawDigger(this.dir, x, y, notfiring && rechargetime == 0);
            game.IncrementPenalty();
            if ((game.bags.BagBits() & clbits) != 0)
            {
                if (mdir == 0 || mdir == 4)
                {
                    push = game.bags.PushBags(mdir, clbits);
                    time++;
                }
                else
                if (!game.bags.PushUpdatedBags(clbits))
                    push = false;
                if (!push)
                { /* Strange, push not completely defined */
                    x = diggerox;
                    y = diggeroy;
                    game.drawing.DrawDigger(mdir, x, y, notfiring && rechargetime == 0);
                    game.IncrementPenalty();
                    this.dir = game.ReverseDir(mdir);
                }
            }
            if (((clbits & 0x3f00) != 0) && bonusmode)
                for (nmon = game.monster.KillMonsters(clbits); nmon != 0; nmon--)
                {
                    game.sound.soundeatm();
                    game.scores.ScoreEatMonster();
                }
            if ((clbits & 0x4000) != 0)
            {
                game.scores.ScoreBonus();
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
                if (game.input.firepflag)
                    if (digonscr)
                    {
                        rechargetime = game.level.LevelOf10() * 3 + 60;
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
                        game.sprite.MoveDrawSprite(15, firex, firey);
                        game.sound.soundfire();
                    }
            }
            else
            {
                switch (firedir)
                {
                    case 0:
                        firex += 8;
                        pix = game.video.GetPixel(firex, firey + 4) | game.video.GetPixel(firex + 4, firey + 4);
                        break;
                    case 4:
                        firex -= 8;
                        pix = game.video.GetPixel(firex, firey + 4) | game.video.GetPixel(firex + 4, firey + 4);
                        break;
                    case 2:
                        firey -= 7;
                        pix = (game.video.GetPixel(firex + 4, firey) | game.video.GetPixel(firex + 4, firey + 1) |
                                game.video.GetPixel(firex + 4, firey + 2) | game.video.GetPixel(firex + 4, firey + 3) |
                                game.video.GetPixel(firex + 4, firey + 4) | game.video.GetPixel(firex + 4, firey + 5) |
                                game.video.GetPixel(firex + 4, firey + 6)) & 0xc0;
                        break;
                    case 6:
                        firey += 7;
                        pix = (game.video.GetPixel(firex, firey) | game.video.GetPixel(firex, firey + 1) |
                                game.video.GetPixel(firex, firey + 2) | game.video.GetPixel(firex, firey + 3) |
                                game.video.GetPixel(firex, firey + 4) | game.video.GetPixel(firex, firey + 5) |
                                game.video.GetPixel(firex, firey + 6)) & 3;
                        break;
                }
                clbits = game.drawing.DrawFire(firex, firey, 0);
                game.IncrementPenalty();
                if ((clbits & 0x3f00) != 0)
                    for (mon = 0, b = 256; mon < 6; mon++, b <<= 1)
                        if ((clbits & b) != 0)
                        {
                            game.monster.KillMonster(mon);
                            game.scores.ScoreKill();
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
                            game.drawing.DrawFire(firex, firey, 0);
                        }
                        break;
                    case 4:
                        if (firex < 16)
                            expsn = 1;
                        else if (pix != 0 && clbits == 0)
                        {
                            expsn = 1;
                            firex += 8;
                            game.drawing.DrawFire(firex, firey, 0);
                        }
                        break;
                    case 2:
                        if (firey < 15)
                            expsn = 1;
                        else if (pix != 0 && clbits == 0)
                        {
                            expsn = 1;
                            firey += 7;
                            game.drawing.DrawFire(firex, firey, 0);
                        }
                        break;
                    case 6:
                        if (firey > 183)
                            expsn = 1;
                        else if (pix != 0 && clbits == 0)
                        {
                            expsn = 1;
                            firey -= 7;
                            game.drawing.DrawFire(firex, firey, 0);
                        }
                        break;
                }
            }
        }
    }
}