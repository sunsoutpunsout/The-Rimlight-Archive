using System.Collections.Generic;

using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Sound;

namespace RimlightArchive.Things
{
    public class LashedPawnLeaving : Skyfaller, IActiveDropPod, IThingHolder
    {
        public int groupID = -1;
        public int destinationTile = -1;
        public TransportPodsArrivalAction arrivalAction;
        private bool alreadyLeft;
        private static List<Thing> tmpActiveDropPods = new List<Thing>();

        public ActiveDropPodInfo Contents
        {
            get
            {
                return ((LashedPawn)this.innerContainer[0]).Contents;
            }
            set
            {
                ((LashedPawn)this.innerContainer[0]).Contents = value;
            }
        }

        // ***
        public Graphic PawnGraphic { get; set; }
        // ***
        public override Graphic Graphic => this.PawnGraphic;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.groupID, "groupID", 0, false);
            Scribe_Values.Look<int>(ref this.destinationTile, "destinationTile", 0, false);
            Scribe_Deep.Look<TransportPodsArrivalAction>(ref this.arrivalAction, "arrivalAction", new object[0]);
            Scribe_Values.Look<bool>(ref this.alreadyLeft, "alreadyLeft", false, false);
        }

        protected override void LeaveMap()
        {
            if (this.alreadyLeft)
            {
                base.LeaveMap();

                return;
            }

            if (this.groupID < 0)
            {
                Log.Error("Drop pod left the map, but its group ID is " + this.groupID, false);
                this.Destroy(DestroyMode.Vanish);

                return;
            }

            if (this.destinationTile < 0)
            {
                Log.Error("Drop pod left the map, but its destination tile is " + this.destinationTile, false);
                this.Destroy(DestroyMode.Vanish);

                return;
            }

            TravelingTransportPods travelingTransportPods = (TravelingTransportPods)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.TravelingTransportPods);
            travelingTransportPods.Tile = base.Map.Tile;
            travelingTransportPods.SetFaction(Faction.OfPlayer);
            travelingTransportPods.destinationTile = this.destinationTile;
            travelingTransportPods.arrivalAction = this.arrivalAction;
            Find.WorldObjects.Add(travelingTransportPods);
            LashedPawnLeaving.tmpActiveDropPods.Clear();
            LashedPawnLeaving.tmpActiveDropPods.AddRange(base.Map.listerThings.ThingsInGroup(ThingRequestGroup.ActiveDropPod));

            foreach (var pawn in LashedPawnLeaving.tmpActiveDropPods)
            {
                if (!(pawn is LashedPawnLeaving lashedPawn)
                    || lashedPawn.groupID != this.groupID)
                {
                    return;
                }

                lashedPawn.alreadyLeft = true;
                travelingTransportPods.AddPod(lashedPawn.Contents, true);
                lashedPawn.Contents = null;
                lashedPawn.Destroy(DestroyMode.Vanish);
            }
        }
    }
}
