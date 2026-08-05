using UnityEngine;
using UnityEngine.UI;

public sealed class DamageVignette : MaskableGraphic
{
    private const int GridSize = 5;
    private float intensity;

    public void SetIntensity(float value)
    {
        value = Mathf.Clamp01(value);
        if (Mathf.Approximately(intensity, value))
        {
            return;
        }

        intensity = value;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vertexHelper)
    {
        vertexHelper.Clear();
        Rect rect = rectTransform.rect;
        Color baseColor = color;

        for (int y = 0; y < GridSize - 1; y++)
        {
            for (int x = 0; x < GridSize - 1; x++)
            {
                AddQuad(vertexHelper, rect, x, y, baseColor);
            }
        }
    }

    private void AddQuad(VertexHelper vertexHelper, Rect rect, int x, int y, Color baseColor)
    {
        int startIndex = vertexHelper.currentVertCount;
        vertexHelper.AddVert(GetPosition(rect, x, y), GetColor(baseColor, x, y), Vector2.zero);
        vertexHelper.AddVert(GetPosition(rect, x, y + 1), GetColor(baseColor, x, y + 1), Vector2.up);
        vertexHelper.AddVert(GetPosition(rect, x + 1, y + 1), GetColor(baseColor, x + 1, y + 1), Vector2.one);
        vertexHelper.AddVert(GetPosition(rect, x + 1, y), GetColor(baseColor, x + 1, y), Vector2.right);
        vertexHelper.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
        vertexHelper.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
    }

    private static Vector3 GetPosition(Rect rect, int x, int y)
    {
        float normalizedX = x / (float)(GridSize - 1);
        float normalizedY = y / (float)(GridSize - 1);
        return new Vector3(Mathf.Lerp(rect.xMin, rect.xMax, normalizedX), Mathf.Lerp(rect.yMin, rect.yMax, normalizedY));
    }

    private Color GetColor(Color baseColor, int x, int y)
    {
        float normalizedX = x / (float)(GridSize - 1);
        float normalizedY = y / (float)(GridSize - 1);
        float edgeStrength = Mathf.Max(Mathf.Abs(normalizedX - 0.5f), Mathf.Abs(normalizedY - 0.5f)) * 2f;
        baseColor.a *= intensity * edgeStrength;
        return baseColor;
    }
}
