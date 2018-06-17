using System;

namespace Digger
{
    public class Monster
    {
        private struct MonsterData
        {
            public int x, y, h, v, xr, yr, dir, hdir, t, hnt, death, bag, dtime, stime;
            public bool flag, nob, alive;
        }

        private Game game;

        private readonly MonsterData[] mondat = new MonsterData[6];

        int nextmonster = 0, totalmonsters = 0, maxmononscr = 0, nextmontime = 0, mongaptime = 0;

        bool unbonusflag = false, mongotgold = false;

        public Monster(Game game)
        {
            this.game = game;
        }

        public void CheckCoincidence(int mon, int bits)
        {
            for (int m = 0, b = 256; m < 6; m++, b <<= 1)
                if (((bits & b) != 0) && (mondat[mon].dir == mondat[m].dir) && (mondat[m].stime == 0) && (mondat[mon].stime == 0))
                    mondat[m].dir = game.ReverseDir(mondat[m].dir);
        }

        public void CheckMonsterScared(int h)
        {
            int m;
            for (m = 0; m < 6; m++)
                if ((h == mondat[m].h) && (mondat[m].dir == 2))
                    mondat[m].dir = 6;
        }

        public void CreateMonster()
        {
            int i;
            for (i = 0; i < 6; i++)
            {
                if (!mondat[i].flag)
                {
                    mondat[i].flag = true;
                    mondat[i].alive = true;
                    mondat[i].t = 0;
                    mondat[i].nob = true;
                    mondat[i].hnt = 0;
                    mondat[i].h = 14;
                    mondat[i].v = 0;
                    mondat[i].x = 292;
                    mondat[i].y = 18;
                    mondat[i].xr = 0;
                    mondat[i].yr = 0;
                    mondat[i].dir = 4;
                    mondat[i].hdir = 4;
                    nextmonster++;
                    nextmontime = mongaptime;
                    mondat[i].stime = 5;
                    game.Sprite.MoveDrawSprite(i + 8, mondat[i].x, mondat[i].y);
                    break;
                }
            }
        }

        public void DoMonsters()
        {
            int i;
            if (nextmontime > 0)
                nextmontime--;
            else
            {
                if (nextmonster < totalmonsters && MonstersOnScreenCount() < maxmononscr && game.Digger.digonscr && !game.Digger.bonusmode)
                    CreateMonster();

                if (unbonusflag && nextmonster == totalmonsters && nextmontime == 0)
                {
                    if (game.Digger.digonscr)
                    {
                        unbonusflag = false;
                        game.Digger.CreateBonus();
                    }
                }
            }
            for (i = 0; i < 6; i++)
            {
                if (mondat[i].flag)
                {
                    if (mondat[i].hnt > 10 - Level.LevelOf10(game.Level))
                    {
                        if (mondat[i].nob)
                        {
                            mondat[i].nob = false;
                            mondat[i].hnt = 0;
                        }
                    }
                    if (mondat[i].alive)
                        if (mondat[i].t == 0)
                        {
                            MonsterAI(i);
                            if (game.RandNo(15 - Level.LevelOf10(game.Level)) == 0 && mondat[i].nob)
                                MonsterAI(i);
                        }
                        else
                            mondat[i].t--;
                    else
                        MonsterDie(i);
                }
            }
        }

        public void EraseMonsters()
        {
            int i;
            for (i = 0; i < 6; i++)
                if (mondat[i].flag)
                    game.Sprite.EraseSprite(i + 8);
        }

        public bool ClearField(int dir, int x, int y)
        {
            switch (dir)
            {
                case 0:
                    if (x < 14)
                        if ((GetField(x + 1, y) & 0x2000) == 0)
                            if ((GetField(x + 1, y) & 1) == 0 || (GetField(x, y) & 0x10) == 0)
                                return true;
                    break;
                case 4:
                    if (x > 0)
                        if ((GetField(x - 1, y) & 0x2000) == 0)
                            if ((GetField(x - 1, y) & 0x10) == 0 || (GetField(x, y) & 1) == 0)
                                return true;
                    break;
                case 2:
                    if (y > 0)
                        if ((GetField(x, y - 1) & 0x2000) == 0)
                            if ((GetField(x, y - 1) & 0x800) == 0 || (GetField(x, y) & 0x40) == 0)
                                return true;
                    break;
                case 6:
                    if (y < 9)
                        if ((GetField(x, y + 1) & 0x2000) == 0)
                            if ((GetField(x, y + 1) & 0x40) == 0 || (GetField(x, y) & 0x800) == 0)
                                return true;
                    break;
            }
            return false;
        }

