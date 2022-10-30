using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sleepey.FF8Mod.Main
{
    public class Kernel
    {
        public uint SectionCount { get; set; }
        public IList<uint> SectionOffsets { get; set; }
        public IList<BattleCommand> BattleCommands { get; set; } = new List<BattleCommand>();
        public IList<Spell> MagicData { get; set; } = new List<Spell>();
        public IList<JunctionableGF> JunctionableGFs { get; set; } = new List<JunctionableGF>();
        public IList<EnemyAttack> EnemyAttackData { get; set; } = new List<EnemyAttack>();
        public IList<Weapon> Weapons { get; set; } = new List<Weapon>();
        public IList<Ability> Abilities { get; set; } = new List<Ability>();
        public IList<byte> MagicText { get; set; }
        public IList<byte> JunctionableGFText { get; set; }
        public IList<byte> EnemyAttackText { get; set; }
        public IList<byte> WeaponText { get; set; }

        private readonly byte[] PostWeaponData, PostAbilityData, PostWeaponTextData;

        public Kernel(Stream stream)
        {
            using (var reader = new BinaryReader(stream))
            {
                // header
                SectionCount = reader.ReadUInt32();
                SectionOffsets = new List<uint>();

                for (int i = 0; i < SectionCount; i++)
                {
                    SectionOffsets.Add(reader.ReadUInt32());
                }

                // section 0 = battle commands
                for (int i = 0; i < 39; i++)
                {
                    BattleCommands.Add(new BattleCommand(reader.ReadBytes(8)));
                }

                // section 1 = magic data
                for (int i = 0; i < 57; i++)
                {
                    MagicData.Add(new Spell(reader.ReadBytes(60)));
                }

                // section 2 = junctionable gf
                for (int i = 0; i < 16; i++)
                {
                    JunctionableGFs.Add(new JunctionableGF(reader.ReadBytes(132)));
                }

                // section 3 = enemy attacks
                for (int i = 0; i < 384; i++)
                {
                    EnemyAttackData.Add(new EnemyAttack(reader.ReadBytes(20)));
                }

                // section 4 = weapons
                for (int i = 0; i < 33; i++)
                {
                    Weapons.Add(new Weapon(reader.ReadBytes(12)));
                }

                //sections 5-10
                PostWeaponData = reader.ReadBytes((int)(SectionOffsets[11] - stream.Position));

                // sections 11-17 = abilities
                while (SectionOffsets[18] - stream.Position >= 8)
                {
                    Abilities.Add(new Ability(reader.ReadBytes(8)));
                }

                // sections 18-31
                PostAbilityData = reader.ReadBytes((int)(SectionOffsets[32] - stream.Position));

                // section 32 = magic text
                MagicText = reader.ReadBytes((int)(SectionOffsets[33] - stream.Position));
                foreach (var m in MagicData) m.Name = FF8String.Decode(MagicText.Skip(m.NameOffset));

                // section 33 = junctionable gf text
                JunctionableGFText = reader.ReadBytes((int)(SectionOffsets[34] - stream.Position));

                // section 34 = enemy attack text
                EnemyAttackText = reader.ReadBytes((int)(SectionOffsets[35] - stream.Position));
                foreach (var a in EnemyAttackData) a.Name = FF8String.Decode(EnemyAttackText.Skip(a.NameOffset));

                // section 35 = weapon text
                WeaponText = reader.ReadBytes((int)(SectionOffsets[36] - stream.Position));
                foreach (var w in Weapons) w.Name = FF8String.Decode(WeaponText.Skip(w.NameOffset));

                // sections 36-55
                PostWeaponTextData = reader.ReadBytes((int)(stream.Length - stream.Position));
            }
        }

        public Kernel(IEnumerable<byte> data) : this(new MemoryStream(data.ToArray())) { }

        public IEnumerable<byte> Encode()
        {
            var result = new List<byte>();
            result.AddRange(BitConverter.GetBytes(SectionCount));
            foreach (var offset in SectionOffsets) result.AddRange(BitConverter.GetBytes(offset));
            foreach (var cmd in BattleCommands) result.AddRange(cmd.Encode());
            foreach (var mag in MagicData) result.AddRange(mag.Encode());
            foreach (var gf in JunctionableGFs) result.AddRange(gf.Encode());
            foreach (var ea in EnemyAttackData) result.AddRange(ea.Encode());
            foreach (var w in Weapons) result.AddRange(w.Encode());
            result.AddRange(PostWeaponData);
            foreach (var a in Abilities) result.AddRange(a.Encode());
            result.AddRange(PostAbilityData);
            result.AddRange(MagicText);
            result.AddRange(JunctionableGFText);
            result.AddRange(EnemyAttackText);
            result.AddRange(WeaponText);
            result.AddRange(PostWeaponTextData);
            return result;
        }
    }
}
