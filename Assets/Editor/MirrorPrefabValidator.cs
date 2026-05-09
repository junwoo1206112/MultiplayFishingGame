using Mirror;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MultiplayFishing.Editor
{
    /// <summary>
    /// Clears stale Mirror scene IDs from scene instances during scene processing.
    /// It intentionally avoids force-loading every prefab in the project because
    /// that can surface unrelated missing-script warnings while the editor is importing.
    /// </summary>
    public class MirrorPrefabValidator : IProcessSceneWithReport
    {
        public int callbackOrder => 0;

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            NetworkIdentity[] identities = Object.FindObjectsByType<NetworkIdentity>(FindObjectsSortMode.None);

            foreach (NetworkIdentity identity in identities)
            {
                if (identity == null || identity.gameObject.scene != scene)
                {
                    continue;
                }

                if (!PrefabUtility.IsPartOfPrefabInstance(identity) || identity.sceneId == 0)
                {
                    continue;
                }

                SerializedObject serializedIdentity = new SerializedObject(identity);
                serializedIdentity.FindProperty("m_SceneId").longValue = 0;
                serializedIdentity.ApplyModifiedProperties();

                Debug.Log($"<color=cyan><b>[MirrorValidator]</b></color> Reset stale sceneId on '{identity.gameObject.name}' in scene '{scene.name}'.");
            }
        }
    }
}
