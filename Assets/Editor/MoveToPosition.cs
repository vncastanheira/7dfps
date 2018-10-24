using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assets.EditorScripts
{
    public class MoveToPosition : EditorWindow
    {
        [MenuItem("GameObject/Move to Position")]
        static void Set()
        {
            Selection.activeGameObject.transform.position = SceneView.lastActiveSceneView.camera.transform.position;
        }

    }
}
