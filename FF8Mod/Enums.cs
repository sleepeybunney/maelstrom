using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sleepey.FF8Mod
{
    public enum ArchiveType
    {
        FS,
        ZZZ
    }

    public enum ArgType
    {
        Byte,
        Short,
        Bool
    }

    public enum MonsterSectionIndex
    {
        Skeleton = 1,
        Mesh = 2,
        Animation = 3,
        Section4 = 4,
        Section5 = 5,
        Section6 = 6,
        Info = 7,
        AI = 8,
        Sounds = 9,
        Section10 = 10,
        Textures = 11
    }

    public enum EntityType
    {
        Line,
        Door,
        Background,
        Other
    }

    public enum AbilityType
    {
        None = 0,
        Magic = 2,
        Item = 4,
        Unique = 8
    }

    public enum SpellTarget
    {
        EnemySingle = 0xc9,
        FriendSingle = 0xca,
        EnemyAll = 0xcc,
        FriendAll = 0xcd,
        EnemyRandom = 0xd0
    }
}
