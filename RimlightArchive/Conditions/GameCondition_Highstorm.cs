using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using UnityEngine;
using Verse;

namespace RimlightArchive.Conditions
{
    class GameCondition_Highstorm : GameCondition
    {
        private static readonly IntRange AreaRadiusRange = new IntRange(45, 60);
        private static readonly IntRange TicksBetweenDamage = new IntRange(100, 200);
        private static readonly IntRange TicksBetweenStrikes = new IntRange(320, 800);
        private const int RainDisableTicksAfterConditionEnds = 30000;

        public IntVec2 centerLocation;
        private int areaRadius = 2;
        private int nextDamageTicks;
        private int nextLightningTicks;
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.areaRadius, "areaRadius", 0, false);
            Scribe_Values.Look(ref this.centerLocation, "centerLocation", default(IntVec2), false);
            Scribe_Values.Look(ref this.nextDamageTicks, "nextDamageTicks", 0, false);
            Scribe_Values.Look(ref this.nextLightningTicks, "nextLightningTicks", 0, false);
        }

        public override void GameConditionTick()
        {
            if (Find.TickManager.TicksGame > this.nextDamageTicks)
            {
                this.nextDamageTicks = Find.TickManager.TicksGame + GameCondition_Highstorm.TicksBetweenDamage.RandomInRange;
                var damageVector = Rand.UnitVector2 * Rand.Range(0f, (float)this.areaRadius);
                var damageIntVec = new IntVec3((int)Math.Round((double)damageVector.x) + this.centerLocation.x, 0, (int)Math.Round((double)damageVector.y) + this.centerLocation.z);

                if (!this.CellImmuneToDamage(damageIntVec))
                {
                    this.DoDamage(damageIntVec, 1f);
                }
            }

            if (Find.TickManager.TicksGame < this.nextLightningTicks)
            {
                return;
            }

            var vector = Rand.UnitVector2 * Rand.Range(0f, (float)this.areaRadius);
            var intVec = new IntVec3((int)Math.Round((double)vector.x) + this.centerLocation.x, 0, (int)Math.Round((double)vector.y) + this.centerLocation.z);

            if (!this.IsGoodLocationForStrike(intVec))
            {
                return;
            }
            
            this.SingleMap.weatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrike(this.SingleMap, intVec));
            this.nextLightningTicks = Find.TickManager.TicksGame + GameCondition_Highstorm.TicksBetweenStrikes.RandomInRange;
        }

        public override void Init()
        {
            Log.Message("GameCondition_Highstorm:Init");
            base.Init();
            this.areaRadius = GameCondition_Highstorm.AreaRadiusRange.RandomInRange;
            this.FindGoodCenterLocation();
        }

        public override void End()
        {
            this.SingleMap.weatherManager.TransitionTo(WeatherDef.Named("RainyThunderstorm"));
            base.End();
        }

        private void FindGoodCenterLocation()
        {
            if (base.SingleMap.Size.x <= 16 || base.SingleMap.Size.z <= 16)
            {
                throw new Exception("Map too small for Highstorm.");
            }

            for (int i = 0; i < 10; i++)
            {
                this.centerLocation = new IntVec2(Rand.Range(8, base.SingleMap.Size.x - 8), Rand.Range(8, base.SingleMap.Size.z - 8));
                if (this.IsGoodCenterLocation(this.centerLocation))
                {
                    break;
                }
            }
        }

        private bool IsGoodLocationForStrike(IntVec3 loc)
        {
            return loc.InBounds(base.SingleMap) && !loc.Roofed(base.SingleMap) && loc.Standable(base.SingleMap);
        }

        private bool IsGoodLocationForSpawn(IntVec3 loc)
        {
            return loc.InBounds(base.SingleMap) && !loc.Roofed(base.SingleMap) && loc.Standable(base.SingleMap) && loc.IsValid && !loc.Fogged(base.SingleMap) && loc.Walkable(base.SingleMap);
        }

        private bool IsGoodCenterLocation(IntVec2 loc)
        {
            int num = 0;
            int num2 = (int)(3.14159274f * (float)this.areaRadius * (float)this.areaRadius / 2f);

            foreach (var current in this.GetPotentiallyAffectedCells(loc))
            {
                if (this.IsGoodLocationForStrike(current))
                {
                    num++;
                }
                if (num >= num2)
                {
                    break;
                }
            }

            return num >= num2;
        }

        private IEnumerable<IntVec3> GetPotentiallyAffectedCells(IntVec2 center)
        {
            for (int x = center.x - this.areaRadius; x <= center.x + this.areaRadius; x++)
            {
                for (int z = center.z - this.areaRadius; z <= center.z + this.areaRadius; z++)
                {
                    if ((center.x - x) * (center.x - x) + (center.z - z) * (center.z - z) <= this.areaRadius * this.areaRadius)
                    {
                        yield return new IntVec3(x, 0, z);
                    }
                }
            }
        }

        private bool CellImmuneToDamage(IntVec3 c)
        {
            if (!c.InBounds(this.SingleMap) || c.Roofed(this.SingleMap))
            {
                return true;
            }

            var edifice = c.GetEdifice(this.SingleMap);

            return edifice != null && edifice.def.category == ThingCategory.Building && (edifice.def.building.isNaturalRock || (edifice.def == ThingDefOf.Wall && edifice.Faction == null));
        }

        private void DoDamage(IntVec3 c, float damageFactor)
        {
            foreach (var thing in c.GetThingList(this.SingleMap).Where(x => x.def.category != ThingCategory.Pawn).ToList())
            {
                //BattleLogEntry_DamageTaken battleLogEntry_DamageTaken = null;

                switch (thing.def.category)
                {
                    //case ThingCategory.Pawn:
                    //    {
                    //        Pawn pawn = (Pawn)Tornado.tmpThings[i];
                    //        battleLogEntry_DamageTaken = new BattleLogEntry_DamageTaken(pawn, RulePackDefOf.DamageEvent_Tornado, null);
                    //        Find.BattleLog.Add(battleLogEntry_DamageTaken);
                    //        if (pawn.RaceProps.baseHealthScale < 1f)
                    //        {
                    //            damageFactor *= pawn.RaceProps.baseHealthScale;
                    //        }
                    //        if (pawn.RaceProps.Animal)
                    //        {
                    //            damageFactor *= 0.75f;
                    //        }
                    //        if (pawn.Downed)
                    //        {
                    //            damageFactor *= 0.2f;
                    //        }
                    //        break;
                    //    }
                    case ThingCategory.Item:
                        damageFactor *= 0.68f;
                        break;
                    case ThingCategory.Building:
                        damageFactor *= 0.8f;
                        break;
                    case ThingCategory.Plant:
                        damageFactor *= 1.7f;
                        break;
                }

                Log.Message($"Damaging thing |{thing}| with {Mathf.Max(GenMath.RoundRandom(30f * damageFactor), 1)}");
                thing.TakeDamage(new DamageInfo(DamageDefOf.Blunt, Mathf.Max(GenMath.RoundRandom(30f * damageFactor), 1), 0f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null));//.AssociateWithLog(battleLogEntry_DamageTaken);
            }
        }
    }
}
