﻿// Created by Ashechol
// 2023-11-02

using UnityEngine;
using UnityEngine.UI;

namespace EasyScrollViewer
{
    /// <summary>
    /// 为了不影响其他项目的 UI 元素继承问题（C# 不支持多类继承），将 Item 定义为接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IScrollViewItem
    {
        // Vector3 LastPosition { get; set; }
        RectTransform RectTrans { get; }
        ContentSizeFitter Fitter { get; }
        
        /// <summary>
        /// 刷新 Item 的信息
        /// </summary>
        /// <param name="data">Item 对应的数据类</param>
        void Refresh(ScrollViewItemData data);
        
        /// <summary>
        /// 保持位置不动的情况更新锚点
        /// </summary>
        /// <param name="min">左和下</param>
        /// <param name="max">右和上</param>
        void SetAnchor(Vector2 min, Vector2 max)
        {
            var lastPosition = RectTrans.position;

            RectTrans.anchorMin = min;
            RectTrans.anchorMax = max;

            RectTrans.position = lastPosition;
        }

        void SetPivot(Vector2 pivot)
        {
            RectTrans.pivot = pivot;
        }

        float Height => RectTrans.rect.height;
        float Width => RectTrans.rect.width;

        void SetActive(bool value) => RectTrans.gameObject.SetActive(value);
        void SetName(string name) => RectTrans.name = name;
    }
    
    /// <summary>
    /// 强制让用户将数据以类的形式传输，防止遇到拆装箱的情况
    /// </summary>
    public class ScrollViewItemData
    {
        
    }
}