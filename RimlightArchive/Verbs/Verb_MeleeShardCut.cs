using RimWorld;
using UnityEngine;
using Verse;

using RimlightArchive.Defs;

namespace RimlightArchive.Verbs
{
    /// <summary>
    /// Handles shardblade cuts to single and adjacent targets.
    /// </summary>
    [StaticConstructorOnStartup]
    public class Verb_MeleeShardCut : Verb_MeleeAttack
    {
        private static readonly Color color = new Color(160f, 160f, 160f);
        private static readonly Material mat = MaterialPool.MatFrom("Spells/cleave_straight", ShaderDatabase.Transparent, Verb_MeleeShardCut.color);

        protected override DamageWorker.DamageResult ApplyMeleeDamageToTarget(LocalTargetInfo target)
        {
            var damageResult = new DamageWorker.DamageResult();
            var armorPen = Utils.IsWearingShardplate(target.Thing as Pawn) ? 0f : 999f;
            var dinfo = new DamageInfo(RadiantDefOf.RA_Shardburn, this.tool.power, armorPen, -1f, this.CasterPawn, null, null, DamageInfo.SourceCategory.ThingOrUnknown);
            damageResult.totalDamageDealt = Mathf.Min((float)target.Thing.HitPoints, dinfo.Amount);

            Log.Message($"ApplyMeleeDamageToTarget|target {target}|armorPen {armorPen}|dinfo {dinfo}|damageResult {damageResult}|");

            foreach (var adjacent in GenAdj.AdjacentCells)
            {
                var intVec = target.Cell + adjacent;
                var cleaveVictim = intVec.GetFirstPawn(target.Thing.Map);

                // add chance to hit friendly if in mental state?
                if (cleaveVictim == null || cleaveVictim.Faction == caster.Faction)
                    continue;
                
                cleaveVictim.TakeDamage(dinfo);
                MoteMaker.ThrowMicroSparks(cleaveVictim.Position.ToVector3(), target.Thing.Map);
                this.DrawCleaving(cleaveVictim, base.CasterPawn, 10);
            }

            target.Thing.TakeDamage(dinfo);

            return damageResult;
        }

        private void DrawCleaving(Pawn cleavedPawn, Pawn caster, int magnitude)
        {
            if (caster.Dead || caster.Downed)
            {
                return;
            }

            var vector = cleavedPawn.Position.ToVector3();
            vector.y = Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead);
            var hyp = Mathf.Sqrt((Mathf.Pow(caster.Position.x - cleavedPawn.Position.x, 2)) + (Mathf.Pow(caster.Position.z - cleavedPawn.Position.z, 2)));
            var angleRad = Mathf.Asin(Mathf.Abs(caster.Position.x - cleavedPawn.Position.x) / hyp);
            var angle = Mathf.Rad2Deg * angleRad;
            var s = new Vector3(3f, 3f, 5f);
            var matrix = default(Matrix4x4);
            matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
            Graphics.DrawMesh(MeshPool.plane10, matrix, Verb_MeleeShardCut.mat, 0);
        }
    }
}
