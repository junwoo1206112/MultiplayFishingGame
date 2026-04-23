using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace MultiplayFishing.Editor
{
    public class TerrainTreeCleaner : EditorWindow
    {
        [MenuItem("Tools/Terrain/Remove Missing Trees")]
        public static void RemoveMissingTrees()
        {
            // 프로젝트 내의 모든 TerrainData 에셋 찾기
            string[] guids = AssetDatabase.FindAssets("t:TerrainData");
            int totalFixed = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TerrainData terrainData = AssetDatabase.LoadAssetAtPath<TerrainData>(path);

                if (terrainData == null) continue;

                // 현재 등록된 나무들 가져오기
                List<TreePrototype> prototypes = new List<TreePrototype>(terrainData.treePrototypes);
                int originalCount = prototypes.Count;

                // 프리팹이 null인 항목(Missing) 제거
                prototypes.RemoveAll(p => p.prefab == null);

                if (prototypes.Count != originalCount)
                {
                    terrainData.treePrototypes = prototypes.ToArray();
                    EditorUtility.SetDirty(terrainData);
                    AssetDatabase.SaveAssets();
                    
                    int removedCount = originalCount - prototypes.Count;
                    Debug.Log($"<b>[Terrain Cleaner]</b> Removed {removedCount} missing trees from: {path}");
                    totalFixed += removedCount;
                }
            }

            AssetDatabase.Refresh();
            
            if (totalFixed > 0)
            {
                EditorUtility.DisplayDialog("Terrain Cleanup", $"총 {totalFixed}개의 유실된 나무 프리팹을 정리했습니다!", "확인");
            }
            else
            {
                Debug.Log("[Terrain Cleaner] No missing trees found.");
                EditorUtility.DisplayDialog("Terrain Cleanup", "유실된 나무 프리팹이 없습니다.", "확인");
            }
        }
    }
}
