using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Milease.Enums;
using Milease.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Debug = UnityEngine.Debug;

namespace Milease.Core.UI
{
    public class MilListView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
    {
        public enum AlignMode
        {
            Normal, Center
        }
        
        public int SelectedIndex { get; internal set; } = -1;

        [HideInInspector]
        public readonly List<object> Items = new();

        public bool Interactable = true;
        public bool Scrollable = true;
        
        public GameObject ItemPrefab;
        public bool Vertical = true;
        public float Spacing;
        public float MouseScrollSensitivity = 300f;
        public float StartPadding, EndPadding, Indentation;
        public AlignMode Align = AlignMode.Normal;

        public bool LoopList = false;

        public UnityEvent OnScrollDone;
        
        private readonly List<MilListViewItem> bindDisplay = new();
        private readonly List<MilListViewItem> display = new();
        private MilListViewItem tempDisplay;
        private float ItemSize;
        private Vector2 ItemPivot;
        private Vector2 ItemAnchorMin, ItemAnchorMax;

        [HideInInspector]
        public float Position;
        
        private RectTransform RectTransform;

        private Vector2 startPos;
        private float orPos;

        private float targetPos;
        private float originPos;
        private const float transDuration = 0.5f;
        private float transTime = transDuration;
        
        private readonly Stopwatch watch = new Stopwatch();

        private float AnchorOffset;
        
        private bool initialized = false;

        private class ItemTracker
        {
            public object ItemData;
            public string UUID = Guid.NewGuid().ToString();
        }
        
        private readonly Dictionary<ItemTracker, int> itemTracker = new();

        private void Awake()
        {
            if (initialized)
            {
                return;
            }

            if (!ItemPrefab.TryGetComponent<MilListViewItem>(out _))
            {
                throw new Exception($"Item prefab '{ItemPrefab.name}' doesn't have a MilListViewItem component.");
            }
            
            var itemRect = ItemPrefab.GetComponent<RectTransform>();
            if (Vertical)
            {
                itemRect.pivot = new Vector2(itemRect.pivot.x, 1f);
                if (Align == AlignMode.Center)
                {
                    Debug.LogWarning($"Vertical mode hasn't supported center align mode yet.");
                }
            }
            else
            {
                switch (Align)
                {
                    case AlignMode.Normal:
                        itemRect.pivot = new Vector2(0f, itemRect.pivot.y);
                        break;
                    case AlignMode.Center:
                        itemRect.pivot = new Vector2(0.5f, itemRect.pivot.y);
                        break;
                }
            }
            
            ItemSize = Vertical ? itemRect.rect.height : itemRect.rect.width;
            RectTransform = GetComponent<RectTransform>();
            ItemPivot = itemRect.pivot;
            ItemAnchorMin = itemRect.anchorMin;
            ItemAnchorMax = itemRect.anchorMax;

            AnchorOffset = (Vertical ? RectTransform.rect.height : RectTransform.rect.width) *
                           (Vertical ? ItemAnchorMin.y : ItemAnchorMin.x) *
                           (Vertical ? -1f : 1f) +
                           (Vertical ? RectTransform.rect.height : 0f);
            
            Position = GetOriginPointPosition();
            targetPos = Position;
            var go = Instantiate(ItemPrefab, transform);
            tempDisplay = go.GetComponent<MilListViewItem>();
            go.SetActive(false);
            
            var size = Vertical ? RectTransform.rect.height : RectTransform.rect.width;
            var cnt = Mathf.CeilToInt(size / (ItemSize + Spacing) + 2);
            CheckObjectPool(cnt);
            
            initialized = true;
        }

        public void Add(object data)
        {
            if (!initialized)
            {
                Awake();
            }
            Items.Add(data);
            bindDisplay.Add(null);
        }

