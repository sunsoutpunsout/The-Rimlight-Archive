using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;

using RimlightArchive.Defs;

namespace RimlightArchive.Comps
{
    /// <summary>
    /// Handler for all Investiture-based regeneration. Regrows limbs, cures diseases, and hopefully prevents death.
    /// </summary>
    [StaticConstructorOnStartup]
    class HediffComp_Regeneration : HediffComp
    {
        private bool initialized = false;
        private int age = 0;
        private int regenRate = 100;
        private int lastRegen = 0;

        private CompAbilityUser_Investiture User => this.Pawn.GetComp<CompAbilityUser_Investiture>();

        // use class?
        private static Dictionary<string, string> MissingPartXref => new Dictionary<string, string>
        {
            { "Arm", "RA_ArmRegrowth" },
            { "Ear", "RA_EarRegrowth" },
            { "Eye", "RA_EyeRegrowth" },
            { "Finger", "RA_FingerRegrowth" },
            { "Foot", "RA_FootRegrowth" },
            { "Hand", "RA_HandRegrowth" },
            { "Heart", "RA_HeartRegrowth" },
            { "Jaw", "RA_JawRegrowth" },
            { "Kidney", "RA_KidneyRegrowth" },
            { "Leg", "RA_LegRegrowth" },
            { "Liver", "RA_LiverRegrowth" },
            { "Lung", "RA_LungRegrowth" },
            { "Nose", "RA_StandardRegrowth" },
            { "Ribcage", "RA_StandardRegrowth" },
            { "Shoulder", "RA_ArmRegrowth" },
            { "Spine", "RA_SpineRegrowth" },
            { "Stomach", "RA_StomachRegrowth" },
            { "Toe", "RA_ToeRegrowth" }
        };

        private static List<HediffDef> Infections => new List<HediffDef>
        {
            HediffDefOf.Flu,
            HediffDefOf.FoodPoisoning,
            HediffDefOf.Malaria,
            HediffDefOf.Plague,
            HediffDefOf.ToxicBuildup,
            HediffDefOf.WoundInfection,
            HediffDef.Named("FibrousMechanites"),
            HediffDef.Named("GutWorms"),
            HediffDef.Named("MuscleParasites"),
            HediffDef.Named("SensoryMechanites"),
            HediffDef.Named("SleepingSickness")
        };

        private static List<HediffDef> BadConditions => new List<HediffDef>
        {
            HediffDefOf.Asthma,
            HediffDefOf.BadBack,
            HediffDefOf.Blindness,
            HediffDefOf.Carcinoma,
            HediffDefOf.Cataract,
            HediffDefOf.CatatonicBreakdown,
            HediffDefOf.CryptosleepSickness,
            HediffDefOf.Dementia,
            HediffDefOf.Frail,
            HediffDefOf.Heatstroke,
            HediffDefOf.Hypothermia,
            HediffDefOf.PsychicShock,
            HediffDefOf.ResurrectionPsychosis,
            HediffDefOf.ResurrectionSickness,
            HediffDef.Named("Alzheimers"),
            HediffDef.Named("Cirrhosis"),
            HediffDef.Named("ChemicalDamageModerate"),
            HediffDef.Named("ChemicalDamageSevere"),
            HediffDef.Named("HearingLoss"),
            HediffDef.Named("HeartArteryBlockage"),
            HediffDef.Named("TraumaSavant")
        };

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            // *** change stormlight cost?
            if (this.Pawn == null
                || this.parent == null
                || this.Pawn.DestroyedOrNull()
                || this.User == null
                || this.User.Stormlight == null)
                return;


            if (!this.User.Stormlight.CanUsePower(RadiantDefOf.RA_Regenerate.StormlightCost))
            {
                // unable to heal warning?
                return;
            }

            if (!initialized)
            {
                initialized = true;

                if (this.Pawn.Spawned)
                {
                    MoteMaker.ThrowLightningGlow(this.Pawn.DrawPos, this.Pawn.Map, 1f);
                }
            }

            this.age++;

            if (this.age <= this.lastRegen + this.regenRate)
                return;

            this.lastRegen = this.age;
            var radiantPowerLevel = this.Pawn.health.hediffSet.GetFirstHediffOfDef(RadiantDefOf.RA_KnightRadiant, false).Severity;

            // dead
            if (this.Pawn.Dead)
            {
                ResurrectionUtility.Resurrect(this.Pawn);
                this.UsePower(radiantPowerLevel);

                // skip further healing
                return;
            }

            // missing parts
            var missingParts = this.Pawn.health.hediffSet.GetHediffsTendable().OfType<Hediff_MissingPart>()
                .Where(x => x.TendableNow() && x.Bleeding);

            var sealedParts = false;

            foreach (var missingPart in missingParts)
            {
                // seal up all parts
                Log.Message($"{this.Pawn.LabelShort} is healing missing part |{missingPart}| with severity {missingPart.Severity}");
                missingPart.Tended(100f);
                sealedParts = true;
            }

            if (sealedParts)
            {
                this.UsePower(radiantPowerLevel);

                // skip further healing
                return;
            }

            // bleeding injuries
            var hediffInjury = this.Pawn.health.hediffSet.GetHediffs<Hediff_Injury>()
                .Where(x => this.Pawn.health.hediffSet.GetInjuredParts().Any(y => x.Part == y) && x.CanHealNaturally() && !x.IsPermanent() && x.Bleeding)
                .FirstOrDefault();

