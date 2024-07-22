using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Milease.Utils
{
    public static class UIUtils
    {
        public static Vector2 GetInsidePos(this RectTransform transform, PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform, eventData.position, eventData.pressEventCamera, out var localCursor);
            return localCursor;
        }
        public static Vector2 GetInsidePos(this Transform transform, PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform, eventData.position, eventData.pressEventCamera, out var localCursor);
            return localCursor;
        }
    }
}