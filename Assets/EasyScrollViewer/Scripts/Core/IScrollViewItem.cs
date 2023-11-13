using UnityEngine;
using UnityEngine.UI;

namespace EasyScrollViewer
{
    public interface IScrollViewItem
    {
        RectTransform RectTrans { get; }

        float MinHeightOrWidth { get; }
        
        string Name
        {
            get => RectTrans.name;
            set => RectTrans.name = value;
        }
        
        /// <summary>
        /// 保持位置不动的情况更新锚点
        /// </summary>
        /// <param name="min">左和下</param>
        /// <param name="max">右和上</param>
        void SetAnchor(Vector2 min, Vector2 max)
        {
            // UGUI 更新锚点的时候会使用直接把当前的 AnchoredPosition 带入新的锚点中计算
            // 从而导致元素的位置发生改变
            
            var lastPosition = RectTrans.position;
        
            RectTrans.anchorMin = min;
            RectTrans.anchorMax = max;
        
            RectTrans.position = lastPosition;
        }
        
        float Height => RectTrans.rect.height;
        float Width => RectTrans.rect.width;
        
        void SetActive(bool value) => RectTrans.gameObject.SetActive(value);
    }
}