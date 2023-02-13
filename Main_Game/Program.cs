using System;
using System.Globalization;
using System.IO;
using System.Linq;
using tm = System.Timers;
using System.Text;
using Methods;
using System.Resources;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

#pragma warning disable
namespace Menu
{


    class Program
    {



        static public Dictionary<char, string[]> routeDict = new()
        {
            {'═',new string[]{"Nyugat","Kelet"}},
            {'╬', new string[]{"Észak","Dél","Nyugat","Kelet"}},
            {'╔',new string[]{"Dél","Kelet"}},
            {'╦',new string[]{"Dél","Kelet","Nyugat"}},

            {'╩', new string[]{"Észak","Kelet","Nyugat"}},
            {'║',new string[]{"Észak","Dél"}},
            {'╗',new string[]{"Dél","Nyugat"}},

            {'╣', new string[]{"Észak","Dél","Nyugat"}},
            {'╚',new string[]{"Észak","Kelet"}},

        };



        public delegate void CenterText();
        public static CenterText ct = () => Console.SetCursorPosition(Console.WindowWidth / 2 - 30, 0);
        public static ResourceManager rm = new ResourceManager("Main_Game.Resources.String", Assembly.GetExecutingAssembly());


        static void HighlightSelected(List<Option> options, Option selected)
        {
            foreach (Option option in options)
            {

                if (option == selected)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("-> ");

                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(" ");
                }
                Console.WriteLine(option.Name);
            }
        }

