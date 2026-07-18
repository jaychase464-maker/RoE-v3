using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RulesOfEntry.UI.TacticalHud
{
    [DisallowMultipleComponent]
    public sealed class TacticalHudRoundedPanelGraphic : MaskableGraphic
    {
        [SerializeField] private Color fillColor = new Color(0.02f, 0.03f, 0.04f, 0.82f);
        [SerializeField] private Color borderColor = new Color(0.62f, 0.7f, 0.74f, 0.5f);
        [SerializeField, Min(0f)] private float cornerRadius = 12f;
        [SerializeField, Min(0f)] private float borderWidth = 1.25f;
        [SerializeField, Range(2, 12)] private int cornerSegments = 6;

        public void Configure(
            Color configuredFillColor,
            Color configuredBorderColor,
            float configuredCornerRadius,
            float configuredBorderWidth)
        {
            fillColor = configuredFillColor;
            borderColor = configuredBorderColor;
            cornerRadius = Mathf.Max(0f, configuredCornerRadius);
            borderWidth = Mathf.Max(0f, configuredBorderWidth);
            raycastTarget = false;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();
            Rect outerRect = GetPixelAdjustedRect();
            float radius = Mathf.Min(
                cornerRadius,
                Mathf.Min(outerRect.width, outerRect.height) * 0.5f);
            List<Vector2> outer = BuildPerimeter(outerRect, radius, cornerSegments);

            float inset = Mathf.Min(
                borderWidth,
                Mathf.Min(outerRect.width, outerRect.height) * 0.5f);
            Rect innerRect = new Rect(
                outerRect.xMin + inset,
                outerRect.yMin + inset,
                Mathf.Max(0f, outerRect.width - inset * 2f),
                Mathf.Max(0f, outerRect.height - inset * 2f));
            List<Vector2> inner = BuildPerimeter(
                innerRect,
                Mathf.Max(0f, radius - inset),
                cornerSegments);

            int perimeterCount = outer.Count;
            for (int index = 0; index < perimeterCount; index++)
            {
                AddVertex(vertexHelper, outer[index], borderColor);
                AddVertex(vertexHelper, inner[index], fillColor);
            }

            for (int index = 0; index < perimeterCount; index++)
            {
                int next = (index + 1) % perimeterCount;
                int outerIndex = index * 2;
                int innerIndex = outerIndex + 1;
                int nextOuterIndex = next * 2;
                int nextInnerIndex = nextOuterIndex + 1;
                vertexHelper.AddTriangle(outerIndex, nextOuterIndex, nextInnerIndex);
                vertexHelper.AddTriangle(outerIndex, nextInnerIndex, innerIndex);
            }

            int centerIndex = vertexHelper.currentVertCount;
            AddVertex(vertexHelper, innerRect.center, fillColor);
            int innerStart = vertexHelper.currentVertCount;
            foreach (Vector2 point in inner)
            {
                AddVertex(vertexHelper, point, fillColor);
            }

            for (int index = 0; index < perimeterCount; index++)
            {
                int next = (index + 1) % perimeterCount;
                vertexHelper.AddTriangle(
                    centerIndex,
                    innerStart + index,
                    innerStart + next);
            }
        }

        private static List<Vector2> BuildPerimeter(
            Rect rect,
            float radius,
            int segmentsPerCorner)
        {
            List<Vector2> points = new List<Vector2>(segmentsPerCorner * 4 + 4);
            AddCorner(points, new Vector2(rect.xMax - radius, rect.yMin + radius), radius, -90f, 0f, segmentsPerCorner);
            AddCorner(points, new Vector2(rect.xMax - radius, rect.yMax - radius), radius, 0f, 90f, segmentsPerCorner);
            AddCorner(points, new Vector2(rect.xMin + radius, rect.yMax - radius), radius, 90f, 180f, segmentsPerCorner);
            AddCorner(points, new Vector2(rect.xMin + radius, rect.yMin + radius), radius, 180f, 270f, segmentsPerCorner);
            return points;
        }

        private static void AddCorner(
            ICollection<Vector2> points,
            Vector2 center,
            float radius,
            float startDegrees,
            float endDegrees,
            int segments)
        {
            int normalizedSegments = Mathf.Max(2, segments);
            for (int index = 0; index <= normalizedSegments; index++)
            {
                float degrees = Mathf.Lerp(
                    startDegrees,
                    endDegrees,
                    index / (float)normalizedSegments);
                float radians = degrees * Mathf.Deg2Rad;
                points.Add(center + new Vector2(
                    Mathf.Cos(radians) * radius,
                    Mathf.Sin(radians) * radius));
            }
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
