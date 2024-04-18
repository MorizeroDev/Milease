using UnityEngine;

namespace Milease.Core
{
    public static class HandleFunction
    {
        public static void Hide(object o, float t)
        {
            (o as GameObject)!.SetActive(t < 1f);
        }
        public static void Show(object o, float t)
        {
            (o as GameObject)!.SetActive(t >= 1f);
        }
        public static void DeativeWhenReset(object o, float t)
        {
            (o as GameObject)!.SetActive(false);
        }
        public static void ActiveWhenReset(object o, float t)
        {
            (o as GameObject)!.SetActive(true);
        }

        public static MileaseHandleFunction AutoActiveReset(GameObject go)
        {
            return go.activeSelf ? ActiveWhenReset : DeativeWhenReset;
        }
    }
}