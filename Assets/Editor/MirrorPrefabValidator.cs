using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MultiplayFishing.Editor
{
    /// <summary>
    /// 빌드 또는 플레이 모드 진입 시 프리팹의 잘못된 Network SceneId를 자동으로 정리하는 도구입니다.
    /// 팀 프로젝트에서 발생할 수 있는 'Scene object Player has no valid sceneId' 에러를 근본적으로 방지합니다.
    /// </summary>
    public class MirrorPrefabValidator : IProcessSceneWithReport
    {
        public int callbackOrder => 0;

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            // 현재 씬에 있는 모든 NetworkIdentity를 검사
            NetworkIdentity[] identities = Object.FindObjectsByType<NetworkIdentity>(FindObjectsSortMode.None);
            
            foreach (NetworkIdentity ni in identities)
            {
                // 프리팹 에셋인데 SceneId가 0이 아닌 경우 (오염된 상태)를 찾아 수정합니다.
                if (PrefabUtility.IsPartOfPrefabAsset(ni) && ni.sceneId != 0)
                {
                    SerializedObject so = new SerializedObject(ni);
                    so.FindProperty("m_SceneId").longValue = 0;
                    so.ApplyModifiedProperties();
                    
                    Debug.Log($"<color=cyan><b>[MirrorValidator]</b></color> 프리팹 '{ni.gameObject.name}'에 잘못 저장된 SceneId를 자동으로 초기화했습니다.");
                }
            }
            
            // 프로젝트 내의 모든 프리팹을 다시 한번 체크 (더 확실한 방법)
            CleanAllPrefabsInProject();
        }

        private void CleanAllPrefabsInProject()
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                NetworkIdentity ni = prefab.GetComponent<NetworkIdentity>();
                if (ni != null && ni.sceneId != 0)
                {
                    SerializedObject so = new SerializedObject(ni);
                    so.FindProperty("m_SceneId").longValue = 0;
                    so.ApplyModifiedProperties();
                    
                    EditorUtility.SetDirty(prefab);
                    Debug.Log($"<color=cyan><b>[MirrorValidator]</b></color> 프로젝트 에셋 '{path}'의 오염된 ID를 정리했습니다.");
                }
            }
        }
    }
}
