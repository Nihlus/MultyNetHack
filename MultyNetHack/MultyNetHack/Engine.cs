﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Threading;

namespace MultyNetHack
{
    class Engine
    {


        List<List<char>> buff1;
        Dictionary<Material, char> texture ;
        List<List<bool>> updated;
        System.Timers.Timer renderTimer, updateTimer;
        Point center;
        Size SceenSize;
        Root root;
        int l
        {
            get
            {
                return center.x - SceenSize.width / 2;
            }
        }
        int r
        {
            get
            {
                return center.x + SceenSize.width / 2;
            }
        }
        int b
        {
            get
            {
                return center.y - SceenSize.height / 2;
            }
        }
        int t
        {
            get
            {
                return center.y + SceenSize.height / 2;
            }
        }
        Rectangle _r;
        Rectangle bounds
        {
            get
            {
                _r.b = b;
                _r.l = l;
                _r.r = r;
                _r.t = t;
                return _r;
            }
        }
        int gh;
        bool showHelp;
        int from;
        double renderInterval;
        Queue<String> Message;
        public Engine(int width, int height)
        {
            
            SceenSize = new Size(width, height);
            center = new Point(0, 0);
            root = new Root();
            Init();
            _r = new Rectangle(0, 0, 0, 0);
            renderInterval = 100;
            Console.CursorVisible = false;
            Message = new Queue<string>();

            renderTimer = new System.Timers.Timer(renderInterval); // update ~60Hz
            updateTimer = new System.Timers.Timer(3); // update ~30HZ
            renderTimer.Enabled = true;
            updateTimer.Enabled = true;
            updateTimer.Elapsed += OnUpdateFrame;
            renderTimer.Elapsed += OnRenderFrame;
            renderTimer.Start();
            updateTimer.Start();
            
        }

