using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using MultiplayFishing.Data.Models;

namespace MultiplayFishing.Editor
{
    public class RodIconMatcher : EditorWindow
    {
        private const string RodAssetPath = "Assets/Resources/Data/Rods";
        private const string SpritePath = "Assets/Art/Rods";

        [MenuItem("Tools/Fish/Match Rod Icons")]
        public static void MatchIcons()
        {
            FixTextureImportSettings();
            AssetDatabase.Refresh();

            var mapping = new Dictionary<string, string>
            {
                { "rod_basic", "basics" },
                { "rod_carbon", "Novice" },
                { "rod_fiberglass", "fiberglass" },
                { "rod_titanium", "titanium" },
                { "rod_legendary", "Legend" },
            };

            string[] assetFiles = Directory.GetFiles(RodAssetPath, "*.asset");
            int matchCount = 0;

            foreach (string assetFile in assetFiles)
            {
                string cleanPath = assetFile.Replace("\\", "/");
                RodDataSO rodData = AssetDatabase.LoadAssetAtPath<RodDataSO>(cleanPath);
                if (rodData == null)
                {
                    Debug.LogWarning($"[RodIconMatcher] Failed to load RodDataSO at: {cleanPath}");
                    continue;
                }

                if (mapping.TryGetValue(rodData.id, out string spriteFile))
                {
                    string spritePath = $"{SpritePath}/{spriteFile}.png";
                    Debug.Log($"[RodIconMatcher] Looking for: {spritePath} (exists: {File.Exists(spritePath)})");
                    var mainAsset = AssetDatabase.LoadMainAssetAtPath(spritePath);
                    string mainType = mainAsset != null ? mainAsset.GetType().Name : "null";
                    string mainName = mainAsset != null ? mainAsset.name : "N/A";
                    Debug.Log($"[RodIconMatcher]   Main asset type: {mainType} ({mainName})");
                    Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(spritePath);
                    Debug.Log($"[RodIconMatcher]   Total assets at path: {allAssets.Length}");
                    foreach (var a in allAssets) Debug.Log($"[RodIconMatcher]     -> {a.GetType().Name}: {a.name}");
                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                    if (sprite == null)
                    {
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
                        Debug.LogWarning($"[RodIconMatcher] No sprite found at: {spritePath} for rod: {rodData.id}");
                        continue;
                    }
                    rodData.icon = sprite;
                    EditorUtility.SetDirty(rodData);
                    matchCount++;
                    Debug.Log($"[RodIconMatcher] Matched {rodData.id} -> {spriteFile}.png");
                }
                else
                {
                    Debug.LogWarning($"[RodIconMatcher] No mapping for rod: {rodData.id}");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Match Rod Icons", $"매칭 완료!\n\n{matchCount}개의 낚싯대 아이콘이 연결되었습니다.", "확인");
        }

        private static void FixTextureImportSettings()
        {
            string[] allSprites = Directory.GetFiles(SpritePath, "*.png", SearchOption.AllDirectories);
            Debug.Log($"[RodIconMatcher] Found {allSprites.Length} PNGs in {SpritePath}");
            foreach (var f in allSprites) Debug.Log($"[RodIconMatcher]   -> {f.Replace("\\", "/")}");
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
                        Debug.Log($"[RodIconMatcher] Fixed texture settings for: {cleanPath}");
                    }
                }
            }
        }
    }
}
