using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class GroupUI : MonoBehaviour
    {

        protected CanvasGroup canvasGroup;

        public void Hide()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            try
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
            catch (System.NullReferenceException)
            {
                Debug.LogError("No CanvasGroup component in " + gameObject.name);
            }

        }

        public void Show()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            try
            {

                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
            catch (System.NullReferenceException)
            {
                Debug.LogError("No CanvasGroup component in " + gameObject.name);
            }
            

        }
    }
}
