using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimlightArchive.Apparel
{
    /// <summary>
    /// Armor that can absorb damage until its Stormlight runs out.
    /// </summary>
    [StaticConstructorOnStartup]
    public class Shardplate : InfusedApparel
    {
        private int lastStatTick = -9999;
        private Vector3 impactAngleVect;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.lastStatTick, "lastStatTick", 0, false);
        }

        public override void Tick()
        {
            base.Tick();

            if (Find.TickManager.TicksGame < this.lastStatTick)
                return;

            // auto repair
            if (this.stormlight > 0 && this.HitPoints < this.MaxHitPoints)
            {
                this.HitPoints++;
                this.stormlight--;
            }
                        
            this.lastStatTick = Find.TickManager.TicksGame + 1000;
        }
        
        public void AbsorbedDamage(float angle, float amount)
        {
            if (this.Wearer == null)
                return;

            SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(base.Wearer.Position, base.Wearer.Map, false));
            this.impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(angle);
            var loc = this.Wearer.TrueCenter() + this.impactAngleVect.RotatedBy(180f) * 0.5f;
            float num = Mathf.Min(10f, 2f + amount / 10f);
            MoteMaker.MakeStaticMote(loc, base.Wearer.Map, ThingDefOf.Mote_ExplosionFlash, num);
            int num2 = (int)num;

            for (int i = 0; i < num2; i++)
            {
                MoteMaker.ThrowDustPuff(loc, base.Wearer.Map, Rand.Range(0.8f, 1.2f));
            }
        }

        public void Break()
        {
            SoundDefOf.EnergyShield_Broken.PlayOneShot(new TargetInfo(base.Wearer.Position, base.Wearer.Map, false));
            MoteMaker.MakeStaticMote(base.Wearer.TrueCenter(), base.Wearer.Map, ThingDefOf.Mote_ExplosionFlash, 12f);

            // make pretty puffs
            for (int i = 0; i < 6; i++)
            {
                var loc = base.Wearer.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle((float)Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f);
                MoteMaker.ThrowDustPuff(loc, base.Wearer.Map, Rand.Range(0.8f, 1.2f));
            }

            this.stormlight = 0;

            if (this.Wearer == null)
                return;

            // drop the plate if broken
            this.Wearer.apparel.TryDrop(this);
        }
    }
}
