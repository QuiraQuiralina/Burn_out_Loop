using UnityEngine;
using UnityEditor;
using System.IO;

public class LutGenerator
{
    [MenuItem("Tools/Generate Neutral LUT")]
    public static void GenerateLuts()
    {
        // 256x16 LUT (16 blocks of 16x16)
        GenerateLut(256, 16, "neutral_lut_256.png");
        
        // 1024x32 LUT (32 blocks of 32x32) - Recommended for higher precision
        GenerateLut(1024, 32, "neutral_lut_1024.png");
        
        AssetDatabase.Refresh();
    }

    private static void GenerateLut(int width, int height, string filename)
    {
        // Create Texture2D with Linear color rendering to ensure accurate LUT mappings
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false, true);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;

        int blockSize = height;
        int numBlocks = width / blockSize;

        for (int y = 0; y < height; y++)
        {
            float g = (float)y / (height - 1);
            for (int x = 0; x < width; x++)
            {
                int blockIndex = x / blockSize;
                int dx = x % blockSize;
                
                float r = (float)dx / (blockSize - 1);
                float b = (float)blockIndex / (numBlocks - 1);

                tex.SetPixel(x, y, new Color(r, g, b));
            }
        }
        tex.Apply();

        byte[] bytes = tex.EncodeToPNG();
        Object.DestroyImmediate(tex);

        string folderPath = Path.Combine(Application.dataPath, "Settings");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string fullPath = Path.Combine(folderPath, filename);
        File.WriteAllBytes(fullPath, bytes);

        Debug.Log($"[LutGenerator] Neutral LUT saved and imported to: Assets/Settings/{filename}");
    }
}