        public int GetField(int x, int y)
        {
            return game.Drawing.field[y * 15 + x];
        }

        public void IncreaseMonsterTime(int n)
        {
            if (n > 6)
                n = 6;
            for (int m = 1; m < n; m++)
                mondat[m].t++;
        }

        public void IncrementPenalties(int bits)
        {
            for (int m = 0, b = 256; m < 6; m++, b <<= 1)
            {
                if ((bits & b) != 0)
                    game.IncrementPenalty();
                b <<= 1;
            }
        }

        public void InitMonsters()
        {
            for (int i = 0; i < 6; i++)
                mondat[i].flag = false;
            nextmonster = 0;
            mongaptime = 45 - (Level.LevelOf10(game.Level) << 1);
            totalmonsters = Level.LevelOf10(game.Level) + 5;
            switch (Level.LevelOf10(game.Level))
            {
                case 1:
                    maxmononscr = 3;
                    break;
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                    maxmononscr = 4;
                    break;
                case 8:
                case 9:
                case 10:
                    maxmononscr = 5;
                    break;
            }
            nextmontime = 10;
            unbonusflag = true;
        }

        public void KillMonster(int mon)
        {
            if (mondat[mon].flag)
            {
                mondat[mon].flag = mondat[mon].alive = false;
                game.Sprite.EraseSprite(mon + 8);
                if (game.Digger.bonusmode)
                    totalmonsters++;
            }
        }

        public int KillMonsters(int bits)
        {
            int n = 0;
            for (int m = 0, b = 256; m < 6; m++, b <<= 1)
                if ((bits & b) != 0)
                {
                    KillMonster(m);
                    n++;
                }

            return n;
        }

