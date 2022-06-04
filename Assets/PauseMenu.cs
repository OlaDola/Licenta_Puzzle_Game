using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public static bool isPause;

    [SerializeField] GameObject unsavedChangesBackground;
    [SerializeField] GameObject pauseMenu;
    [SerializeField] Level_Loader levelLoader;
    [SerializeField] Animator transitionUnsavedChanges;
    [SerializeField] Animator transitionPauseMenu;

    private void Awake()
    {
        if(!levelLoader.gameObject.activeSelf)
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

    public void SelectLevel()
    { 
        transitionPauseMenu.SetTrigger("LevelList");
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
