using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class SpriteGeneratorWindow : EditorWindow
{
    private enum Shape { Rectangle, RoundedRectangle, Ellipse }
    private enum Fill { Solid, Gradient }
    private enum GradientDirection { Vertical, Horizontal, Radial }

    [SerializeField] private Shape shape = Shape.RoundedRectangle;
    [SerializeField] private Fill fill = Fill.Gradient;
    [SerializeField] private GradientDirection direction = GradientDirection.Vertical;
    [SerializeField] private int width = 512;
    [SerializeField] private int height = 256;
    [SerializeField] private bool lockAspectRatio = false;
    [SerializeField] private float lockedAspectRatio = 2f;
    [SerializeField] private float cornerRadius = 32f;
    [SerializeField] private Color solidColor = Color.white;
    [SerializeField] private Gradient gradient = new Gradient();
    [SerializeField] private float pixelsPerUnit = 100f;
    [SerializeField] private string fileName = "GeneratedSprite";

    private Texture2D previewTexture;
    private Vector2 scroll;
    private bool previewDirty = true;
    private const int PreviewMaxDimension = 384;
    private const int MaxDimension = 4096;

    [MenuItem("Tools/Sprite Generator")]
    private static void Open()
    {
        GetWindow<SpriteGeneratorWindow>("Sprite Generator").minSize = new Vector2(360, 550);
    }

    private void OnEnable()
    {
        if (gradient == null) gradient = new Gradient();
        // Set useful defaults only on a newly initialized window.
        if (gradient.colorKeys.Length == 0)
        {
            gradient.SetKeys(
                new[] {
                    new GradientColorKey(new Color(0.15f, 0.72f, 0.95f), 0f),
                    new GradientColorKey(new Color(0.08f, 0.25f, 0.65f), 1f)
                },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
        }
        previewDirty = true;
    }

    private void OnDisable()
    {
        DestroyPreview();
    }

    private void OnGUI()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.LabelField("Shape", EditorStyles.boldLabel);
        shape = (Shape)EditorGUILayout.EnumPopup("Type", shape);
        if (shape != Shape.Ellipse)
        {
            EditorGUI.BeginChangeCheck();
            bool newLock = EditorGUILayout.Toggle("Lock Aspect Ratio", lockAspectRatio);
            if (EditorGUI.EndChangeCheck())
            {
                lockAspectRatio = newLock;
                if (lockAspectRatio)
                    lockedAspectRatio = (float)width / Mathf.Max(1, height);
            }
        }

        EditorGUI.BeginChangeCheck();
        int newWidth = EditorGUILayout.IntField("Width (px)", width);
        if (EditorGUI.EndChangeCheck())
        {
            width = Mathf.Clamp(newWidth, 1, MaxDimension);
            if (shape != Shape.Ellipse && lockAspectRatio)
                height = Mathf.Clamp(Mathf.RoundToInt(width / Mathf.Max(0.0001f, lockedAspectRatio)), 1, MaxDimension);
        }

        EditorGUI.BeginChangeCheck();
        int newHeight = EditorGUILayout.IntField("Height (px)", height);
        if (EditorGUI.EndChangeCheck())
        {
            height = Mathf.Clamp(newHeight, 1, MaxDimension);
            if (shape != Shape.Ellipse && lockAspectRatio)
                width = Mathf.Clamp(Mathf.RoundToInt(height * lockedAspectRatio), 1, MaxDimension);
        }
        if (shape == Shape.RoundedRectangle)
        {
            cornerRadius = EditorGUILayout.Slider("Corner Radius (px)", cornerRadius, 0f, Mathf.Min(width, height) * 0.5f);
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Color", EditorStyles.boldLabel);
        fill = (Fill)EditorGUILayout.EnumPopup("Fill", fill);
        if (fill == Fill.Solid)
        {
            solidColor = EditorGUILayout.ColorField("Color", solidColor);
        }
        else
        {
            direction = (GradientDirection)EditorGUILayout.EnumPopup("Direction", direction);
            gradient = EditorGUILayout.GradientField("Gradient", gradient);
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Export", EditorStyles.boldLabel);
        fileName = EditorGUILayout.TextField("File Name", fileName);
        pixelsPerUnit = EditorGUILayout.FloatField("Pixels Per Unit", pixelsPerUnit);

        if (EditorGUI.EndChangeCheck())
        {
            width = Mathf.Clamp(width, 1, MaxDimension);
            height = Mathf.Clamp(height, 1, MaxDimension);
            cornerRadius = Mathf.Clamp(cornerRadius, 0f, Mathf.Min(width, height) * 0.5f);
            pixelsPerUnit = Mathf.Max(0.01f, pixelsPerUnit);
            previewDirty = true;
        }

        EditorGUILayout.Space(12);
        EditorGUILayout.LabelField("Preview (orange outline is not exported)", EditorStyles.boldLabel);
        DrawPreview();

        EditorGUILayout.Space(12);
        using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(fileName)))
        {
            if (GUILayout.Button("Generate PNG", GUILayout.Height(34)))
                ExportPNG();
        }

        EditorGUILayout.HelpBox("Outside the shape is transparent. PNG files are imported as Sprite (2D and UI). Maximum size: 4096 x 4096.", MessageType.Info);
        EditorGUILayout.EndScrollView();
    }

    private void DrawPreview()
    {
        if (previewDirty || previewTexture == null)
        {
            DestroyPreview();
            float scale = Mathf.Min(1f, (float)PreviewMaxDimension / Mathf.Max(width, height));
            int pw = Mathf.Max(1, Mathf.RoundToInt(width * scale));
            int ph = Mathf.Max(1, Mathf.RoundToInt(height * scale));
            previewTexture = GenerateTexture(pw, ph, (float)pw / width, (float)ph / height);
            previewTexture.hideFlags = HideFlags.HideAndDontSave;
            previewDirty = false;
        }

        float available = Mathf.Max(100f, EditorGUIUtility.currentViewWidth - 44f);
        float displayScale = Mathf.Min(1f, available / previewTexture.width);
        float displayWidth = previewTexture.width * displayScale;
        float displayHeight = previewTexture.height * displayScale;
        Rect rect = GUILayoutUtility.GetRect(available, displayHeight + 16f);
        Rect imageRect = new Rect(rect.x + (rect.width - displayWidth) * 0.5f, rect.y + 8f, displayWidth, displayHeight);

        // Checkerboard shows transparency without being baked into the PNG.
        EditorGUI.DrawRect(imageRect, new Color(0.7f, 0.7f, 0.7f));
        GUI.DrawTexture(imageRect, Texture2D.grayTexture, ScaleMode.StretchToFill);
        GUI.DrawTexture(imageRect, previewTexture, ScaleMode.StretchToFill, true);

        // A guide border around the preview bounds, never exported.
        Handles.BeginGUI();
        Handles.color = new Color(1f, 0.6f, 0.08f, 1f);
        Vector3 a = new Vector3(imageRect.xMin, imageRect.yMin);
        Vector3 b = new Vector3(imageRect.xMax, imageRect.yMin);
        Vector3 c = new Vector3(imageRect.xMax, imageRect.yMax);
        Vector3 d = new Vector3(imageRect.xMin, imageRect.yMax);
        Handles.DrawDottedLine(a, b, 4f);
        Handles.DrawDottedLine(b, c, 4f);
        Handles.DrawDottedLine(c, d, 4f);
        Handles.DrawDottedLine(d, a, 4f);
        Handles.EndGUI();
    }

    private Texture2D GenerateTexture(int textureWidth, int textureHeight, float scaleX, float scaleY)
    {
        var texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        var pixels = new Color[textureWidth * textureHeight];
        float radius = cornerRadius * Mathf.Min(scaleX, scaleY);
        float halfWidth = textureWidth * 0.5f;
        float halfHeight = textureHeight * 0.5f;

        for (int y = 0; y < textureHeight; y++)
        {
            for (int x = 0; x < textureWidth; x++)
            {
                float px = x + 0.5f - halfWidth;
                float py = y + 0.5f - halfHeight;
                float distance = SignedDistance(px, py, halfWidth, halfHeight, radius);
                // A one-pixel transition produces smooth antialiased edges.
                float coverage = Mathf.Clamp01(0.5f - distance);
                float u = (x + 0.5f) / textureWidth;
                float v = (y + 0.5f) / textureHeight;
                Color color = fill == Fill.Solid ? solidColor : gradient.Evaluate(GradientPosition(u, v));
                color.a *= coverage;
                pixels[y * textureWidth + x] = color;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, false);
        return texture;
    }

    private float SignedDistance(float x, float y, float halfWidth, float halfHeight, float radius)
    {
        if (shape == Shape.Ellipse)
        {
            // Approximate signed distance to an ellipse; exact for circles.
            float rx = Mathf.Max(0.5f, halfWidth);
            float ry = Mathf.Max(0.5f, halfHeight);
            float nx = x / rx;
            float ny = y / ry;
            float k0 = Mathf.Sqrt(nx * nx + ny * ny);
            float k1 = Mathf.Sqrt((x / (rx * rx)) * (x / (rx * rx)) + (y / (ry * ry)) * (y / (ry * ry)));
            return k1 > 0.000001f ? k0 * (k0 - 1f) / k1 : -Mathf.Min(rx, ry);
        }

        float r = shape == Shape.Rectangle ? 0f : radius;
        float qx = Mathf.Abs(x) - (halfWidth - r);
        float qy = Mathf.Abs(y) - (halfHeight - r);
        return Mathf.Sqrt(Mathf.Max(qx, 0f) * Mathf.Max(qx, 0f) +
                          Mathf.Max(qy, 0f) * Mathf.Max(qy, 0f)) +
               Mathf.Min(Mathf.Max(qx, qy), 0f) - r;
    }

    private float GradientPosition(float u, float v)
    {
        switch (direction)
        {
            case GradientDirection.Horizontal: return u;
            case GradientDirection.Radial:
                // Normalized elliptical distance reaches 1 at the furthest corners.
                float dx = (u - 0.5f) * 2f;
                float dy = (v - 0.5f) * 2f;
                return Mathf.Clamp01(Mathf.Sqrt(dx * dx + dy * dy));
            default: return 1f - v; // Start color at top.
        }
    }

    private void ExportPNG()
    {
        string safeName = Path.GetFileNameWithoutExtension(fileName.Trim());
        if (string.IsNullOrWhiteSpace(safeName))
        {
            EditorUtility.DisplayDialog("Invalid name", "Enter a valid PNG file name.", "OK");
            return;
        }

        string path = EditorUtility.SaveFilePanelInProject(
            "Save Sprite PNG", safeName, "png", "Choose a folder inside Assets.");
        if (string.IsNullOrEmpty(path)) return;

        Texture2D output = null;
        try
        {
            output = GenerateTexture(width, height, 1f, 1f);
            File.WriteAllBytes(Path.GetFullPath(path), output.EncodeToPNG());
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = Mathf.Max(0.01f, pixelsPerUnit);
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Texture2D>(path));
            Debug.Log("Sprite Generator: Saved " + path);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            EditorUtility.DisplayDialog("Export failed", ex.Message, "OK");
        }
        finally
        {
            if (output != null) DestroyImmediate(output);
        }
    }

    private void DestroyPreview()
    {
        if (previewTexture != null)
        {
            DestroyImmediate(previewTexture);
            previewTexture = null;
        }
    }
}
