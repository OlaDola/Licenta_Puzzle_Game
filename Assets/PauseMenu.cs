using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public static bool isPause;
    public static bool newLevel;
    public static bool newLevelEmpty;

    [SerializeField] Grid_Show GridSystem;
    [SerializeField] GameObject unsavedChangesBackground;
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject levelCompleteMenu;
    [SerializeField] Level_Loader levelLoader;
    [SerializeField] Animator transitionUnsavedChanges;
    [SerializeField] Animator transitionNewLevelName;
    [SerializeField] Animator transitionPauseMenu;
    [SerializeField] Animator transitionListLevelMenu;
    [SerializeField] Animator transitionLevelCompleteMenu;
    [SerializeField] Animator transitionLevelCompleteLevelListMenu;
    [SerializeField] TMPro.TMP_InputField newLevelName;

    private void Awake()
    {
        GridSystem = GameObject.Find("Grid").GetComponent<Grid_Show>();
        if (!levelLoader.gameObject.activeSelf)
            levelLoader.gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPause)
                Resume();
            else
                Pause();
        }
    }

    public void Resume()
    {
        pauseMenu.SetActive(false);
        
        isPause = false;
    }

    void Pause()
    {
        pauseMenu.SetActive(true);
        
        isPause = true;
    }

    public void LevelComplete()
    {
        levelCompleteMenu.SetActive(true);

        isPause = true;
        transitionLevelCompleteMenu.SetTrigger("LevelList");
    }

    public void SelectLevelComplete()
    {
        transitionLevelCompleteMenu.SetTrigger("LevelList");
        transitionLevelCompleteLevelListMenu.SetTrigger("LevelList");
    }

    public void SelectLevel()
    { 
        transitionPauseMenu.SetTrigger("LevelList");
        transitionListLevelMenu.SetTrigger("LevelList");
    }

    public void Continue()
    {
        if (!Grid_Show.isEmpty)
        {
            if (Grid_Show.isSaved)
                LoadLevel();
            else
                UnsavedChanges();
        }
    }

    public void UnsavedChanges()
    {
        isPause = !isPause;
        unsavedChangesBackground.SetActive(isPause);
        transitionUnsavedChanges.SetTrigger("Unsaved");
    }

    public void NewLevelName()
    {
        isPause = !isPause;
        unsavedChangesBackground.SetActive(isPause);
        transitionNewLevelName.SetTrigger("Name");
        newLevelName.text = "";
    }

    public void LoadLevel()
    {
        if (levelLoader.gameObject.activeSelf == false)
            levelLoader.gameObject.SetActive(true);
        isPause = false;
        StartCoroutine(StartPlaying());
    }

    IEnumerator StartPlaying()
    {
        levelLoader.LevelLoader("PlayMode", 1);

        yield return null;
    }

    public void MainMenu()
    {
        Resume();
        levelLoader.LevelLoader("MainMenu", 0);
    }

}
