using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyScrollViewer
{
    public class Tester: MonoBehaviour, IDataSource
    {
        private ScrollViewer _scrollViewer;
        
        private readonly List<MessageData> _dataList = new();

        private int _index;

        private void Awake()
        {
            _scrollViewer = GetComponentInChildren<ScrollViewer>();
        }

        private void Start()
        {
            _scrollViewer.Initialize(this);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                _dataList.Add(new MessageData("Ash", $"Hello There {_index++}"));
                _scrollViewer.Refresh(_dataList.Count - 1, Vector2.zero);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                _dataList.Add(new MessageData("Ash", $"Hello There\nHi {_index++}"));
                _scrollViewer.Refresh(_dataList.Count - 1, Vector2.zero);
            }
        }

        public void RefreshItem(IScrollViewItem item, int index)
        {
            if (item is UIMessageItem message)
                message.Refresh(_dataList[index]);
        }

        public int Count => _dataList.Count;
    }
}