        private void Init()
        {

            Player p = new Player(center.x, center.y);
            buff1 = new List<List<char>>(SceenSize.height);
            for (int j = 0; j <SceenSize.height; j++)
            {
                buff1.Add(new List<char>(SceenSize.width));
                buff1[j] = new List<char>(SceenSize.width);
                if (j == 0)
                {
                    for (int i = 0; i < SceenSize.width; i++)
                    {
                        buff1[j].Add(new char());
                    }
                }
                else
                    buff1[j].AddRange(buff1[0]);
            }
            loadTexture();
        }
        private void loadTexture()
        {
            texture = new Dictionary<Material, char>();
            texture.Add(Material.Air, '.');
            texture.Add(Material.Fire, '~');
            texture.Add(Material.Loot, '$');
            texture.Add(Material.Npc, 'N');
            texture.Add(Material.Path, '#');
            texture.Add(Material.Player, '@');
            texture.Add(Material.Trap, '.');
            texture.Add(Material.Wall, '+');
            texture.Add(Material.Watter, '}');
            texture.Add(Material.Darknes, ' ');
        }
        private void ZbufferUpdate(Component comp, int parentTop, int parentLeft)
        {
            
            foreach (string s in comp.keys)
                ZbufferUpdate(comp.controls[s], parentTop + comp.y, parentLeft + comp.x);    
            if (comp & bounds)
                FillBuffer(parentLeft + comp.l, parentTop + comp.t, comp.height, comp.width, comp.madeOf);

        }
        private void FlushBuffer()
        {
            Console.CursorTop = 0;
            Console.CursorLeft = 0;
            Console.Clear();
            
            foreach (List<char> c in buff1)
                Console.WriteLine(c.ToArray());
            //Console.MoveBufferArea(0, top, width, top, 0, 0);
            if (showHelp)
            {
                if (Message.Count > 0)
                {

                    Console.WriteLine("New Message");
                    Console.WriteLine(new string('-', Console.WindowWidth));
                    Console.WriteLine();
                    Console.WriteLine(Message.Peek());
                    Console.WriteLine();
                    Console.WriteLine(new string('-', Console.WindowWidth));
                    Console.WriteLine();
                    Console.WriteLine("Press 'd' to dismiss messsage");
                }
                else
                {
                    Console.WriteLine("Move h/j/k/l");
                    Console.WriteLine("Generate room g");
                    Console.WriteLine("Curent Location:({0},{1})", center.x, center.y);

                    Console.WriteLine("There are {0} rooms!", root.keys.Count);
                    Console.WriteLine("Shown rooms {0} - {1}.", new object[2] { from, 20 + from, });
                    for (int i = from; i < Math.Min(root.controls.Count, (20 + from)); i++)
                    {
                        string key = root.keys[i];
                        Component c = root.controls[key];

                        Console.WriteLine("{0} is located ({1},{2}) - it's size is {3}x{4} - On The screen? {5} !", new object[] { c.name, c.x, c.y, c.width, c.height, c & bounds });
                    }
                }
            }
            renderTimer.Start();
        }
        private void FillBuffer(int x, int y, int h, int w, Material m)
        {
            Point start = bounds.ToTopLeft(x ,y);
            Point end = bounds.ToTopLeft(x + w, y- h);
            for (int i = Math.Min(end.y, start.y); i < Math.Max(start.y, end.y); i++)
            {
                for (int j = Math.Min(end.x, start.x); j < Math.Max(start.x, end.x); j++)
                {

                    if (!updated[j][i])
                    {
                        buff1[j][i] = texture[m];
                        updated[j][i] = true;
                    }
                }
            }
        }
        private void OnUpdateFrame(object sender, ElapsedEventArgs e)
        {
            
            updateTimer.Stop();
            char c = Console.ReadKey().KeyChar;

            switch (c)
            {
                case 'g':
                    Room r = new Room("room" + gh);
                    root.Insert(r);
                    (root.controls["room" + gh] as Room).generatRandom();
                    gh++;
                    Message.Enqueue("Added 1 new Room at the location " + r.x + "," + r.y);
                    break;
                case 'G':
                    renderTimer.Interval = 20000;
                    Thread.Sleep(Convert.ToInt32( renderInterval*2));
                    Console.Clear();
                    Console.WriteLine("GENERATING ROOMS w8 4 it!!!");
                    int maxLeft = 10;
                    Console.Write("|");
                    Console.CursorLeft = maxLeft;
                    Console.Write("|");
                    int genRooms = 1000; ;
                    for (int i = 0; i < genRooms; i++)
                    {
                        Console.CursorLeft = (int)(((float)i / genRooms) * (maxLeft-1)) + 1;
                        Console.Write("#");
                        Room R = new Room("room" + gh + '.' + i);
                        root.Insert(R);
                        (root.controls["room" + gh + '.' + i] as Room).generatRandom();
                    }
                    Message.Enqueue("Added " + genRooms + " new rooms");
                    renderTimer.Interval = renderInterval;
                    gh++;
                    break;
                case 'h':
                    center.x -= 1;
                    break;
                case 'j':
                    center.y -= 1;
                    break;
                case 'k':
                    center.y += 1;
                    break;
                case 'l':
                    center.x += 1;
                    break;
                case 'H':
                    center.x -= 10;
                    break;
                case 'J':
                    center.y -= 10;
                    break;
                case 'K':
                    center.y += 10;
                    break;
                case 'L':
                    center.x += 10;
                    break;
                case '?':
                    showHelp = !showHelp;
                    break;
                case 'v':
                    from = Math.Max(from - 1, 0);
                    break;
                case 'V':
                    from++;
                    break;
                case 'd':
                    if (Message.Count>0)
                        Message.Dequeue();
                    break;
            }
                

            updateTimer.Start();
        }
        private void OnRenderFrame(object sender, ElapsedEventArgs e)
        {
            renderTimer.Stop();
            updated = new List<List<bool>>(SceenSize.height);
            for (int i = 0; i < SceenSize.height; i++)
            {

                updated.Add(new List<bool>(SceenSize.width));
                if (i == 0)
                {
                    for (int j = 0; j < SceenSize.width; j++)
                    {
                        updated[i].Add(false);
                    }
                }
                else
                {
                    updated[i].AddRange(updated[0]);
                }
            }
            ZbufferUpdate(root, 0, 0);
            FillBuffer(bounds.l, bounds.t, SceenSize.height, SceenSize.width, root.madeOf);
            FlushBuffer();
        }

    }
}