using Verse;

using RimlightArchive.Comps;

namespace RimlightArchive
{
    public class RadiantData : IExposable
    {
        public Pawn User;

        public RadiantData(CompAbilityUser_Investiture newUser)
        {
            this.User = newUser.AbilityUser;
        }
               
        public void ExposeData()
        {
            Scribe_References.Look(ref this.User, "User", false);
        }
    }
}