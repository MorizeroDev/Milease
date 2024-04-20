using System.Collections.Generic;
using Milease.Core.Animator;
using Milease.Core.UI;
using Milease.Enums;
using Milease.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Demos.Scripts
{
    public class MilButtonDemo : MilAnimatedUI
    {
        public RectTransform Arrow, Content, Wave;
        public Color HoverColor;

        protected override IEnumerable<MilStateParameter> ConfigDefaultState()
            => new[]
            {
                Content.GetComponent<TMP_Text>().MilState("color", Color.white),
                Content.MilState(nameof(Content.anchoredPosition), new Vector2(0, 4f), EaseFunction.Back, EaseType.Out),
                Arrow.GetComponent<TMP_Text>().MilState("color", new Color(0f, 0f, 0f, 0f)),
                GetComponent<Image>().MilState("color", ColorUtils.RGBA(94, 11, 255, 0.5f)),
                Arrow.MilState(nameof(Content.anchoredPosition), new Vector2(110, 0), EaseFunction.Back, EaseType.Out)
            };

        protected override IEnumerable<MilStateParameter> ConfigHoverState()
            => new[]
            {
                Content.GetComponent<TMP_Text>().MilState("color", Color.black),
                Content.MilState(nameof(Content.anchoredPosition), new Vector2(-25, 4f), EaseFunction.Back,
                    EaseType.Out),
                Arrow.GetComponent<TMP_Text>().MilState("color", new Color(0f, 0f, 0f, 1f)),
                GetComponent<Image>().MilState("color", HoverColor),
                Arrow.MilState(nameof(Content.anchoredPosition), new Vector2(50, 0), EaseFunction.Back, EaseType.Out)
            };

        protected override IEnumerable<MilStateParameter> ConfigSelectedState()
            => null;

        protected override void OnClick(PointerEventData eventData)
        {
            Wave.localPosition = transform.GetInsidePos(eventData);
        }

        protected override MilInstantAnimator ConfigClickAnimation()
            =>  Wave.Milease(nameof(Wave.localScale),
                    new Vector3(0f, 0f, 0f), new Vector3(4f, 4f, 4f),
                    0.25f, 0f, EaseFunction.Circ)
                .While(
                    Wave.GetComponent<Image>().Milease("color",
                        new Color(0f, 0f, 0f, 0f), new Color(0f, 0f, 0f, 0.3f),
                        0.2f)
                )
                .While(
                    Wave.GetComponent<Image>().Milease("color",
                        new Color(0f, 0f, 0f, 0.3f),new Color(0f, 0f, 0f, 0f), 
                        0.15f, 0.3f)
                );
    }
}