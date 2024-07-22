using Milease.Core;
using Milease.Core.Animation;
using UnityEngine;

namespace Milease.Utils
{
    public static class HandleFunction
    {
        public static void Hide(MilHandleFunctionArgs e)
        {
            e.GetTarget<GameObject>().SetActive(e.Progress < 1f);
        }
        public static void Show(MilHandleFunctionArgs e)
        {
            e.GetTarget<GameObject>().SetActive(e.Progress >= 1f);
        }
        public static void DeativeWhenReset(MilHandleFunctionArgs e)
        {
            e.GetTarget<GameObject>().SetActive(false);
        }
        public static void ActiveWhenReset(MilHandleFunctionArgs e)
        {
            e.GetTarget<GameObject>().SetActive(true);
        }

        public static MileaseHandleFunction AutoActiveReset(GameObject go)
        {
            return go.activeSelf ? ActiveWhenReset : DeativeWhenReset;
        }
    }
}