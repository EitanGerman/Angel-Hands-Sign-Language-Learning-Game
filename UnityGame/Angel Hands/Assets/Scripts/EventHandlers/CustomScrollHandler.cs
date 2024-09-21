using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace Assets.Scripts.EventHandlers
{

    public class CustomScrollHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private ScrollRect scrollRect;
        private bool isPointerInside = false;

        void Awake()
        {
            scrollRect = GetComponent<ScrollRect>();
        }

        void Update()
        {
            // Capture scroll wheel input when the mouse is inside the Scroll View area
            if (isPointerInside && Input.GetAxis("Mouse ScrollWheel") != 0)
            {
                float scrollInput = Input.GetAxis("Mouse ScrollWheel");

                // Apply scrolling based on the scroll sensitivity
                scrollRect.verticalNormalizedPosition += scrollInput * scrollRect.scrollSensitivity;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isPointerInside = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isPointerInside = false;
        }
    }

}
