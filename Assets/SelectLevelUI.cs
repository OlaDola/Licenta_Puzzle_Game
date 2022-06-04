using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Utility_Functions;
using UnityEngine.Events;

public class SelectLevelUI : MonoBehaviour
{ 
    public event EventHandler OnSelectLevel;
    [SerializeField] Grid_Show GridSystem;
    [SerializeField] GameObject LevelUI;
    [SerializeField] GameObject SaveButton;

    private string LevelSelected;

    private void Start()
    {
        LevelSelected = "";
        OnSelectLevel += LevelSelect_OnSelectedChanged;

        RefreshSelectedVisual();

        foreach (Transform child in transform)
        {
            //currentButton = child;
            Button btn = child.GetComponent<Button>();
            btn.onClick.AddListener(SetLevelDisplay);

        }
    }

    private void LateUpdate()
    {
        if (!Grid_Show.isEmpty)
        {
            if (SaveButton.activeSelf == Grid_Show.isSaved)
            {
                SaveButton.SetActive(!Grid_Show.isSaved);
            }
        }
        
    }

    private void SetLevelDisplay()
    {
        LevelSelected = EventSystem.current.currentSelectedGameObject.name;
        GameObject.Find("LevelName").GetComponentInChildren<TMPro.TextMeshProUGUI>().text = LevelSelected;
        GridSystem.Load(LevelSelected);
        RefreshSelectedVisual();
    }

    private void LevelSelect_OnSelectedChanged(object sender, System.EventArgs e)
    {
        RefreshSelectedVisual();
    }

    private void RefreshSelectedVisual()
    {
        DirectoryInfo dirInfo = new DirectoryInfo(SaveSystem.GetFolderPath());
        //DirectoryInfo[] levelFolders = dirInfo.GetDirectories();
        Transform[] childList = transform.GetComponentsInChildren<Transform>();
        List<string> childListName = new List<string>();

        foreach (Transform child in childList)
            if(!childListName.Contains(child.name))
                childListName.Add(child.name);

        foreach (var levelFolder in dirInfo.GetDirectories())
        {
            print(levelFolder.Name);
            if (!childListName.Contains(levelFolder.Name))
            {
                GameObject levelUI = Instantiate(LevelUI, transform);
                SettingUI(levelUI, levelFolder.Name);
            }
        }
        foreach (Transform child in transform)
        {
             child.Find("Selected").gameObject.SetActive(LevelSelected == child.name);
        }
        PlayerPrefs.SetString("LevelSystemSave", LevelSelected);
    }

    public void SettingUI(GameObject UI, string levelName, Sprite image = null)
    {
        //UI.GetComponent<RectTransform>().localScale = new Vector3(1.2f, 1.2f, 1);
        if (image)
            UI.transform.Find("Image").GetComponent<Image>().sprite = image;
        UI.transform.Find("LevelName").GetComponent<TMPro.TextMeshProUGUI>().SetText(levelName);
        UI.transform.gameObject.name = levelName;
    }

    public void SaveLevel()
    {
        if (LevelSelected != "")
        {
            GridSystem.Save(LevelSelected);
            RefreshSelectedVisual();
        }
    }
}
