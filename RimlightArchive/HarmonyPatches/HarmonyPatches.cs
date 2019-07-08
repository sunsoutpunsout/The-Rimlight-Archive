using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Harmony;
using RimWorld;
using Verse;
using Verse.AI;

using RimlightArchive.Apparel;
using RimlightArchive.Defs;

namespace RimlightArchive.HarmonyPatches
{
    [StaticConstructorOnStartup]
    partial class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = HarmonyInstance.Create("rimworld.rimlightarchive");
            //harmony.PatchAll(Assembly.GetExecutingAssembly());

            //Log.Message
                //"rimlightarchive Harmony Patches:" + Environment.NewLine +
                //"  Prefix:" + Environment.NewLine +
                //"    Pawn_HealthTracker.MakeDowned - not blocking" + Environment.NewLine +
                //"    Dialog_FormCaravan.PostOpen" + Environment.NewLine +
                //"    CaravanExitMapUtility.ExitMapAndCreateCaravan(IEnumerable<Pawn>, Faction, int)" + Environment.NewLine +
                //"    CaravanExitMapUtility.ExitMapAndCreateCaravan(IEnumerable<Pawn>, Faction, int, int)" + Environment.NewLine +
                //"    MakeUndowned - Priority First" + Environment.NewLine +
                //"    Pawn.Kill - Priority First" + Environment.NewLine +
                //"    Pawn_EquipmentTracker.AddEquipment - Priority First" + Environment.NewLine +
                //"    Pawn_EquipmentTracker.TryDropEquipment - Priority First" + Environment.NewLine +
                //"    Pawn_EquipmentTracker.MakeRoomFor - Priority First" + Environment.NewLine +
                //"    ScribeSaver.InitSaving" + Environment.NewLine +
                //"    SettlementAbandonUtility.Abandon" + Environment.NewLine +
                //"  Postfix:" + Environment.NewLine +
                //"    Pawn_TraderTracker.ColonyThingsWillingToBuy" + Environment.NewLine +
                //"    TradeShip.ColonyThingsWillingToBuy" + Environment.NewLine +
                //"    Window.PreClose" + Environment.NewLine +
                //"    ReservationManager.CanReserve" + Environment.NewLine +
                //"    CaravanFormingUtility.StopFormingCaravan" + Environment.NewLine +
                //"    Pawn_DraftController.Drafted { set; }" + Environment.NewLine +
                //"    WealthWatcher.ForceRecount" + Environment.NewLine +
                //"    MakeDowned - Priority First" + Environment.NewLine +
                //"    Pawn_EquipmentTracker.TryDropEquipment - Priority First" + Environment.NewLine +
                //"    Pawn.Kill - Priority First" + Environment.NewLine +
                //"    Root.Start - Priority Last");
        }
    }

    [HarmonyPatch(typeof(Pawn_HealthTracker), "MakeDowned")]
    static class Pawn_HealthTracker_MakeDowned_Patch
    {
        static void Postfix(Pawn_HealthTracker __instance, Pawn ___pawn)
        {
            Utils.Radiantify(___pawn);
        }
    }

    [HarmonyPatch(typeof(Pawn_HealthTracker), "PreApplyDamage")]
    static class Pawn_HealthTracker_PreApplyDamage_Patch
    {
        static void Postfix(Pawn_HealthTracker __instance, Pawn ___pawn, ref DamageInfo dinfo, out bool absorbed)
        {
            absorbed = false;

            if (!Utils.IsPawnRadiant(___pawn, out var comp))
                return;

            var amount = dinfo.Amount * 0.25f * Math.Max(1f - comp.Stormlight.CurLevelPercentage, 0.001f);
            var stormlightAmount = 1f - (dinfo.Amount - amount) / dinfo.Amount;
            //Log.Message$"PreApplyDamage_Patch|CurLevel {comp.Stormlight.CurLevel}|stormlightAmount {stormlightAmount}|amount {amount}|can use? {comp.Stormlight.CanUsePower(stormlightAmount)}|stormlight% {comp.Stormlight.CurLevelPercentage:P1}|dinfo.Amount {dinfo.Amount}|");

            if (!comp.Stormlight.CanUsePower(stormlightAmount))
                return;

            dinfo.SetAmount(amount);
            comp.Stormlight.UsePower(stormlightAmount);
        }
    }

    //[HarmonyPatch(typeof(Pawn_HealthTracker), "PostApplyDamage")]
    //static class Pawn_HealthTracker_PostApplyDamage_Patch
    //{
    //    static bool Prefix(Pawn_HealthTracker __instance, Pawn ___pawn, ref DamageInfo dinfo, ref float totalDamageDealt)
    //    {
    //        var deadCheck = Pawn_HealthTracker_PostApplyDamage_Patch.ShouldBeDead(__instance, ___pawn);

    //        if (!deadCheck || !Utils.IsPawnRadiant(___pawn, out var comp))
    //            return true;

    //        Log.Message($"Pawn_HealthTracker_PostApplyDamage_Patch |{___pawn}|dead {___pawn.Dead}|CauseDeathNow {__instance.hediffSet.hediffs.Any(x => x.CauseDeathNow())}| ShouldBeDeadFromRequiredCapacity {__instance.ShouldBeDeadFromRequiredCapacity()}|dinfo {dinfo}|totalDamageDealt {totalDamageDealt}|");
    //        Log.Message($"comp {comp}|Stormlight {comp.Stormlight}|");

    //        return false;
    //    }

    //    private static bool ShouldBeDead(Pawn_HealthTracker instance, Pawn pawn)
    //    {
    //        return instance.Dead
    //            || instance.hediffSet.hediffs.Any(x => x.CauseDeathNow())
    //            || instance.ShouldBeDeadFromRequiredCapacity() != null
    //            || PawnCapacityUtility.CalculatePartEfficiency(instance.hediffSet, pawn.RaceProps.body.corePart, false, null) <= 0.0001f
    //            || instance.ShouldBeDeadFromLethalDamageThreshold();
    //    }
    //}

    //[HarmonyPatch(typeof(Thing), "Destroy")]
    //static class Thing_Destroy_Patch
    //{
    //    static bool Prefix(Thing __instance, ref DestroyMode mode)
    //    {
    //        if (__instance is Pawn)
    //            Log.Message($"dead - |{__instance}|mode {mode}|");

    //        if (mode != DestroyMode.KillFinalize
    //            || !(__instance is Pawn pawn)
    //            || !Utils.IsPawnRadiant(pawn, out var comp))
    //            return true;

    //        Log.Message($"dead? {pawn.Dead}|comp {comp}|CauseDeathNow {pawn.health.hediffSet.hediffs.Any(x => x.CauseDeathNow())}| ShouldBeDeadFromRequiredCapacity {pawn.health.ShouldBeDeadFromRequiredCapacity()}|");
    //        Log.Message($"dead3 {comp.Stormlight}|");

    //        if (!comp.Stormlight.CanUsePower(0.01f))
    //            return true;

    //        comp.Stormlight.UsePower(0.01f);

    //        return false;
    //    }
    //}

    [HarmonyPatch(typeof(DamageWorker_Cut), "ApplySpecialEffectsToPart")]
    static class DamageWorker_Cut_Patch
    {
        static void Postfix(DamageWorker_Cut __instance, Pawn pawn, DamageInfo dinfo)
        {
            if (pawn.DestroyedOrNull() || pawn.Dead || dinfo.Def != RadiantDefOf.RA_Shardburn)
                return;

            var missingParts = pawn.health.hediffSet.GetHediffsTendable().OfType<Hediff_MissingPart>();

            // seal up any bleeding parts if caused by shardburn
            missingParts
                .Where(x => x.TendableNow() && x.Bleeding && x.Part == dinfo.HitPart)
                .ToList()
                .ForEach(x => x.Tended(100f));
        }
    }

    //[HarmonyPatch(typeof(Pawn), "Kill")]
    //static class Pawn_Kill_Patch
    //{
    //    static bool Prefix(Pawn __instance, DamageInfo? dinfo)
    //    {
    //        Log.Message($"try rezzing! |radiant? {Utils.IsPawnRadiant(__instance)}|dinfo {dinfo}|destroyed {__instance.Destroyed}|");
    //        if (!Utils.IsPawnRadiant(__instance))
    //        {
    //            return true;
    //        }

    //        if (!dinfo.HasValue)
    //            return false;

    //        var comp = __instance.GetComp<CompAbilityUserInvestiture>();
    //        Log.Message($"comp |{comp}|use power? {comp.Stormlight.CanUsePower(comp.ActualStormlightCost(RadiantDefOf.RA_Regenerate))}|");

    //        if (comp == null || !comp.Stormlight.CanUsePower(comp.ActualStormlightCost(RadiantDefOf.RA_Regenerate) * dinfo.Value.Amount))
    //            return true;

    //        comp.Stormlight.UsePower(comp.ActualStormlightCost(RadiantDefOf.RA_Regenerate));

    //        return false;
    //    }
    //    static void Postfix(Pawn __instance)
    //    {
    //        Log.Message($"try rezzing! |radiant? {Utils.IsPawnRadiant(__instance)}|dead {__instance.Dead}|destroyed {__instance.Destroyed}|");
    //        if (!__instance.Dead
    //            || !Utils.IsPawnRadiant(__instance))
    //        {
    //            return;
    //        }

    //        var comp = __instance.GetComp<CompAbilityUserInvestiture>();

    //        if (comp == null || !comp.Stormlight.CanUsePower(comp.ActualStormlightCost(RadiantDefOf.RA_Regenerate)))
    //            return;

    //        Log.Message($"comp |{comp}|use power? {comp.Stormlight.CanUsePower(comp.ActualStormlightCost(RadiantDefOf.RA_Regenerate))}|");
    //        Log.Message("rezzing!");
    //        ResurrectionUtility.Resurrect(__instance);
    //        comp.Stormlight.UsePower(comp.ActualStormlightCost(RadiantDefOf.RA_Regenerate));
    //    }
    //}
    [HarmonyPatch(typeof(ArmorUtility), "GetPostArmorDamage")]
    static class ArmorUtility_GetPostArmorDamage_Patch
    {
        static bool Prefix(Pawn pawn, float amount, float armorPenetration, BodyPartRecord part, ref DamageDef damageDef, out bool deflectedByMetalArmor, out bool diminishedByMetalArmor)
        {
            deflectedByMetalArmor = false;
            diminishedByMetalArmor = false;

            if (pawn.apparel == null)
            {
                return true;
            }

            var shardplate = pawn.apparel.WornApparel.OfType<Shardplate>().FirstOrDefault(x => x.def.apparel.CoversBodyPart(part));

            if (shardplate == null)
            {
                return true;
            }

            //Log.Message$"shardplate |{shardplate}|part {part}|damageDef {damageDef}|");

            shardplate.stormlight -= (int)amount;
            shardplate.AbsorbedDamage(0f, amount);
            deflectedByMetalArmor = true;

            if (shardplate.stormlight < 0)
            {
                shardplate.Break();
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    static class Pawn_GetGizmos_Patch
    {
        static void Postfix(ref IEnumerable<Gizmo> __result, ref Pawn __instance)
        {
            if (__instance == null
                || !__instance.RaceProps.Humanlike
                || __result == null
                || !__result.Any()
                || __result.OfType<GizmoStormlightPawnStatus>().Any()
                || !__instance.Faction.Equals(Faction.OfPlayer)
                || __instance.story == null
                || !__instance.story.traits.allTraits.Any()
                || Find.Selector.NumSelected != 1
                || __instance.Destroyed
                || __instance.Dead)
            {
                return;
            }
            
            if (!Utils.IsPawnRadiant(__instance, out var comp))
                return;

            var gizmoList = __result.ToList();
            var gizmo = new GizmoStormlightPawnStatus
            {
                // All gizmo properties done in GizmoStormlightStatus
                // Make it the first thing you see
                Pawn = __instance,
                order = -101f
            };

            gizmoList.Add(gizmo);
            __result = gizmoList;
        }
    }

    [HarmonyPatch(typeof(MentalStateHandler), "TryStartMentalState")]
    static class MentalStateHandler_TryStartMentalState_Patch
    {
        static void Postfix(MentalStateHandler __instance, bool __result, MentalStateDef stateDef, Pawn ___pawn)
        {
            if (!__result)
                return;

            Utils.Radiantify(___pawn);
        }
    }
}
