using System.Collections.Generic;

using RimWorld;
using UnityEngine;
using Verse;

using RimlightArchive.Defs;

namespace RimlightArchive.Apparel
{
    /// <summary>
    /// Adds the ability for apparel to hold Stormlight.
    /// </summary>
    [StaticConstructorOnStartup]
    public class InfusedApparel : RimWorld.Apparel
    {
        private readonly float ApparelScorePerStormlightMax = 0.25f;

        public int stormlight = 0;
        public override string GetInspectString() => $"{base.GetInspectString()}{System.Environment.NewLine}{this.stormlight} / {this.StormlightMax} Stormlight";
        public int StormlightMax => (int)this.GetStatValue(RadiantDefOf.StormlightMax, true);
        public float StormlightPercentage => this.stormlight / (float)this.StormlightMax;
        public float StormlightGainPerTick => this.GetStatValue(RadiantDefOf.StormlightRechargeRate, true) / 60f;

        public override IEnumerable<Gizmo> GetWornGizmos()
        {
            if (Find.Selector.SingleSelectedThing == base.Wearer && !base.Wearer.Dead)
            {
                yield return new GizmoStormlightApparelStatus
                {
                    apparel = this
                };
            }
        }

        public override float GetSpecialApparelScoreOffset()
        {
            return this.StormlightMax * this.ApparelScorePerStormlightMax;
        }

        public override void Tick()
        {
            base.Tick();
                        
            if (!Utils.CanBeInfused(this) && !Utils.CanBeInfused(this.Wearer) && this.stormlight < this.StormlightMax)
                return;

            // recharge if outside in a highstorm
            this.stormlight = System.Math.Min(this.stormlight + (int)Mathf.Max(this.StormlightGainPerTick, 1f), this.StormlightMax);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.stormlight, "stormlight", 0, false);
        }
    }
}
