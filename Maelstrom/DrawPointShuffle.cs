using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF8Mod.Maelstrom
{
    public class DrawPointShuffle
    {
        // assign random spells to each draw point, retaining their other properties
        public static BinaryPatch Patch
        {
            get
            {
                var random = new Random();
                var spells = (int[])Enum.GetValues(typeof(DrawPoint.Magic));

                for (int i = 0; i < DrawPoint.DrawPointDefsLength; i++)
                {
                    var drawPoint = new DrawPoint(DrawPoint.OriginalData[i]);
                    drawPoint.Spell = (DrawPoint.Magic)spells[random.Next(spells.Length)];
                    DrawPoint.UpdatedDrawPoints[i + 1] = drawPoint;
                }

                return new BinaryPatch(Properties.Settings.Default.GameLocation, DrawPoint.DrawPointDefsLocation, DrawPoint.OriginalData, DrawPoint.CurrentData);
            }
        }
    }
}
