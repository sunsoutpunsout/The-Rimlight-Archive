using System.Collections.Generic;

using RimWorld;
using UnityEngine;
using Verse;

using RimlightArchive.Comps;
using RimlightArchive.Defs;

namespace RimlightArchive.Needs
{
    [StaticConstructorOnStartup]
    public class Need_Stormlight : Need
    {
        private const float threshFirst = 0.25f;
        private const float threshSecond = 0.5f;
        private const float threshThird = 0.75f;

        private int needSign = 0;
        private float lastNeed = 0;
        
        public Need_Stormlight(Pawn pawn) : base(pawn)
        {
            this.threshPercents = new List<float>
            {
                threshFirst / this.MaxLevel,
                threshSecond / this.MaxLevel,
                threshThird / this.MaxLevel
            };
        }

        public override string GetTipString() => $"{this.LabelCap}: {this.CurLevel / .01f:n2}{System.Environment.NewLine}{this.def.description}";
        public override float MaxLevel => this.pawn.GetComp<CompAbilityUser_Investiture>().RadiantLevel;        
        public override int GUIChangeArrow => this.needSign;

        public override void SetInitialLevel()
        {
            this.CurLevel = 0;
        }

        public override void NeedInterval()
        {
            if (base.IsFrozen)
            {
                return;
            }
            
            if (!Utils.IsPawnRadiant(pawn, out var comp))
            {
                this.curLevelInt = 0;

                return;
            }
            
            if (this.pawn.Map != null
                && !this.pawn.Dead
                && !this.pawn.NonHumanlikeOrWildMan()
                && pawn.Map.GameConditionManager.ConditionIsActive(RadiantDefOf.RA_Highstorm))
            {
                //*** HIGH STORM
                this.CurLevel = this.MaxLevel;
            }

            this.AdjustThresh();
            var diff = Mathf.Round(this.CurLevel * 100f) - Mathf.Round(lastNeed * 100f);
            Log.Message($"NeedInterval |CurLevel {this.CurLevel}|IsFrozen {base.IsFrozen}|lastNeed {lastNeed}|diff {diff}|sign {(diff == 0f ? 0 : (int)Mathf.Sign(diff))}|");
            this.needSign = diff == 0f ? 0 : (int)Mathf.Sign(diff);
            this.lastNeed = this.CurLevel;
        }

        public bool CanUsePower(float amount) => this.CurLevel - amount >= 0f;

        public void UsePower(float amount)
        {
            this.lastNeed = this.CurLevel;
            this.CurLevel -= amount;
            this.needSign = -1;
            Log.Message($"UsePower |lastNeed {lastNeed}|CurLevel {this.CurLevel}|");
        }
        
        private void AdjustThresh()
        {
            this.threshPercents.Clear();
            this.threshPercents.Add(threshFirst / this.MaxLevel);
            this.threshPercents.Add(threshSecond / this.MaxLevel);
            this.threshPercents.Add(threshThird / this.MaxLevel);
        }
    }
}