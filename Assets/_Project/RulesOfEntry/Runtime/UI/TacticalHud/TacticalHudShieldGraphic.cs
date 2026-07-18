using UnityEngine;
using UnityEngine.UI;

namespace RulesOfEntry.UI.TacticalHud
{
    [DisallowMultipleComponent]
    public sealed class TacticalHudShieldGraphic : MaskableGraphic
    {
        [SerializeField, Min(0.5f)] private float lineWidth = 2f;

        public void Configure(Color configuredColor, float configuredLineWidth)
        {
            color = configuredColor;
            lineWidth = Mathf.Max(0.5f, configuredLineWidth);
            raycastTarget = false;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();
            Rect rect = GetPixelAdjustedRect();
            Vector2[] points =
            {
                Point(rect, 0.16f, 0.9f),
                Point(rect, 0.84f, 0.9f),
                Point(rect, 0.84f, 0.48f),
                Point(rect, 0.72f, 0.2f),
                Point(rect, 0.5f, 0.05f),
                Point(rect, 0.28f, 0.2f),
                Point(rect, 0.16f, 0.48f)
            };

            for (int index = 0; index < points.Length; index++)
            {
                AddLine(
                    vertexHelper,
                    points[index],
                    points[(index + 1) % points.Length],
                    lineWidth,
                    color);
            }
        }

        private static Vector2 Point(Rect rect, float x, float y)
        {
            return new Vector2(
                Mathf.Lerp(rect.xMin, rect.xMax, x),
                Mathf.Lerp(rect.yMin, rect.yMax, y));
        }

        private static void AddLine(
            VertexHelper helper,
            Vector2 start,
            Vector2 end,
            float width,
            Color color)
        {
            Vector2 direction = (end - start).normalized;
            Vector2 normal = new Vector2(-direction.y, direction.x) * width * 0.5f;
            int baseIndex = helper.currentVertCount;
            AddVertex(helper, start - normal, color);
            AddVertex(helper, start + normal, color);
            AddVertex(helper, end + normal, color);
            AddVertex(helper, end - normal, color);
            helper.AddTriangle(baseIndex, baseIndex + 1, baseIndex + 2);
            helper.AddTriangle(baseIndex, baseIndex + 2, baseIndex + 3);
        }

        private static void AddVertex(VertexHelper helper, Vector2 position, Color color)
        {
            UIVertex vertex = UIVertex.simpleVert;
            vertex.position = position;
            vertex.color = color;
            helper.AddVert(vertex);
        }
    }
}
