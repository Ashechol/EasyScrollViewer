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
    
        // private readonly List<IScrollViewItem> _items = new();
        private List<ScrollViewItemData> _dataList = new();
        private readonly Dictionary<string, IScrollViewItem> _itemDict = new();
    
        // private ScrollRect _scrollRect;
        // private RectTransform _content;
        private ContentSizeFitter _fitter;
        private LayoutGroup _group;
    
        private float _spacing;
        private int _frontIndex;
        private int _backIndex;
        private int _activatedItemNum;
        private int _maxItemNum;
        public float itemMinHeight;
        public float boundHeight;
        public Vector2 lastPosition;
    
        private readonly Vector3[] _corners = new Vector3[4];
    
        private static readonly Vector2 Bottom = new Vector2(0.5f, 0);
        private static readonly Vector2 Top = new Vector2(0.5f, 1);

        protected override void Awake()
        {
            base.Awake();
            
            // _scrollRect = GetComponent<ScrollRect>();
            // _content = _scrollRect.content;
    
            _group = content.GetComponent<VerticalLayoutGroup>();
            _fitter = content.GetComponent<ContentSizeFitter>();
            _spacing = content.GetComponent<VerticalLayoutGroup>().spacing;
            
            // _scrollRect.onValueChanged.AddListener(OnScrollRectValueChanged);
    
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
            lastPosition = normalizedPosition;
        }
        
        public void Refresh(int startIndex)
        {
            _activatedItemNum = Mathf.Min(_maxItemNum, _dataList.Count);
            _frontIndex = Mathf.Min(startIndex, _dataList.Count - _activatedItemNum);
            _backIndex = Mathf.Min(_frontIndex + _activatedItemNum, _dataList.Count);
        
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
            ExecuteInNextFrame(() => boundHeight = content.sizeDelta.y);
        }

        private int _sign = 1;
        
        /// <summary>
        /// 复用 Content 当前第一个子物体
        /// </summary>
        /// <param name="index">复用后对应的数据下标</param>
        private void ReuseFront(int index)
        {
            // var front = _content.GetChild(0).GetComponent<IScrollViewItem>();
            // var back = _content.GetChild(_items.Count - 1).GetComponent<IScrollViewItem>();

            var front = _itemDict[content.GetChild(0).name];
            var back = _itemDict[content.GetChild(_activatedItemNum - 1).name];
            
            front.Refresh(_dataList[index]);
            
            front.RectTrans.anchoredPosition = back.RectTrans.anchoredPosition - Vector2.up * (back.Height + _spacing);
            front.RectTrans.SetAsLastSibling();
                
            if (content.sizeDelta.y - (front.Height + _spacing) < boundHeight && _sign < 0)
            {
                if (content.pivot != Top)
                {
                    m_ContentStartPosition -= SetPivot(content, Top);
                    
                    foreach (var pair in _itemDict)
                        pair.Value.SetAnchor(Top, Top);
                }
                    
                _sign = 1;
            }
                
            content.sizeDelta += Vector2.up * (_sign * (front.Height + _spacing));
            
            // ExecuteInNextFrame(() =>
            // {
            //     
            // });
        }
    
        /// <summary>
        /// 复用 Content 当前最后一个子物体
        /// </summary>
        /// <param name="index">复用后对应的数据下标</param>
        private void ReuseBack(int index)
        {
            // var front = _content.GetChild(0).GetComponent<IScrollViewItem>();
            // var back = _content.GetChild(_items.Count - 1).GetComponent<IScrollViewItem>();
            
            var front = _itemDict[content.GetChild(0).name];
            var back = _itemDict[content.GetChild(_activatedItemNum - 1).name];
            
            back.Refresh(_dataList[index]);
            
            if (content.sizeDelta.y - (back.Height + _spacing) < boundHeight && _sign > 0)
            {
                if (content.pivot != Bottom)
                {
                    m_ContentStartPosition -= SetPivot(content, Bottom);

                    foreach (var pair in _itemDict)
                        pair.Value.SetAnchor(Bottom, Bottom);
                    
                    _sign = -1;
                }
            }
            
            content.sizeDelta -= Vector2.up * (back.Height + _spacing) * _sign;
            
            back.RectTrans.anchoredPosition = front.RectTrans.anchoredPosition + Vector2.up * (back.Height + _spacing);
            back.RectTrans.SetAsFirstSibling();
            
            // ExecuteInNextFrame(() =>
            // {
            //     
            // });
        }

        public override void OnDrag(PointerEventData eventData)
        {
            OnScrollRectValueChanged(eventData);
            
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
    
        private void OnScrollRectValueChanged(PointerEventData pointer)
        {
            if (!_dragging) return;

            if (_fitter.enabled)
            {
                _fitter.enabled = false;
                _group.enabled = false;
            }
            
            ExecuteInNextFrame(() =>
            {
                var delta = pointer.position.y - _lastPointerPosition.y;
                _lastPointerPosition = pointer.position; 
                
                // var front = _content.GetChild(0).GetComponent<IScrollViewItem>();
                // var back = _content.GetChild(_items.Count - 1).GetComponent<IScrollViewItem>();
            
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
            });
        }
    
        /// <summary>
        /// 下一帧执行操作
        /// </summary>
        /// <param name="operation">操作委托</param>
        private void ExecuteInNextFrame(Action operation)
        {
            StartCoroutine(Operation());
            return;
    
            IEnumerator Operation()
            {
                yield return null;
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

        private void SetAnchor(RectTransform target, Vector2 anchor)
        {
            var lastPosition = target.position;

            target.anchorMin = anchor;
            target.anchorMax = anchor;

            target.position = lastPosition;
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
