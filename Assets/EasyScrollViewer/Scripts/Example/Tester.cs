// ****************************************
// 作者：gonghanchao
// 创建时间：2023-11-03 9:50
// ****************************************

using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyScrollViewer
{
    public class Tester: MonoBehaviour
    {
        private ScrollViewer _scrollViewer;

        private readonly List<ScrollViewItemData> _dataList = new();

        private int _index;
        public float r, g, b;

        private void Awake()
        {
            _scrollViewer = GetComponentInChildren<ScrollViewer>();
        }

        private void Start()
        {
            _scrollViewer.Initialize(_dataList);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                _dataList.Add(new ColorfulData(new Color(r, g, b)));

                r += 0.01f;
                g += 0.02f;
                b += 0.03f;
                
                _scrollViewer.Refresh(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                _dataList.Add(new MessageData("Ash", $"Hello There {_index++}"));
                _scrollViewer.Refresh(10);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                _dataList.Add(new MessageData("Ash", $"Hello There\nHi {_index++}"));
                _scrollViewer.Refresh(0);
            }
        }
    }
}