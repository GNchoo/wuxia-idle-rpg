#if UNITY_EDITOR
using UnityEditor;

namespace IdleMvp.EditorTools
{
    /// <summary>Auto import settings for curated template art (Phase G).</summary>
    public class TplArtImporter : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if (!assetPath.Replace('\\', '/').Contains("/TplArt/")) return;
            var imp = (TextureImporter)assetImporter;
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.mipmapEnabled = false;
            imp.alphaIsTransparency = true;
            imp.maxTextureSize = assetPath.Contains("/Biomes/") ? 2048 : 512;
        }
    }
}
#endif
