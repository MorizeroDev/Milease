using Milease.Core;
using Milease.Core.Animation;
using UnityEngine;

namespace Milease.Utils
{
    public static class HandleFunction
    {
        public static void Hide(MilHandleFunctionArgs<GameObject, GameObject> e)
        {
            e.Target.SetActive(e.Progress < 1f);
        }
        public static void Show(MilHandleFunctionArgs<GameObject, GameObject> e)
        {
            e.Target.SetActive(e.Progress >= 1f);
        }
        public static void DeativeWhenReset(MilHandleFunctionArgs<GameObject, GameObject> e)
        {
            e.Target.SetActive(false);
        }
        public static void ActiveWhenReset(MilHandleFunctionArgs<GameObject, GameObject> e)
        {
            e.Target.SetActive(true);
        }

        public static MileaseHandleFunction<GameObject, GameObject> AutoActiveReset(GameObject go)
        {
            return go.activeSelf ? ActiveWhenReset : DeativeWhenReset;
        }
    }
}
