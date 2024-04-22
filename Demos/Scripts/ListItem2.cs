using System.Collections;
using System.Collections.Generic;
using Milease.Core.Animator;
using Milease.Core.UI;
using Milease.Enums;
using Milease.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ListItem2 : MilListViewItem
{
    public Image Back, Cover;

    protected override IEnumerable<MilStateParameter> ConfigDefaultState()
        => new[]
        {
            GetComponent<Image>().MilState("color", Color.white),
            transform.MilState(nameof(transform.eulerAngles), Vector3.zero),
            Back.MilState("color", Color.clear),
            transform.MilState(nameof(transform.localScale), Vector3.one * 0.8f, EaseFunction.Back, EaseType.Out)
        };

    protected override IEnumerable<MilStateParameter> ConfigSelectedState()
        => new[]
        {
            GetComponent<Image>().MilState("color", Color.clear),
            transform.MilState(nameof(transform.eulerAngles), new Vector3(0f, -180f, 0f)),
            Back.MilState("color", Color.white),
            transform.MilState(nameof(transform.localScale), Vector3.one * 1f, EaseFunction.Back, EaseType.Out)
        };

    protected override void OnSelect(PointerEventData eventData)
    {
        
    }

    protected override MilInstantAnimator ConfigClickAnimation()
        => null;

    public override void UpdateAppearance()
    {
        var data = Binding as TestData2;
        Back.sprite = data.sprite;
    }

    public override void AdjustAppearance(float pos)
    {
        var a = 1f - Mathf.Abs(pos - 0.5f) / 0.5f;
        var color = new Color(1f, 1f, 1f, a);
        if (animator.CurrentState == (int)UIState.Selected)
            Back.color = color;
        if (animator.CurrentState == (int)UIState.Default)
            Cover.color = color;
        animator.ModifyState(UIState.Selected, Back, "color", color);
        animator.ModifyState(UIState.Default, Cover, "color", color);
    }
}
