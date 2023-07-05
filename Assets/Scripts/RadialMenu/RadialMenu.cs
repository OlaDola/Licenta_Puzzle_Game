using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[System.Serializable]
public class serializableClass
{
    public List<RadialMenuEntry> radialMenuEntrys;
}


public class RadialMenu : MonoBehaviour
{
    [SerializeField]
    GameObject EntryPrefab;

    [SerializeField]
    float Radius;

    [SerializeField]
    List<serializableClass> entries;

    [SerializeField]
    List<serializableClass> openedEntries;
    List<GameObject> entryLists;
    void Start()
    {
        openedEntries = new List<serializableClass>();
        entryLists = new List<GameObject>();
        
    }

    private void Update()
    {
        transform.rotation = Camera.main.transform.rotation;
        if (Input.GetMouseButtonDown(1))
        {
            if(openedEntries.Count > 0)
                Close();
            else
                Open();
            transform.position = Mouse_3D.GetMouseWorldPosition();
            
        }
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray;
            RaycastHit hit;
            if (openedEntries.Count != 0)
            {
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.tag == "ButtonEditing")
                        CloseNormal();
                }
            }
            
        }
    }

    void SetEntry(RadialMenuEntry Entry, RadialMenuEntry.RadialMenuEntryDelegate pCallback, Transform parent, int ListIndex)
    {
        RadialMenuEntry entry = Instantiate(Entry, parent);
        entry.SetCallback(pCallback);
        if (openedEntries.Count == ListIndex)
        {
            serializableClass MenuEntryes = new serializableClass();
            MenuEntryes.radialMenuEntrys = new List<RadialMenuEntry>();
            MenuEntryes.radialMenuEntrys.Add(entry);
            openedEntries.Add(MenuEntryes);
        }
        else
        {
            openedEntries[ListIndex].radialMenuEntrys.Add(entry);
        }
    }

    public void Open()
    {
        GameObject entryList = Instantiate(new GameObject(), transform);
        entryList.name = "salut";
        entryLists.Add(entryList);
        for (int i = 0; i < entries.Count; i++)
        {
            for (int j = 0; j < entries[i].radialMenuEntrys.Count; j++)
            {
                SetEntry(entries[i].radialMenuEntrys[j], DoButtonFunction, entryList.transform, i);
            }
        }
        Rearrage();
    }

    public void CloseNormal()
    {
        for (int i = 0; i < openedEntries.Count; i++)
        {
            for (int j = 0; j < openedEntries[i].radialMenuEntrys.Count; j++)
            {
                RectTransform rect = openedEntries[i].radialMenuEntrys[j].GetComponent<RectTransform>();
                GameObject entry = openedEntries[i].radialMenuEntrys[j].gameObject;
                rect.DOScale(Vector3.zero, .3f).SetEase(Ease.OutQuad).SetDelay(.05f * j);
                rect.DOAnchorPos(Vector3.zero, .3f).SetEase(Ease.OutQuad).onComplete =
                    delegate ()
                    {
                        Destroy(entry);
                        print("delegate");
                        GameObject entryL = entryLists[0];
                        if (entryL.transform.childCount == 0)
                        {
                            Destroy(entryL);
                            entryLists.RemoveAt(0);
                        }
                    };
            }
 
        }
        openedEntries.Clear();

    }
    
    public void Close()
    {
        for (int i = 0; i < openedEntries.Count; i++)
        {
            print(i);
            for (int j = 0; j < openedEntries[i].radialMenuEntrys.Count; j++)
            {
                GameObject entry = openedEntries[i].radialMenuEntrys[j].gameObject;
                Destroy(entry);
            }
        }
        GameObject entryL = entryLists[0];
        Destroy(entryL);
        entryLists.RemoveAt(0);
        print("Close");
        openedEntries.Clear();

    }

    //public void Toggle()
    //{
    //    if(openedEntries.Count == 0)
    //    {
    //        Open();
    //    }
    //    else
    //    {
    //        Close();
    //    }
    //}

    void Rearrage()
    {
        
        for (int i = 0; i < openedEntries.Count; i++)
        {
            float buttonStart = i*(2*Mathf.PI)/ openedEntries.Count;
            
            for (int j = 0; j < openedEntries[i].radialMenuEntrys.Count; j++)
            {
                float radianToStart = openedEntries[i].radialMenuEntrys.Count > 1 ? (Mathf.PI) / (openedEntries.Count*2) : 0;
                float radianOfSeparation = openedEntries[i].radialMenuEntrys.Count > 1 ? 2 * radianToStart / (openedEntries[i].radialMenuEntrys.Count - 1) : 0;
                float x = Mathf.Sin(buttonStart - radianToStart + radianOfSeparation * j) * Radius;
                float y = Mathf.Cos(buttonStart - radianToStart + radianOfSeparation * j) * Radius;

                RectTransform rect = openedEntries[i].radialMenuEntrys[j].GetComponent<RectTransform>();

                rect.localScale = Vector3.zero;
                rect.DOScale(Vector3.one, .3f).SetEase(Ease.OutQuad).SetDelay(.05f * i);
                openedEntries[i].radialMenuEntrys[j].GetComponent<RectTransform>().DOAnchorPos(new Vector3(x, y, 0), .3f).SetEase(Ease.OutQuad).SetDelay(.05f * i);
            }
        }

    }


    void DoButtonFunction(RadialMenuEntry pEntry)
    {
        pEntry.DoFunction(transform.position);
        CloseNormal();
    }

}