        public bool Remove(int index)
        {
            if (!initialized)
            {
                Awake();
            }
            if (index < 0 || index >= Items.Count)
            {
                Debug.LogWarning("Index out of range.");
                return false;
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
            
            foreach (var tracker in itemTracker.Keys.Where(x => x.ItemData == Items[index]))
            {
                if (SelectedIndex != -1)
                {
                    Select(-1);
                }
                itemTracker.Remove(tracker);
            }
            
            foreach (var pair in itemTracker)
            {
                if (pair.Value > index)
                {
                    itemTracker[pair.Key]--;
                }
            }
            Items.RemoveAt(index);
            bindDisplay.RemoveAt(index);
            CheckLoopListPosition();
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
            itemTracker.Clear();
            if (!initialized)
            {
                Awake();
            }
            foreach (var obj in display)
            {
                obj.Index = -1;
            }
            Select(-1);
            bindDisplay.Clear();
            Items.Clear();
            CheckLoopListPosition();
            targetPos = Position;
            CheckPosition();
        }

        public void Select(int index, bool dontCall = false)
        {
            Select(index, dontCall, null);
        }
        
        internal void Select(int index, bool dontCall, PointerEventData args)
        {
            if (!initialized)
            {
                Awake();
            }

            if (!Interactable)
            {
                return;
            }
            if (SelectedIndex != -1)
            {
                if (bindDisplay[SelectedIndex])
                {
                    bindDisplay[SelectedIndex].animator.Transition(MilListViewItem.UIState.Default);
                }
            }
            
            if (index < 0 || index >= Items.Count)
            {
                SelectedIndex = index;
                return;
            }

            var tracker = new ItemTracker()
            {
                ItemData = Items[index]
            };
            itemTracker.Add(tracker, index);
            
            if (!bindDisplay[index] && !dontCall)
            {
                tempDisplay.Binding = Items[index];
                tempDisplay.Index = index;
                tempDisplay.ParentListView = this;
                tempDisplay.OnSelect(args);
            }
            else if (bindDisplay[index])
            {
                bindDisplay[index].animator.Transition(MilListViewItem.UIState.Selected);
                if (!dontCall)
                {
                    bindDisplay[index].OnSelect(args);
                }
            }

            if (itemTracker.ContainsKey(tracker))
            {
                SelectedIndex = itemTracker[tracker];
                itemTracker.Remove(tracker);
            }
        }

        public void SlideTo(float position, bool withoutTransition = false)
        {
            if (!initialized)
            {
                Awake();
            }
            if (LoopList)
            {
                // ensure that the slide progress is smooth in loop list
                CheckLoopListPosition();
                var len = (ItemSize + Spacing) * Items.Count;
                if (Vertical)
                {
                    while (position > len)
                    {
                        position -= len;
                    }
                }
                else
                {
                    while (position < len * -1f)
                    {
                        position += len;
                    }
                }
                var minLength = Mathf.Abs(position - Position);
                var tmp = position;
                bool isInBoundary()
                {
                    if (Vertical)
                    {
                        return tmp < len * 3f;
                    }
                    else
                    {
                        return tmp > len * -3f;
                    }
                }
                while (isInBoundary())
                {
                    tmp += len * (Vertical ? 1f : -1f);
                    var dis = Mathf.Abs(tmp - Position);
                    if (dis < minLength)
                    {
                        minLength = dis;
                        position = tmp;
                    }
                }
            }

            originPos = Position;
            targetPos = position;
            transTime = 0f;
            if (withoutTransition)
            {
                Position = targetPos;
                transTime = transDuration;
            }
        }

        public float GetItemPosition(int index)
        {
            if (!initialized)
            {
                Awake();
            }
            return index * (ItemSize + Spacing) + StartPadding;
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
                    var item = go.GetComponent<MilListViewItem>();
                    item.Initialize();
                    display.Add(item);
                    go.SetActive(false);
                }
            }
        }
        
