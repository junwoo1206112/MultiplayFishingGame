using System.Linq;
using UnityEditor;
using UnityEngine;
using MultiplayFishing.Gameplay;

namespace MultiplayFishing.Editor
{
    public static class PlayerFishingPrefabSetupTool
    {
        private const string PlayerPrefabPath = "Assets/Prefabs/Player/Player.prefab";

        [MenuItem("MultiplayFishing/Setup/Fix Player Fishing Prefab References")]
        public static void FixPlayerFishingPrefabReferences()
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            if (prefabRoot == null)
            {
                Debug.LogError($"[PlayerFishingPrefabSetupTool] Player prefab not found: {PlayerPrefabPath}");
                return;
            }

            try
            {
                bool changed = false;
                Animator animator = prefabRoot.GetComponentInChildren<Animator>(true);
                Transform rodSocket = FindChildRecursive(prefabRoot.transform, "RodSocket");
                Transform rodVisual = FindBestRodVisual(rodSocket) ?? rodSocket;

                changed |= FixRodVisibility(prefabRoot, animator, rodVisual);
                changed |= FixFishingLineVisual(prefabRoot, rodSocket);

                if (changed)
                {
                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, PlayerPrefabPath);
                    AssetDatabase.SaveAssets();
                    Debug.Log("[PlayerFishingPrefabSetupTool] Player fishing prefab references fixed and saved.");
                }
                else
                {
                    Debug.Log("[PlayerFishingPrefabSetupTool] Player fishing prefab references already look valid.");
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static bool FixRodVisibility(GameObject prefabRoot, Animator animator, Transform rodVisual)
        {
            bool changed = false;
            FishingRodVisibility visibility = prefabRoot.GetComponent<FishingRodVisibility>();
            if (visibility == null)
            {
                visibility = prefabRoot.AddComponent<FishingRodVisibility>();
                changed = true;
            }

            SerializedObject serialized = new SerializedObject(visibility);
            changed |= SetObjectReference(serialized, "animator", animator);
            changed |= SetObjectReference(serialized, "rodVisualRoot", rodVisual != null ? rodVisual.gameObject : null);
            changed |= SetString(serialized, "rodHideStateName", "rod-out");
            changed |= SetString(serialized, "rodShowStateName", "rod-in");
            changed |= SetFloat(serialized, "rodHideNormalizedTime", 0.95f);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            return changed;
        }

        private static bool FixFishingLineVisual(GameObject prefabRoot, Transform rodSocket)
        {
            FishingLineVisual lineVisual = prefabRoot.GetComponentInChildren<FishingLineVisual>(true);
            if (lineVisual == null || rodSocket == null)
            {
                return false;
            }

            bool changed = false;
            SerializedObject serialized = new SerializedObject(lineVisual);
            changed |= SetObjectReference(serialized, "rodLineFixed", FindChildRecursive(rodSocket, "RodLineFixed")?.GetComponent<LineRenderer>());
            changed |= SetObjectReference(serialized, "rodLineCast", FindChildRecursive(rodSocket, "RodLineCast")?.GetComponent<LineRenderer>());
            changed |= SetObjectReference(serialized, "reelPoint", FindChildRecursive(rodSocket, "ReelPoint"));
            changed |= SetObjectReference(serialized, "tipPoint", FindChildRecursive(rodSocket, "TipPoint"));
            changed |= SetObjectReference(serialized, "hookPoint", FindChildRecursive(rodSocket, "HookPoint"));

            Transform[] guides = rodSocket
                .GetComponentsInChildren<Transform>(true)
                .Where(child => child.name.StartsWith("GuidePoint"))
                .OrderBy(child => child.name)
                .ToArray();

            SerializedProperty guidePoints = serialized.FindProperty("guidePoints");
            if (guidePoints != null && !SameArray(guidePoints, guides))
            {
                guidePoints.arraySize = guides.Length;
                for (int i = 0; i < guides.Length; i++)
                {
                    guidePoints.GetArrayElementAtIndex(i).objectReferenceValue = guides[i];
                }

                changed = true;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            return changed;
        }

        private static bool SetObjectReference(SerializedObject serialized, string propertyName, Object value)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null || property.objectReferenceValue == value)
            {
                return false;
            }

            property.objectReferenceValue = value;
            return true;
        }

        private static bool SetString(SerializedObject serialized, string propertyName, string value)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null || property.stringValue == value)
            {
                return false;
            }

            property.stringValue = value;
            return true;
        }

        private static bool SetFloat(SerializedObject serialized, string propertyName, float value)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null || Mathf.Approximately(property.floatValue, value))
            {
                return false;
            }

            property.floatValue = value;
            return true;
        }

        private static bool SameArray(SerializedProperty property, Transform[] values)
        {
            if (property.arraySize != values.Length)
            {
                return false;
            }

            for (int i = 0; i < values.Length; i++)
            {
                if (property.GetArrayElementAtIndex(i).objectReferenceValue != values[i])
                {
                    return false;
                }
            }

            return true;
        }

        private static Transform FindChildRecursive(Transform parent, string exactName)
        {
            if (parent == null) return null;

            foreach (Transform child in parent)
            {
                if (child.name == exactName)
                {
                    return child;
                }

                Transform result = FindChildRecursive(child, exactName);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private static Transform FindBestRodVisual(Transform rodSocket)
        {
            if (rodSocket == null) return null;

            foreach (Transform child in rodSocket.GetComponentsInChildren<Transform>(true))
            {
                if (child == rodSocket) continue;
                if (IsRodVisualCandidate(child))
                {
                    return child;
                }
            }

            return null;
        }

        private static bool IsRodVisualCandidate(Transform child)
        {
            string lower = child.name.ToLowerInvariant();
            if (lower.Contains("line") ||
                lower.Contains("hook") ||
                lower.Contains("tip") ||
                lower.Contains("guide") ||
                lower.Contains("socket") ||
                lower.Contains("lure"))
            {
                return false;
            }

            return lower == "model" ||
                   lower.Contains("rod") ||
                   lower.Contains("fishingrod") ||
                   lower.Contains("sk_fishingrod");
        }
    }
}
