using AbilityUser;

namespace RimlightArchive.Defs
{
    public class RadiantAbilityDef : AbilityDef
    {
        public int AbilityPoints { get; set; } = 1;
        public string PointDescription => $"{this.GetDescription()}{System.Environment.NewLine}";
        public float StormlightCost = 0.01f;
    }
}
