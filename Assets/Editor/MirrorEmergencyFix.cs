using UnityEditor;
using Mirror;
using UnityEngine;

namespace MultiplayFishing.Editor
{
    public class MirrorEmergencyFix
    {
        [MenuItem("Mirror/Emergency Prefab Fix")]
        public static void FixPlayerPrefab()
        {
            // 1. 프로젝트 내의 모든 프리팹을 검색합니다 (경로 문제를 피하는 가장 확실한 방법)
            string[] guids = AssetDatabase.FindAssets("t:Prefab");
            bool fixedAny = false;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                
                if (prefab == null) continue;

                // 2. NetworkIdentity가 있는 프리팹만 골라냅니다.
                NetworkIdentity ni = prefab.GetComponent<NetworkIdentity>();
                if (ni != null && ni.sceneId != 0)
                {
                    // 3. 잘못된 SceneId(0이 아닌 값)가 발견되면 0으로 초기화!
                    SerializedObject so = new SerializedObject(ni);
                    var prop = so.FindProperty("m_SceneId");
                    if (prop != null)
                    {
                        prop.longValue = 0;
                        so.ApplyModifiedProperties();
                        
                        EditorUtility.SetDirty(prefab);
                        Debug.Log($"<color=green><b>[MirrorFix]</b> '{prefab.name}' 프리팹 세척 완료!</color>");
                        fixedAny = true;
                    }
                }
            }

            if (fixedAny)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("<color=cyan><b>[MirrorFix]</b> 모든 프리팹 정리가 완료되었습니다. 이제 Play를 눌러보세요!</color>");
            }
            else
            {
                Debug.Log("<color=yellow><b>[MirrorFix]</b> 세척할 프리팹이 발견되지 않았습니다. 이미 깨끗한 상태이거나 NetworkIdentity가 프리팹에 없습니다.</color>");
            }
        }
    }
}
