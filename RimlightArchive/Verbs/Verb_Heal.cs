using System;
using System.Collections.Generic;
using System.Linq;

using AbilityUser;
using Harmony;
using Verse;

namespace RimlightArchive.Verbs
{
    class Verb_AdvancedHeal : Verb_UseAbility
    {
        public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
        {
            return false;
        }

        protected override bool TryCastShot()
        {
            return true;
        }

    }
}
