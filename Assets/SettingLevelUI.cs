using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingLevelUI : MonoBehaviour
{
    public void SettingUI( string levelName, Sprite image=null)
    {   
        if(image)
            transform.Find("Image").GetComponent<Image>().sprite = image;
        print(levelName);
        transform.Find("LevelName").GetComponent<TMPro.TextMeshPro>().SetText(levelName);
        transform.gameObject.name = levelName;
    }
}
