using Verse;

namespace RimlightArchive.Comps
{
    /// <summary>
    /// Handler for removing body parts via Shardblade.
    /// </summary>
    [StaticConstructorOnStartup]
    public class Hediff_Injury_Shardburn : Hediff_Injury
    {
        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);

            if (this.Part == null || this.pawn.DestroyedOrNull() || !dinfo.HasValue)
                return;

            // sever the part
            dinfo.Value.SetAllowDamagePropagation(false);
            dinfo.Value.SetInstantPermanentInjury(true);
            this.Severity = this.Part.def.GetMaxHealth(this.pawn);
        }

        public override bool ShouldRemove => false;
    }
}
