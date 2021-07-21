using UnityEngine;

#if (UNITY_EDITOR)
public class KeepSceneAlive : MonoBehaviour
{
    public bool KeepSceneViewActive;

    void Start()
    {
        if (this.KeepSceneViewActive && Application.isEditor)
        {
            UnityEditor.SceneView.FocusWindowIfItsOpen(typeof(UnityEditor.SceneView));
        }
    }



}
#endif