            if (hediffInjury != null)
            {
                Log.Message($"{this.Pawn.LabelShort} is healing bleeding injury |{hediffInjury}| with severity {2.5f * radiantPowerLevel}");
                hediffInjury.Heal(3.5f * radiantPowerLevel);
                this.UsePower(radiantPowerLevel);

                // skip further healing
                return;
            }

            // blood loss
            var firstHediffOfDef = this.Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss, false);

            if (firstHediffOfDef != null)
            {
                Log.Message($"{this.Pawn.LabelShort} is healing blood loss |{firstHediffOfDef}| which has severity |{firstHediffOfDef.SeverityLabel}|{firstHediffOfDef.Severity}|");
                //this.Pawn.health.RemoveHediff(firstHediffOfDef);
                firstHediffOfDef.Heal(1f * radiantPowerLevel);
                this.UsePower(radiantPowerLevel);

                // skip further healing
                return;
            }

            // infections
            firstHediffOfDef = this.Pawn.health.hediffSet.hediffs
                .FirstOrDefault(x => HediffComp_Regeneration.Infections.Any(y => y == x.def) || x.def.makesSickThought);

            if (firstHediffOfDef != null)
            {
                Log.Message($"{this.Pawn.LabelShort} is healing infection |{firstHediffOfDef}| with severity {2.5f * radiantPowerLevel} which has severity |{firstHediffOfDef.SeverityLabel}|{firstHediffOfDef.Severity}|");
                firstHediffOfDef.Heal(2.5f * radiantPowerLevel);
                this.UsePower(radiantPowerLevel);

                // skip further healing
                return;
            }

            // non-bleeding injuries
            hediffInjury = this.Pawn.health.hediffSet.GetHediffs<Hediff_Injury>()
                .FirstOrDefault(x => this.Pawn.health.hediffSet.GetInjuredParts().Any(y => x.Part == y) && x.CanHealNaturally() && !x.IsPermanent());

            if (hediffInjury != null)
            {
                Log.Message($"{this.Pawn.LabelShort} is healing non-bleeding injury |{hediffInjury}| with severity {2.5f * radiantPowerLevel}");
                hediffInjury.Heal(2.5f * radiantPowerLevel);
                this.UsePower(radiantPowerLevel);

                // skip further healing
                return;
            }

            // bad conditions
            firstHediffOfDef = this.Pawn.health.hediffSet.hediffs
                .FirstOrDefault(x => HediffComp_Regeneration.BadConditions.Any(y => y == x.def));

            if (firstHediffOfDef != null)
            {

                if (firstHediffOfDef.def == HediffDefOf.Asthma)
                {
                    Log.Message($"{this.Pawn.LabelShort} is removing |{firstHediffOfDef}|");

                    // remove the asthma and skip further healing
                    this.Pawn.health.RemoveHediff(firstHediffOfDef);
                    this.UsePower(radiantPowerLevel);

                    return;
                }

                Log.Message($"{this.Pawn.LabelShort} is healing non-bleeding injury |{firstHediffOfDef}| with severity {2.5f * radiantPowerLevel}");
                firstHediffOfDef.Heal(2.5f * radiantPowerLevel);
                this.UsePower(radiantPowerLevel);

                // skip further healing
                return;
            }

            // scars
            hediffInjury = this.Pawn.health.hediffSet.GetHediffs<Hediff_Injury>()
                .FirstOrDefault(x => this.Pawn.health.hediffSet.GetInjuredParts().Any(y => x.Part == y) && x.IsPermanent());

            if (hediffInjury != null)
            {
                Log.Message($"{this.Pawn.LabelShort} is healing scar |{hediffInjury}| with severity {2.5f * radiantPowerLevel}");
                hediffInjury.Heal(0.5f * radiantPowerLevel);
                this.UsePower(radiantPowerLevel);

                // skip further healing
                return;
            }

            // addictions
            // consciousness
            // other body conditions

            // regrow parts -*** only do this if not removed for surgery??
            var part = this.Pawn.health.hediffSet.GetMissingPartsCommonAncestors().FirstOrDefault()?.Part;

            if (part == null || !HediffComp_Regeneration.MissingPartXref.TryGetValue(part.def.defName, out string defName))
                return;

            Log.Message($"{this.Pawn.LabelShort} is starting to regrow |{part}|");
            this.Pawn.health.RestorePart(part, null, true);
            this.Pawn.health.AddHediff(HediffDef.Named(defName), part, null);
            this.UsePower(radiantPowerLevel);
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref this.initialized, "initialized", false, false);
            Scribe_Values.Look(ref this.age, "age", 0, false);
            Scribe_Values.Look(ref this.regenRate, "regenRate", 300, false);
            Scribe_Values.Look(ref this.lastRegen, "lastRegen", 0, false);
        }

        private void UsePower(float radiantPowerLevel)
        {
            this.User.Stormlight.UsePower(RadiantDefOf.RA_Regenerate.StormlightCost);

            if (!this.Pawn.DrawPos.ShouldSpawnMotesAt(this.Pawn.Map) || this.Pawn.Map.moteCounter.SaturatedLowPriority)
                return;

            var moteThrown = (MoteThrown)ThingMaker.MakeThing(RadiantDefOf.Mote_Regen, null);
            moteThrown.Scale = 1f * radiantPowerLevel;
            moteThrown.rotationRate = Rand.Range(-60, 60);
            moteThrown.exactPosition = this.Pawn.DrawPos;
            moteThrown.SetVelocity(Rand.Range(0, 360), Rand.Range(0.2f, 0.3f));
            GenSpawn.Spawn(moteThrown, this.Pawn.DrawPos.ToIntVec3(), this.Pawn.Map);
        }
    }
}