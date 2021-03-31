using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sleepey.FF8Mod.Field
{
    // header data for scripts in the jsm file
    public class ScriptInfo
    {
        public int Position { get; set; }
        public int Flag { get; set; }

        public ScriptInfo(int position, int flag)
        {
            Position = position;
            Flag = flag;
        }

        public ScriptInfo(ushort data)
        {
            Position = (data & 0x7fff) * 4;
            Flag = data >> 15;
        }

        public ushort Encode()
        {
            return (ushort)((Flag << 15) + Position / 4);
        }
    }
}
