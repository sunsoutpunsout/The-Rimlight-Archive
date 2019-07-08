using AbilityUser;
using Verse;

using RimlightArchive.Comps;
using RimlightArchive.Defs;

namespace RimlightArchive.Verbs
{
    public class Verb_Flight : Verb_UseAbility
    {
        private float distance = 0f;
        private CompAbilityUser_Investiture Comp => this.AbilityUserComp as CompAbilityUser_Investiture;
        private RadiantAbilityDef Def => this.Ability.Def as RadiantAbilityDef;

        public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
        {
            if (!targ.IsValid 
                || !targ.CenterVector3.InBounds(this.CasterPawn.Map) 
                || targ.Cell.Fogged(this.CasterPawn.Map) 
                || !targ.Cell.Walkable(this.CasterPawn.Map))
            {
                return false;
            }

            this.distance = (root - targ.Cell).LengthHorizontal;
            var amount = this.distance * this.Def.StormlightCost;

            if (this.distance >= this.verbProps.range
                || !this.Comp.Stormlight.CanUsePower(amount))
            {
                return false;
            }

            return true; //***this.TryFindShootLineFromTo(root, targ, out var shootLine);
        }

        public virtual void Effect()
        {
            var t = this.TargetsAoE[0];

            if (t.Cell == default(IntVec3))
                return;

            this.CasterPawn.rotationTracker.Face(t.CenterVector3);
            
            LongEventHandler.QueueLongEvent(() =>
            {
                if (this.CasterPawn.Map == null)
                {
                    Log.Message("***returning from flying??");
                    return;
                }

                var flyingObject = GenSpawn.Spawn(RadiantDefOf.FlyingObject_Flight, this.CasterPawn.Position, this.CasterPawn.Map) as ThingWithComps_Flight;
                flyingObject.Launch(this.CasterPawn, t.Cell, this.CasterPawn);
            }, "LaunchingFlyer", false, null);
        }

        protected override bool TryCastShot()
        {
            var result = base.TryCastShot();

            if (!result)
                return false;

            var amount = this.distance * Def.StormlightCost;
            this.Comp.Stormlight.UsePower(amount);
            this.Effect();

            return result;
        }
    }
}
