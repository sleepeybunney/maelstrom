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
        public IList<BattleItem> BattleItems { get; set; } = new List<BattleItem>();
        public IList<NonBattleItem> NonBattleItems { get; set; } = new List<NonBattleItem>();
        public IList<Ability> Abilities { get; set; } = new List<Ability>();
        public IList<byte> MagicText { get; set; }
        public IList<byte> JunctionableGFText { get; set; }
        public IList<byte> EnemyAttackText { get; set; }
        public IList<byte> WeaponText { get; set; }
        public IList<byte> BattleItemText { get; set; }
        public IList<byte> NonBattleItemText { get; set; }

        private readonly byte[] PostWeaponData, PostItemData, PostAbilityData, PostWeaponTextData, PostItemTextData;

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

                // sections 5-6
                PostWeaponData = reader.ReadBytes((int)(SectionOffsets[7] - stream.Position));

                // section 7 = battle items
                BattleItems = ReadSection(reader, SectionOffsets[7], SectionOffsets[8], 24).Select(i => new BattleItem(i)).ToList();

                // section 8 = non-battle items
                NonBattleItems = ReadSection(reader, SectionOffsets[8], SectionOffsets[9], 4).Select(i => new NonBattleItem(i)).ToList();

                // sections 9-10
                PostItemData = reader.ReadBytes((int)(SectionOffsets[11] - stream.Position));

                // sections 11-17 = abilities
                Abilities = ReadSection(reader, SectionOffsets[11], SectionOffsets[18], 8).Select(i => new Ability(i)).ToList();

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

                // sections 36-37
                PostWeaponTextData = reader.ReadBytes((int)(SectionOffsets[38] - stream.Position));

                // section 38 = battle item text
                BattleItemText = reader.ReadBytes((int)(SectionOffsets[39] - stream.Position));
                foreach (var bi in BattleItems)
                {
                    bi.Name = FF8String.Decode(BattleItemText.Skip(bi.NameOffset));
                    bi.Description = FF8String.Decode(BattleItemText.Skip(bi.DescriptionOffset));
                }

                // section 39 = non-battle item text
                NonBattleItemText = reader.ReadBytes((int)(SectionOffsets[40] - stream.Position));
                foreach (var nbi in NonBattleItems)
                {
                    nbi.Name = FF8String.Decode(NonBattleItemText.Skip(nbi.NameOffset));
                    nbi.Description = FF8String.Decode(NonBattleItemText.Skip(nbi.DescriptionOffset));
                }

                // sections 40-55
                PostItemTextData = reader.ReadBytes((int)(stream.Length - stream.Position));
            }
        }

        public Kernel(IEnumerable<byte> data) : this(new MemoryStream(data.ToArray())) { }

        public IEnumerable<byte> Encode()
        {
            var result = new List<byte>();

            var encodedText = new List<byte>();
            ushort currentOffset = 0;
            foreach (var bi in BattleItems)
            {
                if (string.IsNullOrEmpty(bi.Name))
                {
                    bi.NameOffset = ushort.MaxValue;
                }
                else
                {
                    bi.NameOffset = currentOffset;
                    var encodedName = FF8String.Encode(bi.Name);
                    encodedText.AddRange(encodedName);
                    currentOffset += (ushort)encodedName.Count();
                }

                if (string.IsNullOrEmpty(bi.Description))
                {
                    bi.DescriptionOffset = ushort.MaxValue;
                }
                else
                {
                    bi.DescriptionOffset = currentOffset;
                    var encodedDesc = FF8String.Encode(bi.Description);
                    encodedText.AddRange(encodedDesc);
                    currentOffset += (ushort)encodedDesc.Count();
                }
            }
            BattleItemText = encodedText;

            var oldOffset = SectionOffsets[39];
            SectionOffsets[39] = SectionOffsets[38] + (uint)BattleItemText.Count;
            var diff = SectionOffsets[39] - oldOffset;
            if (diff != 0)
            {
                for (int i = 40; i < SectionOffsets.Count; i++)
                {
                    SectionOffsets[i] += diff;
                }
            }

            encodedText = new List<byte>();
            currentOffset = 0;
            foreach (var nbi in NonBattleItems)
            {
                if (string.IsNullOrEmpty(nbi.Name))
                {
                    nbi.NameOffset = ushort.MaxValue;
                }
                else
                {
                    nbi.NameOffset = currentOffset;
                    var encodedName = FF8String.Encode(nbi.Name);
                    encodedText.AddRange(encodedName);
                    currentOffset += (ushort)encodedName.Count();
                }

                if (string.IsNullOrEmpty(nbi.Description))
                {
                    nbi.DescriptionOffset = ushort.MaxValue;
                }
                else
                {
                    nbi.DescriptionOffset = currentOffset;
                    var encodedDesc = FF8String.Encode(nbi.Description);
                    encodedText.AddRange(encodedDesc);
                    currentOffset += (ushort)encodedDesc.Count();
                }
            }
            NonBattleItemText = encodedText;

            oldOffset = SectionOffsets[40];
            SectionOffsets[40] = SectionOffsets[39] + (uint)NonBattleItemText.Count;
            diff = SectionOffsets[40] - oldOffset;
            if (diff != 0)
            {
                for (int i = 41; i < SectionOffsets.Count; i++)
                {
                    SectionOffsets[i] += diff;
                }
            }

            result.AddRange(BitConverter.GetBytes(SectionCount));
            foreach (var offset in SectionOffsets) result.AddRange(BitConverter.GetBytes(offset));
            foreach (var cmd in BattleCommands) result.AddRange(cmd.Encode());
            foreach (var mag in MagicData) result.AddRange(mag.Encode());
            foreach (var gf in JunctionableGFs) result.AddRange(gf.Encode());
            foreach (var ea in EnemyAttackData) result.AddRange(ea.Encode());
            foreach (var w in Weapons) result.AddRange(w.Encode());
            result.AddRange(PostWeaponData);
            foreach (var bi in BattleItems) result.AddRange(bi.Encode());
            foreach (var nbi in NonBattleItems) result.AddRange(nbi.Encode());
            result.AddRange(PostItemData);
            foreach (var a in Abilities) result.AddRange(a.Encode());
            result.AddRange(PostAbilityData);
            result.AddRange(MagicText);
            result.AddRange(JunctionableGFText);
            result.AddRange(EnemyAttackText);
            result.AddRange(WeaponText);
            result.AddRange(PostWeaponTextData);
            result.AddRange(BattleItemText);
            result.AddRange(NonBattleItemText);
            result.AddRange(PostItemTextData);
            return result;
        }

        private List<byte[]> ReadSection(BinaryReader reader, uint sectionOffset, uint nextSectionOffset, int itemLength)
        {
            var result = new List<byte[]>();
            reader.BaseStream.Seek(sectionOffset, SeekOrigin.Begin);
            while (nextSectionOffset - reader.BaseStream.Position >= itemLength)
            {
                result.Add(reader.ReadBytes(itemLength));
            }
            return result;
        }
    }
}
