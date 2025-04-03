#if TMP_SETUP
using System.Collections.Generic;
using Milease.Core.Animation;
using Milease.Core.Animator;
using Milease.Core.UI;
using Milease.DSL;
using Milease.Enums;
using Milease.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Milease.BuiltinUI
{
    public class MilButton : MilAnimatedUI
    {
        [Header("Default Style")] 
        public Color DefaultBgColor;
        public Color DefaultTextColor;

        [Header("Hover Style")] 
        public Color HoverBgColor;
        public Color HoverTextColor;

        [Header("Bindings")] 
        public RectTransform Shadow;
        public RectTransform Content, Ripple;

        [Header("Events")] 
        public UnityEvent OnButtonClicked;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            Content.GetComponent<TMP_Text>().color = DefaultTextColor;
            GetComponent<Image>().color = DefaultBgColor;
        }
#endif

        protected override IEnumerable<MilStateParameter> ConfigDefaultState()
            => new[]
            {
                Content.GetComponent<TMP_Text>()
                    .MilState(x => x.color, DefaultTextColor),
                Content.GetComponent<TMP_Text>()
                    .MilState(x => x.characterSpacing, 0, 
                        EaseFunction.Back, EaseType.Out),
                Shadow.GetComponent<Image>()
                    .MilState(x => x.color, HoverBgColor.Clear(),
                        EaseFunction.Quad, EaseType.Out),
                GetComponent<Image>()
                    .MilState(x => x.color, DefaultBgColor)
            };

        protected override IEnumerable<MilStateParameter> ConfigHoverState()
            => new[]
            {
                Content.GetComponent<TMP_Text>()
                    .MilState(x => x.color, HoverTextColor),
                Content.GetComponent<TMP_Text>()
                    .MilState(x => x.characterSpacing, 8f, 
                        EaseFunction.Back, EaseType.Out),
                Shadow.GetComponent<Image>()
                    .MilState(x => x.color, HoverBgColor.Opacity(0.5f),
                        EaseFunction.Quad, EaseType.Out),
                GetComponent<Image>()
                    .MilState(x => x.color, HoverBgColor)
            };

        protected override IEnumerable<MilStateParameter> ConfigSelectedState()
            => null;

        protected override void OnClick(PointerEventData eventData)
        {
            Ripple.localPosition = transform.GetInsidePos(eventData);
            OnButtonClicked?.Invoke();
        }

        protected override MilInstantAnimator ConfigClickAnimation()
        {
            var waveImage = Ripple.GetComponent<Image>();
            return 
                (0.25f / Ripple.MCirc(x => x.localScale, 
                    new Vector3(0f, 0f, 0f), new Vector3(4f, 4f, 4f)))
                .And(
                    0.2f / waveImage.MQuad(x => x.color,
                        HoverTextColor.Clear(), HoverTextColor.Opacity(0.4f))
                )
                .And(
                    0.3f + 0.15f / waveImage.MQuad(x => x.color,
                        HoverTextColor.Opacity(0.4f),HoverTextColor.Clear())
                );
        }
        
        protected override void OnInitialize()
        {
        
        }

        protected override void OnTerminate()
        {

        }
    }
}
#endif
