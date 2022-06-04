using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utility_Functions;

public class Level_Loader : MonoBehaviour
{
    [SerializeField] Animator transition;
    [SerializeField] int transitionTime;


    public void LevelLoader(string SceneName, int playMode)
    {
        StartCoroutine(LoadLevel(SceneName, playMode));
    }

    IEnumerator LoadLevel(string SceneName, int playMode)
    {
        transition.SetTrigger("Start");

        yield return new WaitForSeconds(transitionTime);

        PlayerPrefs.SetInt("PlayMode", playMode);
        SceneManager.LoadScene(SceneName);
    }
}
