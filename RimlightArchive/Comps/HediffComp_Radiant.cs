using System;
using System.Collections.Generic;

using RimWorld;
using Verse;

using RimlightArchive.Defs;

namespace RimlightArchive.Comps
{
    /// <summary>
    /// Handler for making a pawn Radiant.
    /// </summary>
    [StaticConstructorOnStartup]
    public class HediffComp_Radiant : HediffComp
    {
        private bool initialized = false;
        private bool firstWordsSaid = false;
        private bool secondWordsSaid = false;
        private bool finalWordsSaid = false;
        private int nextApplyTick = 0;

        public bool IsFullRadiant => firstWordsSaid && secondWordsSaid && finalWordsSaid;

        public override void CompExposeData()
        {
            base.CompExposeData();
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            // *** needed for caravans?
            if (this.Pawn?.Map == null)
                return;

            if (!initialized)
            {
                initialized = true;
                
                if (base.Pawn.Spawned && !Utils.IsPawnRadiant(this.Pawn, out var comp))
                {
                    this.nextApplyTick = Find.TickManager.TicksGame + Rand.Range(1000, 3000);
                    //Log.Message($"init next apply:{this.nextApplyTick}");
                }
                else
                {
                    //Log.Message"RemoveHediff");
                    this.Pawn.health.RemoveHediff(this.parent);
                }
            }

            // only proc when pawn is downed or in a mental break with active threat
            if (this.Pawn.story == null
                || this.Pawn.story.traits == null
                || !(this.Pawn.health.Downed || this.Pawn.InMentalState)
                || !Utils.IsThreatPresent(this.Pawn))
                return;

            if (!this.IsFullRadiant && this.parent.Severity < 1.0f && Find.TickManager.TicksGame > this.nextApplyTick)
            {
                HealthUtility.AdjustSeverity(this.Pawn, RadiantDefOf.RA_Radiant, 0.25f);
                this.nextApplyTick = Find.TickManager.TicksGame + Rand.Range(1000, 3000);
                MoteMaker.ThrowSmoke(this.Pawn.DrawPos, this.Pawn.Map, 1f);
                MoteMaker.ThrowLightningGlow(this.Pawn.DrawPos, this.Pawn.Map, 0.8f);
                //Log.Message$"***post next apply:{this.nextApplyTick}, post severity:{severityAdjustment}");
            }

            if (Find.TickManager.TicksGame % 1200 == 0)
            {
                DetermineHediff();
            }
        }

        public override bool CompShouldRemove => this.IsFullRadiant;
        
        private void DetermineHediff()
        {
            var comp = this.Pawn.GetComp<CompAbilityUser_Investiture>();

            if (comp == null || parent.def != RadiantDefOf.RA_Radiant)
                return;
            
            if (this.IsFullRadiant)
            {
                this.Pawn.health.hediffSet.GetFirstHediffOfDef(RadiantDefOf.RA_Radiant, false).Severity = 1f;

                return;
            }

            if (this.parent.Severity > 0.75f && !finalWordsSaid && secondWordsSaid)
            {
                finalWordsSaid = true;
                var playLogEntry_Interaction = new PlayLogEntry_Interaction(RadiantDefOf.RA_RadiantAttemptFinalWords, this.Pawn, this.Pawn, null);
                Find.PlayLog.Add(playLogEntry_Interaction);
                MoteMaker.MakeInteractionBubble(this.Pawn, this.Pawn, RadiantDefOf.RA_RadiantAttemptFinalWords.interactionMote, RadiantDefOf.RA_RadiantAttemptFinalWords.Symbol);

                // give radiant thought
                this.Pawn.needs.mood.thoughts.memories.TryGainMemory(RadiantDefOf.RA_NewRadiantThought, null);

                // add trait - base it on other traits?
                if (Utils.EnabledRadiantTraitDefs.Any())
                    this.Pawn.story.traits.GainTrait(new Trait(Utils.EnabledRadiantTraitDefs.RandomElement(), 4, false));

                // add hediff for investiture
                var hediff = this.Pawn.health.hediffSet.GetFirstHediffOfDef(RadiantDefOf.RA_KnightRadiant, false);

                if (hediff == null)
                {
                    hediff = HediffMaker.MakeHediff(RadiantDefOf.RA_KnightRadiant, this.Pawn, null);
                    hediff.Severity = 1f;
                    this.Pawn.health.AddHediff(hediff, null, null);
                }

                // add hediff for regen
                hediff = this.Pawn.health.hediffSet.GetFirstHediffOfDef(RadiantDefOf.RA_Regeneration, false);

                if (hediff == null)
                {
                    hediff = HediffMaker.MakeHediff(RadiantDefOf.RA_Regeneration, this.Pawn, null);
                    hediff.Severity = 1f;
                    this.Pawn.health.AddHediff(hediff, null, null);
                }
            }
            else if (this.parent.Severity > 0.5f && !secondWordsSaid && firstWordsSaid)
            {
                secondWordsSaid = true;
                var list = new List<RulePackDef>();
                var playLogEntry_Interaction = new PlayLogEntry_Interaction(RadiantDefOf.RA_RadiantAttemptSecondWords, this.Pawn, this.Pawn, list);
                Find.PlayLog.Add(playLogEntry_Interaction);
                MoteMaker.MakeInteractionBubble(this.Pawn, this.Pawn, RadiantDefOf.RA_RadiantAttemptSecondWords.interactionMote, RadiantDefOf.RA_RadiantAttemptSecondWords.Symbol);
            }
            else if (this.parent.Severity > 0.25f && !firstWordsSaid)
            {
                firstWordsSaid = true;
                var list = new List<RulePackDef>();
                var playLogEntry_Interaction = new PlayLogEntry_Interaction(RadiantDefOf.RA_RadiantAttemptFirstWords, this.Pawn, this.Pawn, list);
                Find.PlayLog.Add(playLogEntry_Interaction);

                Find.LetterStack.ReceiveLetter($"Knight Radiant: {this.Pawn.Name.ToStringShort}",
                    $"{this.Pawn.Name.ToStringShort} is attempting to become Radiant!{Environment.NewLine}{Environment.NewLine}Get {this.Pawn.gender.GetObjective()} some Stormlight!",
                    LetterDefOf.ThreatSmall, this.Pawn);

                MoteMaker.MakeInteractionBubble(this.Pawn, this.Pawn, RadiantDefOf.RA_RadiantAttemptFirstWords.interactionMote, RadiantDefOf.RA_RadiantAttemptFirstWords.Symbol);
            }
        }
    }
}
