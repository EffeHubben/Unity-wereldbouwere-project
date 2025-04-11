using UnityEngine;
using UnityEngine.UI;
using System;

public class DragDrop : MonoBehaviour
{
    public bool isDragging;
    public MenuPanel menuPanel;
    private void OnMouseDown()
    {
        isDragging = !isDragging;

        if (isDragging == false)
        {
            menuPanel.HideMenu(true);
        }
    }
    void Update()
    {
        if (isDragging)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(mousePosition.x, mousePosition.y, 0);
        }
    }
}