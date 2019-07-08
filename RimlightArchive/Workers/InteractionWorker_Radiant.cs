using RimWorld;
using Verse;

namespace RimlightArchive.Workers
{
    class InteractionWorker_Radiant : InteractionWorker
    {
        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            return 0f;
        }
    }
}