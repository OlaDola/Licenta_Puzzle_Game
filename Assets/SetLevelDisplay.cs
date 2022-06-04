using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetLevelDisplay : MonoBehaviour
{
    [SerializeField]
    Grid_Show GridSystem;
    // Start is called before the first frame update
    private void Awake()
    {
        GridSystem = GameObject.Find("Grid").GetComponent<Grid_Show>();
    }
    private void OnMouseOver()
    {
        print("HERE");
        if (Input.GetMouseButtonUp(0))
        {
            print("HERE");
            GameObject.Find("InputLevelName").GetComponent<TMPro.TextMeshProUGUI>().SetText(transform.name);
            GridSystem.Load(transform.name);
        }
    }
}
