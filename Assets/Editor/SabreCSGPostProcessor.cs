using Sabresaurus.SabreCSG;
using UnityEngine;

namespace Assets.EditorScripts
{
    public static class SabreCSGPostProcessor {

        [PostProcessCSGBuild]
        public static void OnPostProcessCSGBuild(Transform meshGroup)
        {
            if (meshGroup != null)
            {
                SetLayerRecursively(meshGroup.gameObject, 11); // 11 :World
            }
        }

        public static void SetLayerRecursively(GameObject GO, int layer)
        {
            GO.layer = layer;
            GO.isStatic = true;
            for (int i = 0; i < GO.transform.childCount; i++)
                SetLayerRecursively(GO.transform.GetChild(i).gameObject, layer);
        }
    }
}
