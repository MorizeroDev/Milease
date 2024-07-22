using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Milease.Core.UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public sealed class MilContentSizeFitter : UIBehaviour, ILayoutSelfController
    {
        [Serializable]
        public struct FitSetting
        {
            public bool Enabled;
            public float SplitBy;
        }
        private RectTransform mRect;
        private RectTransform rectTransform
        {
            get
            {
                if (mRect == null)
                    mRect = GetComponent<RectTransform>();
                return mRect;
            }
        }
        private DrivenRectTransformTracker mTracker;

        public FitSetting horizontalFit, verticalFit;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            SetDirty();
        }

        protected override void OnDisable()
        {
            mTracker.Clear();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            base.OnDisable();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            SetDirty();
        }

        private void HandleSelfFittingAlongAxis(int axis)
        {
            var fitting = (axis == 0 ? horizontalFit : verticalFit);
            if (!fitting.Enabled)
            {
                mTracker.Add(this, rectTransform, DrivenTransformProperties.None);
                return;
            }

            mTracker.Add(this, rectTransform, (axis == 0 ? DrivenTransformProperties.SizeDeltaX : DrivenTransformProperties.SizeDeltaY));

            if (fitting.SplitBy == 0)
            {
                rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, LayoutUtility.GetPreferredSize(mRect, axis));
            }
            else
            {
                var axisSize = LayoutUtility.GetPreferredSize(mRect, axis);
                rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, Mathf.Min(axisSize, fitting.SplitBy));
            }
        }
        
        public void SetLayoutHorizontal()
        {
            mTracker.Clear();
            HandleSelfFittingAlongAxis(0);
        }
        
        public void SetLayoutVertical()
        {
            HandleSelfFittingAlongAxis(1);
        }

        private void SetDirty()
        {
            if (!IsActive())
                return;

            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

    #if UNITY_EDITOR
        protected override void OnValidate()
        {
            SetDirty();
        }

    #endif
    }
}