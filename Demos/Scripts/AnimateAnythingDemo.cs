using System.Collections;
using System.Collections.Generic;
using Milease;
using Milease.Utils;
using UnityEngine;

/// <summary>
/// Simply animate the music volume by one line code using Milease.
/// </summary>
public class AnimateAnythingDemo : MonoBehaviour
{
    public AudioSource AudioSource;

    public void FadeIn()
    {
        AudioSource.Milease(nameof(AudioSource.volume),0f, 1f, 1f).Start();
    }
    
    public void FadeOut()
    {
        AudioSource.Milease(nameof(AudioSource.volume),1f, 0f, 1f, 0f, EaseFunction.Quad, EaseType.Out).Start();
    }
}
