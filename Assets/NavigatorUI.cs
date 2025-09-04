using UnityEngine;

public class NavigatorUI : MonoBehaviour
{
    public GameObject currentPanel;
    public GameObject targetPanel;

    public void SwitchPanels()
    {
        if (currentPanel != null)
            currentPanel.SetActive(false);

        if (targetPanel != null)
            targetPanel.SetActive(true);
    }
}
