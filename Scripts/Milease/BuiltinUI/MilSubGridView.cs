using System;
using System.Collections;
using System.Collections.Generic;
using Milease.Core.Animator;
using Milease.Core.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Milease.BuiltinUI
{
    [RequireComponent(typeof(MilListView))]
    public class MilSubGridView : MilListViewItem, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
    {
        [Tooltip("The list view attached to this object")]
        public MilListView SubListView;
        [Tooltip("The parent list view, which contains these sub list views")]
        public MilListView MixedListView;
        
        protected override IEnumerable<MilStateParameter> ConfigDefaultState()
            => Array.Empty<MilStateParameter>();

        protected override IEnumerable<MilStateParameter> ConfigSelectedState()
            => Array.Empty<MilStateParameter>();

        public override void OnSelect(PointerEventData eventData)
        {

        }

        protected override void OnInitialize()
        {

        }

        protected override void OnTerminate()
        {

        }

        protected override MilInstantAnimator ConfigClickAnimation()
            => null!;

        public override void UpdateAppearance()
        {
            if (!(Binding is IList items))
            {
                return;
            }
            
            SubListView.Clear();
            
            for (var i = 0; i < items.Count; i++)
            {
                if (i >= SubListView.Items.Count)
                {
                    SubListView.Add(items[i]);
                }
                else if (!Equals(SubListView.Items[i], items[i]))
                {
                    SubListView.UpdateItem(i, items[i]);
                }
            }
        }

        public override void AdjustAppearance(float pos)
        {

        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            MixedListView.OnBeginDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            MixedListView.OnDrag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            MixedListView.OnEndDrag(eventData);
        }

        public void OnScroll(PointerEventData eventData)
        {
            MixedListView.OnScroll(eventData);
        }
    }
}
