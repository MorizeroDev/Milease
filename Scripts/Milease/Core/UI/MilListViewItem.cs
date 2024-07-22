using System;
using System.Collections.Generic;
using Milease.Core.Animator;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Milease.Core.UI
{
    public abstract class MilListViewItem : MonoBehaviour, IPointerClickHandler
    {
        public enum UIState
        {
            Default, Selected
        }
        private MilInstantAnimator clickAnimator;
        public readonly MilStateAnimator animator = new();

        public float DefaultTransition = 0.25f;
        public float SelectTransition = 0.5f;
        public UnityEvent<int> OnSelectEvent;

        [HideInInspector]
        public MilListView ParentListView;

        [HideInInspector] 
        public int Index { get; internal set; } = -1;

        private object mBinding;

        public object Binding
        {
            get => mBinding;
            set
            {
                mBinding = value;
                UpdateAppearance();
            }
        }
        
        public RectTransform RectTransform { get; private set; }
        public GameObject GameObject { get; private set; }
        
        public void Initialize()
        {
            RectTransform = GetComponent<RectTransform>();
            var state = ConfigDefaultState();
            state ??= Array.Empty<MilStateParameter>();
            animator.AddState(UIState.Default, DefaultTransition, state);
            state = ConfigSelectedState();
            state ??= Array.Empty<MilStateParameter>();
            animator.AddState(UIState.Selected, SelectTransition, state);
            animator.SetDefaultState(UIState.Default);
            clickAnimator = ConfigClickAnimation();
            GameObject = gameObject;
            OnInitialize();
        }
        
        private void OnDestroy()
        {
            animator.Stop();
            OnTerminate();
        }
        
        protected abstract IEnumerable<MilStateParameter> ConfigDefaultState();
        protected abstract IEnumerable<MilStateParameter> ConfigSelectedState();
        public abstract void OnSelect(PointerEventData eventData);
        protected abstract void OnInitialize();
        protected abstract void OnTerminate();
        protected abstract MilInstantAnimator ConfigClickAnimation();
        public abstract void UpdateAppearance();
        public abstract void AdjustAppearance(float pos);
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.dragging)
            {
                return;
            }
            OnSelectEvent.Invoke(Index);
            ParentListView.Select(Index, false, eventData);
            clickAnimator?.Play();
        }
    }
}