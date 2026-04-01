#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using NUnit.Framework;
using System;


public enum ConversionType
{
    Items,
    Dialogs
}

[Serializable]

public class DialogRowData
{
    public int? id;
    public string characterName;
    public string text;
    public int? nextId;
    public string portraitPath;
    public string choiceText;
    public int? choiceNextId;
}

public class JsonToScriptableConverter : EditorWindow
{
    private string jsonFilePath = "";
    private string outputFolder = "Assets/ScriptableObjects";
    private bool createDatabase = true;
    private ConversionType conversionType = ConversionType.Items;

    [MenuItem("Tools/JSON to Scriptable Objects")]

    public static void ShowWindow()
    {
        GetWindow<JsonToScriptableConverter>("JSON to Scriptable Objects");
    }

    void OnGUI()
    {
        GUILayout.Label("JSON to Scritable ogject Converter", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if(GUILayout.Button("Select JSON File"))
        {
            jsonFilePath = EditorUtility.OpenFilePanel("Select JSON File", "", "json");
        }

        EditorGUILayout.LabelField("Selected File : ", jsonFilePath);
        EditorGUILayout.Space();

        conversionType = (ConversionType)EditorGUILayout.EnumPopup("Conversion Type:", conversionType);

        if (conversionType == ConversionType.Items && outputFolder == "Assets/ScriptableObjects")
        {
            outputFolder = "Assets/ScriptableObjects/Item";
        }
        else if (conversionType == ConversionType.Dialogs && outputFolder == "Assets/ScriptableObjects")
        {
            outputFolder = "Assets/ScriptableObjects";
        }

        outputFolder = EditorGUILayout.TextField("Output Foloder : ", outputFolder);
        createDatabase = EditorGUILayout.Toggle("Create Databse Asset", createDatabase);
        EditorGUILayout.Space();

        if (GUILayout.Button("Convert to Scriptable Object"))
        {
            if(string.IsNullOrEmpty(jsonFilePath))
            {
                EditorUtility.DisplayDialog("Error", "Pease Select a JSON file first", "OK");
                return;
            }

            switch (conversionType)
            {
                case ConversionType.Items:
                ConvertJsonToItemScriptableObject();
                break;
                case ConversionType.Dialogs:
                ConvertJsonToDialogScrptableObjects();
                break;
            }
            
        }
    }

    private void ConvertJsonToItemScriptableObject()
    {
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        string jsonText = File.ReadAllText(jsonFilePath);

        try
        {
            List<ItemData> itemDataList = JsonConvert.DeserializeObject<List<ItemData>>(jsonText);

            List<ItemSO> createdItems = new List<ItemSO>();

            foreach (ItemData itemData in itemDataList)
            {
                ItemSO itemSO = ScriptableObject.CreateInstance<ItemSO>();

                itemSO.id = itemData.id;
                itemSO.itemName = itemData.itemName;
                itemSO.nameEng = itemData.nameEng;
                itemSO.description = itemData.description;

                if(System.Enum.TryParse(itemData.itemTypeString, out ItemType parsedType))
                {
                    itemSO.itemType = parsedType;
                }
                else
                {
                    Debug.LogWarning($"아이템 {itemData.itemName}의 유효하지 않은 타입 : {itemData.itemTypeString}");
                }

                itemSO.price = itemData.price;
                itemSO.power = itemData.power;
                itemSO.level = itemData.levle;
                itemSO.isStackable = itemData.isStackable;

                if(!string.IsNullOrEmpty(itemData.iconPath))
                {
                    itemSO.icon = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Resources/{itemData.iconPath}.png");

                    if (itemSO.icon == null)
                    {
                        Debug.LogWarning($"아이템 {itemData.nameEng} 의 아이콘을 찾을 수 없습니다. : {itemData.iconPath}");
                    }
                }

                string assetPath = $"{outputFolder}/Item_{itemData.id.ToString("D4")}_{itemData.nameEng}.asset";
                AssetDatabase.CreateAsset(itemSO, assetPath);

                itemSO.name = $"Item_{itemData.id.ToString("D4")} + {itemData.nameEng}";
                createdItems.Add(itemSO);

                EditorUtility.SetDirty(itemSO);
            }
            if (createDatabase && createdItems.Count > 0)
            {
                ItemDataBaseSO dataBase = ScriptableObject.CreateInstance<ItemDataBaseSO>();
                dataBase.items = createdItems;

                AssetDatabase.CreateAsset(dataBase, $"{outputFolder}/ItemDatabase.asset");
                EditorUtility.SetDirty(dataBase);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Sucess", $"Created {createdItems.Count} ScriptableObject objects!", "OK");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to Convert JSON : {e.Message}", "OK");
            Debug.LogError($"JSON 변환 오류 : {e}");
        }
    }

    private void ConvertJsonToDialogScrptableObjects()
    {
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        string JsonText = File.ReadAllText(jsonFilePath);

        try
        {

            List<DialogRowData> rowDataList = JsonConvert.DeserializeObject<List<DialogRowData>>(JsonText);

            Dictionary<int, DialogSO> dialogMap = new Dictionary<int, DialogSO>();
            List<DialogSO> createDialogs = new List<DialogSO>();

            //1단계 : 대화 항목 생성
            foreach(var rowData in rowDataList)
            {
                if (!rowData.id.HasValue)
                    continue;

                DialogSO dialogSO = ScriptableObject.CreateInstance<DialogSO>();

                dialogSO.id = rowData.id.Value;
                dialogSO.characterName = rowData.characterName;
                dialogSO.text = rowData.text;
                dialogSO.nextId = rowData.nextId.HasValue ? rowData.nextId.Value : -1;
                dialogSO.portraitPath = rowData.portraitPath;
                dialogSO.choices = new List<DialogChoiceSO>();

                if (!string.IsNullOrEmpty(rowData.portraitPath))
                {
                    dialogSO.portrait = Resources.Load<Sprite>(rowData.portraitPath);

                    if (dialogSO.portrait == null)
                    {
                        Debug.LogWarning($"대화 {rowData.id}의 초상화를 찾을 수 없습니다.");
                    }
                }
                dialogMap[dialogSO.id] = dialogSO;
                createDialogs.Add(dialogSO);
            }
            //2단계 : 선택지 항목 처리 및 연결
            foreach (var rowData in rowDataList)
            {
                if (!rowData.id.HasValue && !string.IsNullOrEmpty(rowData.choiceText) && rowData.choiceNextId.HasValue)
                {
                    int parentId = -1;
                    
                    int currentIndex = rowDataList.IndexOf(rowData);
                    for (int i = currentIndex - 1; i >= 0; i--)
                    {
                        if (rowDataList[i].id.HasValue)
                        {
                            parentId = rowDataList[i].id.Value;
                            break;
                        }
                    }

                    if(parentId == -1)
                    {
                        Debug.LogWarning($"선택지 {rowData.choiceText} 의 부모 대화를 찾을 수 없습니다.");
                    }

                    if(dialogMap.TryGetValue(parentId , out DialogSO parentDialog))
                    {
                        DialogChoiceSO choiceSO = ScriptableObject.CreateInstance<DialogChoiceSO>();
                        choiceSO.text = rowData.choiceText;
                        choiceSO.nextId = rowData.choiceNextId.Value;

                        string choiceAssetPath = $"{outputFolder}/choice_{parentId}_{parentDialog.choices.Count + 1}.asset";
                        AssetDatabase.CreateAsset(choiceSO, choiceAssetPath);
                        EditorUtility.SetDirty(choiceSO);
                        parentDialog.choices.Add(choiceSO);
                    }
                    else
                    {
                        Debug.LogWarning($"선택지 {rowData.choiceText}를 연결할 대화 (ID : {parentId}를 찾을 수 없습니다.");
                    }
                }
            }

            //3단계 : 대화 스크립터블 오브젝트 저장

            foreach(var dialog in createDialogs)
            {
                string assetPath = $"{outputFolder}/Dialog_{dialog.id.ToString("D4")}.asset";
                AssetDatabase.CreateAsset( dialog, assetPath );

                dialog.name = $"Dialog_{dialog.id.ToString("D4")}";

                EditorUtility.SetDirty(dialog);
            }

            if (createDatabase && createDialogs.Count > 0)
            {
                DialogDatabaseSO database = ScriptableObject.CreateInstance<DialogDatabaseSO>();
                database.dialogs = createDialogs;

                AssetDatabase.CreateAsset(database, $"{outputFolder}/DialogDatabase.asset");
                EditorUtility.SetDirty(database);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Success", $"created {createDialogs.Count} dialog scriptable objects!", "OK");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Faild ot conver JSON : {e.Message}", "OK");
            Debug.LogError($"JSON 변환 오류 : {e}");
        }
    }
}

#endif