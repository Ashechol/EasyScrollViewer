using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EasyScrollViewer : MonoBehaviour
{
    public DataSO testData;
    public GameObject scrollViewItem;

    private ScrollRect _scrollRect;
    private VerticalLayoutGroup _group;
    private ContentSizeFitter _fitter;
    private RectTransform _content;
    private float _spacing;
    
    private readonly Vector3[] _corners = new Vector3[4];

    private readonly LinkedList<ScrollViewItem> _items = new();

    private readonly Vector2 _anchorBottom = new Vector2(0, 0);
    private readonly Vector2 _anchorTop = new Vector2(0, 1);
    

    private void Awake()
    {
        _scrollRect = GetComponent<ScrollRect>();
        _content = _scrollRect.content;

        _group = _content.GetComponent<VerticalLayoutGroup>();
        _fitter = _content.GetComponent<ContentSizeFitter>();
        _spacing = _content.GetComponent<VerticalLayoutGroup>().spacing;
        
        _scrollRect.onValueChanged.AddListener(OnScrollRectValueChanged);
    }

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        foreach (var num in testData.nums)
        {
            var item = Instantiate(scrollViewItem, _content).GetComponent<ScrollViewItem>();
            item.Refresh(num.ToString());
            _items.AddLast(item);

            if (_items.Count == 8)
                break;
        }
    }

    private void OnScrollRectValueChanged(Vector2 value)
    {
        var topItem = _items.First.Value;
        var lastItem = _items.Last.Value;
        
        _fitter.enabled = false;
        _group.enabled = false;
        
        if (_scrollRect.velocity.y > 0 && IsOutOfViewport(topItem.rect))
        {
            foreach (var item in _items)
            {
                if (item.rect.anchorMin == _anchorTop) break;
                
                item.rect.anchorMin = item.rect.anchorMax = _anchorTop;
                item.rect.anchoredPosition -= Vector2.up * _content.rect.height;
                item.rect.ForceUpdateRectTransforms();
            }
            
            topItem.rect.anchoredPosition =
                lastItem.rect.anchoredPosition - Vector2.up * (topItem.rect.rect.height + _spacing);

            _items.AddLast(topItem);
            _items.RemoveFirst();
            
            var rect = _content.rect;
            if (_content.anchorMin != _anchorTop) return;
                _content.anchorMax = _content.anchorMin = _anchorTop;
            _content.sizeDelta = new Vector2(rect.width, rect.height + topItem.rect.rect.height + _spacing);
        }
        else if (_scrollRect.velocity.y < 0 && IsOutOfViewport(lastItem.rect, false))
        {
            foreach (var item in _items)
            {
                if (item.rect.anchorMin == _anchorBottom) break;
                
                item.rect.anchorMin = item.rect.anchorMax = _anchorBottom;
                item.rect.anchoredPosition += Vector2.up * _content.rect.height;
                item.rect.ForceUpdateRectTransforms();
            }
            
            lastItem.rect.anchoredPosition = 
                topItem.rect.anchoredPosition + Vector2.up * (lastItem.rect.rect.height + _spacing);
            
            _items.AddFirst(lastItem);
            _items.RemoveLast();
            
            var rect = _content.rect;
            if (_content.anchorMin != _anchorBottom);
                _content.anchorMax = _content.anchorMin = _anchorBottom;
            _content.sizeDelta = new Vector2(rect.width, rect.height - lastItem.rect.rect.height + _spacing);
        }
    }

    public bool IsOutOfViewport(RectTransform itemRect, bool checkTop = true)
    {
        itemRect.GetWorldCorners(_corners);
        var itemBottom = _corners[0].y;
        var itemTop = _corners[1].y;
        
        _scrollRect.viewport.GetWorldCorners(_corners);
        var viewportTop = _corners[1].y;
        var viewportBottom = _corners[0].y;

        return checkTop ? itemBottom > viewportTop : itemTop < viewportBottom;
    }
}
