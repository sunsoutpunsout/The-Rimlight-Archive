using UnityEngine;
using Verse;

using RimlightArchive.Comps;

namespace RimlightArchive.Apparel
{
    /// <summary>
    /// Handles the on-screen level of Stormlight in pawns.
    /// </summary>
    [StaticConstructorOnStartup]
    public class GizmoStormlightPawnStatus : Gizmo
    {
        private static readonly Texture2D FullTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.24f));
        private static readonly Texture2D EmptyTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);

        public Pawn Pawn { get; set; }

        public GizmoStormlightPawnStatus()
        {
            this.order = -110f;
        }

        public override float GetWidth(float maxWidth) => 140f;

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth)
        {
            if (this.Pawn.DestroyedOrNull() || this.Pawn.Dead)
                return new GizmoResult(GizmoState.Clear);

            var pawnInvestiture = this.Pawn.GetComp<CompAbilityUser_Investiture>();

            if (pawnInvestiture == null || pawnInvestiture.Stormlight == null)
                return new GizmoResult(GizmoState.Clear);

            var overRect = new Rect(topLeft.x, topLeft.y, this.GetWidth(maxWidth), 75f);
            Find.WindowStack.ImmediateWindow(Gen.HashCombineInt(Rand.Int, 45574281), overRect, WindowLayer.GameUI, delegate
            {
                var rect = overRect.AtZero().ContractedBy(6f);
                var rect2 = rect;
                rect2.height = overRect.height / 2f;
                Text.Font = GameFont.Tiny;
                Widgets.Label(rect2, $"{this.Pawn?.LabelCap}'s Stormlight");
                var rect3 = rect;
                rect3.yMin = overRect.height / 2f;
                var fillPercent = (pawnInvestiture?.Stormlight?.CurLevel).GetValueOrDefault(0f);
                Widgets.FillableBar(rect3, fillPercent, GizmoStormlightPawnStatus.FullTex, GizmoStormlightPawnStatus.EmptyTex, false);
                Text.Font = GameFont.Tiny;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect3, $"{pawnInvestiture?.Stormlight?.CurLevel * 100:0} / {pawnInvestiture?.MaxStormlight * 100:0}");
                Text.Anchor = TextAnchor.UpperLeft;
            }, true, false, 1f);

            return new GizmoResult(GizmoState.Clear);
        }
    }
}
