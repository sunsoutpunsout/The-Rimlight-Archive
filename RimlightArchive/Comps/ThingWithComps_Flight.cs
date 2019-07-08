using System.Linq;

using RimWorld;
using UnityEngine;
using Verse;

namespace RimlightArchive.Comps
{
    /// <summary>
    /// Handles flying.
    /// </summary>
    [StaticConstructorOnStartup]
    public class ThingWithComps_Flight : ThingWithComps
    {
        protected Vector3 origin;
        protected Vector3 destination;
        protected float speed = 20f;
        protected int ticksToImpact;
        protected Thing assignedTarget;
        protected Thing flyingThing;

        private bool drafted = false;
        public DamageInfo? impactDamage;
        public bool damageLaunched = true;
        public bool explosion = false;
        public int weaponDmg = 0;
        public Pawn pawn;
        public CompAbilityUser_Investiture comp;

        protected int StartingTicksToImpact => Mathf.RoundToInt(Mathf.Max((this.origin - this.destination).magnitude / (this.speed / 100f), 1f));

        protected IntVec3 DestinationCell => new IntVec3(this.destination);

        public virtual Vector3 ExactPosition => this.origin + ((this.destination - this.origin) * (1f - (float)this.ticksToImpact / (float)this.StartingTicksToImpact)) + Vector3.up * this.def.Altitude;

        public virtual Quaternion ExactRotation => Quaternion.LookRotation(this.destination - this.origin);

        public override Vector3 DrawPos => this.ExactPosition;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.origin, "origin", default(Vector3), false);
            Scribe_Values.Look(ref this.destination, "destination", default(Vector3), false);
            Scribe_Values.Look(ref this.ticksToImpact, "ticksToImpact", 0, false);
            Scribe_Values.Look(ref this.damageLaunched, "damageLaunched", true, false);
            Scribe_Values.Look(ref this.explosion, "explosion", false, false);
            Scribe_References.Look(ref this.assignedTarget, "assignedTarget", false);
            Scribe_References.Look(ref this.pawn, "pawn", false);
            Scribe_Deep.Look(ref this.flyingThing, "flyingThing", new object[0]);
        }

        public override void Tick()
        {
            base.Tick();
            this.ticksToImpact--;

            if (!this.ExactPosition.InBounds(base.Map))
            {
                this.ticksToImpact++;
                base.Position = this.ExactPosition.ToIntVec3();
                this.Destroy(DestroyMode.Vanish);

                return;
            }

            base.Position = this.ExactPosition.ToIntVec3();

            if (Find.TickManager.TicksGame % 2 == 0)
            {
                MoteMaker.ThrowDustPuff(base.Position, base.Map, Rand.Range(0.8f, 1.2f));
            }

            if (this.ticksToImpact > 0)
            {
                return;
            }

            if (this.DestinationCell.InBounds(base.Map))
            {
                base.Position = this.DestinationCell;
            }

            this.ImpactSomething();
        }

        public override void Draw()
        {
            if (this.flyingThing != null
                && this.flyingThing is Pawn flyingPawn)
            {
                if (!this.DrawPos.ToIntVec3().IsValid)
                {
                    return;
                }

                flyingPawn.Drawer.DrawAt(this.DrawPos);
            }

            //***Graphics.DrawMesh(MeshPool.plane10, this.DrawPos, this.ExactRotation, this.flyingThing.def.DrawMatSingle, 0);
            base.Comps_PostDraw();
        }

        private void Initialize()
        {
            if (pawn == null)
                return;

            MoteMaker.MakeStaticMote(pawn.TrueCenter(), pawn.Map, ThingDefOf.Mote_ExplosionFlash, 12f);
            SoundDefOf.Ambient_AltitudeWind.sustainFadeoutTime.Equals(30.0f);
            MoteMaker.ThrowDustPuff(pawn.Position, pawn.Map, Rand.Range(1.2f, 1.8f));            
        }

        public void Launch(Thing launcher, LocalTargetInfo targ, Thing flyingThing, DamageInfo? impactDamage)
        {
            this.Launch(launcher, base.Position.ToVector3Shifted(), targ, flyingThing, impactDamage);
        }

        public void Launch(Thing launcher, LocalTargetInfo targ, Thing flyingThing)
        {
            this.Launch(launcher, base.Position.ToVector3Shifted(), targ, flyingThing, null);
        }

        public void Launch(Thing launcher, Vector3 origin, LocalTargetInfo targ, Thing flyingThing, DamageInfo? newDamageInfo = null)
        {
            this.pawn = launcher as Pawn;
            this.drafted = pawn.Drafted;
            this.comp = pawn.GetComp<CompAbilityUser_Investiture>();

            if (flyingThing.Spawned)
            {
                flyingThing.DeSpawn();
            }

            this.origin = origin;
            this.impactDamage = newDamageInfo;
            this.flyingThing = flyingThing;

            if (targ.Thing != null)
            {
                this.assignedTarget = targ.Thing;
            }

            this.destination = targ.Cell.ToVector3Shifted() + new Vector3(Rand.Range(-0.3f, 0.3f), 0f, Rand.Range(-0.3f, 0.3f));
            this.ticksToImpact = this.StartingTicksToImpact;
            this.Initialize();
        }

        protected virtual void Impact(Thing hitThing)
        {
            if (hitThing == null)
            {
                hitThing = base.Position.GetThingList(base.Map).OfType<Pawn>().FirstOrDefault(x => x == this.assignedTarget);
            }

            if (this.impactDamage.HasValue)
            {
                hitThing.TakeDamage(this.impactDamage.Value);
            }

            SoundDefOf.Ambient_AltitudeWind.sustainFadeoutTime.Equals(30.0f);
            GenSpawn.Spawn(this.flyingThing, base.Position, base.Map);
            //***ModOptions.Constants.SetPawnInFlight(false);
            var p = this.flyingThing as Pawn;

            if (p.IsColonist)
            {
                //CameraJumper.TryJumpAndSelect(p);
                p.drafter.Drafted = this.drafted;
            }

            this.Destroy(DestroyMode.Vanish);
        }

        private void ImpactSomething()
        {
            if ((this.assignedTarget is Pawn targetPawn)
                && targetPawn.GetPosture() != PawnPosture.Standing
                && (this.origin - this.destination).MagnitudeHorizontalSquared() >= 20.25f && Rand.Value > 0.2f)
            {
                this.Impact(null);

                return;
            }

            this.Impact(this.assignedTarget);
        }
    }
}
