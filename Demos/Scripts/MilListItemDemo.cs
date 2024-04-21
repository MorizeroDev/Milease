using System.Collections;
using System.Collections.Generic;
using Milease.Core.Animator;
using Milease.Core.UI;
using Milease.Enums;
using Milease.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TestData
{
    public string Name;
}

public class MilListItemDemo : MilListViewItem
{
    public TMP_Text Content, Arrow;
    public Image Background;

    protected override IEnumerable<MilStateParameter> ConfigDefaultState()
        => new[]
        {
            Background.MilState("color", Color.white),
            Content.rectTransform.MilState(nameof(Content.rectTransform.anchoredPosition), new Vector2(138f, 2.3f),
                EaseFunction.Back, EaseType.Out),
            Arrow.MilState("color", Color.clear),
            Arrow.rectTransform.MilState(nameof(Arrow.rectTransform.anchoredPosition), new Vector2(88f, 2.3f),
                EaseFunction.Back, EaseType.Out),
            Content.MilState("color", Color.black)
        };

    protected override IEnumerable<MilStateParameter> ConfigSelectedState()
        => new[]
        {
            Background.MilState("color", ColorUtils.RGB(0,153,255)),
            Content.rectTransform.MilState(nameof(Content.rectTransform.anchoredPosition), new Vector2(186f, 2.3f),
                EaseFunction.Back, EaseType.Out),
            Arrow.MilState("color", Color.white),
            Arrow.rectTransform.MilState(nameof(Arrow.rectTransform.anchoredPosition), new Vector2(138f, 2.3f),
                EaseFunction.Back, EaseType.Out),
            Content.MilState("color", Color.white)
        };

    protected override void OnSelect(PointerEventData eventData)
    {
        animator.ModifyState(UIState.Selected, Background, "color",
            ColorUtils.RGB(Random.Range(0, 128), Random.Range(0, 128), Random.Range(0, 128)));
    }

    protected override MilInstantAnimator ConfigClickAnimation()
        => null;

    public override void UpdateAppearance()
    {
        var data = Binding as TestData;
        Content.text = data.Name;
    }

    public override void AdjustAppearance(float pos)
    {

    }
}
