using System.Linq;

using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimlightArchive.Comps
{
    [StaticConstructorOnStartup]
    public class ThingWithComps_Launch : ThingWithComps
    {
        protected Vector3 origin;
        protected Vector3 destination;
        protected int ticksToImpact;
        protected int freefall;
        protected Thing assignedTarget;
        protected Pawn flyingPawn;

        private bool returning = false;
        public bool damageLaunched = true;
        public bool explosion = false;
        public int weaponDmg = 0;
        public Pawn pawn;
        public float angle;

        protected int StartingTicksToImpact => 1;
        //***Mathf.RoundToInt(Mathf.Max((this.origin - this.destination).magnitude / (this.speed / 100f), 1f));

        private float Speed => (this.returning ? -1f : 1f) * (this.def.projectile.speed + (float)(this.ticksToImpact % 2) * -0.5f);//;//* (float)(this.ticksToImpact * this.ticksToImpact) * this.def.projectile.speed + 1f;

        //(float)(this.ticksToImpact * this.ticksToImpact) * this.def.projectile.speed * 0.5f + 10f;

        //

        //: Mathf.Pow((float)this.ticksToImpact, 0.95f) * 1.7f * this.def.projectile.speed;

        //***(float)(this.returning ? -1 * this.ticksToImpact : this.ticksToImpact) * this.def.projectile.speed * 0.00721f;
        protected IntVec3 DestinationCell => new IntVec3(this.destination);

        public virtual Vector3 ExactPosition => base.DrawPos + Vector3Utility.FromAngleFlat(this.angle - 90f) * this.Speed;

            //this.returning ? 
            //base.DrawPos + Vector3Utility.FromAngleFlat(this.angle - 90f) * (float)(this.ticksToImpact * this.ticksToImpact) * 0.00721f * this.def.projectile.speed
            //: ;
            //***this.origin + ((this.destination - this.origin) * (1f - (float)this.ticksToImpact / (float)this.StartingTicksToImpact)) + Vector3.up * this.def.Altitude;

        public virtual Quaternion ExactRotation => Quaternion.LookRotation(this.destination - this.origin);

        public override Vector3 DrawPos => this.ExactPosition;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.origin, "origin", default(Vector3), false);
            Scribe_Values.Look(ref this.destination, "destination", default(Vector3), false);
            Scribe_Values.Look(ref this.ticksToImpact, "ticksToImpact", 0, false);
            Scribe_Values.Look(ref this.freefall, "freefall", 0, false);
            Scribe_Values.Look(ref this.damageLaunched, "damageLaunched", true, false);
            Scribe_Values.Look(ref this.returning, "returning", false, false);
            Scribe_Values.Look(ref this.explosion, "explosion", false, false);
            Scribe_Values.Look(ref this.angle, "angle", -33.7f, false);
            Scribe_References.Look(ref this.assignedTarget, "assignedTarget", false);
            Scribe_References.Look(ref this.pawn, "pawn", false);
            Scribe_Deep.Look(ref this.flyingPawn, "flyingThing", new object[0]);
        }

        public override void Tick()
        {
            base.Tick();

            if (this.freefall > 0)
            {
                //Log.Message($"freefallin! {freefall} 1|ticksToImpact {ticksToImpact}|returning {returning}|ExactPosition {ExactPosition}|base.Position {base.Position}|base.DrawPos {base.DrawPos}|speeeed {Speed}|destination {destination}|flyingPawn {flyingPawn}|assignedTarget {assignedTarget}|");
                this.freefall--;

                return;
            }
            
            if (this.returning)
            {
                this.ticksToImpact--;
            }
            else
            {
                this.ticksToImpact++;
            }

            //Log.Message($"tick 1|ticksToImpact {ticksToImpact}|returning {returning}|ExactPosition {ExactPosition}|base.Position {base.Position}|base.DrawPos {base.DrawPos}|speeeed {Speed}|destination {destination}|flyingPawn {flyingPawn}|assignedTarget {assignedTarget}|");

            if (!this.ExactPosition.InBounds(base.Map))
            {
                ////Log.Message$"Tick returning! |base.Map {base.Map}|ExactPosition {this.ExactPosition}||");
                this.returning = true;
                this.freefall = 777;

                return;
            }

            if (this.ticksToImpact == 15)
            {
                this.HitRoof();
            }

            //Log.Message$"tick 2|ticksToImpact {ticksToImpact}|returning {returning}|ExactPosition {ExactPosition}|base.Position {base.Position}|base.DrawPos {base.DrawPos}|speeeed {Speed}|destination {destination}|flyingPawn {flyingPawn}|assignedTarget {assignedTarget}|");

            if (Find.TickManager.TicksGame % 2 == 0)
            {
                MoteMaker.ThrowDustPuff(base.Position, base.Map, Rand.Range(0.8f, 1.2f));
            }

            if (this.ticksToImpact > 0)
            {
                base.Position = this.ExactPosition.ToIntVec3();

                return;
            }

            if (this.DestinationCell.InBounds(base.Map))
            {
                base.Position = this.DestinationCell;
            }

            this.Impact();
        }

        public override void Draw()
        {
            if (this.freefall > 0 || !this.DrawPos.ToIntVec3().IsValid)
                return;
            
            //Log.Message($"pawn draw |ticksToImpact {ticksToImpact}|returning {returning}|ExactPosition {ExactPosition}|base.Position {base.Position}|base.DrawPos {base.DrawPos}|speeeed {Speed}|destination {destination}|flyingPawn {flyingPawn}|assignedTarget {assignedTarget}|");
            flyingPawn.Drawer.DrawAt(this.DrawPos);
            base.Comps_PostDraw();
        }

        public void Launch(Thing launcher, Vector3 origin, LocalTargetInfo targ, Pawn flyingPawn)
        {
            this.pawn = launcher as Pawn;

            //if (!flyingThing.Spawned)
            //{
            //    this.Destroy(DestroyMode.Vanish);

            //    return;
            //}
            if (flyingPawn.Spawned)
            {
                flyingPawn.DeSpawn();
            }
            
            this.origin = origin;
            this.flyingPawn = flyingPawn;

            if (targ.Thing != null)
            {
                this.assignedTarget = targ.Thing;
            }

            this.destination = targ.Cell.ToVector3Shifted();//*** + new Vector3(Rand.Range(-0.3f, 0.3f), 0f, Rand.Range(-0.3f, 0.3f));
            this.ticksToImpact = 0;//***this.StartingTicksToImpact;
            this.angle = -33.7f;
            //this.angle = Rand.Range(-25f, 25f);
            this.freefall = 0;

            //Log.Message$"Launch |this {this}|ticksToImpact {ticksToImpact}|FromAngleFlat {Vector3Utility.FromAngleFlat(this.angle - 90f)}| ExactPosition {ExactPosition}|base.Position {base.Position}|base.DrawPos {base.DrawPos}|speeeed {Speed}|destination {destination}|flyingThing {flyingPawn}|assignedTarget {assignedTarget}|");

            MoteMaker.MakeStaticMote(pawn.TrueCenter(), pawn.Map, ThingDefOf.Mote_ExplosionFlash, 12f);
            SoundDefOf.Ambient_AltitudeWind.sustainFadeoutTime.Equals(30.0f);
            MoteMaker.ThrowDustPuff(pawn.Position, pawn.Map, Rand.Range(1.2f, 1.8f));
        }

        protected virtual void Impact()
        {
            //Log.Message$"IMPACT |this {this}|flyingThing {this.flyingPawn}|DestroyedOrNull {flyingPawn.DestroyedOrNull()}|Spawned {flyingPawn?.Spawned}|| ");
            SoundDefOf.Ambient_AltitudeWind.sustainFadeoutTime.Equals(30.0f);
            GenSpawn.Spawn(this.flyingPawn, base.Position, base.Map);
            var dinfo = new DamageInfo(this.def.projectile.damageDef, 99999f, 999f, -1f, this.pawn, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null);
            dinfo.SetBodyRegion((BodyPartHeight)Rand.Range(0, 3), (BodyPartDepth)Rand.Range(0, 2));

            while (!this.flyingPawn.Dead)
                this.flyingPawn.TakeDamage(dinfo);

            this.Destroy(DestroyMode.Vanish);
        }

        private void HitRoof()
        {
            var cr = this.OccupiedRect();

            if (!cr.Cells.Any(x => x.Roofed(this.Map)))
            {
                return;
            }

            var roof = cr.Cells.First(x => x.Roofed(this.Map)).GetRoof(base.Map);

            if (!roof.soundPunchThrough.NullOrUndefined())
            {
                roof.soundPunchThrough.PlayOneShot(new TargetInfo(base.Position, base.Map, false));
            }

            var cells = cr.ExpandedBy(1).ClipInsideMap(this.Map).Cells
                    .Where(c => c.InBounds(this.Map) && cr.Contains(c) && c.GetFirstPawn(this.Map) == null && (c.GetEdifice(this.Map) == null || !c.GetEdifice(this.Map).def.holdsRoof));

            RoofCollapserImmediate.DropRoofInCells(cells, base.Map, null);
        }
    }
}
