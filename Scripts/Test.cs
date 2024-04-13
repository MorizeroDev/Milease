using System.Collections;
using System.Collections.Generic;
using Milease.Core;
using Milease.Utils;
using TMPro;
using UnityEngine;

public class Test : MonoBehaviour
{
    public TMP_Text Text;
    void Start()
    {
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
                    1f, 0f, EaseUtility.EaseType.In, EaseUtility.EaseFunction.Bounce)
            ).Delayed(1f)
            .Then(
                Text.Milease(nameof(Text.text), "Start!", "Finish!", 1f)
            )
            .Then(
                Text.Milease(nameof(Text.text), "Hide After 2s...")
            )
            .Then(
                Text.Milease(nameof(Text.text), "Hide After 2s...")
            ).Delayed(2f);
    }
}
