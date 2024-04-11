using System.Collections;
using System.Collections.Generic;
using Milease.Core;
using Milease.Utils;
using UnityEngine;

public class Test : MonoBehaviour
{
    void Start()
    {
        transform.MilAnimate(nameof(transform.position),
            MilAnimation.SimplePart(
                new Vector3(0f, 0f, 0f), new Vector3(1f, 1f, 0f),
                1f)
            )
        .Then(transform.MilAnimate(nameof(transform.localScale),
            MilAnimation.SimplePart(
                new Vector3(1f, 1f, 1f), new Vector3(2f, 2f, 2f),
                1f, EaseUtility.EaseType.In, EaseUtility.EaseFunction.Bounce)
            ))
        .Then(transform.MilAnimate(nameof(transform.eulerAngles),
            MilAnimation.SimplePart(
                new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 360f),
                1f, EaseUtility.EaseType.Out, EaseUtility.EaseFunction.Circ)
            ));
    }
}
