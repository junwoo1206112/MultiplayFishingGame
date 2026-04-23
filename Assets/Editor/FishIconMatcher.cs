using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using MultiplayFishing.Data.Models;

namespace MultiplayFishing.Editor
{
    public class FishIconMatcher : EditorWindow
    {
        private const string AssetPath = "Assets/Resources/Data/Fish";
        private const string SpritePath = "Assets/Fish";

        [MenuItem("Tools/Fish/Match Icons to Assets")]
        public static void MatchIcons()
        {
            FixTextureImportSettings();

            int matchCount = 0;
            string[] assetFiles = Directory.GetFiles(AssetPath, "*.asset");
            
            string[] allSpriteFiles = Directory.GetFiles(SpritePath, "*.png", SearchOption.AllDirectories)
                                           .Select(s => s.Replace("\\", "/")).ToArray();

            var spriteLookup = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
            foreach (var s in allSpriteFiles)
            {
                spriteLookup[Path.GetFileNameWithoutExtension(s)] = s;
            }

            foreach (string assetFile in assetFiles)
            {
                string cleanAssetPath = assetFile.Replace("\\", "/");
                FishDataSO fishData = AssetDatabase.LoadAssetAtPath<FishDataSO>(cleanAssetPath);
                if (fishData == null) continue;

                string id = fishData.id.ToLower();
                string targetFileName = GetMappedName(id);
                
                if (string.IsNullOrEmpty(targetFileName))
                {
                    if (spriteLookup.ContainsKey(id)) targetFileName = id;
                    else if (id.StartsWith("fish_") && spriteLookup.ContainsKey(id.Substring(5))) targetFileName = id.Substring(5);
                    // 부분 일치 검색 (예: horseshoecrab -> crab 이미지라도)
                    else 
                    {
                        var bestMatch = spriteLookup.Keys.FirstOrDefault(k => id.Contains(k.ToLower()) || k.ToLower().Contains(id.Replace("fish_", "")));
                        if (bestMatch != null) targetFileName = bestMatch;
                    }
                }

                if (!string.IsNullOrEmpty(targetFileName) && spriteLookup.TryGetValue(targetFileName, out string fullSpritePath))
                {
                    Sprite sprite = GetSpriteFromAsset(fullSpritePath);
                    if (sprite != null)
                    {
                        fishData.fishIcon = sprite;
                        EditorUtility.SetDirty(fishData);
                        matchCount++;
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            EditorUtility.DisplayDialog("Match Icons", $"매칭 완료!\n\n총 {matchCount}개의 물고기 아이콘이 연결되었습니다.", "확인");
            Debug.Log($"<b>[Icon Matcher]</b> Matched {matchCount} fish icons.");
        }

        private static Sprite GetSpriteFromAsset(string path)
        {
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s != null) return s;
            Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var obj in allAssets) if (obj is Sprite sprite) return sprite;
            return null;
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
                    if (importer.textureType != TextureImporterType.Sprite || importer.mipmapEnabled)
                    {
                        importer.textureType = TextureImporterType.Sprite;
                        importer.mipmapEnabled = false;
                        importer.filterMode = FilterMode.Point;
                        importer.SaveAndReimport();
                    }
                }
            }
        }

        private static string GetMappedName(string id)
        {
            var mapping = new Dictionary<string, string>
            {
                { "fish_001", "fish1" },
                { "fish_alligator", "fish1" }, { "fish_axolotl", "fish2" }, { "fish_bass", "fish3" },
                { "fish_bluegill", "fish4" }, { "fish_bowfin", "fish5" }, { "fish_bullshark", "fish6" },
                { "fish_carp", "fish7" }, { "fish_catfish", "fish8" }, { "fish_crab", "fish9" },
                { "fish_crappie", "fish10" }, { "fish_crayfish", "fish11" }, { "fish_drum", "fish12" },
                { "fish_frog", "fish13" }, { "fish_gar", "fish14" }, { "fish_bass_golden", "fish15" },
                { "fish_goldfish", "fish16" }, { "fish_guppy", "fish17" }, { "fish_salmon_king", "fish18" },
                { "fish_koi", "fish19" }, { "fish_leech", "fish20" }, { "fish_mooneye", "fish21" },
                { "fish_muskellunge", "fish22" }, { "fish_perch", "fish23" }, { "fish_pike", "fish24" },
                { "fish_pupfish", "fish25" }, { "fish_trout_rainbow", "fish26" }, { "fish_salmon", "fish27" },
                { "fish_snail", "fish28" }, { "fish_sturgeon", "fish29" }, { "fish_toad", "fish30" },
                { "fish_turtle", "fish31" }, { "fish_walleye", "fish32" },
                { "fish_angelfish", "fish33" }, { "fish_bluefish", "fish34" }, { "fish_clownfish", "fish35" },
                { "fish_dogfish", "fish36" }, { "fish_eel", "fish37" }, { "fish_flounder", "fish38" },
                { "fish_goldfish_salt", "fish39" }, { "fish_grouper", "fish40" }, { "fish_herring", "fish41" },
                { "fish_krill", "fish42" }, { "fish_lionfish", "fish43" }, { "fish_lobster", "fish44" },
                { "fish_manowar", "fish45" }, { "fish_mantaray", "fish46" }, { "fish_manta_ray", "fish46" },
                { "fish_marlin", "ocean_fish2" }, { "fish_octopus", "ocean_fish3" }, { "fish_oyster", "fish49" },
                { "fish_sawfish", "fish50" }, { "fish_seahorse", "fish51" }, { "fish_seaturtle", "fish52" },
                { "fish_sea_turtle", "fish52" }, { "fish_shark_greatwhite", "fish53" },
                { "fish_shark_hammerhead", "fish54" }, { "fish_shrimp", "fish55" }, { "fish_squid", "fish56" },
                { "fish_stingray", "fish57" }, { "fish_sunfish", "fish58" }, { "fish_swordfish", "fish59" },
                { "fish_tuna", "fish60" }, { "fish_whale", "fish61" },
                { "fish_anomalocaris", "prehistoric_fish1" }, { "fish_helicoprion", "prehistoric_fish2" },
                { "fish_leedsichthys", "prehistoric_fish3" }, { "fish_coelacanth", "prehistoric_fish1" },
                { "fish_ufo", "alien_creature2" }, { "fish_mantaray_golden", "ocean_fish1" },
                { "fish_salmon_atlantic", "fish27" }, { "fish_wolffish", "deep_sea_fish2" },
                { "fish_horseshoecrab", "fish9" }
            };
            if (mapping.TryGetValue(id.ToLower(), out string name)) return name;
            return null;
        }
    }
}
