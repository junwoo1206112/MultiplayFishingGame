using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using MultiplayFishing.Data.Models;

namespace MultiplayFishing.Editor
{
    public class BaitIconMatcher : EditorWindow
    {
        private const string BaitAssetPath = "Assets/Resources/Data/Baits";
        private const string SpritePath = "Assets/Art/Baits";

        [MenuItem("Tools/Fish/Match Bait Icons")]
        public static void MatchIcons()
        {
            FixTextureImportSettings();
            AssetDatabase.Refresh();

            var mapping = new Dictionary<string, string>
            {
                { "bait_basic", "baits1" },
                { "bait_worm", "baits2" },
                { "bait_shrimp", "baits3" },
                { "bait_lure", "baits4" },
                { "bait_golden", "baits5" },
                { "bait_ancient", "baits6" },
            };

            string[] assetFiles = Directory.GetFiles(BaitAssetPath, "*.asset");
            int matchCount = 0;

            foreach (string assetFile in assetFiles)
            {
                string cleanPath = assetFile.Replace("\\", "/");
                BaitDataSO baitData = AssetDatabase.LoadAssetAtPath<BaitDataSO>(cleanPath);
                if (baitData == null)
                {
                    Debug.LogWarning($"[BaitIconMatcher] Failed to load BaitDataSO at: {cleanPath}");
                    continue;
                }

                if (mapping.TryGetValue(baitData.id, out string spriteFile))
                {
                    string spritePath = $"{SpritePath}/{spriteFile}.png";
                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                    if (sprite == null)
                    {
                        Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(spritePath);
                        foreach (var obj in allAssets)
                        {
                            if (obj is Sprite subSprite)
                            {
                                sprite = subSprite;
                                break;
                            }
                        }
                    }
                    if (sprite == null)
                    {
                        Debug.LogWarning($"[BaitIconMatcher] No sprite found at: {spritePath} for bait: {baitData.id}");
                        continue;
                    }
                    baitData.icon = sprite;
                    EditorUtility.SetDirty(baitData);
                    matchCount++;
                    Debug.Log($"[BaitIconMatcher] Matched {baitData.id} -> {spriteFile}.png");
                }
                else
                {
                    Debug.LogWarning($"[BaitIconMatcher] No mapping for bait: {baitData.id}");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Match Bait Icons", $"매칭 완료!\n\n{matchCount}개의 미끼 아이콘이 연결되었습니다.", "확인");
        }

        private static void FixTextureImportSettings()
        {
            string[] allSprites = Directory.GetFiles(SpritePath, "*.png", SearchOption.AllDirectories);
            foreach (var path in allSprites)
            {
                string cleanPath = path.Replace("\\", "/");
                TextureImporter importer = AssetImporter.GetAtPath(cleanPath) as TextureImporter;
                if (importer != null)
                {
                    bool needsFix = importer.textureType != TextureImporterType.Sprite
                        || importer.spriteImportMode != SpriteImportMode.Single
                        || importer.mipmapEnabled
                        || importer.textureCompression != TextureImporterCompression.Uncompressed;
                    if (needsFix)
                    {
                        importer.textureType = TextureImporterType.Sprite;
                        importer.spriteImportMode = SpriteImportMode.Single;
                        importer.mipmapEnabled = false;
                        importer.textureCompression = TextureImporterCompression.Uncompressed;
                        importer.isReadable = true;
                        importer.filterMode = FilterMode.Point;
                        importer.SaveAndReimport();
                        Debug.Log($"[BaitIconMatcher] Fixed texture settings for: {cleanPath}");
                    }
                }
            }
        }
    }
}
