using UnityEngine;
using Verse;

using RimlightArchive.Apparel;

namespace RimlightArchive.Comps
{
    /// <summary>
    /// Handler for applying Shardplate buffs/de-buffs.
    /// </summary>
    [StaticConstructorOnStartup]
    public class ThingComp_Shardplate : ThingComp
    {
        private bool initialized = false;
        private HediffDef hediff = null;

        public CompProperties_Shardplate Props => this.props as CompProperties_Shardplate;

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            //***Log.Message($"Initialize spam |hediff {this.Props.hediff}|parent {this.parent}|| ");

            if (initialized)
                return;

            this.hediff = this.Props.hediff;
            //***this.nextApplyTick = Find.TickManager.TicksGame + 100;
            this.initialized = true;
        }

        public override void CompTick()
        {
            base.CompTick();

            //***if (Find.TickManager.TicksGame < this.nextApplyTick)
            //    return;
            //***this.nextApplyTick = Find.TickManager.TicksGame + 100;
            //***Log.Message($"CompTick spam |hediff {hediff}|parent {this.parent}|not infused? {!(this.parent is InfusedApparel)}|not plate? {!(this.parent is Shardplate)}|");

            if (this.hediff == null
                || !(this.parent is InfusedApparel plate)
                || plate == null
                || plate.Wearer == null)
                return;

            //***Log.Message($"CompTick spam |Wearer {plate.Wearer}|wearer's diff {plate.Wearer.health.hediffSet.GetFirstHediffOfDef(hediff, false)}");

            if (plate.Wearer.health.hediffSet.GetFirstHediffOfDef(hediff, false) != null)
            {
                return;
            }
            
            //***Log.Message($"adding |hediff {hediff}|");

            HealthUtility.AdjustSeverity(plate.Wearer, hediff, Mathf.Max(hediff.minSeverity, plate.StormlightPercentage));
            plate.Wearer.health.hediffSet.GetFirstHediffOfDef(hediff, false).TryGetComp<HediffComp_Shardplate>().InfusedApparel = plate;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref this.initialized, "initialized", false, false);
            Scribe_Defs.Look(ref this.hediff, "hediff");
        }
    }

    [StaticConstructorOnStartup]
    public class CompProperties_Shardplate : CompProperties, IExposable
    {
        public HediffDef hediff = null;

        public CompProperties_Shardplate()
        {
            this.compClass = typeof(ThingComp_Shardplate);
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref this.hediff, "hediff");
        }
    }
}
