using System.Collections;
using System.Collections.Generic;
using Milease.Core;
using Milease.Utils;
using UnityEditor;
using UnityEngine;

//[ExecuteInEditMode]
public class MilAnimator : MonoBehaviour
{
    public GameObject TestObject;
    private MilAnimation.RuntimeAnimationPart test;
    private float time = 0f;
    void Start()
    {
        var ani = new MilAnimation.AnimationPart()
        {
            StartValue = JsonUtility.ToJson(new Vector3(0, 0, 0)),
            ToValue = JsonUtility.ToJson(new Vector3(1, 1, 0)),
            EaseType = EaseUtility.EaseType.IO,
            EaseFunction = EaseUtility.EaseFunction.Circ,
            Binding = new List<string>() { "transform", "position" }
        };
        test = new MilAnimation.RuntimeAnimationPart(TestObject, ani, typeof(GameObject));
    }
    
    void Update()
    {
        time += Time.deltaTime;
        if (time >= 1f)
        {
            time -= 1f;
        }
        MilAnimation.RuntimeAnimationPart.SetValue(test, time);
    }
}
