using System;
using System.Collections.Generic;

using Verse;

using RimlightArchive.Comps;
using RimlightArchive.Needs;

namespace RimlightArchive.Apparel
{
    /// <summary>
    /// Apparel that can be infused with Stormlight in a Highstorm and used to give Stormlight Need.
    /// </summary>
    [StaticConstructorOnStartup]
    public class SphereBag : InfusedApparel
    {
        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override IEnumerable<Gizmo> GetWornGizmos()
        {
            if (Find.Selector.SingleSelectedThing == base.Wearer)
            {
                yield return new GizmoStormlightApparelStatus
                {
                    apparel = this
                };
            }
        }

        public override void Tick()
        {
            base.Tick();

            if (this.stormlight <= 0)
            {
                this.stormlight = 0;

                return;
            }

            Need_Stormlight stormlightNeed = null;

            if (this.Wearer != null)
            {
                stormlightNeed = this.Wearer.GetComp<CompAbilityUser_Investiture>()?.Stormlight;
            }
            else
            {
                if (this.Map != Find.CurrentMap)
                    return;

                foreach (var cell in GenAdj.AdjacentCellsAndInside)
                {
                    var checkCell = this.Position + cell;

                    if (!checkCell.InBounds(this.Map))
                        continue;

                    stormlightNeed = checkCell.GetFirstThingWithComp<CompAbilityUser_Investiture>(this.Map)?.GetComp<CompAbilityUser_Investiture>()?.Stormlight;

                    if (stormlightNeed != null && stormlightNeed.CurLevel < stormlightNeed.MaxLevel)
                        break;
                }
            }

            if (stormlightNeed == null || stormlightNeed.CurLevel >= stormlightNeed.MaxLevel)
                return;

            var floatDiff = stormlightNeed.MaxLevel - stormlightNeed.CurLevel;
            var intDiff = Math.Max(1, (int)Math.Min(floatDiff * 100, this.stormlight));
            //***Log.Message($"{this} ticking last here |{this.Position}|{this.stormlight}|{this.Wearer}|{stormlightNeed.CurLevel}/{stormlightNeed.MaxLevel}|diff {intDiff} - {floatDiff}| ");
            stormlightNeed.CurLevel += floatDiff;
            this.stormlight -= intDiff;
        }

        public override bool AllowVerbCast(IntVec3 root, Map map, LocalTargetInfo targ, Verb verb) => true;
    }
}
