using AbilityUser;
using RimWorld;
using Verse;

namespace RimlightArchive.Defs
{
    [DefOf]
    public static class RadiantDefOf
    {
        public static AbilityDef RA_Adhesion;
        public static AbilityDef RA_Flight;
        public static AbilityDef RA_Launch;
        public static AbilityDef RA_MakeWindrunnerSquire;

        public static DamageDef RA_Shardburn;

        public static GameConditionDef RA_Highstorm;
        
        public static HediffDef RA_KnightRadiant;
        public static HediffDef RA_Radiant;
        public static HediffDef RA_Regeneration;
        public static HediffDef RA_ShardplateBuff;
        public static HediffDef RA_Uncertainty;

        // regrown parts
        public static HediffDef RA_ArmRegrowth;
        public static HediffDef RA_EarRegrowth;
        public static HediffDef RA_EyeRegrowth;
        public static HediffDef RA_FingerRegrowth;
        public static HediffDef RA_FootRegrowth;
        public static HediffDef RA_HandRegrowth;
        public static HediffDef RA_HeartRegrowth;
        public static HediffDef RA_JawRegrowth;
        public static HediffDef RA_KidneyRegrowth;
        public static HediffDef RA_LegRegrowth;
        public static HediffDef RA_LiverRegrowth;
        public static HediffDef RA_LungRegrowth;
        public static HediffDef RA_StandardRegrowth;
        public static HediffDef RA_SpineRegrowth;
        public static HediffDef RA_StomachRegrowth;
        public static HediffDef RA_ToeRegrowth;

        public static InteractionDef RA_RadiantAttemptFirstWords;
        public static InteractionDef RA_RadiantAttemptSecondWords;
        public static InteractionDef RA_RadiantAttemptFinalWords;
        public static InteractionDef RA_RadiantSpokeIdeal;

        public static NeedDef RA_Stormlight;

        // abilities
        public static RadiantAbilityDef RA_Regenerate;
        public static RadiantAbilityDef RA_Regrowth;

        public static StatDef StormlightMax;
        public static StatDef StormlightRechargeRate;

        public static ThingDef RA_SphereBag;
        public static ThingDef RA_Shardblade;
        public static ThingDef RA_Shardplate;
        public static ThingDef FlyingObject_Flight;
        public static ThingDef FlyingObject_Launch;
        public static ThingDef Mote_Regen;

        public static ThoughtDef RA_NewRadiantThought;

        public static TraitDef RA_Bondsmith;
        public static TraitDef RA_Dustbringer;
        public static TraitDef RA_Edgedancer;
        public static TraitDef RA_Elsecaller;
        public static TraitDef RA_Lightweaver;
        public static TraitDef RA_Skybreaker;
        public static TraitDef RA_Stoneward;
        public static TraitDef RA_Truthwatcher;
        public static TraitDef RA_Willshaper;
        public static TraitDef RA_Windrunner;

        public static WeatherDef RA_HighstormWindWeather;

        static RadiantDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(RadiantDefOf));
        }
    }
}
