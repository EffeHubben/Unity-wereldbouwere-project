using UnityEngine;

public class MenuPanel : MonoBehaviour
{
    public GameObject sidePanel;
    public bool isPanelOpen = false;

    public void TogglePanel()
    {
        isPanelOpen = !isPanelOpen;
        sidePanel.SetActive(isPanelOpen);
    }

    public void OpenPanel()
    {
        isPanelOpen = true;
        sidePanel.SetActive(true);
    }

    public void ClosePanel()
    {
        isPanelOpen = false;
        sidePanel.SetActive(false);
    }
}