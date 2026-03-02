using System;
using System.Collections.Generic;
using Milease.Core.UI;
using UnityEngine;

namespace Milease.BuiltinUI
{
    public class MilGridViewProxy<T>
    {
        private int _columnCount;
        private readonly MilListView _chargedListView;

        private readonly List<List<T>> _listSegments = new List<List<T>>();
        
        /// <summary>
        /// Create a grid view proxy class
        /// </summary>
        /// <param name="listView">The list view to contain sub list views</param>
        /// <param name="columnCount">Column count</param>
        public MilGridViewProxy(MilListView listView, int columnCount)
        {
            _columnCount = columnCount;
            _chargedListView = listView;
        }

        public void SetColumnCount(int columnCount)
        {
            var data = new List<T>();
            foreach (var segment in _listSegments)
            {
                data.AddRange(segment);
            }
            
            _columnCount = columnCount;
            
            Set(data);
        }
        
        public void Add(T item)
        {
            if (_listSegments.Count > 0 && _listSegments[_listSegments.Count - 1].Count < _columnCount)
            {
                _listSegments[_listSegments.Count - 1].Add(item);
            }
            else
            {
                _listSegments.Add(new List<T>() { item });
                _chargedListView.Add(_listSegments[_listSegments.Count - 1]);
            }
        }

        private void MoveForward(int row)
        {
            if (row >= _listSegments.Count - 1)
            {
                return;
            }
            
            _listSegments[row].Add(_listSegments[row + 1][0]);

            for (var i = row + 1; i < _listSegments.Count; i++)
            {
                if (i + 1 <= _listSegments.Count - 1)
                {
                    _listSegments[i].Add(_listSegments[i + 1][0]);
                }

                _listSegments[i].RemoveAt(0);
            }
        }
        
        public void Remove(T item)
        {
            for (var i = 0; i < _listSegments.Count; i++)
            {
                var j = _listSegments[i].IndexOf(item);
                if (j == -1)
                {
                    continue;
                }
                
                _listSegments[i].RemoveAt(j);
                MoveForward(i);
            }
        }

        public void Set(List<T> items)
        {
            _chargedListView.Clear();
            _listSegments.Clear();
            for (var i = 0; i < items.Count; i += _columnCount)
            {
                var segment = items.GetRange(i, Math.Min(_columnCount, items.Count - i));
                _chargedListView.Add(segment);
                _listSegments.Add(segment);
            }
        }
    }
}