        public void MonsterAI(int mon)
        {
            int clbits, monox, monoy, dir, mdirp1, mdirp2, mdirp3, mdirp4, t;
            bool push;
            monox = mondat[mon].x;
            monoy = mondat[mon].y;

            if (mondat[mon].xr == 0 && mondat[mon].yr == 0)
            {
                /* If we are here the monster needs to know which way to turn next. */

                /* Turn hobbin back into nobbin if it's had its time */

                if (mondat[mon].hnt > 30 + (Level.LevelOf10(game.Level) << 1))
                    if (!mondat[mon].nob)
                    {
                        mondat[mon].hnt = 0;
                        mondat[mon].nob = true;
                    }

                /* Set up monster direction properties to chase digger */

                if (Math.Abs(game.Digger.y - mondat[mon].y) > Math.Abs(game.Digger.x - mondat[mon].x))
                {
                    if (game.Digger.y < mondat[mon].y) { mdirp1 = 2; mdirp4 = 6; }
                    else { mdirp1 = 6; mdirp4 = 2; }
                    if (game.Digger.x < mondat[mon].x) { mdirp2 = 4; mdirp3 = 0; }
                    else { mdirp2 = 0; mdirp3 = 4; }
                }
                else
                {
                    if (game.Digger.x < mondat[mon].x) { mdirp1 = 4; mdirp4 = 0; }
                    else { mdirp1 = 0; mdirp4 = 4; }
                    if (game.Digger.y < mondat[mon].y) { mdirp2 = 2; mdirp3 = 6; }
                    else { mdirp2 = 6; mdirp3 = 2; }
                }

                /* In bonus mode, run away from digger */

                if (game.Digger.bonusmode)
                {
                    t = mdirp1; mdirp1 = mdirp4; mdirp4 = t;
                    t = mdirp2; mdirp2 = mdirp3; mdirp3 = t;
                }

                /* Adjust priorities so that monsters don't reverse direction unless they
                   really have to */

                dir = game.ReverseDir(mondat[mon].dir);
                if (dir == mdirp1)
                {
                    mdirp1 = mdirp2;
                    mdirp2 = mdirp3;
                    mdirp3 = mdirp4;
                    mdirp4 = dir;
                }
                if (dir == mdirp2)
                {
                    mdirp2 = mdirp3;
                    mdirp3 = mdirp4;
                    mdirp4 = dir;
                }
                if (dir == mdirp3)
                {
                    mdirp3 = mdirp4;
                    mdirp4 = dir;
                }

                /* Introduce a randno element on levels <6 : occasionally swap p1 and p3 */

                if (game.RandNo(Level.LevelOf10(game.Level) + 5) == 1 && Level.LevelOf10(game.Level) < 6)
                {
                    t = mdirp1;
                    mdirp1 = mdirp3;
                    mdirp3 = t;
                }

                /* Check field and find direction */

                if (ClearField(mdirp1, mondat[mon].h, mondat[mon].v))
                    dir = mdirp1;
                else if (ClearField(mdirp2, mondat[mon].h, mondat[mon].v))
                    dir = mdirp2;
                else if (ClearField(mdirp3, mondat[mon].h, mondat[mon].v))
                    dir = mdirp3;
                else if (ClearField(mdirp4, mondat[mon].h, mondat[mon].v))
                    dir = mdirp4;

                /* Hobbins don't care about the field: they go where they want. */

                if (!mondat[mon].nob)
                    dir = mdirp1;

                /* Monsters take a time penalty for changing direction */

                if (mondat[mon].dir != dir)
                    mondat[mon].t++;

                /* Save the new direction */

                mondat[mon].dir = dir;
            }

            /* If monster is about to go off edge of screen, stop it. */

            if ((mondat[mon].x == 292 && mondat[mon].dir == 0) ||
                (mondat[mon].x == 12 && mondat[mon].dir == 4) ||
                (mondat[mon].y == 180 && mondat[mon].dir == 6) ||
                (mondat[mon].y == 18 && mondat[mon].dir == 2))
                mondat[mon].dir = -1;

            /* Change hdir for hobbin */

            if (mondat[mon].dir == 4 || mondat[mon].dir == 0)
                mondat[mon].hdir = mondat[mon].dir;

            /* Hobbins digger */

            if (!mondat[mon].nob)
                game.Drawing.EatField(mondat[mon].x, mondat[mon].y, mondat[mon].dir);

            /* (Draw new tunnels) and move monster */

            switch (mondat[mon].dir)
            {
                case 0:
                    if (!mondat[mon].nob)
                        game.Drawing.DrawRightBlob(mondat[mon].x, mondat[mon].y);

                    mondat[mon].x += 4;
                    break;
                case 4:
                    if (!mondat[mon].nob)
                        game.Drawing.DrawLeftBlob(mondat[mon].x, mondat[mon].y);

                    mondat[mon].x -= 4;
                    break;
                case 2:
                    if (!mondat[mon].nob)
                        game.Drawing.DrawTopBlob(mondat[mon].x, mondat[mon].y);

                    mondat[mon].y -= 3;
                    break;
                case 6:
                    if (!mondat[mon].nob)
                        game.Drawing.DrawBottomBlob(mondat[mon].x, mondat[mon].y);

                    mondat[mon].y += 3;
                    break;
            }

            /* Hobbins can eat emeralds */

            if (!mondat[mon].nob)
                game.Digger.IsEmeraldHit((mondat[mon].x - 12) / 20, (mondat[mon].y - 18) / 18, (mondat[mon].x - 12) % 20, (mondat[mon].y - 18) % 18, mondat[mon].dir);

            /* If digger's gone, don't bother */

            if (!game.Digger.digonscr)
            {
                mondat[mon].x = monox;
                mondat[mon].y = monoy;
            }

            /* If monster's just started, don't move yet */

            if (mondat[mon].stime != 0)
            {
                mondat[mon].stime--;
                mondat[mon].x = monox;
                mondat[mon].y = monoy;
            }

            /* Increase time counter for hobbin */

            if (!mondat[mon].nob && mondat[mon].hnt < 100)
                mondat[mon].hnt++;

            /* Draw monster */

            push = true;
            clbits = game.Drawing.DrawMonster(mon, mondat[mon].nob, mondat[mon].hdir, mondat[mon].x, mondat[mon].y);
            game.IncrementPenalty();

            /* Collision with another monster */

            if ((clbits & 0x3f00) != 0)
            {
                mondat[mon].t++; /* Time penalty */
                CheckCoincidence(mon, clbits); /* Ensure both aren't moving in the same dir. */
                IncrementPenalties(clbits);
            }

            /* Check for collision with bag */

            if ((clbits & game.Bags.BagBits()) != 0)
            {
                mondat[mon].t++; /* Time penalty */
                mongotgold = false;
                if (mondat[mon].dir == 4 || mondat[mon].dir == 0)
                { /* Horizontal push */
                    push = game.Bags.PushBags(mondat[mon].dir, clbits);
                    mondat[mon].t++; /* Time penalty */
                }
                else
                  if (!game.Bags.PushUpdatedBags(clbits)) /* Vertical push */
                    push = false;
                if (mongotgold) /* No time penalty if monster eats gold */
                    mondat[mon].t = 0;
                if (!mondat[mon].nob && mondat[mon].hnt > 1)
                    game.Bags.RemoveBags(clbits); /* Hobbins eat bags */
            }

            /* Increase hobbin cross counter */

            if (mondat[mon].nob && ((clbits & 0x3f00) != 0) && game.Digger.digonscr)
                mondat[mon].hnt++;

            /* See if bags push monster back */

            if (!push)
            {
                mondat[mon].x = monox;
                mondat[mon].y = monoy;
                game.Drawing.DrawMonster(mon, mondat[mon].nob, mondat[mon].hdir, mondat[mon].x, mondat[mon].y);
                game.IncrementPenalty();
                if (mondat[mon].nob) /* The other way to create hobbin: stuck on h-bag */
                    mondat[mon].hnt++;
                if ((mondat[mon].dir == 2 || mondat[mon].dir == 6) && mondat[mon].nob)
                    mondat[mon].dir = game.ReverseDir(mondat[mon].dir); /* If vertical, give up */
            }

            /* Collision with digger */

            if (((clbits & 1) != 0) && game.Digger.digonscr)
                if (game.Digger.bonusmode)
                {
                    KillMonster(mon);
                    game.Scores.ScoreEatMonster();
                    game.Sound.soundeatm(); /* Collision in bonus mode */
                }
                else
                    game.Digger.KillDigger(3, 0); /* Kill digger */

            /* Update co-ordinates */

            mondat[mon].h = (mondat[mon].x - 12) / 20;
            mondat[mon].v = (mondat[mon].y - 18) / 18;
            mondat[mon].xr = (mondat[mon].x - 12) % 20;
            mondat[mon].yr = (mondat[mon].y - 18) % 18;
        }

