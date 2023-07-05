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
    [SerializeField] PauseMenu PauseMenu;
    [SerializeField] Grid_Show GridSystem;
    [SerializeField] GameObject LevelUI;
    [SerializeField] GameObject SaveButton;
    [SerializeField] TMPro.TMP_InputField newLevelName;

    private string LevelSelected;
    public static bool newLevel;

    private void Start()
    {
        PauseMenu.newLevelEmpty = true;
        LevelSelected = "";
        OnSelectLevel += LevelSelect_OnSelectedChanged;

        RefreshSelectedVisual();

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
        LevelSelected = EventSystem.current.currentSelectedGameObject.transform.parent.name;
        GameObject.Find("LevelName").GetComponentInChildren<TMPro.TextMeshProUGUI>().text = LevelSelected;
        GridSystem.Load(LevelSelected);
        RefreshSelectedVisual();
        PauseMenu.newLevel = true;
        PauseMenu.newLevelEmpty = false;
    }

    private void LevelSelect_OnSelectedChanged(object sender, System.EventArgs e)
    {
        RefreshSelectedVisual();
    }

    public void RefreshSelectedVisual()
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
            if (!childListName.Contains(levelFolder.Name))
            {
                print(levelFolder.Name);
                GameObject levelUI = Instantiate(LevelUI, transform);
                SettingUI(levelUI, levelFolder.Name);
                Button levelButton = levelUI.transform.Find("LevelButton").GetComponent<Button>();
                levelButton.onClick.AddListener(SetLevelDisplay);
                Button deleteButton = levelUI.transform.Find("DeleteButton").GetComponent<Button>();
                deleteButton.onClick.AddListener(DeleteLevel);
            }
        }
        foreach (Transform child in transform)
        {
             child.Find("LevelButton").Find("Selected").gameObject.SetActive(LevelSelected == child.name);
        }
        PlayerPrefs.SetString("LevelSystemSave", LevelSelected);
    }

    private void DeleteLevel()
    {
        string LevelToDelete = EventSystem.current.currentSelectedGameObject.transform.parent.name;
        if (LevelSelected == LevelToDelete)
        {
            ClearLevel();
        }
        GridSystem.Delete(LevelToDelete);
        Destroy(transform.Find(LevelToDelete).gameObject);
        RefreshSelectedVisual();
    }

    public void SettingUI(GameObject UI, string levelName, Sprite image = null)
    {
        //UI.GetComponent<RectTransform>().localScale = new Vector3(1.2f, 1.2f, 1);
        if (image)
            UI.transform.Find("Image").GetComponent<Image>().sprite = image;
        UI.transform.Find("LevelButton").Find("LevelName").GetComponent<TMPro.TextMeshProUGUI>().SetText(levelName);
        UI.transform.gameObject.name = levelName;
    }

    public void ClearChildren()
    {
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }

    public void EmptyLevel()
    {
        if (!Grid_Show.isEmpty)
        {
            if (!Grid_Show.isSaved)
            {
                SaveLevel();
            }
            if (PauseMenu.newLevel)
            {
                ClearChildren();
                ClearLevel();
            }
        }
    }

    private void ClearLevel()
    {
        LevelSelected = "";
        GameObject.Find("LevelName").GetComponentInChildren<TMPro.TextMeshProUGUI>().text = LevelSelected;
        GridSystem.HandleAllDemolish();
        Grid_Show.isSaved = true;
        SaveButton.SetActive(!Grid_Show.isSaved);
        RefreshSelectedVisual();
    }

    public void newLevelTrue()
    {
        newLevel = true;
    }
    public void newLevelFalse()
    {
        newLevel = false;
    }

    public void SaveLevel()
    {
        if (LevelSelected != "")
        {
            GridSystem.Save(LevelSelected);
            RefreshSelectedVisual();
        }
        else
        {   
            PauseMenu.NewLevelName();
        }
    }
    

    public void NewLevelName()
    {

        if (newLevelName.text != "")
        {
            
            print(newLevelName.text.Length);
            GridSystem.Save(newLevelName.text);
            LevelSelected = newLevelName.text;
            RefreshSelectedVisual();
            PauseMenu.NewLevelName();
            if (newLevel)
            { 
                ClearChildren(); 
                ClearLevel(); 
            }
            GameObject.Find("LevelName").GetComponentInChildren<TMPro.TextMeshProUGUI>().text = LevelSelected;
        }
    }
}
