using UnityEditor;
using Mirror;
using UnityEngine;
using System.IO;

namespace MultiplayFishing.Editor
{
    /// <summary>
    /// 유니티 6 환경에서 Mirror 프리팹의 잘못된 Network SceneId를 정밀 타격하여 제거하는 최종 도구입니다.
    /// </summary>
    public class MirrorPrefabFixer
    {
        [MenuItem("Mirror/Ultimate Precision Wipe (Unity 6)")]
        public static void UltimateWipe()
        {
            // 에러 메시지에 보고된 경로와 실제 경로 모두 시도
            string[] paths = { 
                "Assets/Prefabs/Player/GamePlayer.prefab",
                "Assets/Prefabs/GamePlayer.prefab" 
            };

            int successCount = 0;

            foreach (string path in paths)
            {
                if (!File.Exists(Path.Combine(Application.dataPath, "..", path))) continue;

                GameObject contents = null;
                try 
                {
                    contents = PrefabUtility.LoadPrefabContents(path);
                    if (contents == null) continue;

                    NetworkIdentity ni = contents.GetComponent<NetworkIdentity>();
                    if (ni != null)
                    {
                        SerializedObject so = new SerializedObject(ni);
                        SerializedProperty sceneIdProp = so.FindProperty("m_SceneId");
                        
                        if (sceneIdProp != null)
                        {
                            sceneIdProp.longValue = 0;
                            so.ApplyModifiedPropertiesWithoutUndo();
                            
                            PrefabUtility.SaveAsPrefabAsset(contents, path);
                            Debug.Log($"<color=green><b>[Success]</b></color> 프리팹 정밀 세척 완료: {path}");
                            successCount++;
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[MirrorFix] {path} 처리 중 오류: {e.Message}");
                }
                finally
                {
                    if (contents != null) PrefabUtility.UnloadPrefabContents(contents);
                }
            }

            if (successCount > 0)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                Debug.Log("<color=cyan><b>[Done]</b> 모든 정밀 세척이 완료되었습니다. 이제 안심하고 Play 하세요!</color>");
            }
            else
            {
                Debug.Log("<color=yellow><b>[Info]</b> 세척할 파일을 찾지 못했거나 이미 깨끗합니다. 씬 하이어라키를 확인하세요.</color>");
            }
        }
    }
}
