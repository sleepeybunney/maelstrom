using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FF8Mod
{
    public class Encounter
    {
        public byte Scene;
        public bool NoEscape, NoVictorySequence, ShowTimer, NoExp, NoResults, StruckFirst, BackAttack, ScriptedBattle;
        public byte MainCamera, MainCameraAnimation, SecondaryCamera, SecondaryCameraAnimation;
        public EncounterSlot[] Slots;

        public Encounter()
        {
            Scene = 0;
            NoEscape = false;
            NoVictorySequence = false;
            ShowTimer = false;
            NoExp = false;
            NoResults = false;
            StruckFirst = false;
            BackAttack = false;
            ScriptedBattle = false;
            MainCamera = 0;
            MainCameraAnimation = 0;
            SecondaryCamera = 0;
            SecondaryCameraAnimation = 0;
            Slots = new EncounterSlot[8];
            for (int i = 0; i < 8; i++) Slots[i] = new EncounterSlot();
        }

        public static Encounter FromBytes(int id, byte[] data)
        {
            var result = new Encounter();

            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream))
            {
                stream.Seek(id * 128, SeekOrigin.Begin);
                result.Scene = reader.ReadByte();

                var flags = reader.ReadByte();
                result.NoEscape = (flags & 1) != 0;
                result.NoVictorySequence = (flags & 2) != 0;
                result.ShowTimer = (flags & 4) != 0;
                result.NoExp = (flags & 8) != 0;
                result.NoResults = (flags & 16) != 0;
                result.StruckFirst = (flags & 32) != 0;
                result.BackAttack = (flags & 64) != 0;
                result.ScriptedBattle = (flags & 128) != 0;

                var mainCamera = reader.ReadByte();
                result.MainCamera = (byte)(mainCamera >> 4);
                result.MainCameraAnimation = (byte)(mainCamera & 0x0F);

                var secondaryCamera = reader.ReadByte();
                result.SecondaryCamera = (byte)(secondaryCamera >> 4);
                result.SecondaryCameraAnimation = (byte)(secondaryCamera & 0x0F);

                var hidden = reader.ReadByte();
                var unloaded = reader.ReadByte();
                var untargetable = reader.ReadByte();
                var enabled = reader.ReadByte();

                for (int i = 0; i < 8; i++)
                {
                    result.Slots[i].Hidden = ((hidden >> (7 - i)) & 1) != 0;
                    result.Slots[i].Unloaded = ((unloaded >> (7 - i)) & 1) != 0;
                    result.Slots[i].Untargetable = ((untargetable >> (7 - i)) & 1) != 0;
                    result.Slots[i].Enabled = ((enabled >> (7 - i)) & 1) != 0;
                    result.Slots[i].Position = new EncounterSlot.Coords(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16());
                }

                foreach (var s in result.Slots) s.MonsterID = reader.ReadByte();
                foreach (var s in result.Slots) s.Unknown1 = reader.ReadInt16();
                foreach (var s in result.Slots) s.Unknown2 = reader.ReadInt16();
                foreach (var s in result.Slots) s.Unknown3 = reader.ReadInt16();
                foreach (var s in result.Slots) s.Unknown4 = reader.ReadByte();
                foreach (var s in result.Slots) s.Level = reader.ReadByte();
            }

            return result;
        }

        public static Encounter FromFile(int id, string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Encounter file not found");
            }

            return FromBytes(id, File.ReadAllBytes(path));
        }

        public static Encounter FromSource(int id, FileSource source, string path)
        {
            return FromBytes(id, source.GetFile(path));
        }
    }
}
