using UnityEngine;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;

public class ItemDataLoader : MonoBehaviour
{
    [SerializeField]
    private string jsonFileName = "items";      //Resource 폴더에서 가저올 JSON 파일 이름

    private List<ItemData> itemList;

    void Start()
    {
        LoadItemData();    
    }

    private string EncodeKorean(string Text)
    {
        if (string.IsNullOrEmpty(Text)) return "";
        byte[] bytes = Encoding.Default.GetBytes(Text);
        return Encoding.UTF8.GetString(bytes);
    }

    // Update is called once per frame
    void LoadItemData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(jsonFileName);

        if (jsonFile != null)
        {
            byte[] bytes = Encoding.Default.GetBytes(jsonFile.text);
            string currnetText = Encoding.UTF8.GetString(bytes);

            itemList = JsonConvert.DeserializeObject<List<ItemData>>(currnetText);

            Debug.Log($"로드된 아이템 수 : {itemList.Count}");

            foreach(var item in itemList)
            {
                Debug.Log($"아이템 : {EncodeKorean(item.itemName)}, 설명 : {EncodeKorean(item.description)}");
            }
        }
        else
        {
            Debug.LogError($"JSON 파일을 찾을 수 없습니다. : {jsonFileName}");
        }
    }
}
