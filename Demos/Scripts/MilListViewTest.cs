using System;
using System.Collections;
using System.Collections.Generic;
using Milease.Core.UI;
using UnityEngine;

public class MilListViewTest : MonoBehaviour
{
    public MilListView ListView;

    private void Awake()
    {
        for (var i = 0; i <= 100; i++)
        {
            ListView.Add(new TestData(){ Name = "Item " + i });
        }
    }
}
