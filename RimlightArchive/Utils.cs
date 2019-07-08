using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;

using RimlightArchive.Comps;
using RimlightArchive.Defs;
using RimlightArchive.Apparel;

namespace RimlightArchive
{
    public static class Utils
    {
        public static List<TraitDef> RadiantTraitDefs => new List<TraitDef> {
            RadiantDefOf.RA_Bondsmith,
            RadiantDefOf.RA_Dustbringer,
            RadiantDefOf.RA_Edgedancer,
            RadiantDefOf.RA_Elsecaller,
            RadiantDefOf.RA_Lightweaver,
            RadiantDefOf.RA_Skybreaker,
            RadiantDefOf.RA_Stoneward,
            RadiantDefOf.RA_Truthwatcher,
            RadiantDefOf.RA_Willshaper,
            RadiantDefOf.RA_Windrunner
        };

        public static List<TraitDef> EnabledRadiantTraitDefs
        {
            get
            {
                var enabled = new List<TraitDef>();

                if (RimlightArchiveSettings.RA_Bondsmith)
                    enabled.Add(RadiantDefOf.RA_Bondsmith);

                if (RimlightArchiveSettings.RA_Dustbringer)
                    enabled.Add(RadiantDefOf.RA_Dustbringer);

                if (RimlightArchiveSettings.RA_Edgedancer)
                    enabled.Add(RadiantDefOf.RA_Edgedancer);

                if (RimlightArchiveSettings.RA_Elsecaller)
                    enabled.Add(RadiantDefOf.RA_Elsecaller);

                if (RimlightArchiveSettings.RA_Lightweaver)
                    enabled.Add(RadiantDefOf.RA_Lightweaver);

                if (RimlightArchiveSettings.RA_Skybreaker)
                    enabled.Add(RadiantDefOf.RA_Skybreaker);

                if (RimlightArchiveSettings.RA_Stoneward)
                    enabled.Add(RadiantDefOf.RA_Stoneward);

                if (RimlightArchiveSettings.RA_Truthwatcher)
                    enabled.Add(RadiantDefOf.RA_Truthwatcher);

                if (RimlightArchiveSettings.RA_Willshaper)
                    enabled.Add(RadiantDefOf.RA_Willshaper);

                if (RimlightArchiveSettings.RA_Windrunner)
                    enabled.Add(RadiantDefOf.RA_Windrunner);

                return enabled;
            }
        }

        #region Pawn

        public static bool CanPawnBecomeRadiant(Pawn pawn)
        {
            // skip if disabled in settings
            var result = RimlightArchiveSettings.baseRadiantChance > 0f
                && pawn != null
                && pawn.Spawned
                // currently only human PCs allowed to be radiant
                && pawn.Faction == Faction.OfPlayer
                && (pawn.def.race?.Humanlike).GetValueOrDefault(false)
                // capable of violence
                && !pawn.story.WorkTagIsDisabled(WorkTags.Violent)
                // not already radiant
                && !RadiantTraitDefs.Any(x => pawn.story.traits.HasTrait(x))
                && !pawn.health.hediffSet.HasHediff(RadiantDefOf.RA_Radiant)
                // threat nearby
                && Utils.IsThreatPresent(pawn);

            if (!result)
                return false;

            // roll 20-sided die (with extra % chance of succeeding if there's a highstorm) to pass base chance check
            var roll = Rand.Range(1, 20);
            var contest = 20f - 20f * RimlightArchiveSettings.baseRadiantChance;
            var highstormWeight = 10 * (pawn.Map.GameConditionManager.ConditionIsActive(RadiantDefOf.RA_Highstorm) ? 1 : 0);

            //Log.Message$"CanPawnBecomeRadiant |roll {roll}|baseRadiantChance {RimlightArchiveSettings.baseRadiantChance}|contest {contest}|");

            return roll + highstormWeight >= contest;
        }

        public static bool CanPawnSayAnotherIdeal(Pawn pawn)
        {
            return pawn != null
                && pawn.Spawned
                && (pawn.def.race?.Humanlike).GetValueOrDefault(false)
                && RadiantTraitDefs.Any(x => pawn.story.traits.HasTrait(x))
                && pawn.health.hediffSet.HasHediff(RadiantDefOf.RA_Radiant)
                // threat nearby
                && Utils.IsThreatPresent(pawn)
                && Rand.Range(1, 20) + 10 * (pawn.Map.GameConditionManager.ConditionIsActive(RadiantDefOf.RA_Highstorm) ? 1 : 0) >= 20;
        }

        public static bool IsPawnRadiant(Pawn pawn, out CompAbilityUser_Investiture comp)
        {
            if (pawn == null)
            {
                comp = null;

                return false;
            }

            comp = pawn.GetComp<CompAbilityUser_Investiture>();

            return comp != null
                && pawn.story != null
                && (pawn.def.race?.Humanlike).GetValueOrDefault(false)
                && RadiantTraitDefs.Any(x => pawn.story.traits.HasTrait(x));
        }

        public static bool IsThreatPresent(Pawn pawn)
        {
            if (!RimlightArchiveSettings.threatRequired)
                return true;

            return GenHostility.AnyHostileActiveThreatTo(pawn.Map, pawn.Faction);
        }

        public static bool IsWearingShardplate(Pawn pawn)
        {
            return (pawn?.apparel?.WornApparel.Any(x => x.def == RadiantDefOf.RA_Shardplate)).GetValueOrDefault(false);
        }

        public static void Radiantify(Pawn pawn)
        {
            if (Utils.CanPawnBecomeRadiant(pawn))
            {
                // add the radiant hediff
                var hediff = HediffMaker.MakeHediff(RadiantDefOf.RA_Radiant, pawn, null);
                hediff.Severity = 0f;
                pawn.health.AddHediff(hediff, null, null);

                return;
            }

            if (Utils.CanPawnSayAnotherIdeal(pawn))
            {
                // gain a level
                HealthUtility.AdjustSeverity(pawn, RadiantDefOf.RA_KnightRadiant, 1f);
                var playLogEntry_Interaction = new PlayLogEntry_Interaction(RadiantDefOf.RA_RadiantAttemptFinalWords, pawn, pawn, null);
                Find.PlayLog.Add(playLogEntry_Interaction);
                MoteMaker.MakeInteractionBubble(pawn, pawn, RadiantDefOf.RA_RadiantAttemptFinalWords.interactionMote, RadiantDefOf.RA_RadiantAttemptFinalWords.Symbol);

                return;
            }

            if (pawn.health.hediffSet.HasHediff(RadiantDefOf.RA_Radiant))
            {
                // bump up existing severity
                HealthUtility.AdjustSeverity(pawn, RadiantDefOf.RA_Radiant, 0.25f);

                return;
            }
        }

        #endregion

        #region Highstorm

        public static bool CanBeInfused(Pawn pawn) => 
            pawn != null 
            && pawn.Map.GameConditionManager.ConditionIsActive(RadiantDefOf.RA_Highstorm) 
            && !pawn.Position.Roofed(pawn.Map);

        public static bool CanBeInfused(InfusedApparel apparel) => 
            apparel.Map != null 
            && apparel.Map.GameConditionManager.ConditionIsActive(RadiantDefOf.RA_Highstorm) 
            && !apparel.Position.Roofed(apparel.Map);

        #endregion
    }
}
