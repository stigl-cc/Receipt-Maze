class MazeGen {
    /*
      Maze representation through walls:
      00112233445
       _ _ _ _ _     0
      |_|_|_|_|_| 1, 2 => 0,0 1,0 2,0 3,0 4,0
      |_|_|_|_|_| 3, 4 => 0,1 1,1 2,1 3,1 4,1
      |_|_|_|_|_| 5, 6 => 0,2 1,2 2,2 3,2 4,2
      |_|_|_|_|_| 7, 8 => 0,3 1,3 2,3 3,3 4,3
      |_|_|_|_|_| 9,10 => 0,4 1,4 2,4 3,4 4,4

      0, 0 => Up: 0, 0 Down: 0, 2 Left: 0, 1 Right: 1, 1
      0, 3 => Up: 0, 6 Down: 0, 8 Left: 0, 7 Right: 1, 7
      3, 0 => Up: 3, 0 Down: 3, 2 Left: 3, 1 Right: 4, 1
      3, 3 => Up: 3, 6 Down: 3, 8 Left: 3, 7 Right: 4, 7

      X, Y => Up: X, Y * 2 Down: X, Y * 2 + 2 Left: X, Y * 2 + 1, Right: X + 1, Y * 2 + 1
      2, 2 => Up: 2, 4 Down: 2, 6 Left: 2, 5 Right: 3, 5
    */

    int Width, Height, Scale;
    bool Revisiting;
    bool[,] Field;
    bool[,] Visited;

    const bool Wall = true, Space = false;
    protected enum Direction { Up, Down, Left, Right };
    protected Direction[] Directions = { Direction.Up, Direction.Down, Direction.Left, Direction.Right };

    public MazeGen(int Width, int Height, int Scale, bool Revisiting = false) {
        this.Width = Width;
        this.Height = Height;
        this.Scale = Scale;
        this.Revisiting = Revisiting;
        this.Field = new bool[Width + 1, Height * 2 + 1];
        this.Visited = new bool[Width, Height];

        for(int x =0; x < Field.GetLength(0); x++) {
            for(int y =0; y < Field.GetLength(1); y++) {
                Field[x, y] = Wall;
            }
        }

        for(int x =0; x < Width; x++) {
            for(int y =0; y < Height; y++) {
                Visited[x, y] = false;
            }
        }
    }

    protected Point2 GetWall(Point2 p, Direction d) {
        switch(d) {
            case Direction.Up:
                return new Point2(p.X, p.Y * 2);
            case Direction.Down:
                return new Point2(p.X, p.Y * 2 + 2);
            case Direction.Left:
                return new Point2(p.X, p.Y * 2 + 1);
            case Direction.Right:
                return new Point2(p.X + 1, p.Y * 2 + 1);
            default:
                throw new ArgumentOutOfRangeException(nameof(d));
        }
    }

    protected Point2 GetDirectionPoint2(Direction d) {
        switch(d) {
            case Direction.Up:
                return -Point2.UnitY;
            case Direction.Down:
                return Point2.UnitY;
            case Direction.Left:
                return -Point2.UnitX;
            case Direction.Right:
                return Point2.UnitX;
            default:
                throw new ArgumentOutOfRangeException(nameof(d));
        }
    }

    protected bool IsPathPossible(Point2 p, Direction d, bool revisit) {
        Point2 wall = GetWall(p, d);
        Point2 dest = p + GetDirectionPoint2(d);

        if(dest.X < 0 || dest.X >= Width || dest.Y < 0 || dest.Y >= Height)
            return false;

        if(Field[wall.X, wall.Y] != Wall && !revisit)
            return false;

        if(Visited[dest.X, dest.Y])
            return false;

        return true;
    }

    protected Direction[] GetPossibleDirections(Point2 p, bool revisit) {
        return Directions.Where(d => IsPathPossible(p, d, revisit)).ToArray();
    }

    public void Generate(int seed) {
        Random random = new Random(seed);
        Point2 Pos = Point2.One;
        Stack<Point2> History = new Stack<Point2>();

        Visited[Pos.X, Pos.Y] = true;

        while(true) {
            Direction[] PossibleDirections = GetPossibleDirections(Pos, false);

            if(!PossibleDirections.Any()) {
                if(!History.Any()) {
                    break;
                } else {
                    Pos = History.Pop();
                }
                continue;
            }

            Direction ChosenDirection = PossibleDirections[random.Next(0, PossibleDirections.Count())];
            Point2 Wall = GetWall(Pos, ChosenDirection);

            Pos += GetDirectionPoint2(ChosenDirection);
            Field[Wall.X, Wall.Y] = Space;
            Visited[Pos.X, Pos.Y] = true;
            History.Push(Pos);
        }
    }

    protected void DrawWall(Point2 p, Direction d, BinaryImage img) {
        Point2 Wall = GetWall(p, d);

        if(!Field[Wall.X, Wall.Y])
            return;

        bool IsLineHorizontal = Wall.Y % 2 == 0;

        Point2 a = new Point2() {
            X = Wall.X * Scale - 1,
            Y = (Wall.Y - (Wall.Y % 2)) / 2 * Scale -1
        };
        if(a.X < 0) a.X = 0;
        if(a.Y < 0) a.Y = 0;

        Point2 b = a;

        if(IsLineHorizontal)
            b.X += Scale - 1;
        else
            b.Y += Scale - 1;

        img.Line(a, b, true);
    }

    public BinaryImage Export() {
        BinaryImage result = new BinaryImage(Width * Scale, Height * Scale);

        result.CheckerBoard(Point2.Zero, Point2.One * Scale);
        result.CheckerBoard(new Point2((Width - 1) * Scale, (Height - 1) * Scale), Point2.One * (Scale - 1));

        for(int y =0; y < Height; y++) {
            DrawWall(new Point2(0, y), Direction.Left, result);
            for(int x =0; x < Width; x++) {
                Point2 Pos = new Point2(x, y);
                if(y == 0)
                    DrawWall(Pos, Direction.Up, result);
                DrawWall(Pos, Direction.Down, result);
                DrawWall(Pos, Direction.Right, result);
            }
        }

        return result;
    }

    public void Print() {
        // First line (the specialized spacing is because of the missing | when making individual rows instead of pairs)
        Console.Write(' ');
        for(int x =0; x < Width; x++) {
            Point2 UpWall = GetWall(new Point2(x, 0), Direction.Up);
            if(Field[UpWall.X, UpWall.Y])
                Console.Write('_');
            else Console.Write(' ');
            Console.Write(' ');
        }
        Console.WriteLine();

        for(int y =0; y < Height; y++) {
            // First column
            Point2 LeftWall = GetWall(new Point2(0, y), Direction.Left);
            if(Field[LeftWall.X, LeftWall.Y])
                Console.Write('|');
            else Console.Write(' ');

            for(int x =0; x < Width; x++) {
                Point2 DownWall = GetWall(new Point2(x, y), Direction.Down);
                Point2 RightWall = GetWall(new Point2(x, y), Direction.Right);
                if(Field[DownWall.X, DownWall.Y])
                    Console.Write('_');
                else Console.Write(' ');

                if(Field[RightWall.X, RightWall.Y])
                    Console.Write('|');
                else Console.Write(' ');
            }
            Console.WriteLine();
        }

        // Line feed
        for(int i =0; i < 4; i++)
            Console.WriteLine();
        // Cut receipt paper
        Console.Write("\x1d\x56\x00");
    }
}

