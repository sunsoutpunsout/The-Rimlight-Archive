using System.Collections.Generic;

using UnityEngine;
using Verse;

using RimlightArchive.Defs;

namespace RimlightArchive
{
    public class RimlightArchiveSettings : ModSettings
    {
        // radiant
        public static bool aICanCast;
        public static bool threatRequired;
        public static bool RA_Bondsmith;
        public static bool RA_Dustbringer;
        public static bool RA_Edgedancer;
        public static bool RA_Elsecaller;
        public static bool RA_Lightweaver;
        public static bool RA_Skybreaker;
        public static bool RA_Stoneward;
        public static bool RA_Truthwatcher;
        public static bool RA_Willshaper;
        public static bool RA_Windrunner;
        public static float baseRadiantChance;

        // shards
        public static float baseShardbladeChance;
        public static float baseShardplateChance;

        // highstorms
        public static bool toggleHighstorms;
        public static float baseHighstormChance;
        public static float baseHighstormStrikeChance;
        public static float baseHighstormDamageMulti;
        public static float baseHighstormDamageTick;

        public Vector2 scrollPos;

        public RimlightArchiveSettings() : base() { }

        /// <summary>
        /// The part that writes our settings to file. Note that saving is by ref.
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref aICanCast, "aICanCast", true);
            Scribe_Values.Look(ref threatRequired, "threatRequired", true);
            Scribe_Values.Look(ref baseRadiantChance, "baseRadiantChance", 0.2f);
            Scribe_Values.Look(ref RA_Bondsmith, "RA_Bondsmith", false);
            Scribe_Values.Look(ref RA_Dustbringer, "RA_Dustbringer", false);
            Scribe_Values.Look(ref RA_Edgedancer, "RA_Edgedancer", false);
            Scribe_Values.Look(ref RA_Elsecaller, "RA_Elsecaller", false);
            Scribe_Values.Look(ref RA_Lightweaver, "RA_Lightweaver", false);
            Scribe_Values.Look(ref RA_Skybreaker, "RA_Skybreaker", false);
            Scribe_Values.Look(ref RA_Stoneward, "RA_Stoneward", false);
            Scribe_Values.Look(ref RA_Truthwatcher, "RA_Truthwatcher", false);
            Scribe_Values.Look(ref RA_Willshaper, "RA_Willshaper", false);
            Scribe_Values.Look(ref RA_Windrunner, "RA_Windrunner", true);

            Scribe_Values.Look(ref baseShardbladeChance, "baseShardbladeChance", 0.2f);
            Scribe_Values.Look(ref baseShardplateChance, "baseShardplateChance", 0.2f);