        static void Main(string[] args)
        {
            //This should work
            if (Thread.CurrentThread.CurrentUICulture.Name != "hu-HU" && Thread.CurrentThread.CurrentUICulture.Name != "en-GB")
            {
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-GB", true);
            }


            //Clear the Console Screen
            Console.Clear();
            //Fire up the Main Menu
            MainMenu();

        }
        ///<summary>Main Menu</summary>
        public static void MainMenu()
        {
            int index = 0;


            List<Option> options = new()
                {
                    new Option(rm.GetString("Option1"),Language),
                    new Option( rm.GetString("Option2"),LoadMap),
                    new Option(rm.GetString("Option3"),() => Environment.Exit(0))
                };

            while (true)
            {
                Console.Clear();
                ct();
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(rm.GetString("Title"));
                Console.WriteLine($"{rm.GetString("Lang")} {Thread.CurrentThread.CurrentUICulture} ");
                HighlightSelected(options, options[index]);
                ConsoleKeyInfo key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.W:
                    case ConsoleKey.UpArrow:
                        index = Math.Clamp(--index, 0, options.Count - 1);
                        break;
                    case ConsoleKey.S:
                    case ConsoleKey.DownArrow:
                        index = Math.Clamp(++index, 0, options.Count - 1);
                        break;
                    case ConsoleKey.Enter:
                        options[index].Selected.Invoke();
                        break;
                }
            }


        }

        ///<summary>Nyelvválasztó menü</summary>
        public static void Language()
        {
            List<Option> langOpt = new()
            {
                new Option(rm.GetString("langHU"),()=>{ Thread.CurrentThread.CurrentUICulture = new CultureInfo("hu-HU", true); MainMenu();}),
                new Option( rm.GetString("langEN"), ()=> { Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-GB", true); MainMenu();}),
                new Option(rm.GetString("langESC"),()=> {MainMenu();})
            };
            int index = 0;
            HighlightSelected(langOpt, langOpt[index]);
            while (true)
            {
                Console.Clear();
                ct();
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(rm.GetString("langTitle"));
                HighlightSelected(langOpt, langOpt[index]);
                switch (Console.ReadKey(true).Key)
                {
                    case ConsoleKey.W:
                    case ConsoleKey.UpArrow:
                        index = Math.Clamp(--index, 0, langOpt.Count - 1);
                        break;
                    case ConsoleKey.S:
                    case ConsoleKey.DownArrow:

                        index = Math.Clamp(++index, 0, langOpt.Count - 1);
                        break;
                    case ConsoleKey.Enter:
                        langOpt[index].Selected.Invoke();
                        break;

                }
            }

        }
        ///<summary>Pályaválasztás</summary>
        public static void LoadMap()
        {

            //Generáljunk választható opciókat a beolvasott pályáknak megfelelően
            List<Option> maps = new();


            DirectoryInfo d = new(@"./Maps");
            FileInfo[] fullmaps = d.GetFiles("*.txt");
            FileInfo[] savedmaps = d.GetFiles("*.sav");
            if (!(fullmaps.Count() > 0))
            {
                Console.WriteLine(rm.GetString("findMapError"));
            }
            else
            {
                //Index for the map List
                int mapIndex = 0;
                int Index = 1;
                foreach (FileInfo f in fullmaps)
                {

                    maps.Add(new Option($"[{Index}]. {f.Name}", () => FireGame(f.Name)));
                    Index++;
                }
                //Add exit for the last option
                maps.Add(new Option($"[{maps.Count + 1}].{rm.GetString("langESC").Substring(4)}", MainMenu));
                HighlightSelected(maps, maps[mapIndex]);

                while (true)
                {
                    Console.Clear();
                    ct();
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(rm.GetString("mapTitle"));
                    HighlightSelected(maps, maps[mapIndex]);
                    ConsoleKeyInfo consoleKey = Console.ReadKey(true);
                    switch (consoleKey.Key)
                    {

                        case ConsoleKey.W:
                        case ConsoleKey.UpArrow:
                            mapIndex = Math.Clamp(--mapIndex, 0, maps.Count - 1);
                            break;
                        case ConsoleKey.S:
                        case ConsoleKey.DownArrow:
                            mapIndex = Math.Clamp(++mapIndex, 0, maps.Count - 1);
                            break;
                        case ConsoleKey.Enter:
                            maps[mapIndex].Selected.Invoke();
                            break;

                    }
                }

            }







        }
        static void FireGame(string FileName)
        {

            int diff = 1;
            Maze maze = new Maze(FileName, diff);
            maze.CreateMap(ref FileName);
            maze.ShowMap();
            Player player = new Player(maze);
            player.Move();
        }



    }



    ///<summary>Class for the Maze (Map). This object is Disposable</summary>
    public class Maze : IDisposable
    {

        /*
            - Kilépés a labirintusból
        */
        #region //PROPERTIES
        private char[,]? map;
        public bool _disposed;
        public char[,]? Map { get => map; }

        public string? FileName { get; set; }

        public int? Difficulty { get; set; }
        #endregion
        public Maze(string fname, int diff)
        {
            FileName = fname;
            Difficulty = diff;
        }
        internal void CreateMap(ref string filename)
        {
            string[] file = File.ReadAllLines($".//Maps//{filename}");
            if (file.Length == 0)
            {
                Console.WriteLine(Program.rm.GetString("findMapError"));
                Console.ReadKey();
                this.Dispose();
                Program.MainMenu();
            }
            map = new char[file.Length, file[0].Length];
            for (int row = 0; row < map.GetLength(0); row++)
            {
                for (int col = 0; col < map.GetLength(1); col++)
                {
                    map[row, col] = file[row][col];
                }
            }
        }
        ///<summary>Clears the console screen and shows the map</summary>
        public void ShowMap()
        {
            Console.Clear();
            Console.WriteLine("\n");
            for (int row = 0; row < map!.GetLength(0); row++)
            {
                for (int col = 0; col < map.GetLength(1); col++)
                {
                    Console.Write(map[row, col]);
                }
                Console.WriteLine();
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Sets all fields to default before getting disposed.
        /// </summary>
        /// <param name="_disposing">Boolean for whenever it is currently getting disposed or not.</param>
        protected virtual void Dispose(bool _disposing)
        {
            if (_disposed) return;
            if (_disposing)
            {
                this.map = null;
                this.FileName = string.Empty;
                this.Difficulty = 0;

            }
            _disposed = true;

        }
        /// <summary>
        /// Loops over the boundaries of the map and finds all the exits
        /// </summary>
        /// <returns>A list of all the entrance's coordinates</returns>
        public List<(int, int)> GetRandomEntrance()
        {
            List<(int, int)> coords = new();
            for (int row = 0; row < map.GetLength(0); row++)
            {
                for (int col = 0; col < map.GetLength(1); col++)
                {
                    if (col == 0 || col == map.GetLength(1) - 1)
                    {
                        if (map[row, col].Equals('═')) coords.Add((row, col));
                    }
                    else if (row == 0 || row == map.GetLength(0) - 1)
                    {
                        if (map[row, col].Equals('║')) coords.Add((row, col));
                    }
                }
            }
            return coords;


        }
    }

    /// <summary>
    /// Class for the Player (Control,Movement,GameLoop). This object is Disposable.
    /// </summary>

    [Serializable]
    public class Player : IDisposable
    {
        //todo Create Method for exiting
        delegate int Clamp(int val, int min, int max);
        Clamp clamp = Math.Clamp;
        private Maze Mz;

        static int secs = 60;
        tm.Timer timer = new tm.Timer(1000);
        const char CHARACTER_SPRITE = '░';
        private static int Counter;
        static int Steps;
        static HashSet<(int, int)> stored = new();
        bool _disposed;
        bool leftFromStart;
        public int posX { get; set; }
        public int posY { get; set; }
        public int NewPosX;
        public int NewPosY;
        (int, int) coords;
        private Random rnd = new();

        public Player(Maze mz)
        {
            this.Mz = mz;
        }

        /// <summary>
        /// Main Game loop
        /// </summary>
        
        public void Move()
        {
            coords = this.Mz.GetRandomEntrance()[rnd.Next(this.Mz.GetRandomEntrance().Count)];
            this.posX = coords.Item2;
            this.posY = coords.Item1 + 2;
            this.NewPosX = this.posX;
            this.NewPosY = this.posY;
            Console.CursorVisible = false;
            timer.Start();
            timer.Enabled = true;
            timer.Elapsed += Timer_Elapsed;
            Console.SetCursorPosition(this.posX, this.posY);
            Console.Write(CHARACTER_SPRITE);
            while (true)
            {




                switch (Console.ReadKey(true).Key)
                {

                    case ConsoleKey.W:
                        this.NewPosY = clamp(--this.NewPosY, 2, this.Mz.Map.GetLength(0) + 1);
                        break;
                    case ConsoleKey.S:
                        this.NewPosY = clamp(++this.NewPosY, 0, this.Mz.Map.GetLength(0) + 1);
                        break;
                    case ConsoleKey.A:
                        this.NewPosX = clamp(--this.NewPosX, 0, this.Mz.Map.GetLength(1) - 1);
                        break;
                    case ConsoleKey.D:
                        this.NewPosX = clamp(++this.NewPosX, 0, this.Mz.Map.GetLength(1) - 1);
                        break;
                    case ConsoleKey.K:
                        this.SaveMap();
                        break;

                }
                if (this.Mz.Map[clamp(NewPosY - 2, 0, this.Mz.Map.GetLength(0) - 1), clamp(NewPosX, 0, this.Mz.Map.GetLength(1) - 1)] != '.')
                {
                    //Egyetlen mód arra, hogy Console.Clear nélkül mozogjunk, hogy az előző pozíciónknál kiírjuk a karaktert miután léptünk és UTÁNA
                    //iratjuk ki a játékost
                    Console.SetCursorPosition(this.posX, this.posY);
                    Console.Write(this.Mz.Map[this.posY - 2, this.posX]);
                    this.posX = this.NewPosX;
                    this.posY = this.NewPosY;
                    leftFromStart = true;
                }
                else
                {
                    this.NewPosX = this.posX;
                    this.NewPosY = this.posY;
                }
                Console.SetCursorPosition(this.posX, this.posY);
                Console.Write(CHARACTER_SPRITE);
                IsAtEntrace();
                Exit();
                Steps++;


            }
        }

        private void Timer_Elapsed(object? sender, tm.ElapsedEventArgs e)
        {
            Console.SetCursorPosition(0, 0);
            Console.Write($"Pálya neve: {this.Mz.FileName}, mérete: {this.Mz.Map.GetLength(0)} sor x {this.Mz.Map.GetLength(1)} oszlop\n Felfedezett termek száma: {this.RoomsFound()}\t{secs}\t {{0}}",string.Join(",", Program.routeDict[this.Mz.Map[this.posY-2,this.posX]]));
            secs--;
        }


        /// <summary>
        /// Keeps track of how many rooms does the player visited
        /// </summary>
        /// <returns>the current number of rooms visited</returns>
        private int RoomsFound()
        {


            if (this.Mz.Map[clamp(this.posY - 2, 0, this.Mz.Map.GetLength(0) - 1), clamp(this.posX, 0, this.Mz.Map.GetLength(1) - 1)] == '█' && !(stored.Contains((this.posX, this.posY))))
            {
                Counter++;
                stored.Add((this.posX, this.posY));
            }
            return Counter;
        }

        private void SaveMap()
        {

            //idk...
            //todo SAVE THIS GODDAMN THING
        }

        void Exit()
        {
            if(IsAtEntrace())
            {
                Console.SetCursorPosition(0,Console.WindowHeight-3);
                Console.Write("KKKKKK");
            }
            
            //this.Mz.Dispose();
            //this.Dispose();
            //Program.MainMenu();
        }
        bool IsAtEntrace()
        {
           if(leftFromStart && this.Mz.GetRandomEntrance().Contains((this.posY-2,this.posX)))
            {
                return true;
            }
           return false;

        }
        

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool _disposing)
        {
            if (_disposed) return;
            if (_disposing)
            {
                Counter = 0;
                this.posX = default;
                this.posY = default;
                this.Mz = null;
                stored.Clear();
                this.rnd = null;
                Steps = default;

            }
            _disposed = true;
        }

    }
    public class Option
    {
        public string Name { get; set; }
        public Action Selected { get; set; }
        public Option(string name, Action action)
        {
            Name = name;
            Selected = action;
        }
    }
}