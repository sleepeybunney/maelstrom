using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Sleepey.FF8Mod
{
    public class Tim
    {
        public byte ClutMode { get; set; } = 0;
        public bool ClutPresent { get; set; } = false;
        public Clut Clut { get; set; }
        public IEnumerable<byte> ImageData { get; set; } = new List<byte>();

        public Tim(byte mode, IEnumerable<byte> data)
        {
            if (mode > 3) throw new ArgumentOutOfRangeException("Invalid CLUT/BPP mode");
            ClutMode = mode;
            ClutPresent = false;
            ImageData = data;
        }

        public Tim(byte mode, Clut clut, IEnumerable<byte> data) : this(mode, data)
        {
            ClutPresent = true;
            Clut = clut;
        }

        public Tim(IEnumerable<byte> data)
        {
            using (var stream = new MemoryStream(data.ToArray()))
            using (var reader = new BinaryReader(stream))
            {
                stream.Seek(4, SeekOrigin.Begin);

                var flags = reader.ReadByte();
                ClutMode = (byte)(flags & 3);
                ClutPresent = (flags >> 3 & 1) == 1;
                stream.Seek(3, SeekOrigin.Current);

                if (ClutPresent)
                {
                    var clutLength = reader.ReadUInt32();
                    stream.Seek(-4, SeekOrigin.Current);
                    Clut = new Clut(reader.ReadBytes((int)clutLength));
                }

                var imageLength = reader.ReadUInt32();
                stream.Seek(-4, SeekOrigin.Current);
                ImageData = reader.ReadBytes((int)imageLength);
            }
        }

        public IEnumerable<byte> Encode()
        {
            var result = new List<byte>();
            result.AddRange(new byte[] { 16, 0, 0, 0 });

            var flags = ClutMode;
            if (ClutPresent) flags += 8;
            result.AddRange(new byte[] { flags, 0, 0, 0 });

            if (ClutPresent) result.AddRange(Clut.Encode());

            result.AddRange(ImageData);

            return result;
        }
    }

    public class Clut
    {
        public ushort X { get; set; } = 0;
        public ushort Y { get; set; } = 0;
        public ushort Width { get; } = 16;
        public ushort Height { get; } = 16;

        public Colour[,] Data;

        public Clut(ushort x, ushort y, ushort width, ushort height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Data = new Colour[Height, Width];
        }

        public Clut(IEnumerable<byte> data)
        {
            using (var stream = new MemoryStream(data.ToArray()))
            using (var reader = new BinaryReader(stream))
            {
                stream.Seek(4, SeekOrigin.Begin);
                X = reader.ReadUInt16();
                Y = reader.ReadUInt16();
                Width = reader.ReadUInt16();
                Height = reader.ReadUInt16();
                Data = new Colour[Height, Width];

                for (int row = 0; row < Height; row++)
                {
                    for (int col = 0; col < Width; col++)
                    {
                        Data[row, col] = new Colour(reader.ReadUInt16());
                    }
                }
            }
        }

        public IEnumerable<byte> Encode()
        {
            var result = new List<byte>();
            
            // total length
            result.AddRange(BitConverter.GetBytes(12 + Width * Height * 2));

            result.AddRange(BitConverter.GetBytes(X));
            result.AddRange(BitConverter.GetBytes(Y));
            result.AddRange(BitConverter.GetBytes(Width));
            result.AddRange(BitConverter.GetBytes(Height));

            for (int row = 0; row < Height; row++)
            {
                for (int col = 0; col < Width; col++)
                {
                    result.AddRange(BitConverter.GetBytes(Data[row, col].Encode()));
                }
            }

            return result;
        }
    }

    public class Colour
    {
        private byte r, g, b;

        public byte Red
        {
            get { return r; }
            set { r = Math.Min(value, (byte)31); }
        }

        public byte Green
        {
            get { return g; }
            set { g = Math.Min(value, (byte)31); }
        }

        public byte Blue
        {
            get { return b; }
            set { b = Math.Min(value, (byte)31); }
        }

        public bool STP { get; set; } = false;

        public Colour(byte red, byte green, byte blue, bool stp)
        {
            Red = red;
            Green = green;
            Blue = blue;
            STP = stp;
        }

        public Colour(ushort data)
        {
            Red = (byte)(data & 31);
            Green = (byte)(data >> 5 & 31);
            Blue = (byte)(data >> 10 & 31);
            STP = data >> 15 == 1;
        }

        public ushort Encode()
        {
            ushort result = Red;
            result += (ushort)(Green << 5);
            result += (ushort)(Blue << 10);
            if (STP) result += (1 << 15);
            return result;
        }
    }
}
