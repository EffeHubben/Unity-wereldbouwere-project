using System;
using UnityEngine;

[System.Serializable]
public class PrefabData
{
    public Guid id;
    public string environmentId;
    public string prefabId;
    public float positionX;
    public float positionY;
    public float scaleX;
    public float scaleY;
    public float rotationZ;
    public int sortingLayer;
}