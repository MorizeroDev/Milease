using System.Collections;
using System.Collections.Generic;
using Milease;
using Milease.Core;
using Milease.Core.Animator;
using Milease.Enums;
using Milease.Utils;
using TMPro;
using UnityEngine;

public class Test : MonoBehaviour
{
    public TMP_Text Text;
    private MilInstantAnimator Animation;
    
    public void PauseAnimation()
    {
        Animation.Pause();
    }

    public void ResetAnimation()
    {
        Animation.Reset();
    }

    public void PlayAnimation()
    {
        Animation.Play();
    }
    
    void Start()
    {
        Animation =
        transform.Milease(nameof(transform.position),
                new Vector3(0f, 0f, 0f), new Vector3(1f, 1f, 0f),
                1f)
            .While(
                GetComponent<SpriteRenderer>().Milease("color",
                    new Color(1f, 1f, 1f, 1f), new Color(1f, 0f, 0f, 1f),
                    1f, 0.5f)
            )
            .Then(
                transform.Milease(nameof(transform.localScale),
                    new Vector3(1f, 1f, 1f), new Vector3(2f, 2f, 2f),
                    1f, 0f, EaseFunction.Bounce)
            ).Delayed(1f)
            .Then(
                Text.Milease(nameof(Text.text), "Start!", "Finish!", 1f)
            )
            .Then(
                Text.Milease((o, p) =>
                {
                    var text = o as TMP_Text;
                    text.text = $"Hide after {((1f - p) * 2f):F1}s...";
                }, null,2f, 0f, EaseFunction.Linear)
            )
            .Then(
                Text.gameObject.Milease(HandleFunction.Hide, HandleFunction.AutoActiveReset(Text.gameObject), 0f)
            )
            .Then(
                transform.MileaseTo(nameof(transform.position), new Vector3(0f, 0f, 0f), 1f)
            );
    }
}