class Program {
    const string HelpString = @"ReceiptMaze v 0.0.1
usage: receiptmaze [--width/-w maze_width] [--height/-h maze_height] [--scale/-s maze_image_scale] [--seed maze_gen_seed] [--revisit/-r] [--help]

--help                 Get Help
--width/-w <number>    Width of the maze grid (not of the resulting image) [default = 42]
--height/-h <number>   Height of the maze grid (not of the resulting image) [default = 64]
--scale/-s <number>    Pixels per maze grid cell [default = 13]
--seed <number>        Seed for maze generation (makes for reproducible mazes) [default = current unix ms]
--revisit/-r           Allows revisiting cells, leading to imperfect mazes and mazes with loops [default = false; flag by itself enables it]
--ppm4/-p              Enables netpbm output instead of ESC/POS for receipt printers [default = escpos; flag by itself makes it netpbm]
";
    private static void ParseIntArgument(in string[] args, ref int i, ref int ParameterValue, string ParameterName) {
        if(++i >= args.Length || !int.TryParse(args[i], out ParameterValue))
            throw new ArgumentException($"{ParameterName} value was either not provided or was the wrong type, use e.g., --{ParameterName} 30");
    }

    public static int Main(string[] args) {
        int
            Width = 42,
            Height = 64,
            Scale = 13,
            Seed = (int)DateTimeOffset.Now.ToUnixTimeMilliseconds();

        bool Revisiting = false;
        bool EscPos = true;

        try {
        for(int i =0; i < args.Length; i++) {
            switch(args[i]) {
                case "--width" or "-w":
                    ParseIntArgument(in args, ref i, ref Width, "width");
                    break;
                case "--height" or "-h":
                    ParseIntArgument(in args, ref i, ref Height, "height");
                    break;
                case "--scale" or "-s":
                    ParseIntArgument(in args, ref i, ref Scale, "scale");
                    break;
                case "--seed":
                    ParseIntArgument(in args, ref i, ref Seed, "seed");
                    break;
                case "--revisit" or "-r":
                    Revisiting = true;
                    break;
                case "--ppm4" or "-p":
                    EscPos = false;
                    break;
                case "--help" or "-h":
                    Console.Error.WriteLine(HelpString);
                    return 0;
                default:
                    throw new InvalidOperationException($"Unknown option '{args[i]}', --help to get the usage.");
            }
        }
        } catch (Exception e) when (e is ArgumentException || e is InvalidOperationException) {
            // Lets not mess up regular stdout since its possibly being piped to a printer.
            Console.Error.WriteLine(e.Message);
            return 1;
        }

        Console.WriteLine($"Maze: w:{Width} h:{Height} s:{Scale} time: {DateTime.Now.ToString("MM/dd/yyyy hh:mm tt")}");
        MazeGen mazeGen = new MazeGen(Width, Height, Scale, Revisiting);
        mazeGen.Generate(Seed);
        mazeGen.Export().Export(EscPos);
        return 0;
    }
}
