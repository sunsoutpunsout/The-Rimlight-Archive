using System.Linq;

using Verse;

using RimlightArchive.Defs;

namespace RimlightArchive.Comps
{
    /// <summary>
    /// Handler for regenerating parts created with Investiture.
    /// </summary>
    [StaticConstructorOnStartup]
    class HediffComp_RegeneratingPart : HediffComp
    {
        private int nextApplyTick = 0;
        private CompAbilityUser_Investiture User => this.Pawn.GetComp<CompAbilityUser_Investiture>();

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            // *** change stormlight cost?
            if (Find.TickManager.TicksGame < this.nextApplyTick
                || this.Pawn == null
                || this.parent == null
                || this.Pawn.DestroyedOrNull()
                || this.User == null
                || this.User.Stormlight == null)
                return;

            var regrowingPart = this.Pawn.health.hediffSet.hediffs
                .FirstOrDefault(x =>
                    string.Equals(this.Def.defName, x.def.defName, System.StringComparison.OrdinalIgnoreCase)
                    && (x.def.stages?.Any()).GetValueOrDefault(false)
                    && x.CurStageIndex < x.def.stages.Count - 1);

            if (regrowingPart == null || !this.User.Stormlight.CanUsePower(RadiantDefOf.RA_Regenerate.StormlightCost))
            {
                // unable to grow warning?
                return;
            }

            // tick regrowing parts
            HealthUtility.AdjustSeverity(this.Pawn, this.Def, 0.25f);
            //***Log.Message($"{this.Pawn.LabelShort} is regrowing |{this.Def}| for 0.25f.");
            this.User.Stormlight.UsePower(RadiantDefOf.RA_Regenerate.StormlightCost);
            this.nextApplyTick = Find.TickManager.TicksGame + Rand.Range(1000, 3000); ;
        }
    }
}
