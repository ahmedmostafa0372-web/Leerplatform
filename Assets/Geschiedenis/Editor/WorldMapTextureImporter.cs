using UnityEditor;
using UnityEngine;

// De kleurmasker-PNG moet leesbaar zijn voor muisdetectie.
public class WorldMapTextureImporter : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        if (!assetPath.Contains("/Resources/WorldMap/")) return;
        TextureImporter importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Default;
        importer.mipmapEnabled = false;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        if (assetPath.EndsWith("RegionMask.png"))
        {
            importer.isReadable = true;
            importer.filterMode = FilterMode.Point;
            importer.sRGBTexture = false;
        }
        else
        {
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Bilinear;
        }
    }
}
