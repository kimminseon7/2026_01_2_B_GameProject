using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDataBaseSO", menuName = "Inventory/DataBase")]
public class ItemDataBaseSO : ScriptableObject
{
    public List<ItemSO> items = new List<ItemSO>();

    //캐싱을 위한 Dictrionary
    private Dictionary<int, ItemSO> itemByld;
    private Dictionary<string, ItemSO> itemByName;

    public void Initialze()
    {
        itemByld = new Dictionary<int, ItemSO>();
        itemByName = new Dictionary<string, ItemSO>();

        foreach(var item in items)
        {
            itemByld[item.id] = item;
            itemByName[item.itemName] = item;
        }
    }

    //ID 로 아이템 찾기

    public ItemSO GetItemByld(int id)
    {
        if(itemByld == null)
        {
            Initialze();
        }
        if(itemByld.TryGetValue(id, out ItemSO item))
            return item;

        return null;
    }
    //이름으로 아이템 찾기
    public ItemSO GetItemByName(string name)
    {
        if(itemByName == null)
        {
            Initialze();             
        }

        if (itemByName.TryGetValue(name, out ItemSO item))
            return item;

        return null;
    }

    //타입으로 아이템 필터링
    public List<ItemSO> GetItemByType(ItemType type)
    {
        return items.FindAll(item => item.itemType == type);
    }
}
