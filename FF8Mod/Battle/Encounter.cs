using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Sleepey.FF8Mod.Battle
{
    public class Encounter
    {
        public byte Scene { get; set; }
        public bool NoEscape { get; set; }
        public bool NoVictorySequence { get; set; }
        public bool ShowTimer { get; set; }
        public bool NoExp { get; set; }
        public bool NoResults { get; set; }
        public bool StruckFirst { get; set; }
        public bool BackAttack { get; set; }
        public bool ScriptedBattle { get; set; }
        public byte MainCamera { get; set; }
        public byte MainCameraAnimation { get; set; }
        public byte SecondaryCamera { get; set; }
        public byte SecondaryCameraAnimation { get; set; }
        public List<EncounterSlot> Slots { get; } = new List<EncounterSlot>(Enumerable.Range(0, 8).Select(i => new EncounterSlot()));

        public Encounter() { }

        public Encounter(IEnumerable<byte> data)
        {
            using (var stream = new MemoryStream(data.ToArray()))
            using (var reader = new BinaryReader(stream))
            {
                Scene = reader.ReadByte();

                var flags = reader.ReadByte();
                NoEscape = (flags & 1) != 0;
                NoVictorySequence = (flags & 2) != 0;
                ShowTimer = (flags & 4) != 0;
                NoExp = (flags & 8) != 0;
                NoResults = (flags & 16) != 0;
                StruckFirst = (flags & 32) != 0;
                BackAttack = (flags & 64) != 0;
                ScriptedBattle = (flags & 128) != 0;

                var mainCamera = reader.ReadByte();
                MainCamera = (byte)(mainCamera >> 4);
                MainCameraAnimation = (byte)(mainCamera & 0x0F);

                var secondaryCamera = reader.ReadByte();
                SecondaryCamera = (byte)(secondaryCamera >> 4);
                SecondaryCameraAnimation = (byte)(secondaryCamera & 0x0F);

                var hidden = reader.ReadByte();
                var unloaded = reader.ReadByte();
                var untargetable = reader.ReadByte();
                var enabled = reader.ReadByte();

                for (int i = 0; i < 8; i++)
                {
                    // shift right until the relevant bit is LSB & check that
                    Slots[i].Hidden = ((hidden >> (7 - i)) & 1) != 0;
                    Slots[i].Unloaded = ((unloaded >> (7 - i)) & 1) != 0;
                    Slots[i].Untargetable = ((untargetable >> (7 - i)) & 1) != 0;
                    Slots[i].Enabled = ((enabled >> (7 - i)) & 1) != 0;
                    Slots[i].Position = new Coords(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16());
                }

                foreach (var s in Slots) s.MonsterID = (byte)(reader.ReadByte() - 0x10);
                foreach (var s in Slots) s.Unknown1 = reader.ReadInt16();
                foreach (var s in Slots) s.Unknown2 = reader.ReadInt16();
                foreach (var s in Slots) s.Unknown3 = reader.ReadInt16();
                foreach (var s in Slots) s.Unknown4 = reader.ReadByte();
                foreach (var s in Slots) s.Level = reader.ReadByte();
            }
        }

        public IEnumerable<byte> Encode()
        {
            var result = new byte[128];

            using (var stream = new MemoryStream(result))
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(Scene);

                byte flags = 0;
                if (NoEscape) flags += 1;
                if (NoVictorySequence) flags += 2;
                if (ShowTimer) flags += 4;
                if (NoExp) flags += 8;
                if (NoResults) flags += 16;
                if (StruckFirst) flags += 32;
                if (BackAttack) flags += 64;
                if (ScriptedBattle) flags += 128;
                writer.Write(flags);

                byte mainCamera = (byte)(MainCamera << 4);
                mainCamera += MainCameraAnimation;
                writer.Write(mainCamera);

                byte secondaryCamera = (byte)(SecondaryCamera << 4);
                secondaryCamera += SecondaryCameraAnimation;
                writer.Write(secondaryCamera);

                byte hidden = 0;
                byte unloaded = 0;
                byte untargetable = 0;
                byte enabled = 0;

                for (int i = 0; i < 8; i++)
                {
                    if (Slots[i].Hidden) hidden++;
                    if (i < 7) hidden <<= 1;

                    if (Slots[i].Unloaded) unloaded++;
                    if (i < 7) unloaded <<= 1;

                    if (Slots[i].Untargetable) untargetable++;
                    if (i < 7) untargetable <<= 1;

                    if (Slots[i].Enabled) enabled++;
                    if (i < 7) enabled <<= 1;
                }

                writer.Write(hidden);
                writer.Write(unloaded);
                writer.Write(untargetable);
                writer.Write(enabled);

                for (int i = 0; i < 8; i++)
                {
                    writer.Write(Slots[i].Position.X);
                    writer.Write(Slots[i].Position.Y);
                    writer.Write(Slots[i].Position.Z);
                }

                foreach (var s in Slots) writer.Write((byte)(s.MonsterID + 0x10));
                foreach (var s in Slots) writer.Write(s.Unknown1);
                foreach (var s in Slots) writer.Write(s.Unknown2);
                foreach (var s in Slots) writer.Write(s.Unknown3);
                foreach (var s in Slots) writer.Write(s.Unknown4);
                foreach (var s in Slots) writer.Write(s.Level);
            }

            return result;
        }
    }
}
