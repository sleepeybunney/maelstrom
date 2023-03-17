using Sleepey.FF8Mod.Archive;
using Sleepey.FF8Mod.Battle;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

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

            for (int i = 0; i < 144; i++)
            {
                var hue = random.Next(360);
                var size = (random.NextDouble() * 2.2) + 0.2;
                var weight = (random.NextDouble() * 0.8) + 0.6;

                // sphinxaur & sphinxara should match
                if (i == 11)
                {
                    sphinxHue = hue;
                    sphinxSize = size;
                    sphinxWeight = weight;
                }
                else if (i == 12)
                {
                    hue = sphinxHue;
                    size = sphinxSize;
                    weight = sphinxWeight;
                }

                Monster monster;
                try
                {
                    monster = Monster.ByID(battleSource, i);

                    Debug.WriteLine("{0} - h={1}/s={2}/w={3}", monster.Info.Name, hue, size, weight);

                    if (monster.Textures != null)
                    {
                        foreach (var texture in monster.Textures.Textures)
                        {
                            foreach (var colour in texture.Clut.Data)
                            {
                                colour.ShiftHue(hue);
                            }
                        }
                    }

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

                    if (monster.Skeleton != null)
                    {
                        monster.Skeleton.Size = (short)(monster.Skeleton.Size * size);
                    }

                    battleSource.ReplaceFile(Monster.GetPath(i), monster.Encode());
                }
                catch { }
            }
        }
    }
}
