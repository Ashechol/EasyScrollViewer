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
        private List<ScrollViewItemData> _dataList;
        private readonly Dictionary<string, IScrollViewItem> _itemDict = new();
    
        private float _spacing;
        private int _frontIndex;
        private int _backIndex;
        private int _activatedItemNum;
        private int _maxItemNum;
        public float itemMinHeight;

        private Vector2 _prevPosition;
    
        private readonly Vector3[] _corners = new Vector3[4];
    
        private static readonly Vector2 Bottom = new(0.5f, 0);
        private static readonly Vector2 Top = new(0.5f, 1);

        protected override void Awake()
        {
            base.Awake();
            
            _spacing = content.GetComponent<VerticalLayoutGroup>().spacing;
    
            itemGameObject = content.GetChild(0).gameObject;
        }

        protected override void LateUpdate()
        {
            if (!_dragging && velocity.y != 0)
                ReuseItem(velocity.y);

            var lastVelocity = velocity;
            
            base.LateUpdate();
            
            if (_dragging && inertia)
            {
                var newVelocity = (content.anchoredPosition - _prevPosition) / Time.unscaledDeltaTime;
                velocity = Vector3.Lerp(lastVelocity, newVelocity, Time.unscaledDeltaTime * 10);
            }

            _prevPosition = content.anchoredPosition;
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
            if (_dataList == null) Debug.LogWarning("ScrollViewer is not initialized!");
            
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
            
            if (_itemDict["Item"].RectTrans.anchorMin != Top)
            {
                foreach (var pair in _itemDict)
                    pair.Value.SetAnchor(Top, Top);
                
            }
            m_ContentStartPosition -=SetPivot(content, Top);
             
            ExecuteEndOfFrame(() => normalizedPosition = normPos);
        }
        
        /// <summary>
        /// 复用 Content 当前第一个子物体
        /// </summary>
        /// <param name="index">复用后对应的数据下标</param>
        private void ReuseFront(int index)
        {
            var front = _itemDict[content.GetChild(0).name];

            Vector2 offset;
            if (content.pivot != Top)
            {
                offset = SetPivot(content, Top);
                
                if (_dragging)
                    m_ContentStartPosition -= offset;
                else
                    SetContentAnchoredPosition(content.anchoredPosition - offset);
                
                _prevPosition -= offset;
            }

            offset = Vector2.up * (front.Height + _spacing);
            if (_dragging)
                m_ContentStartPosition -= offset;
            else
                SetContentAnchoredPosition(content.anchoredPosition - offset);
            
            _prevPosition -= offset;
                
            front.RectTrans.SetAsLastSibling();
            
            ExecuteEndOfFrame(() => front.Refresh(_dataList[index]));
        }
    
        /// <summary>
        /// 复用 Content 当前最后一个子物体
        /// </summary>
        /// <param name="index">复用后对应的数据下标</param>
        private void ReuseBack(int index)
        {
            var back = _itemDict[content.GetChild(_activatedItemNum - 1).name];
            
            Vector2 offset;
            if (content.pivot != Bottom)
            {
                offset = SetPivot(content, Bottom);
                
                if (_dragging)
                    m_ContentStartPosition -= offset;
                else
                    SetContentAnchoredPosition(content.anchoredPosition - offset);
                
                _prevPosition -= offset;
            }
            
            offset = Vector2.up * (back.Height + _spacing);
            if (_dragging)
                m_ContentStartPosition += offset;
            else
                SetContentAnchoredPosition(content.anchoredPosition + offset);
                
            _prevPosition += offset;
            
            back.RectTrans.SetAsFirstSibling();
            
            ExecuteEndOfFrame(() => back.Refresh(_dataList[index]));
        }

        public override void OnDrag(PointerEventData eventData)
        {
            ReuseItemDrag(eventData);
            
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
        
        private void ReuseItem(float delta)
        {
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
        
        private void ReuseItemDrag(PointerEventData eventData)
        {
            var delta = eventData.position.y - _lastPointerPosition.y;
            if (delta != 0)
                _lastPointerPosition = eventData.position;

            ReuseItem(delta);
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
