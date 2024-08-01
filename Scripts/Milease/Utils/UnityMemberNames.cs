using UnityEngine;

namespace Milease.Utils
{
    /// <summary>
    /// Short for Unity Member Names
    /// </summary>
    public static class UMN
    {
        public const string Color = nameof(SpriteRenderer.color);
        public const string Size = nameof(SpriteRenderer.size);
        public const string Sprite = nameof(SpriteRenderer.sprite);
        
        public const string LEulerAngles = nameof(Transform.localEulerAngles);
        public const string EulerAngles = nameof(Transform.eulerAngles);
        public const string LScale = nameof(Transform.localScale);
        public const string Scale = nameof(Transform.lossyScale);
        public const string LPosition = nameof(Transform.localPosition);
        public const string Position = nameof(Transform.position);
        public const string LRotation = nameof(Transform.localRotation);
        public const string Rotation = nameof(Transform.rotation);
        
        public const string SizeDelta = nameof(RectTransform.sizeDelta);
        public const string AnchoredPosition = nameof(RectTransform.anchoredPosition);
    }
}
