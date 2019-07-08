using UnityEngine;
using Verse;

using RimlightArchive.Apparel;
using RimlightArchive.Defs;

namespace RimlightArchive.Comps
{
    [StaticConstructorOnStartup]
    public class HediffComp_Shardplate : HediffComp
    {
        private bool removeNow = false;
        private bool initialized = false;

        public InfusedApparel InfusedApparel;

        public override void CompPostTick(ref float severityAdjustment)
        {
            //Log.Message($"CompPostTick|Pawn {this.Pawn}|Map {this.Pawn?.Map}|removeNow {removeNow}|initialized {initialized}|");

            // *** needed for caravans?
            if (this.Pawn?.Map == null)
                return;

            //***base.CompPostTick(ref severityAdjustment);

            if (!this.initialized)
            {
                this.initialized = true;

                if (this.Pawn.Spawned && !this.IsApparelWorn())
                {
                    Log.Message("CompPostTick_RemoveHediff");
                    this.Pawn.health.RemoveHediff(this.parent);
                }
            }

            //Log.Message($"CompPostTick|IsApparelWorn {IsApparelWorn()}|health {this.Pawn.health}|hediff {this.Pawn.health.hediffSet.GetFirstHediffOfDef(RadiantDefOf.RA_ShardplateBuff, false)}|");
            //Log.Message($"CompPostTick|InfusedApparel {this.InfusedApparel}|StormlightPercentage {this.InfusedApparel?.StormlightPercentage}|");

            this.IsApparelWorn();
            this.Pawn.health.hediffSet.GetFirstHediffOfDef(RadiantDefOf.RA_ShardplateBuff, false).Severity = this.InfusedApparel.StormlightPercentage;
        }

        public override bool CompShouldRemove => base.CompShouldRemove || this.removeNow;
        //public override bool CompShouldRemove 
        //{
        //    get
        //    {
        //        if (base.CompShouldRemove || this.removeNow)
        //        {
        //            Log.Message($"Removing shardplate comp!");
        //        }

        //        return base.CompShouldRemove || this.removeNow;
        //    }
        //}

        public override void CompExposeData()
        {
            Scribe_Values.Look(ref this.initialized, "initialized", false, false);
            Scribe_Values.Look(ref this.removeNow, "removeNow", false, false);
            Scribe_References.Look(ref this.InfusedApparel, "InfusedApparel", false);
            base.CompExposeData();
        }

        private bool IsApparelWorn()
        {
            var worn = this.InfusedApparel != null && (this.Pawn?.apparel?.WornApparel?.Contains(this.InfusedApparel)).GetValueOrDefault(false);
            //Log.Message($"IsApparelWorn |InfusedApparel {InfusedApparel}|Pawn {Pawn}|apparel {this.Pawn?.apparel}|WornApparel {this.Pawn?.apparel?.WornApparel}|wearing {this.Pawn?.apparel?.WornApparel?.Contains(this.InfusedApparel)}|worn {worn}");
            this.removeNow = !worn;

            return worn;
        }
    }
}
