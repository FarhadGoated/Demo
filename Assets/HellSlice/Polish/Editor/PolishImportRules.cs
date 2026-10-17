using UnityEditor;

namespace HellSlice.Polish.EditorTools
{
    /// <summary>Import budgets from docs/ASSET_REQUESTS.md: character textures start at 1024px; models import without clips.</summary>
    sealed class PolishImportRules : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith("Assets/HellSlice/Polish/Characters/")) return;
            var t = (TextureImporter)assetImporter;
            t.maxTextureSize = 1024;
            t.textureShape = TextureImporterShape.Texture2D; // never auto-detect as cubemap
            if (assetPath.ToLowerInvariant().Contains("normal")) t.textureType = TextureImporterType.NormalMap;
        }
        void OnPreprocessModel()
        {
            if (!assetPath.StartsWith("Assets/HellSlice/Polish/Characters/")) return;
            var m = (ModelImporter)assetImporter;
            // Keep the supplied Lava Giant idle; its bind pose is a T-pose.
            m.importAnimation = assetPath.Contains("/LavaGiant/");
            m.animationType = ModelImporterAnimationType.Generic;
        }
    }
}
