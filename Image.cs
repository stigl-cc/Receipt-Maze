class BinaryImage {
    // Decides if export format should be PPM4(false) or ESC/POS (true)
    public static bool EscPos = true;

    public int Width, Height;
    bool[,] Data;

    public BinaryImage(int Width, int Height) {
        this.Width = Width;
        this.Height = Height;
        Data = new bool[Width, Height];
    }

    public void Export() {
        Stream stream = Console.OpenStandardOutput();

        if(EscPos) {
            byte[] image = { 0x1D, 0x76, 0x30, 0x00 };
            stream.Write(image, 0, image.Length);

            ushort headerX = (ushort)((Width + 7) / 8);
            ushort headerY = (ushort)Height;

            byte[] sizeheader = {
                (byte)((headerX >> 0) & 0xff),
                (byte)((headerX >> 8) & 0xff),
                (byte)((headerY >> 0) & 0xff),
                (byte)((headerY >> 8) & 0xff),
            };
            stream.Write(sizeheader, 0, sizeheader.Length);
        }
        else {
            Console.Write($"P4\n{Width} {Height}\n");
        }


        int i =0;
        byte temp = 0x00;
        for(int y =0; y < Height; y++) {
            for(int x =0; x < Width; x++) {
                temp <<= 1;
                temp |= (byte)((Data[x, y]) ? 0x01 : 0x00);
                i++;

                if(i == 8) {
                    stream.Write(new byte[]{temp}, 0, 1);
                    i = 0;
                    temp = 0;
                }
            }

            // In case the bitcount isnt % 8 == 0
            if(i != 0) {
                temp <<= 8 - i;
                stream.Write(new byte[]{ temp }, 0, 1);
                temp = 0;
                i = 0;
            }
        }


        if(EscPos) {
            for(int n =0; n < 5; n++) {
                byte[] linefeed = { 0x0a };
                stream.Write(linefeed, 0, linefeed.Length);
            }

            byte[] cut = { 0x1d, 0x56, 0x00};
            stream.Write(cut, 0, cut.Length);
        }

        stream.Close();
    }

    public void Pixel(Point2 p, int radius, bool val) {
        int range = (radius - 1) / 2;
        for(int x = (p.X >= range)? -range : 0; x < ((p.X < Width - range)? 1 + range : 1); x++) {
            for(int y = (p.Y >= range)? -range : 0; y < ((p.Y < Height - range)? 1 + range : 1); y++) {
                Data[p.X + x, p.Y + y] = val;
            }
        }
    }

    public void Line(Point2 a, Point2 b, bool val = true) {
        int
            dx = +Math.Abs(b.X - a.X), sx = a.X < b.X ? 1 : -1,
            dy = -Math.Abs(b.Y - a.Y), sy = a.Y < b.Y ? 1 : -1;

        int err = dx + dy, e2;

        while(a != b) {
            Pixel(a, 3, val);

            e2 = 2 * err;
            if(e2 >= dy) { err += dy; a.X += sx; }
            if(e2 <= dx) { err += dx; a.Y += sy; }
        }

        Pixel(a, 5, val);
    }

    public void CheckerBoard(Point2 pos, Point2 size) {
        for(int x =0; x < size.X; x++) {
            for(int y =0; y < size.Y; y++) {
                bool color = ((x % 2) ^ (y % 2)) == 1;
                Data[pos.X + x, pos.Y + y] = color;
            }
        }
    }
}
