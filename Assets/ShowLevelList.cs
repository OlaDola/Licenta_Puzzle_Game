using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility_Functions;

public class ShowLevelList : MonoBehaviour
{
    public event EventHandler OnShowList;
    [SerializeField] Animator transition;

    public void Awake()
    {
        transform.Find("ShowListButton").GetComponent<ButtonUI>().ClickFunc = () => {
            transition.SetTrigger("LevelList");
        };
    }
    // Start is called before the first frame update
    void Start()
    {
        OnShowList += ShowList_OnSelectedChanged;
        RefreshSelectedVisual();
    }

    private void ShowList_OnSelectedChanged(object sender, System.EventArgs e)
    {
        RefreshSelectedVisual();
    }
    void RefreshSelectedVisual()
    {

    }
}