            Scribe_Values.Look(ref toggleHighstorms, "toggleHighstorms");
            Scribe_Values.Look(ref baseHighstormChance, "baseHighstormChance", 0.2f);
            Scribe_Values.Look(ref baseHighstormStrikeChance, "baseHighstormStrikeChance", 0.2f);
            Scribe_Values.Look(ref baseHighstormDamageMulti, "baseHighstormDamageMulti", 0.2f);
            Scribe_Values.Look(ref baseHighstormDamageTick, "baseHighstormDamageTick", 1000f);
        }
    }

    public class RimlightArchive : Mod
    {
        /// <summary>
        /// A reference to our settings.
        /// </summary>
        RimlightArchiveSettings settings;

        /// <summary>
        /// A mandatory constructor which resolves the reference to our settings.
        /// </summary>
        /// <param name="content"></param>
        public RimlightArchive(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<RimlightArchiveSettings>();
        }

        /// <summary>
        /// The (optional) GUI part to set your settings.
        /// </summary>
        /// <param name="inRect">A Unity Rect with the size of the settings window.</param>
        public override void DoSettingsWindowContents(Rect inRect)
        {
            var spacer = 6f;
            var mainSection = new Listing_Standard { ColumnWidth = (inRect.width - 34f) / 2f };
            var outRect = new Rect(0f, 0f, inRect.width, inRect.height - 30f);
            var viewRect = new Rect(0f, 0f, inRect.width - 16f, inRect.height - 16f);
            mainSection.Begin(inRect);
            Widgets.BeginScrollView(outRect, ref this.settings.scrollPos, viewRect);
            mainSection.Gap(spacer);

            // column 1
            mainSection.Label("RadiantOptions".Translate());
            mainSection.GapLine(0f);
            mainSection.Gap(spacer);

            mainSection.Label($"{"baseRadiantChance".Translate()} {RimlightArchiveSettings.baseRadiantChance:P1}", -1f);
            RimlightArchiveSettings.baseRadiantChance = mainSection.Slider(RimlightArchiveSettings.baseRadiantChance, 0f, 1f);
            mainSection.Gap(spacer);
            
            mainSection.CheckboxLabeled("AICanCast".Translate(), ref RimlightArchiveSettings.aICanCast, "AICanCast".Translate());
            mainSection.Gap(spacer);

            mainSection.CheckboxLabeled("threatRequired".Translate(), ref RimlightArchiveSettings.threatRequired, "threatRequired".Translate());
            mainSection.Gap(spacer);

            mainSection.CheckboxLabeled(RadiantDefOf.RA_Bondsmith.defName.Translate(), ref RimlightArchiveSettings.RA_Bondsmith, RadiantDefOf.RA_Bondsmith.description);
            mainSection.CheckboxLabeled(RadiantDefOf.RA_Dustbringer.defName.Translate(), ref RimlightArchiveSettings.RA_Dustbringer, RadiantDefOf.RA_Dustbringer.description);
            mainSection.CheckboxLabeled(RadiantDefOf.RA_Edgedancer.defName.Translate(), ref RimlightArchiveSettings.RA_Edgedancer, RadiantDefOf.RA_Edgedancer.description);
            mainSection.CheckboxLabeled(RadiantDefOf.RA_Elsecaller.defName.Translate(), ref RimlightArchiveSettings.RA_Elsecaller, RadiantDefOf.RA_Elsecaller.description);
            mainSection.CheckboxLabeled(RadiantDefOf.RA_Lightweaver.defName.Translate(), ref RimlightArchiveSettings.RA_Lightweaver, RadiantDefOf.RA_Lightweaver.description);
            mainSection.CheckboxLabeled(RadiantDefOf.RA_Skybreaker.defName.Translate(), ref RimlightArchiveSettings.RA_Skybreaker, RadiantDefOf.RA_Skybreaker.description);
            mainSection.CheckboxLabeled(RadiantDefOf.RA_Stoneward.defName.Translate(), ref RimlightArchiveSettings.RA_Stoneward, RadiantDefOf.RA_Stoneward.description);
            mainSection.CheckboxLabeled(RadiantDefOf.RA_Truthwatcher.defName.Translate(), ref RimlightArchiveSettings.RA_Truthwatcher, RadiantDefOf.RA_Truthwatcher.description);
            mainSection.CheckboxLabeled(RadiantDefOf.RA_Willshaper.defName.Translate(), ref RimlightArchiveSettings.RA_Willshaper, RadiantDefOf.RA_Willshaper.description);
            mainSection.CheckboxLabeled(RadiantDefOf.RA_Windrunner.defName.Translate(), ref RimlightArchiveSettings.RA_Windrunner, RadiantDefOf.RA_Windrunner.description);

            mainSection.Gap(spacer);

            mainSection.Label("ShardOptions".Translate());
            mainSection.GapLine(0f);

            mainSection.Label($"{"baseShardbladeChance".Translate()} {RimlightArchiveSettings.baseShardbladeChance:P1}", -1f);
            RimlightArchiveSettings.baseShardbladeChance = mainSection.Slider(RimlightArchiveSettings.baseShardbladeChance, 0f, 1f);
            mainSection.Gap(spacer);

            mainSection.Label($"{"baseShardplateChance".Translate()} {RimlightArchiveSettings.baseShardplateChance:P1}", -1f);
            RimlightArchiveSettings.baseShardplateChance = mainSection.Slider(RimlightArchiveSettings.baseShardplateChance, 0f, 1f);
            mainSection.Gap(spacer);

            // column 2
            mainSection.NewColumn();
            mainSection.Gap(spacer);

            mainSection.Label("HighstormOptions".Translate());
            mainSection.GapLine(1f);

            mainSection.CheckboxLabeled("toggleHighstorms".Translate(), ref RimlightArchiveSettings.toggleHighstorms, "toggleHighstorms".Translate());
            mainSection.Gap(spacer);

            mainSection.Label($"{"baseHighstormChance".Translate()} {RimlightArchiveSettings.baseHighstormChance:P1}", -1f);
            RimlightArchiveSettings.baseHighstormChance = mainSection.Slider(RimlightArchiveSettings.baseHighstormChance, 0f, 1f);
            mainSection.Gap(spacer);

            mainSection.Label($"{"baseHighstormStrikeChance".Translate()} {RimlightArchiveSettings.baseHighstormStrikeChance:P1}", -1f);
            RimlightArchiveSettings.baseHighstormStrikeChance = mainSection.Slider(RimlightArchiveSettings.baseHighstormStrikeChance, 0f, 1f);
            mainSection.Gap(spacer);

            mainSection.Label($"{"baseHighstormDamageMulti".Translate()} {RimlightArchiveSettings.baseHighstormDamageMulti:P1}", -1f);
            RimlightArchiveSettings.baseHighstormDamageMulti = mainSection.Slider(RimlightArchiveSettings.baseHighstormDamageMulti, 0f, 1f);
            mainSection.Gap(spacer);

            mainSection.Label($"{"baseHighstormDamageTick".Translate()} {RimlightArchiveSettings.baseHighstormDamageTick:0}", -1f);
            RimlightArchiveSettings.baseHighstormDamageTick = mainSection.Slider(RimlightArchiveSettings.baseHighstormDamageTick, 1f, 100000f);
            mainSection.Gap(spacer);

            Widgets.EndScrollView();
            mainSection.End();
            base.DoSettingsWindowContents(inRect);
        }

        /// <summary>
        /// Override SettingsCategory to show up in the list of settings.
        /// Using .Translate() is optional, but does allow for localisation.
        /// </summary>
        /// <returns>The (translated) mod name.</returns>
        public override string SettingsCategory() => "RimlightArchive".Translate();
    }
}
