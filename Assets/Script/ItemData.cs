using System;
using UnityEngine;

[Serializable]
public class ItemData
{
    public int id;
    public string itemName;
    public string description;
    public string nameEng;
    public string itemTypeString;

    [NonSerialized]
    public ItemType itemType;
    public int price;
    public int power;
    public int levle;
    public bool isStackable;
    public string iconPath;
}
