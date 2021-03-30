namespace FF8Mod.Maelstrom
{
    // todo: finish implementing the kernel class & get this data from there instead
    public class AbilityMeta
    {
        public int AbilityID { get; set; }
        public string AbilityName { get; set; }
        public bool ItemExclusive { get; set; } = false;
        public bool MenuAbility { get; set; } = false;
    }
}