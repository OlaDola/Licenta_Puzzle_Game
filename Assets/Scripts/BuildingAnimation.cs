using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingAnimation : MonoBehaviour
{
    [SerializeField] private AnimationCurve animationCurve = null;

    private float time;

    void Update()
    {
        time += Time.deltaTime;

        transform.localScale = new Vector3(1, animationCurve.Evaluate(time), 1);
    }
}
