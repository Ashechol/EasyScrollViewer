using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EasyScrollViewer
{
    public class ScrollViewer : ScrollRect
    {
        public GameObject itemGameObject;
        private List<ScrollViewItemData> _dataList = new();
        private readonly Dictionary<string, IScrollViewItem> _itemDict = new();
        
        private ContentSizeFitter _fitter;
        private LayoutGroup _group;
    
        private float _spacing;
        private int _frontIndex;
        private int _backIndex;
        private int _activatedItemNum;
        private int _maxItemNum;
        public float itemMinHeight;
        public float boundHeight;
    
        private readonly Vector3[] _corners = new Vector3[4];
    
        private static readonly Vector2 Bottom = new Vector2(0.5f, 0);
        private static readonly Vector2 Top = new Vector2(0.5f, 1);

        protected override void Awake()
        {
            base.Awake();
    
            _group = content.GetComponent<VerticalLayoutGroup>();
            _fitter = content.GetComponent<ContentSizeFitter>();
            _spacing = content.GetComponent<VerticalLayoutGroup>().spacing;
    
            itemGameObject = content.GetChild(0).gameObject;
        }
        
        /// 初始化
        public void Initialize(List<ScrollViewItemData> dataList)
        {
            _frontIndex = 0;
            _backIndex = 0;
            
            if (itemMinHeight == 0)
                Debug.LogWarning("[ScrollViewer]: 请设置列表每项的最小大小");
            
            var maxNum = Math.Ceiling(viewport.rect.height / (itemMinHeight + _spacing)) + 1;
            
            // 预加载列表项
            for (var i = 1; i < maxNum; ++i)
            {
                var item = Instantiate(itemGameObject, content).GetComponent<IScrollViewItem>();
                item.SetName($"Item {i.ToString()}");
                item.SetActive(false);
                _itemDict.Add(item.Name, item);
            }
        
            itemGameObject.name = "Item";
            itemGameObject.SetActive(false);
            _itemDict.Add("Item", itemGameObject.GetComponent<IScrollViewItem>());
        
            _maxItemNum = _itemDict.Count;
        
            _dataList = dataList;
        }
        
        public void Refresh(int startIndex, Vector2 normPos)
        {
            _activatedItemNum = Mathf.Min(_maxItemNum, _dataList.Count);
            _frontIndex = Mathf.Min(startIndex, _dataList.Count - _activatedItemNum);
            _backIndex = Mathf.Min(_frontIndex + _activatedItemNum, _dataList.Count);

            _group.enabled = true;
            _fitter.enabled = true;
        
            var items = GetComponentsInChildren<IScrollViewItem>(true);
        
            for (var i = 0; i < _activatedItemNum; ++i)
            {
                items[i].SetActive(true);
                items[i].Refresh(_dataList[_frontIndex + i]);
            }
            for (var i = _activatedItemNum; i < _maxItemNum; ++i)
            {
                items[i].SetActive(false);
            }
            
            if (_itemDict["Item"].RectTrans.anchorMin != Top)
                foreach (var pair in _itemDict)
                    pair.Value.SetAnchor(Top, Top);
            SetPivot(content, Top);
            
            ExecuteEndOfFrame(() =>
            {
                boundHeight = content.sizeDelta.y;
                normalizedPosition = normPos;
                _sign = 1;
            });
        }

        private int _sign = 1;
        
        /// <summary>
        /// 复用 Content 当前第一个子物体
        /// </summary>
        /// <param name="index">复用后对应的数据下标</param>
        private void ReuseFront(int index)
        {
            var front = _itemDict[content.GetChild(0).name];
            var back = _itemDict[content.GetChild(_activatedItemNum - 1).name];
            
            front.Refresh(_dataList[index]);
            
            boundHeight -= front.Height;
            AdjustContentBound(front, Vector2.up);
 
            front.RectTrans.SetAsLastSibling();
            ExecuteEndOfFrame(() =>
            {
                boundHeight += front.Height;
                front.RectTrans.anchoredPosition = back.RectTrans.anchoredPosition - Vector2.up * (back.Height + _spacing); 
            });
        }
    
        /// <summary>
        /// 复用 Content 当前最后一个子物体
        /// </summary>
        /// <param name="index">复用后对应的数据下标</param>
        private void ReuseBack(int index)
        {
            var front = _itemDict[content.GetChild(0).name];
            var back = _itemDict[content.GetChild(_activatedItemNum - 1).name];
            
            back.Refresh(_dataList[index]);
            
            boundHeight -= back.Height;
            AdjustContentBound(back, Vector2.down);
            
            back.RectTrans.SetAsFirstSibling();
            ExecuteEndOfFrame(() =>
            {
                boundHeight += back.Height;
                back.RectTrans.anchoredPosition = front.RectTrans.anchoredPosition + Vector2.up * (back.Height + _spacing);
            });
        }
        
        /// 调整 Content 的边界
        private void AdjustContentBound(IScrollViewItem item, Vector2 direction)
        {
            var amount = item.Height + _spacing;
            
            if (content.sizeDelta.y - amount < boundHeight && _sign * direction.y < 0 )
            {
                // 因为 boundHeight 还没有加上变化后的 item 的高度，
                // 如果变化后的 item 高度大于减少的高度，则实际上 content 边界没有缩小到 items 边界内。
                // 为此这里需要 ForceRebuild 一下。
                LayoutRebuilder.ForceRebuildLayoutImmediate(item.RectTrans);
                if (content.sizeDelta.y - amount > boundHeight + item.Height)
                    return;
                
                if (content.pivot != Bottom)
                {
                    m_ContentStartPosition -= SetPivot(content, Bottom);
            
                    foreach (var pair in _itemDict)
                        pair.Value.SetAnchor(Bottom, Bottom);
                }
                else
                {
                    m_ContentStartPosition -= SetPivot(content, Top);
                    
                    foreach (var pair in _itemDict)
                        pair.Value.SetAnchor(Top, Top);
                }

                _sign = -_sign;
            }
            
            if (_sign * direction.y > 0)
                ExecuteEndOfFrame(() => content.sizeDelta += (item.Height + _spacing) * _sign * direction);
            else
                content.sizeDelta += amount * _sign * direction;
        }

        public override void OnDrag(PointerEventData eventData)
        {
            ReuseItem(eventData);
            
            base.OnDrag(eventData);
        }

        /// <summary>
        /// 判断第一项或者最后一项，是否超出显示窗口
        /// </summary>
        /// <param name="itemRect">待判断的 RectTransform</param>
        /// <param name="checkTop">是否是检测顶部</param>
        /// <returns>是否超出显示窗口</returns>
        private bool IsOutOfViewport(RectTransform itemRect, bool checkTop = true)
        {
            itemRect.GetWorldCorners(_corners);
            var itemBottom = _corners[0].y;
            var itemTop = _corners[1].y;
            
            viewport.GetWorldCorners(_corners);
            var viewportTop = _corners[1].y;
            var viewportBottom = _corners[0].y;
    
            return checkTop ? itemBottom > viewportTop : itemTop < viewportBottom;
        }
    
        private void ReuseItem(PointerEventData pointer)
        {
            if (!_dragging) return;
            
            _fitter.enabled = false;
            _group.enabled = false;
            
            var delta = pointer.position.y - _lastPointerPosition.y;
            if (delta != 0)
                _lastPointerPosition = pointer.position; 
        
            var front = _itemDict[content.GetChild(0).name];
            var back = _itemDict[content.GetChild(_activatedItemNum - 1).name];
        
            if (delta > 0 && _backIndex < _dataList.Count && IsOutOfViewport(front.RectTrans))
            {
                ReuseFront(_backIndex++);
                ++_frontIndex;
            }
            else if (delta < 0 && _frontIndex > 0 && IsOutOfViewport(back.RectTrans, false))
            {
                ReuseBack(--_frontIndex);
                --_backIndex;
            }
        }
    
        /// <summary>
        /// 下一帧执行操作
        /// </summary>
        /// <param name="operation">操作委托</param>
        private void ExecuteEndOfFrame(Action operation)
        {
            StartCoroutine(Operation());
            return;
    
            IEnumerator Operation()
            {
                yield return new WaitForEndOfFrame();
                operation?.Invoke();
            }
        }

        private Vector2 SetPivot(RectTransform target, Vector2 pivot)
        {
            var pivotOffset = target.pivot - pivot;

            var offset = Vector2.Scale(pivotOffset, target.sizeDelta);
                
            target.pivot = pivot;

            return offset;
        }

        private bool _dragging;
        private Vector2 _lastPointerPosition;
        public override void OnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);
            _lastPointerPosition = eventData.position;
            _dragging = true;
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);
            
            _dragging = false;
        }
    }
}
