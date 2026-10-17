using UnityEditor;

namespace HellSlice.Polish.EditorTools
{
    /// <summary>Import budgets from docs/ASSET_REQUESTS.md: character and environment textures start at 1024px; models import without clips.</summary>
    sealed class PolishImportRules : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if (assetPath.StartsWith("Assets/HellSlice/Polish/Environment/Surfaces/"))
            {
                // Tiling environment sets (Poly Haven 1K): BaseColor sRGB, Normal map, ARM (AO/Rough/Metal) linear.
                var s = (TextureImporter)assetImporter;
                s.maxTextureSize = 1024;
                s.wrapMode = UnityEngine.TextureWrapMode.Repeat;
                if (assetPath.Contains("_Normal")) s.textureType = TextureImporterType.NormalMap;
                if (assetPath.Contains("_ARM")) s.sRGBTexture = false;
                return;
            }
            if (!assetPath.StartsWith("Assets/HellSlice/Polish/Characters/")) return;
            var t = (TextureImporter)assetImporter;
            t.maxTextureSize = 1024;
            t.textureShape = TextureImporterShape.Texture2D; // never auto-detect as cubemap
            if (assetPath.ToLowerInvariant().Contains("normal")) t.textureType = TextureImporterType.NormalMap;
        }
        void OnPreprocessModel()
        {
            if (assetPath.StartsWith("Assets/HellSlice/Polish/Environment/Ruins/"))
            {
                // Static kit pieces: the HellStone materials are assigned by hand, so no imported materials, clips or colliders.
                var k = (ModelImporter)assetImporter;
                k.importAnimation = false;
                k.animationType = ModelImporterAnimationType.None;
                k.materialImportMode = ModelImporterMaterialImportMode.None;
                k.addCollider = false;
                return;
            }
            if (!assetPath.StartsWith("Assets/HellSlice/Polish/Characters/")) return;
            var m = (ModelImporter)assetImporter;
            // Keep the supplied Lava Giant idle; its bind pose is a T-pose.
            m.importAnimation = assetPath.Contains("/LavaGiant/");
            m.animationType = ModelImporterAnimationType.Generic;
        }
    }
}
