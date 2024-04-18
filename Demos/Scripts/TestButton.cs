using System;
using System.Collections;
using System.Collections.Generic;
using Milease;
using Milease.Core;
using Milease.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TestButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public RectTransform Arrow, Content, Wave;
    private MilInstantAnimator PointerEnter, PointerExit, PointerClick;

    private void Awake()
    {
        PointerEnter =
            Content.MileaseAdditive(nameof(Content.anchoredPosition),
                Vector2.zero, new Vector2(-25, 0),
                0.5f, 0f, EaseFunction.Back, EaseType.Out)
            .While(
                Arrow.GetComponent<TMP_Text>().Milease("color",
                    new Color(1f, 1f, 1f, 0f), new Color(1f, 1f, 1f, 1f),
                    0.5f)
            )
            .While(
                GetComponent<Image>().MileaseAdditive("color",
                    Color.clear, new Color(0f, 0f, 0f, 0.5f),
                    0.5f)
            )
            .While(
                Arrow.MileaseAdditive(nameof(Content.anchoredPosition),
                    new Vector2(60, 0), Vector2.zero,
                    0.5f, 0f, EaseFunction.Back, EaseType.Out)
            );
        PointerClick =
            Wave.Milease(nameof(Wave.localScale),
                    new Vector3(0f, 0f, 0f), new Vector3(4f, 4f, 4f),
                    0.25f, 0f, EaseFunction.Circ)
                .While(
                    Wave.GetComponent<Image>().Milease("color",
                        new Color(1f, 1f, 1f, 0f), new Color(1f, 1f, 1f, 0.3f),
                        0.2f)
                )
                .While(
                    Wave.GetComponent<Image>().Milease("color",
                        new Color(1f, 1f, 1f, 0.3f),new Color(1f, 1f, 1f, 0f), 
                        0.15f, 0.3f)
                );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PointerEnter.Start();
    }

    public void OnPointerExit(PointerEventData eventData)
    {

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)transform, eventData.position, eventData.pressEventCamera, out var localCursor);
        Wave.localPosition = localCursor;
        PointerClick.Start();
    }
}
