using System;
using System.Collections.Generic;
using Milease.Core.Animator;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Milease.Core.UI
{
    public abstract class MilAnimatedUI : UIBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, ISelectHandler, IDeselectHandler
    {
        private enum UIState
        {
            Default, Hover, Selected
        }
        private MilInstantAnimator clickAnimator;
        private readonly MilStateAnimator animator = new();
        private UIState lastState = UIState.Default;
        
        public float DefaultTransition = 0.25f;
        public float HoverTransition = 0.5f;
        public UnityEvent OnClickEvent;
        
        protected override void Awake()
        {
            var state = ConfigDefaultState();
            state ??= Array.Empty<MilStateParameter>();
            animator.AddState(UIState.Default, DefaultTransition, state);
            state = ConfigHoverState();
            state ??= Array.Empty<MilStateParameter>();
            animator.AddState(UIState.Hover, HoverTransition, state);
            state = ConfigSelectedState();
            state ??= Array.Empty<MilStateParameter>();
            animator.AddState(UIState.Selected, HoverTransition, state);
            animator.SetDefaultState(UIState.Default);
            clickAnimator = ConfigClickAnimation();
            OnInitialize();
        }

        protected override void OnDestroy()
        {
            animator.Stop();
            OnTerminate();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            lastState = UIState.Hover;
            if (animator.CurrentState == (int)UIState.Selected && !animator.IsStateEmpty(UIState.Selected))
            {
                return;
            }
            animator.Transition(UIState.Hover);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            lastState = UIState.Default;
            if (animator.CurrentState == (int)UIState.Selected && !animator.IsStateEmpty(UIState.Selected))
            {
                return;
            }
            animator.Transition(UIState.Default);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.dragging)
            {
                return;
            }
            clickAnimator?.Play();
            OnClick(eventData);
            OnClickEvent.Invoke();
            if (EventSystem.current)
            {
                EventSystem.current.SetSelectedGameObject(gameObject, eventData);
            }
        }
        
        public void OnSelect(BaseEventData eventData)
        {
            animator.Transition(UIState.Selected);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            animator.Transition(lastState);
        }

        protected abstract IEnumerable<MilStateParameter> ConfigDefaultState();
        protected abstract IEnumerable<MilStateParameter> ConfigHoverState();
        protected abstract IEnumerable<MilStateParameter> ConfigSelectedState();
        protected abstract MilInstantAnimator ConfigClickAnimation();
        protected abstract void OnClick(PointerEventData eventData);
        protected abstract void OnInitialize();
        protected abstract void OnTerminate();
    }
}
