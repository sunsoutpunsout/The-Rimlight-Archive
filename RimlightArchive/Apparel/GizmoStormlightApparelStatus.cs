using UnityEngine;
using Verse;

namespace RimlightArchive.Apparel
{
    /// <summary>
    /// Handles the on-screen level of Stormlight in apparel.
    /// </summary>
    [StaticConstructorOnStartup]
    public class GizmoStormlightApparelStatus : Gizmo
    {
        private static readonly Texture2D FullTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.24f));
        private static readonly Texture2D EmptyTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);

        public InfusedApparel apparel;

        public GizmoStormlightApparelStatus()
        {
            this.order = -100f;
        }

        public override float GetWidth(float maxWidth) => 140f;

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth)
        {
            var overRect = new Rect(topLeft.x, topLeft.y, this.GetWidth(maxWidth), 75f);
            Find.WindowStack.ImmediateWindow(Gen.HashCombineInt(Rand.Int, 45574281), overRect, WindowLayer.GameUI, delegate
            {
                var rect = overRect.AtZero().ContractedBy(6f);
                var rect2 = rect;
                rect2.height = overRect.height / 2f;
                Text.Font = GameFont.Tiny;
                Widgets.Label(rect2, this.apparel.LabelCap);
                var rect3 = rect;
                rect3.yMin = overRect.height / 2f;
                var fillPercent = this.apparel.StormlightPercentage;
                Widgets.FillableBar(rect3, fillPercent, GizmoStormlightApparelStatus.FullTex, GizmoStormlightApparelStatus.EmptyTex, false);
                Text.Font = GameFont.Tiny;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect3, $"{this.apparel.stormlight} / {this.apparel.StormlightMax}");
                Text.Anchor = TextAnchor.UpperLeft;
            }, true, false, 1f);

            return new GizmoResult(GizmoState.Clear);
        }
    }
}