        private void UpdateListView()
        {
            // Transform scroll position
            if (transTime <= transDuration)
            {
                transTime += Time.deltaTime;
                var pro = Mathf.Min(1f, transTime / transDuration);
                Position = originPos + (targetPos - originPos) *
                    EaseUtility.GetEasedProgress(pro, EaseType.Out, EaseFunction.Circ);
                if (transTime > transDuration && LoopList)
                {
                    CheckLoopListPosition();
                }
            }

            var size = Vertical ? RectTransform.rect.height : RectTransform.rect.width;

            var calPos = Vertical ? (Position - AnchorOffset) : (-Position + AnchorOffset);
            var start = Mathf.FloorToInt(calPos / (ItemSize + Spacing)) + (calPos < 0 ? 1 : 0);
            var cnt = Mathf.CeilToInt(size / (ItemSize + Spacing) + 1);

            CheckObjectPool(cnt);

            if (LoopList && Items.Count < cnt - 1)
            {
                Debug.LogWarning($"Your item count({Items.Count}) is smaller than the list can display({cnt}), this may cause abnormal appearance.");
            }

            if (LoopList && start < 0)
            {
                start += Items.Count;
            }

            // Check avaliable item object
            var avaliable = new List<MilListViewItem>();
            for (var i = 0; i < display.Count; i++)
            {
                var canRecycle = false;
                if (!LoopList)
                {
                    canRecycle = (display[i].Index != -1) && (display[i].Index < start || display[i].Index >= start + cnt);
                }
                else
                {
                    var b1 = start % Items.Count;
                    var b2 = b1 + cnt;
                    if (b2 >= Items.Count)
                    {
                        canRecycle = (display[i].Index != -1) && (display[i].Index < b1 && display[i].Index >= b2 % Items.Count);
                    }
                    else
                    {
                        canRecycle = (display[i].Index != -1) && (display[i].Index < b1 || display[i].Index >= b2);
                    }
                }
                
                if (canRecycle)
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
                var k = i;
                if (LoopList)
                {
                    k %= Items.Count;
                }
                if (k >= 0 && k < Items.Count && !bindDisplay[k])
                {
                    if (j >= avaliable.Count)
                    {
                        Debug.LogWarning("Lack of item object.");
                        continue;
                    }
                    avaliable[j].Binding = Items[k];
                    bindDisplay[k] = avaliable[j];
                    avaliable[j].Index = k;
                    avaliable[j].ParentListView = this;
                    avaliable[j].animator.SetState(SelectedIndex == k ? MilListViewItem.UIState.Selected : MilListViewItem.UIState.Default);
                    avaliable[j].UpdateAppearance();
                    if (!avaliable[j].GameObject.activeSelf)
                    {
                        avaliable[j].GameObject.SetActive(true);
                    }
                    j++;
                }
            }

            // Update item object
            var pos = (Position - AnchorOffset) % (ItemSize + Spacing) - AnchorOffset;
            for (var i = start; i < start + cnt; i++)
            {
                var k = i;
                if (LoopList)
                {
                    k %= Items.Count;
                }
                if (k >= 0 && k < Items.Count && bindDisplay[k])
                {
                    var p = Vertical switch
                    {
                        true => new Vector2(Indentation, pos),
                        false => new Vector2(pos, Indentation)
                    };
                    if (bindDisplay[k].RectTransform.anchoredPosition != p)
                    {
                        bindDisplay[k].RectTransform.anchoredPosition = p;
                        bindDisplay[k].AdjustAppearance((pos - AnchorOffset) / size * -1f);
                    }
                }

                pos -= (ItemSize + Spacing) * (Vertical ? 1f : -1f);
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
            if (!Scrollable)
            {
                return;
            }
            
            // Stop transforming the position
            transTime = transDuration;  
            
            startPos = eventData.position;
            orPos = Position;
            watch.Restart();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!Scrollable)
            {
                return;
            }
            
            if (Vertical)
            {
                Position = orPos + (eventData.position - startPos).y;
            }
            else
            {
                Position = orPos + (eventData.position - startPos).x;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!Scrollable)
            {
                return;
            }
            
            watch.Stop();
            OnDrag(eventData);
            var time = watch.ElapsedMilliseconds / 1000f;
            var factor = Vertical ? 1080f / Screen.height : 1920f / Screen.width;
            var delta = eventData.position - startPos;
            CheckLoopListPosition();
            targetPos = Position + (delta.magnitude * factor * Mathf.Sign((Vertical ? delta.y : delta.x))) / time * 0.3f;
            CheckPosition();
            OnScrollDone?.Invoke();
        }

        public void RefreshItemAppearance()
        {
            foreach (var item in display)
            {
                if (item.GameObject.activeSelf)
                {
                    item.UpdateAppearance();
                }
            }
        }

        public List<MilListViewItem> GetDisplayingItems()
        {
            return display.FindAll(_ => true);
        }

        private float GetOriginPointPosition()
        {
            return (
                       -Spacing 
                       - ItemSize * (Vertical ? 1f - ItemPivot.y : ItemPivot.x)
                       - Mathf.Max(0, AnchorOffset * 2 - ItemSize * (Vertical ? 1f - ItemPivot.y : ItemPivot.x))
                       - StartPadding
                   ) 
                   * (Vertical ? 1f : -1f);
        }

        private void GetPositionBoundary(out float minPos, out float maxPos)
        {
            minPos = Vertical ?
                GetOriginPointPosition():
                Mathf.Min(0f, -1f * (Items.Count * (ItemSize + Spacing) - RectTransform.rect.width - ItemSize * ItemPivot.x + EndPadding));
            maxPos = Vertical ?
                Mathf.Max(0f, Items.Count * (ItemSize + Spacing) - RectTransform.rect.height - ItemSize * (1f - ItemPivot.y) + EndPadding) :
                GetOriginPointPosition();
        }
        
        private void CheckLoopListPosition()
        {
            if (LoopList)
            {
                // secretly decrease the position number
                var tmp = Position;
                var len = (ItemSize + Spacing) * Items.Count;
                while (tmp < len * (Vertical ? 1f : -3f))
                {
                    tmp += len;
                }
                while (tmp > len * (Vertical ? 3f : -1f))
                {
                    tmp -= len;
                }
                Position = tmp;
            }
        }
        
        private void CheckPosition(bool noTrans = false)
        {
            originPos = Position;
            
            GetPositionBoundary(out var minPos, out var maxPos);
            
            if (!LoopList && (targetPos < minPos || targetPos > maxPos))
            {
                targetPos = Mathf.Clamp(targetPos, minPos, maxPos);
            }
            
            transTime = 0f;
            if (noTrans)
            {
                Position = targetPos;
                transTime = transDuration;
            }
        }
        
        private void Update()
        {
            UpdateListView();
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (!Scrollable)
            {
                return;
            }
            CheckLoopListPosition();
            targetPos = Position - (Vertical ? eventData.scrollDelta.y : eventData.scrollDelta.x) * MouseScrollSensitivity * 2f;
            CheckPosition();
        }
    }
}
