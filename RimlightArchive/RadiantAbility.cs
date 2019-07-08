using AbilityUser;
using Verse;

using RimlightArchive.Comps;
using RimlightArchive.Defs;

namespace RimlightArchive
{
    /// <summary>
    /// Handler for pawn Radiant abilities.
    /// </summary>
    [StaticConstructorOnStartup]
    public class RadiantAbility : PawnAbility
    {
        public CompAbilityUser_Investiture User => this.Pawn.GetComp<CompAbilityUser_Investiture>();
        public RadiantAbilityDef AbilityDef => base.Def as RadiantAbilityDef;

        public RadiantAbility() : base() { }

        public RadiantAbility(CompAbilityUser abilityUser) : base(abilityUser)
        {
            this.abilityUser = abilityUser as CompAbilityUser_Investiture;
        }

        public RadiantAbility(AbilityData data) : base(data) { }

        public RadiantAbility(Pawn user, AbilityDef pdef) : base(user, pdef) {}
        
        /// <summary>
        /// Builds out a string for ability attributes.
        /// </summary>
        /// <param name="verbDef"></param>
        /// <returns></returns>
        public override string PostAbilityVerbCompDesc(VerbProperties_Ability verbDef)
        {
            var result = string.Empty;

            if (verbDef.abilityDef is RadiantAbilityDef radiantAbilityDef)
            {
                var cost = radiantAbilityDef.StormlightCost * 100;
                var text1 = "RA_AbilityDescBaseStormlightCost".Translate($"{radiantAbilityDef.StormlightCost * 100:n1}{System.Environment.NewLine}{"RA_AbilityDescAdjustedStormlightCost".Translate($"{cost:n1}")}{System.Environment.NewLine}");
                var text2 = this.User.CoolDown != 1f ?$"{"RA_AdjustedCooldown".Translate($"{this.MaxCastingTicks * this.User.CoolDown / 60:0.00}")}{System.Environment.NewLine}" : string.Empty;

                result = $"{text1}{text2}";
            }

            return result;
        }

        public override bool CanCastPowerCheck(AbilityContext context, out string reason)
        {
            if(!base.CanCastPowerCheck(context, out reason))
                return false;

            if (!this.User.Stormlight.CanUsePower(this.AbilityDef.StormlightCost))
            {
                reason = "RA_NotEnoughStormlight".Translate(base.Pawn.Label);

                return false;
            }

            return true;
        }
    }
}