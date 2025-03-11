using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;
using System.Collections.Generic;

public class MenuPanel : MonoBehaviour
{

    public List<GameObject> prefabs;

    public static List<GameObject> items = new List<GameObject>();

    public void CreateGameObjectFromClick(int prefabIndex)
    {
        var well = Instantiate(prefabs[prefabIndex], Vector3.zero, Quaternion.identity);
        var daWell = well.GetComponent<DragDrop>();

        daWell.isDragging = true;
        daWell.menuPanel = this;

        well.tag = "Instantiated";

        HideMenu(false);
        items.Add(well);
    }

    public void HideMenu(bool show)
    {
        this.gameObject.SetActive(show);
    }

    public void ResetGame()
    {
        foreach (var well in items)
        {
            Destroy(well);
            Debug.Log("Destroying well");
        }
    }

    //public void SaveWorld()
    //{
    //  foreach (var well in items)
    //{
    //    Debug.Log(well.gameObject.tag);
    //}

    //    }
}