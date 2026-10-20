using UnityEngine;
using UnityEditor;
using System.IO;

public class ImageResizeWindow : EditorWindow
{
    private Texture2D source;
    private Texture2D referenceImage;
    private int targetWidth = 1024;
    private int targetHeight = 1024;
    private float centerX = 0.5f;
    private float centerY = 0.5f;
    private Texture2D preview;
    private bool previewDirty = true;

    [MenuItem("Tools/Image Resize")]
    public static void Open()
    {
        GetWindow<ImageResizeWindow>("Image Resize");
    }

    private void OnGUI()
    {
        EditorGUI.BeginChangeCheck();
        source = (Texture2D)EditorGUILayout.ObjectField(
            "Source Image", source, typeof(Texture2D), false);

        EditorGUILayout.Space(10);
        Texture2D newReference = (Texture2D)EditorGUILayout.ObjectField(
            "Reference Image", referenceImage, typeof(Texture2D), false);
        if (newReference != referenceImage)
        {
            referenceImage = newReference;
            if (referenceImage != null)
            {
                targetWidth = referenceImage.width;
                targetHeight = referenceImage.height;
            }
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Output Size", EditorStyles.boldLabel);
        targetWidth = Mathf.Clamp(EditorGUILayout.IntField("Width", targetWidth), 1, 8192);
        targetHeight = Mathf.Clamp(EditorGUILayout.IntField("Height", targetHeight), 1, 8192);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Crop Center", EditorStyles.boldLabel);
        centerX = EditorGUILayout.Slider("Center X", centerX, 0f, 1f);
        centerY = EditorGUILayout.Slider("Center Y", centerY, 0f, 1f);

        if (EditorGUI.EndChangeCheck())
            previewDirty = true;

        EditorGUILayout.Space(10);
        if (source == null)
        {
            EditorGUILayout.HelpBox("Select a source image.", MessageType.Info);
            return;
        }

        if (previewDirty)
        {
            UpdatePreview();
            previewDirty = false;
        }

        if (preview != null)
        {
            float availableWidth = Mathf.Max(1f, position.width - 20f);
            float ratio = (float)targetHeight / targetWidth;
            float width = Mathf.Min(availableWidth, 400f);
            float height = width * ratio;
            if (height > 400f)
            {
                height = 400f;
                width = height / ratio;
            }

            Rect rect = GUILayoutUtility.GetRect(availableWidth, height);
            rect.x += (rect.width - width) / 2f;
            rect.width = width;
            EditorGUI.DrawPreviewTexture(rect, preview, null, ScaleMode.StretchToFill);
        }

        EditorGUILayout.Space(10);
        if (GUILayout.Button("Export PNG", GUILayout.Height(32)))
            Export();
    }

    private void UpdatePreview()
    {
        if (preview != null)
            DestroyImmediate(preview);

        preview = null;
        if (source == null)
            return;

        float scale = Mathf.Min(1f, 512f / Mathf.Max(targetWidth, targetHeight));
        int width = Mathf.Max(1, Mathf.RoundToInt(targetWidth * scale));
        int height = Mathf.Max(1, Mathf.RoundToInt(targetHeight * scale));
        preview = Resize(width, height);
    }

    private Texture2D Resize(int width, int height)
    {
        RenderTexture rt = RenderTexture.GetTemporary(
            source.width, source.height, 0,
            RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
        RenderTexture previous = RenderTexture.active;
        Texture2D readable = null;

        try
        {
            Graphics.Blit(source, rt);
            RenderTexture.active = rt;
            readable = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            readable.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
            readable.Apply();

            Texture2D result = new Texture2D(width, height, TextureFormat.RGBA32, false);
            result.filterMode = FilterMode.Bilinear;

            float scale = Mathf.Max((float)width / source.width, (float)height / source.height);
            float visibleWidth = width / scale;
            float visibleHeight = height / scale;
            float offsetX = (source.width - visibleWidth) * centerX;
            float offsetY = (source.height - visibleHeight) * centerY;
            Color[] pixels = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float u = (offsetX + (x + 0.5f) / scale) / source.width;
                    float v = (offsetY + (y + 0.5f) / scale) / source.height;
                    pixels[y * width + x] = readable.GetPixelBilinear(u, v);
                }
            }

            result.SetPixels(pixels);
            result.Apply();
            return result;
        }
        finally
        {
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);
            if (readable != null)
                DestroyImmediate(readable);
        }
    }

    private void Export()
    {
        string path = EditorUtility.SaveFilePanel(
            "Export Resized Image", Application.dataPath,
            source.name + "_resized", "png");
        if (string.IsNullOrEmpty(path))
            return;

        Texture2D result = null;
        try
        {
            result = Resize(targetWidth, targetHeight);
            File.WriteAllBytes(path, result.EncodeToPNG());
            AssetDatabase.Refresh();
            Debug.Log("Image exported: " + path);
        }
        finally
        {
            if (result != null)
                DestroyImmediate(result);
        }
    }

    private void OnDisable()
    {
        if (preview != null)
            DestroyImmediate(preview);
    }
}
