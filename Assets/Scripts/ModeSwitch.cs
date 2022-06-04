using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Utility_Functions;


public class ModeSwitch : MonoBehaviour
{
    [SerializeField] Grid_Show Grim_System;
    [SerializeField] Level_Loader levelLoader;
    public event EventHandler OnModeSwitch;


    [SerializeField] private TextMeshProUGUI levelName;
    [SerializeField] private Vector3 SpawnPosition;
    [SerializeField] private GameObject Player;

    public void Awake()
    {
        Grim_System = GameObject.Find("Grid").GetComponent<Grid_Show>();
        levelLoader = GameObject.Find("LevelLoader").GetComponent<Level_Loader>();
        transform.Find("ModeButton").GetComponent<ButtonUI>().ClickFunc = () => {
            if (PlayerPrefs.GetInt("PlayMode") != 1)
            {
                EnterPlayMode(levelName.text);
            }
            else
                levelLoader.LevelLoader("LevelEditor", 0);
        };
    }

    private void Start()
    {
        OnModeSwitch += EnterPlayMode_OnSelectedChanged;

        RefreshSelectedVisual();
    }

    private void RefreshSelectedObjectType()
    {
        OnModeSwitch?.Invoke(this, EventArgs.Empty);
    }

    private void EnterPlayMode_OnSelectedChanged(object sender, System.EventArgs e)
    {
        RefreshSelectedVisual();
    }

    void RefreshSelectedVisual()
    {

    }
    public void EnterPlayMode(string levelName)
    {
        StartCoroutine(StartPlaying(levelName));
        //PlayerPrefs.SetInt("PlayMode", 1);
        //transform.parent.gameObject.SetActive(false);
        //GameObject.Find("PositionLocation").SetActive(false);

        //float gridSizeCell = GridSystem.gridCellSize;
        //offset = new Vector3(gridSizeCell / 2, 2f, gridSizeCell / 2);
        //SpawnPosition = GridSystem.grid.GetWorldPosition(0, 0) + offset;
        //Instantiate(Player, SpawnPosition, Quaternion.identity);
    }

    IEnumerator StartPlaying(string levelName)
    {
        if(levelName != null)
        {
            Grim_System.Save(levelName);

            levelLoader.LevelLoader("PlayMode", 1);

        }
        yield return null;
    }

}
