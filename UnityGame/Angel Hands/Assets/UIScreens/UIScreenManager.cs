using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIScreenManager : MonoBehaviour
{
    public UIDocument uiDocumentOne;
    public UIDocument uiDocumentTwo;

    void Start()
    {
        // Initially show first UI and hide second UI
        ShowUI(uiDocumentOne, true);
        ShowUI(uiDocumentTwo, false);
    }

    void ShowUI(UIDocument uiDocument, bool show)
    {
        // This approach depends on whether your version supports the `visibility` property
        // or if you need to add/remove elements from the tree.
        uiDocument.rootVisualElement.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
    }

    public void ToggleUI()
    {
        bool isUIDocOneVisible = uiDocumentOne.rootVisualElement.style.display == DisplayStyle.Flex;
        ShowUI(uiDocumentOne, !isUIDocOneVisible);
        ShowUI(uiDocumentTwo, isUIDocOneVisible);
    }
}