        public void MonsterDie(int mon)
        {
            switch (mondat[mon].death)
            {
                case 1:
                    if (game.Bags.BagY(mondat[mon].bag) + 6 > mondat[mon].y)
                        mondat[mon].y = game.Bags.BagY(mondat[mon].bag);
                    game.Drawing.DrawMonsterDie(mon, mondat[mon].nob, mondat[mon].hdir, mondat[mon].x, mondat[mon].y);
                    game.IncrementPenalty();
                    if (game.Bags.GetBagDir(mondat[mon].bag) == -1)
                    {
                        mondat[mon].dtime = 1;
                        mondat[mon].death = 4;
                    }
                    break;
                case 4:
                    if (mondat[mon].dtime != 0)
                        mondat[mon].dtime--;
                    else
                    {
                        KillMonster(mon);
                        game.Scores.ScoreKill();
                    }
                    break;
            }
        }

        public void MonsterGetGold()
        {
            mongotgold = true;
        }

        public int MonstersLeftCount()
        {
            return MonstersOnScreenCount() + totalmonsters - nextmonster;
        }

        public int MonstersOnScreenCount()
        {
            int i, n = 0;
            for (i = 0; i < 6; i++)
                if (mondat[i].flag)
                    n++;
            return n;
        }

        public void SquashMonster(int mon, int death, int bag)
        {
            mondat[mon].alive = false;
            mondat[mon].death = death;
            mondat[mon].bag = bag;
        }

        public void SquashMonsters(int bag, int bits)
        {
            int m, b;
            for (m = 0, b = 256; m < 6; m++, b <<= 1)
                if ((bits & b) != 0)
                    if (mondat[m].y >= game.Bags.BagY(bag))
                        SquashMonster(m, 1, bag);
        }
    }
}