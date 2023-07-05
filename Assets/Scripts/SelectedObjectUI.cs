using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility_Functions;

public class SelectedObjectUI : MonoBehaviour
{
    private Dictionary<PlacedObjectType, Transform> placedObjectTypeTransformDic = new Dictionary<PlacedObjectType, Transform>();

    public GroundListAssets AssetList;
    public void Awake()
    {
        placedObjectTypeTransformDic.Add(AssetList.GroundPlatform, transform.Find("GroundPlatform"));
        placedObjectTypeTransformDic.Add(AssetList.EndPlatform, transform.Find("EndPlatform"));

        transform.Find("ArrowBtn").GetComponent<ButtonUI>().ClickFunc = () => {
            Grid_Show.Instance.DeselectObjectType();
        };

        transform.Find("DeleteBtn").GetComponent<ButtonUI>().ClickFunc = () => {
            Grid_Show.Instance.SetDemolishActive();
        };

        foreach (PlacedObjectType placedObjectType in placedObjectTypeTransformDic.Keys)
        {
            placedObjectTypeTransformDic[placedObjectType].GetComponent<ButtonUI>().ClickFunc = () => {
                Grid_Show.Instance.SelectPlacedObjectType(placedObjectType);
            };
        }
    }

    private void Start()
    {
        Grid_Show.Instance.OnSelectedChanged += HouseBuildingSystem_OnSelectedChanged;

        RefreshSelectedVisual();
    }

    private void HouseBuildingSystem_OnSelectedChanged(object sender, System.EventArgs e)
    {
        RefreshSelectedVisual();
    }

    private void RefreshSelectedVisual()
    {
        foreach (PlacedObjectType placedObjectTypeSO in placedObjectTypeTransformDic.Keys)
        {
           placedObjectTypeTransformDic[placedObjectTypeSO].Find("Selected").gameObject.SetActive(false);

            if (Grid_Show.Instance.GetPlaceObjectType() == Grid_Show.PlaceObjectType.GroundObject)
            {
                placedObjectTypeTransformDic[placedObjectTypeSO].Find("Selected").gameObject.SetActive(
                    Grid_Show.Instance.GetPlacedObjectType() == placedObjectTypeSO
                );
            }
        }

        transform.Find("ArrowBtn").Find("Selected").gameObject.SetActive(Grid_Show.Instance.GetPlacedObjectType() == null && Grid_Show.Instance.IsDemolishActive() == false);
        transform.Find("DeleteBtn").Find("Selected").gameObject.SetActive(Grid_Show.Instance.GetPlacedObjectType() == null && Grid_Show.Instance.IsDemolishActive() == true);
    }
}
