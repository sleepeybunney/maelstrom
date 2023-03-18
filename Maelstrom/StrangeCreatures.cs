using Sleepey.FF8Mod;
using Sleepey.FF8Mod.Archive;
using Sleepey.FF8Mod.Battle;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Sleepey.Maelstrom
{
    public static class StrangeCreatures
    {
        public static void Apply(FileSource battleSource, int seed)
        {
            var random = new Random(seed + 13);

            int sphinxHue = 0;
            double sphinxSize = 1;
            double sphinxWeight = 1;

            for (int monsterID = 0; monsterID < 144; monsterID++)
            {
                var hue = random.Next(360);
                var size = (random.NextDouble() * 2.0) + 0.2;
                var weight = (random.NextDouble() * 1.2) + 0.6;

                // sphinxaur & sphinxara should match
                if (monsterID == 11)
                {
                    sphinxHue = hue;
                    sphinxSize = size;
                    sphinxWeight = weight;
                }
                else if (monsterID == 12)
                {
                    hue = sphinxHue;
                    size = sphinxSize;
                    weight = sphinxWeight;
                }

                Monster monster;
                try
                {
                    monster = Monster.ByID(battleSource, monsterID);

                    // change texture colours
                    if (monster.Textures != null)
                    {
                        foreach (var texture in monster.Textures.Textures)
                        {
                            foreach (var colour in texture.Clut.Data)
                            {
                                var shifted = ShiftHue(colour.Red, colour.Green, colour.Blue, hue, 31);
                                colour.Red = shifted.R;
                                colour.Green = shifted.G;
                                colour.Blue = shifted.B;
                            }
                        }
                    }

                    // change remastered texture colours
                    if (Env.Remastered && Directory.Exists(Env.HDTexturePath))
                    {
                        var searchPath = Path.Combine(Env.HDTexturePath, Path.GetFileNameWithoutExtension(Monster.GetPath(monsterID)));
                        var textures = Directory.GetFiles(Env.HDTexturePath).Where(x => x.StartsWith(searchPath) && x.EndsWith(".png"));

                        foreach (var texFile in textures)
                        {
                            try
                            {
                                if (!File.Exists(texFile + ".bak")) File.Copy(texFile, texFile + ".bak");
                                else File.Copy(texFile + ".bak", texFile, true);

                                var png = File.ReadAllBytes(texFile);

                                using (var stream = new MemoryStream(png))
                                using (var bmp = (Bitmap)Image.FromStream(stream))
                                {
                                    var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);
                                    var bpp = Bitmap.GetPixelFormatSize(bmp.PixelFormat);
                                    var ptr = bmpData.Scan0;
                                    var numBytes = bmpData.Stride * bmp.Height;
                                    var bytes = new byte[numBytes];
                                    Marshal.Copy(ptr, bytes, 0, numBytes);

                                    var inc = 3;
                                    if (bpp == 32) inc = 4;

                                    for (int i = 0; i < numBytes; i += inc)
                                    {
                                        var shifted = ShiftHue(bytes[i + 2], bytes[i + 1], bytes[i], hue);
                                        bytes[i + 2] = shifted.R;
                                        bytes[i + 1] = shifted.G;
                                        bytes[i] = shifted.B;
                                    }

                                    Marshal.Copy(bytes, 0, ptr, numBytes);
                                    bmp.UnlockBits(bmpData);

                                    bmp.Save(texFile, ImageFormat.Png);
                                }
                            }
                            catch { }
                        }
                    }

                    // expand or contract mesh
                    if (monster.Mesh != null)
                    {
                        foreach (var obj in monster.Mesh.Objects)
                        {
                            foreach (var group in obj.Groups)
                            {
                                foreach (var vertex in group.Vertices)
                                {
                                    vertex.X = (short)(vertex.X * weight);
                                    vertex.Y = (short)(vertex.Y * weight);
                                    vertex.Z = (short)(vertex.Z * weight);
                                }
                            }
                        }
                    }

                    // resize skeleton
                    if (monster.Skeleton != null)
                    {
                        monster.Skeleton.Size = (short)(monster.Skeleton.Size * size);
                    }

                    battleSource.ReplaceFile(Monster.GetPath(monsterID), monster.Encode());
                }
                catch { }
            }
        }

        public static void ResetTextures(FileSource battleSource)
        {
            if (!(Env.Remastered && Directory.Exists(Env.HDTexturePath))) return;

            var backups = Directory.GetFiles(Env.HDTexturePath).Where(x => x.EndsWith(".bak"));
            foreach (var backup in backups)
            {
                File.Copy(backup, backup.Substring(0, backup.Length - 4), true);
            }
        }

        public static Color ShiftHue(byte r, byte g, byte b, float degrees, int maxValue = byte.MaxValue)
        {
            var theta = degrees / 180 * Math.PI;
            var cos = (float)Math.Cos(theta);
            var sin = (float)Math.Sin(theta);

            // hue rotation matrix from chromium project
            var m = new float[3, 3];

            m[0, 0] = 0.213f + 0.787f * cos - 0.213f * sin;
            m[0, 1] = 0.213f - 0.213f * cos + 0.143f * sin;
            m[0, 2] = 0.213f - 0.213f * cos - 0.787f * sin;

            m[1, 0] = 0.715f - 0.715f * cos - 0.715f * sin;
            m[1, 1] = 0.715f + 0.285f * cos + 0.140f * sin;
            m[1, 2] = 0.715f - 0.715f * cos + 0.715f * sin;

            m[2, 0] = 0.072f - 0.072f * cos + 0.928f * sin;
            m[2, 1] = 0.072f - 0.072f * cos - 0.283f * sin;
            m[2, 2] = 0.072f + 0.928f * cos + 0.072f * sin;

            var tr = r * m[0, 0] + g * m[1, 0] + b * m[2, 0];
            var tg = r * m[0, 1] + g * m[1, 1] + b * m[2, 1];
            var tb = r * m[0, 2] + g * m[1, 2] + b * m[2, 2];

            tr = Math.Max(0, Math.Min(maxValue, (float)Math.Round(tr, MidpointRounding.AwayFromZero)));
            tg = Math.Max(0, Math.Min(maxValue, (float)Math.Round(tg, MidpointRounding.AwayFromZero)));
            tb = Math.Max(0, Math.Min(maxValue, (float)Math.Round(tb, MidpointRounding.AwayFromZero)));

            return Color.FromArgb((int)tr, (int)tg, (int)tb);
        }
    }
}
