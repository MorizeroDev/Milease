using System;
using System.Collections;
using System.Collections.Generic;
using Milease.Core.UI;
using UnityEngine;
using Random = UnityEngine.Random;

public class TestData2
{
    public Sprite sprite;
}

public class ListViewTest2 : MonoBehaviour
{
    public MilListView ListView;
    public List<Sprite> Sprites;
    
    private void Awake()
    {
        for (var i = 0; i < 20; i++)
        {
            ListView.Add(new TestData2(){ sprite = Sprites[Random.Range(0, Sprites.Count)]});
        }
    }
}
