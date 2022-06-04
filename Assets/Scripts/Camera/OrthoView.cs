using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrthoView : MonoBehaviour
{
    [SerializeField] Transform frontOrtho;
    [SerializeField] Transform leftOrtho;
    [SerializeField] Transform rightOrtho;
    [SerializeField] Transform backOrtho;

    [SerializeField] static Dictionary<Dir, Transform> OrthoViews = new Dictionary<Dir, Transform>();
    public Grid_Show GridSystem;

    public static Transform GetDir(Dir dir)
    {
        switch (dir)
        {
            default:
            case Dir.Front:
                return OrthoViews[Dir.Front];
            case Dir.Left:
                return OrthoViews[Dir.Left];
            case Dir.Right:
                return OrthoViews[Dir.Right];
            case Dir.Back:
                return OrthoViews[Dir.Back];

        }
    }

    public static Dir GetNextDir(Dir dir, float rotation)
    {
        switch (dir)
        {
            default:
            case Dir.Back:
                if (rotation < 0)
                    return Dir.Left;
                else
                    return Dir.Right;
            case Dir.Left:
                if (rotation < 0)
                    return Dir.Front;
                else
                    return Dir.Back;
            case Dir.Front:
                if (rotation < 0)
                    return Dir.Right;
                else
                    return Dir.Left;
            case Dir.Right:
                if (rotation < 0)
                    return Dir.Back;
                else
                    return Dir.Front;
        }
    }

    public Transform MinOrthoDist(Vector3 target)
    {
        float min = Mathf.Infinity;
        Transform minDistTransform = OrthoViews[0];
        foreach(Dir t in OrthoViews.Keys)
        {
            if(Mathf.Abs(Vector3.Distance(target, OrthoViews[t].position)) < min){
                min = Mathf.Abs(Vector3.Distance(target, OrthoViews[t].position));
                minDistTransform = OrthoViews[t];
            }
        }
        return minDistTransform;
    }

    public Dir MinOrthoDistDir(Vector3 target)
    {
        float min = Mathf.Infinity;
        Transform minDistTransform = OrthoViews[0];
        Dir dir = Dir.Front;
        foreach (Dir t in OrthoViews.Keys)
        {
            if (Mathf.Abs(Vector3.Distance(target, OrthoViews[t].position)) < min)
            {
                min = Mathf.Abs(Vector3.Distance(target, OrthoViews[t].position));
                minDistTransform = OrthoViews[t];
                dir = t;
            }
        }
        return dir;
    }

    public enum Dir
    {
        Front,
        Left,
        Back,
        Right,
    }

    private void Awake()
    {
        frontOrtho = transform.Find("OrthoFront");
        leftOrtho = transform.Find("OrthoLeft");
        rightOrtho = transform.Find("OrthoRight");
        backOrtho = transform.Find("OrthoBack");
    }

    public void Setup(Vector3 LevelMiddle)
    {
        OrthoViews = new Dictionary<Dir, Transform>();

        float gridDistance = GridSystem.gridCellSize * GridSystem.gridSize / 2;

        //Front
        frontOrtho.transform.position = LevelMiddle + new Vector3(0, 3, -(gridDistance+11.25f));
        OrthoViews.Add(Dir.Front,frontOrtho);

        //Left
        leftOrtho.transform.position = LevelMiddle + new Vector3(-(gridDistance + 11.25f), 3, 0);
        leftOrtho.transform.rotation = Quaternion.Euler(0, 90, 0);
        OrthoViews.Add(Dir.Left,leftOrtho);

        //Right
        rightOrtho.transform.position = LevelMiddle + new Vector3(gridDistance + 11.25f, 3, 0);
        rightOrtho.transform.rotation = Quaternion.Euler(0, -90, 0);
        OrthoViews.Add(Dir.Right,rightOrtho);
        //Back
        backOrtho.transform.position = LevelMiddle + new Vector3(0, 3, gridDistance + 11.25f);
        backOrtho.transform.rotation = Quaternion.Euler(0, -180, 0);
        OrthoViews.Add(Dir.Back,backOrtho);
    }
}
