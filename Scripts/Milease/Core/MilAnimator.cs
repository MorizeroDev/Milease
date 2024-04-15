using System.Collections;
using System.Collections.Generic;
using Milease;
using Milease.Core;
using Milease.Utils;
using UnityEditor;
using UnityEngine;

//[ExecuteInEditMode]
public class MilAnimator : MonoBehaviour
{
    public Object TestObject;
    private MilAnimation.AnimationPart ani;
    private RuntimeAnimationPart test;
    private float time = 0f;
    public float testValue;
    void Start()
    {
        ani = MilAnimation.Part("transform.position",
            new Vector3(0, 0, 0), new Vector3(0, -2.5f, 0),
            0f, 1f, EaseFunction.Back);
        ani = MilAnimation.Part("testValue",
            0f, 100f,
            0f, 1f, EaseFunction.Back);
        test = new RuntimeAnimationPart(TestObject, ani, typeof(MilAnimator));
    }
    
    void Update()
    {
        time += Time.deltaTime;
        if (time >= 1f)
        {
            time -= 1f;
        }
        RuntimeAnimationPart.SetValue(test, EaseUtility.GetEasedProgress(time, ani.EaseType, ani.EaseFunction));
    }
}
