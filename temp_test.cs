using UnityEngine;
using UnityEditor;

public class GetUnityStatus : EditorWindow
{
    [MenuItem("Tools/Get Unity Status")]
    public static void ShowStatus()
    {
        var status = new
        {
            IsPlaying = EditorApplication.isPlaying,
            IsPaused = EditorApplication.isPaused,
            IsCompiling = EditorApplication.isCompiling,
            CurrentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
            SelectedObject = Selection.activeObject?.name ?? "None",
            Time = System.DateTime.Now.ToString()
        };
        
        Debug.Log($"Unity Status: {status}");
    }
}
