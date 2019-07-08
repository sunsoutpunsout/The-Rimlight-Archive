﻿using System.Collections.Generic;

using RimWorld;
using Verse;
using Verse.Sound;

namespace RimlightArchive.Things
{
    public class LashedPawn : Thing, IActiveDropPod, IThingHolder
    {
        public int age;

        private ActiveDropPodInfo contents;

        public ActiveDropPodInfo Contents
        {
            get
            {
                return this.contents;
            }
            set
            {
                if (this.contents != null)
                {
                    this.contents.parent = null;
                }
                if (value != null)
                {
                    value.parent = this;
                }
                this.contents = value;
            }
        }
        
        // ***
        public Graphic PawnGraphic { get; set; }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.age, "age", 0, false);
            Scribe_Deep.Look(ref this.contents, "contents", new object[]
            {
                this
            });
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return null;
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
            if (this.contents != null)
            {
                outChildren.Add(this.contents);
            }
        }

        // ***
        public override Graphic Graphic => this.PawnGraphic;

        public override void Tick()
        {
            if (this.contents == null)
            {
                return;
            }
            this.contents.innerContainer.ThingOwnerTick(true);
            if (base.Spawned)
            {
                this.age++;
                if (this.age > this.contents.openDelay)
                {
                    this.PodOpen();
                }
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (this.contents != null)
            {
                this.contents.innerContainer.ClearAndDestroyContents(DestroyMode.Vanish);
            }
            Map map = base.Map;
            base.Destroy(mode);
            if (mode == DestroyMode.KillFinalize)
            {
                for (int i = 0; i < 1; i++)
                {
                    Thing thing = ThingMaker.MakeThing(ThingDefOf.ChunkSlagSteel, null);
                    GenPlace.TryPlaceThing(thing, base.Position, map, ThingPlaceMode.Near, null, null);
                }
            }
        }

        private void PodOpen()
        {
            for (int i = this.contents.innerContainer.Count - 1; i >= 0; i--)
            {
                Thing thing = this.contents.innerContainer[i];
                Thing thing2;
                GenPlace.TryPlaceThing(thing, base.Position, base.Map, ThingPlaceMode.Near, out thing2, delegate (Thing placedThing, int count)
                {
                    if (Find.TickManager.TicksGame < 1200 && TutorSystem.TutorialMode && placedThing.def.category == ThingCategory.Item)
                    {
                        Find.TutorialState.AddStartingItem(placedThing);
                    }
                }, null);
                Pawn pawn = thing2 as Pawn;
                if (pawn != null)
                {
                    if (pawn.RaceProps.Humanlike)
                    {
                        TaleRecorder.RecordTale(TaleDefOf.LandedInPod, new object[]
                        {
                            pawn
                        });
                    }
                    if (pawn.IsColonist && pawn.Spawned && !base.Map.IsPlayerHome)
                    {
                        pawn.drafter.Drafted = true;
                    }
                }
            }
            this.contents.innerContainer.ClearAndDestroyContents(DestroyMode.Vanish);
            if (this.contents.leaveSlag)
            {
                for (int j = 0; j < 1; j++)
                {
                    Thing thing3 = ThingMaker.MakeThing(ThingDefOf.ChunkSlagSteel, null);
                    GenPlace.TryPlaceThing(thing3, base.Position, base.Map, ThingPlaceMode.Near, null, null);
                }
            }
            SoundDefOf.DropPod_Open.PlayOneShot(new TargetInfo(base.Position, base.Map, false));
            this.Destroy(DestroyMode.Vanish);
        }
    }
}
