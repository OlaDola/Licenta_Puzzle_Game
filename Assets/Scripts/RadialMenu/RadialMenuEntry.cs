using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class RadialMenuEntry : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{

    public delegate void RadialMenuEntryDelegate(RadialMenuEntry pEntry);

    //[SerializeField]
    //TextMeshProUGUI label;

    [SerializeField]
    Texture icon;

    RectTransform Rect;
    RadialMenuEntryDelegate Callback;
    private void Start()
    {
        Rect = GetComponent<RectTransform>();
        GetComponentInChildren<RawImage>().texture = icon;
    }

    public virtual void DoFunction(Vector3 position)
    {

    }

    public void SetCallback(RadialMenuEntryDelegate pCallback)
    {
        Callback = pCallback;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Callback?.Invoke(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Rect.DOComplete();
        Rect.DOScale(Vector3.one * 1.5f, .3f).SetEase(Ease.OutQuad);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Rect.DOComplete();
        Rect.DOScale(Vector3.one, .3f).SetEase(Ease.OutQuad);
    }
}
