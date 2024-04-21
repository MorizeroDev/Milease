using System;
using System.Collections.Generic;
using System.Diagnostics;
using Milease.Enums;
using Milease.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using Debug = UnityEngine.Debug;

namespace Milease.Core.UI
{
    public class MilListView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
    {
        public int SelectedIndex { get; internal set; } = -1;

        [HideInInspector]
        public readonly List<object> Items = new();

        public GameObject ItemPrefab;
        public bool Vertical = true;
        public float Spacing;
        public float MouseScrollSensitivity = 300f;
        
        private readonly List<MilListViewItem> bindDisplay = new();
        private readonly List<MilListViewItem> display = new();
        private float ItemSize;

        [HideInInspector]
        public float Position;
        
        private RectTransform RectTransform;

        private Vector2 startPos;
        private float orPos;

        private float targetPos;
        private float originPos;
        private float transTime = 0.5f;
        
        private readonly Stopwatch watch = new Stopwatch();
        
        private void Awake()
        {
            ItemSize = ItemPrefab.GetComponent<RectTransform>().sizeDelta.y;
            RectTransform = GetComponent<RectTransform>();
        }

        public void Add(object data)
        {
            Items.Add(data);
            bindDisplay.Add(null);
        }

        public bool Remove(int index)
        {
            if (index < 0 || index >= Items.Count)
            {
                Debug.LogWarning("Index out of range.");
                return false;
            }
            if (SelectedIndex == index)
            {
                Select(-1);
            }
            if (bindDisplay[index])
            {
                bindDisplay[index].Index = -1;
            }
            foreach (var obj in display)
            {
                if (obj.Index > index)
                {
                    obj.Index--;
                }
            }
            Items.RemoveAt(index);
            bindDisplay.RemoveAt(index);
            targetPos = Position;
            CheckPosition();
            return true;
        }
        
        public bool Remove(object data)
        {
            var index = Items.FindIndex(x => x == data);
            if (index == -1)
                return false;
            return Remove(index);
        }

        public void Clear()
        {
            foreach (var obj in display)
            {
                obj.Index = -1;
            }
            Select(-1);
            bindDisplay.Clear();
            Items.Clear();
            targetPos = Position;
            CheckPosition();
        }

        public void Select(int index)
        {
            if (SelectedIndex != -1)
            {
                if (bindDisplay[SelectedIndex])
                {
                    bindDisplay[SelectedIndex].animator.Transition(MilListViewItem.UIState.Default);
                }
            }

            SelectedIndex = index;
            if (SelectedIndex < 0 || SelectedIndex >= Items.Count)
                return;
            if (bindDisplay[SelectedIndex])
            {
                bindDisplay[SelectedIndex].animator.Transition(MilListViewItem.UIState.Selected);
            }
        }

        private void CheckObjectPool(int cnt)
        {
            if (display.Count > cnt)
            {
                for (var i = cnt; i < display.Count; i++)
                {
                    if (display[i].Index < 0 || display[i].Index >= Items.Count)
                        continue;
                    bindDisplay[display[i].Index] = null;
                    Destroy(display[i].gameObject);
                }
                display.RemoveRange(cnt, display.Count - cnt);
            }
            else
            {
                var cnt2 = cnt - display.Count;
                for (var i = 0; i < cnt2; i++)
                {
                    var go = Instantiate(ItemPrefab, transform);
                    display.Add(go.GetComponent<MilListViewItem>());
                    go.SetActive(true);
                }
            }
        }
        
        private void UpdateListView()
        {
            // Transform scroll position
            if (transTime <= 0.5f)
            {
                transTime += Time.deltaTime;
                var pro = Mathf.Min(1f, transTime / 0.5f);
                Position = originPos + (targetPos - originPos) *
                    EaseUtility.GetEasedProgress(pro, EaseType.Out, EaseFunction.Circ);
            }
            
            var start = Mathf.FloorToInt((Position) / (ItemSize + Spacing)) + (Position < 0 ? 1 : 0);
            var cnt = Mathf.CeilToInt(RectTransform.sizeDelta.y / (ItemSize + Spacing) + 2);

            CheckObjectPool(cnt);

            // Check avaliable item object
            var avaliable = new List<MilListViewItem>();
            for (var i = 0; i < display.Count; i++)
            {
                if (display[i].Index != -1 && (display[i].Index < start || display[i].Index >= start + cnt))
                {
                    bindDisplay[display[i].Index] = null;
                    display[i].Index = -1;
                }

                if (display[i].Index == -1)
                {
                    avaliable.Add(display[i]);
                }
            }

            // Distribute avaliable item object
            var j = 0;
            for (var i = start; i < start + cnt; i++)
            {
                if (i >= 0 && i < Items.Count && !bindDisplay[i])
                {
                    if (j >= avaliable.Count)
                    {
                        Debug.LogWarning("Lack of item object.");
                        continue;
                    }
                    avaliable[j].Binding = Items[i];
                    bindDisplay[i] = avaliable[j];
                    avaliable[j].Index = i;
                    avaliable[j].ParentListView = this;
                    avaliable[j].animator.SetState(SelectedIndex == i ? MilListViewItem.UIState.Selected : MilListViewItem.UIState.Default);
                    avaliable[j].UpdateAppearance();
                    if (!avaliable[j].GameObject.activeSelf)
                    {
                        avaliable[j].GameObject.SetActive(true);
                    }
                    j++;
                }
            }

            // Update item object
            var pos = Position % (ItemSize + Spacing);
            for (var i = start; i < start + cnt; i++)
            {
                if (i >= 0 && i < Items.Count && bindDisplay[i])
                {
                    var p = new Vector2(0, pos);
                    if (bindDisplay[i].RectTransform.anchoredPosition != p)
                    {
                        bindDisplay[i].RectTransform.anchoredPosition = p;
                        bindDisplay[i].AdjustAppearance(pos / RectTransform.sizeDelta.y * -1f);
                    }
                }

                pos -= (ItemSize + Spacing);
            }
            
            // Recycle unused item object
            for (var i = j; i < avaliable.Count; i++)
            {
                if (avaliable[i].GameObject.activeSelf)
                {
                    avaliable[i].GameObject.SetActive(false);
                }
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            transTime = 0.5f;
            startPos = eventData.position;
            orPos = Position;
            watch.Restart();
        }

        public void OnDrag(PointerEventData eventData)
        {
            Position = orPos + (eventData.position - startPos).y;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            watch.Stop();
            OnDrag(eventData);
            var time = watch.ElapsedMilliseconds / 1000f;
            var delta = eventData.position - startPos;
            targetPos = Position + (delta.magnitude * Mathf.Sign(delta.y)) / time * 0.3f;
            CheckPosition();
        }

        private void CheckPosition()
        {
            originPos = Position;
            var minPos = 0f;
            var maxPos = Mathf.Max(0f, Items.Count * (ItemSize + Spacing) - RectTransform.sizeDelta.y);
            if (targetPos < minPos || targetPos > maxPos)
            {
                targetPos = Mathf.Clamp(Position, minPos, maxPos);
            }
            transTime = 0f;
        }
        
        private void Update()
        {
            UpdateListView();
        }

        public void OnScroll(PointerEventData eventData)
        {
            Position -= eventData.scrollDelta.y * MouseScrollSensitivity;
            targetPos = Position;
            CheckPosition();
        }
    }
}