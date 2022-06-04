using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowListMainMenu : MonoBehaviour
{
    [SerializeField] Animator transitionMainMenu;
    [SerializeField] Animator transitionLevelList;

    [SerializeField] Level_Loader levelLoader;
    public void LevelList()
    {
        transitionLevelList.SetTrigger("LevelList");
        transitionMainMenu.SetTrigger("LevelList");
    }

    public void EnterEditor()
    {
        if (levelLoader.gameObject.activeSelf == false)
            levelLoader.gameObject.SetActive(true);
        StartCoroutine(StartEditor());
    }

    public void EnterPlayMode()
    {
        if(levelLoader.gameObject.activeSelf == false)
            levelLoader.gameObject.SetActive(true);
        StartCoroutine(StartPlaying());
    }

    IEnumerator StartPlaying()
    {
        levelLoader.LevelLoader("PlayMode", 1);

        yield return null;
    }
    
    IEnumerator StartEditor()
    {
        PlayerPrefs.DeleteKey("LevelSystemSave");
        levelLoader.LevelLoader("LevelEditor", 0);

        yield return null;
    }
}
