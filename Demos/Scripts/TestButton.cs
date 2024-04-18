using System;
using System.Collections;
using System.Collections.Generic;
using Milease;
using Milease.Core;
using Milease.Core.Animator;
using Milease.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TestButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public RectTransform Arrow, Content, Wave;
    private MilStateAnimator animator;
    private MilInstantAnimator waveAnimator;

    private enum State
    {
        Default, Hover
    }

    private void Awake()
    {
        animator = new MilStateAnimator()
            .AddState((int)State.Default, 0.5f, new[]
            {
                Content.MilState(nameof(Content.anchoredPosition), new Vector2(0, 4f), EaseFunction.Back, EaseType.Out),
                Arrow.GetComponent<TMP_Text>().MilState("color", new Color(1f, 1f, 1f, 0f)),
                GetComponent<Image>().MilState("color", ColorUtils.RGBA(94, 11, 255, 0.5f)),
                Arrow.MilState(nameof(Content.anchoredPosition), new Vector2(110, 0), EaseFunction.Back, EaseType.Out)
            })
            .AddState((int)State.Hover, 0.25f, new[]
            {
                Content.MilState(nameof(Content.anchoredPosition), new Vector2(-25, 4f), EaseFunction.Back,
                    EaseType.Out),
                Arrow.GetComponent<TMP_Text>().MilState("color", new Color(1f, 1f, 1f, 1f)),
                GetComponent<Image>().MilState("color", ColorUtils.RGBA(11, 255, 232, 0.5f)),
                Arrow.MilState(nameof(Content.anchoredPosition), new Vector2(50, 0), EaseFunction.Back, EaseType.Out)
            })
            .SetDefaultState((int)State.Default);

        waveAnimator =
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
        animator.Transition((int)State.Hover);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        animator.Transition((int)State.Default);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)transform, eventData.position, eventData.pressEventCamera, out var localCursor);
        Wave.localPosition = localCursor;
        waveAnimator.Play();
    }
}
