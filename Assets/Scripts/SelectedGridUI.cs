using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Utility_Functions;

public class SelectedGridUI : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI myGridNumber;

    public void Awake()
    {

        transform.Find("ArrowLeft").GetComponent<ButtonUI>().ClickFunc = () => {
            Grid_Show.Instance.Decrement();
        };

        transform.Find("ArrowRight").GetComponent<ButtonUI>().ClickFunc = () => {
            Grid_Show.Instance.Increment();
        };

    }

    private void Start()
    {
        Grid_Show.Instance.GridNumberChanged += HouseBuildingSystem_GridNumberChanged;

        RefreshSelectedVisual();
    }


    private void HouseBuildingSystem_GridNumberChanged(object sender, System.EventArgs e)
    {
        RefreshSelectedVisual();
    }

    private void RefreshSelectedVisual()
    {
        myGridNumber.text = Grid_Show.Instance.GetGridSize().ToString();

    }
}
