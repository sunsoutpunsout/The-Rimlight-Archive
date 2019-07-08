using System;
using System.Runtime.CompilerServices;

using AbilityUser;
using Verse;

using RimlightArchive.Defs;
using RimlightArchive.Needs;

namespace RimlightArchive.Comps
{
    /// <summary>
    /// Handler for Investiture-based abilities.
    /// </summary>
    [CompilerGenerated]
    [Serializable]
    [StaticConstructorOnStartup]
    public class CompAbilityUser_Investiture : CompAbilityUser
    {
        private bool firstTick = true;
        private bool abilitiesInitialized = false;
        private RadiantData radiantData;

        public float CoolDown { get; set; } = 0f;
        public float MaxStormlight => 1f * this.RadiantLevel;
        public float RadiantLevel => (this.AbilityUser?.health.hediffSet.GetFirstHediffOfDef(RadiantDefOf.RA_KnightRadiant, false)?.Severity).GetValueOrDefault(0f);
        public Need_Stormlight Stormlight => !this.AbilityUser.DestroyedOrNull() && !this.AbilityUser.Dead ? this.AbilityUser.needs.TryGetNeed<Need_Stormlight>() : null;

        public RadiantData RadiantData
        {
            get
            {
                if (this.radiantData == null && Utils.IsPawnRadiant(this.AbilityUser, out var comp))
                {
                    this.radiantData = new RadiantData(this);

                    return this.radiantData;
                }

                return null;
            }
        }

        public override void CompTick()
        {
            if (this.AbilityUser == null 
                || !this.AbilityUser.Spawned 
                || !Utils.IsPawnRadiant(this.AbilityUser, out var comp) 
                || this.Pawn.NonHumanlikeOrWildMan())
            {
                return;
            }

            if (this.firstTick)
            {
                this.PostInitializeTick();
            }

            base.CompTick();

            if (!this.Pawn.IsColonist)
                return;
            
            //ResolveSquires();
            //ResolveSustainers();
            //ResolveEffecter();
            //this.ResolveClassSkills();
        }

        public override void PostInitialize()
        {
            base.PostInitialize();

            if (!this.abilitiesInitialized)
            {
                this.AssignAbilities();
            }

            this.abilitiesInitialized = true;
            base.UpdateAbilities();
        }

        public override void PostPreApplyDamage(DamageInfo dinfo, out bool absorbed)
        {
            base.PostPreApplyDamage(dinfo, out absorbed);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
        }

        public void PostInitializeTick()
        {
            if (this.AbilityUser == null || !this.AbilityUser.Spawned || this.AbilityUser.story == null)
                return;

            this.firstTick = false;
            this.Initialize();

            if (this.Stormlight != null)
                return;

            var firstHediffOfDef = this.AbilityUser.health.hediffSet.GetFirstHediffOfDef(RadiantDefOf.RA_KnightRadiant, false);

            if (firstHediffOfDef != null)
            {
                firstHediffOfDef.Severity = 1f;
                return;
            }

            Log.Message("don't get here!");
            var hediff = HediffMaker.MakeHediff(RadiantDefOf.RA_KnightRadiant, this.AbilityUser, null);
            hediff.Severity = 1f;
            this.AbilityUser.health.AddHediff(hediff, null, null);
        }

        private void AssignAbilities()
        {
            if (!Utils.IsPawnRadiant(this.AbilityUser, out var comp))
                return;

            if (this.AbilityUser.story.traits.HasTrait(RadiantDefOf.RA_Bondsmith))
            {
                Log.Message("Initializing RA_Bondsmith Abilities");
                // add adhesion
                // add tension
            }

            if (this.AbilityUser.story.traits.HasTrait(RadiantDefOf.RA_Dustbringer))
            {
                Log.Message("Initializing RA_Dustbringer Abilities");
                // add Division
                // add Abrasion
            }

            if (this.AbilityUser.story.traits.HasTrait(RadiantDefOf.RA_Edgedancer))
            {
                Log.Message("Initializing RA_Edgedancer Abilities");
                // add Abrasion
                // add Progression
                this.RemovePawnAbility(RadiantDefOf.RA_Regrowth);
                this.AddPawnAbility(RadiantDefOf.RA_Regrowth);
            }

            if (this.AbilityUser.story.traits.HasTrait(RadiantDefOf.RA_Elsecaller))
            {
                Log.Message("Initializing RA_Elsecaller Abilities");
                // add Transformation
                // add Transportation
            }

            if (this.AbilityUser.story.traits.HasTrait(RadiantDefOf.RA_Lightweaver))
            {
                Log.Message("Initializing RA_Lightweaver Abilities");
                // add Illumination
                // add Transformation
            }

            if (this.AbilityUser.story.traits.HasTrait(RadiantDefOf.RA_Skybreaker))
            {
                Log.Message("Initializing RA_Skybreaker Abilities");
                // add Division
                // add gravitation
            }

            if (this.AbilityUser.story.traits.HasTrait(RadiantDefOf.RA_Stoneward))
            {
                Log.Message("Initializing RA_Stoneward Abilities");
                // add Cohesion
                // add Tension
            }

            if (this.AbilityUser.story.traits.HasTrait(RadiantDefOf.RA_Truthwatcher))
            {
                Log.Message("Initializing RA_Truthwatcher Abilities");
                // add Progression
                // add Illumination
            }

            if (this.AbilityUser.story.traits.HasTrait(RadiantDefOf.RA_Willshaper))
            {
                Log.Message("Initializing RA_Willshaper Abilities");
                // add Transportation
                // add Cohesion
            }

            if (this.AbilityUser.story.traits.HasTrait(RadiantDefOf.RA_Windrunner))
            {
                Log.Message("Initializing Windrunner Abilities");

                // add adhesion
                this.RemovePawnAbility(RadiantDefOf.RA_Adhesion);
                this.AddPawnAbility(RadiantDefOf.RA_Adhesion);

                // add gravitation
                this.RemovePawnAbility(RadiantDefOf.RA_Flight);
                this.AddPawnAbility(RadiantDefOf.RA_Flight);
                this.RemovePawnAbility(RadiantDefOf.RA_Launch);
                this.AddPawnAbility(RadiantDefOf.RA_Launch);

                // add resonance
                if (this.RadiantLevel > 1)
                {
                    this.RemovePawnAbility(RadiantDefOf.RA_MakeWindrunnerSquire);
                    this.AddPawnAbility(RadiantDefOf.RA_MakeWindrunnerSquire);
                }
            }
        }
    }
}