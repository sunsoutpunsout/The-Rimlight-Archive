using AbilityUser;
using Verse;

using RimlightArchive.Comps;
using RimlightArchive.Defs;

namespace RimlightArchive.Verbs
{
    [StaticConstructorOnStartup]
    public class Verb_Launch : Verb_UseAbility
    {
        private float distance = 0f;
        private CompAbilityUser_Investiture Comp => this.AbilityUserComp as CompAbilityUser_Investiture;
        private RadiantAbilityDef Def => this.Ability.Def as RadiantAbilityDef;

        public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
        {
            //if (!targ.IsValid
            //    || !targ.CenterVector3.InBounds(this.CasterPawn.Map)
            //    || targ.Cell.Fogged(this.CasterPawn.Map)
            //    || !targ.Cell.Walkable(this.CasterPawn.Map))
            //{
            //    return false;
            //}

            if ((root - targ.Cell).LengthHorizontal > this.verbProps.range
                || !this.Comp.Stormlight.CanUsePower(this.Def.StormlightCost)
                || !(targ.Thing is Pawn pawn)
                || pawn == this.caster
                || Utils.IsWearingShardplate(pawn))
            {
                return false;
            }

            //var r = this.TryFindShootLineFromTo(root, targ, out var shootLine);
            //Log.Message($"CanHitTargetFrom |root {root}|targ |{targ}|Thing {targ.Thing}|r {r}|shootLine {shootLine}| ");
            //Log.Message($"CanHitTargetFrom |root {root}|targ {targ}|targ.HasThing {targ.HasThing}|Thing {targ.Thing}|targ.Thing is Pawn {targ.Thing is Pawn}|targ.Thing.Map {targ.Thing.Map}|caster.Map {this.caster.Map}| ");
            //return r;

            return this.TryFindShootLineFromTo(root, targ, out var shootLine);
        }

        public virtual void Effect()
        {
            //Log.Message($"Effect |TargetsAoE {TargetsAoE}|TargetsAoE[0] {TargetsAoE[0]}|");
            var t = this.TargetsAoE[0];

            if (t.Cell == default(IntVec3))
                return;

            this.CasterPawn.rotationTracker.Face(t.CenterVector3);
            //Log.Message($"Effect |Comp {Comp}|Comp.Stormlight {Comp.Stormlight}|Def {Def}|Def.StormlightCost {Def.StormlightCost}|");

            LongEventHandler.QueueLongEvent(() =>
            {
                if (this.CasterPawn.Map == null)
                {
                    //Log.Message("***returning from flying??");
                    return;
                }

                //Log.Message("***QueueLongEvent??");

                var flyingObject = GenSpawn.Spawn(RadiantDefOf.FlyingObject_Launch, t.Thing.Position, this.CasterPawn.Map) as ThingWithComps_Launch;
                flyingObject.Launch(this.CasterPawn, t.Thing.Position.ToVector3(), t, t.Thing as Pawn);
            }, "LaunchingFlyer", false, null);
        }

        protected override bool TryCastShot()
        {
            var result = base.TryCastShot();
            //Log.Message($"TryCastShot 1 |Comp {Comp}|Comp.Stormlight {Comp.Stormlight}|Def {Def}|Def.StormlightCost {Def.StormlightCost}|result {result}|");

            if (!result)
                return false;

            var amount = this.distance * Def.StormlightCost;
            //Log.Message($"TryCastShot 2 |Comp {Comp}|Comp.Stormlight {Comp.Stormlight}|Def {Def}|Def.StormlightCost {Def.StormlightCost}|");
            this.Comp.Stormlight.UsePower(amount);
            this.Effect();

            return result;
        }
    }
}