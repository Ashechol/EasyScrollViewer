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

    private readonly Vector2 _anchorBottom = new Vector2(0.5f, 0);
    private readonly Vector2 _anchorTop = new Vector2(0.5f, 1);

    public int topIndex;
    public int bottomIndex;
    
    private void Awake()
    {
        _scrollRect = GetComponent<ScrollRect>();
        _content = _scrollRect.content;

        _group = _content.GetComponent<VerticalLayoutGroup>();
        _fitter = _content.GetComponent<ContentSizeFitter>();
        _spacing = _content.GetComponent<VerticalLayoutGroup>().spacing;
        
        _scrollRect.onValueChanged.AddListener(OnScrollRectValueChanged);
        
        testData.messages.Clear();
        for (var i = 0; i < 100; ++i)
        {
            testData.messages.Add("dawdaw" + (i % 2 == 0 ? "\nwaeqwe" : "awd"));
        }

        testData.messages[10] = "dadaw\ndwadaw\ndwadawd\ndawd";
    }

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        foreach (var message in testData.messages)
        {
            var item = Instantiate(scrollViewItem, _content).GetComponent<ScrollViewItem>();
            item.Refresh(message);
            _items.AddLast(item);

            if (_items.Count == 8)
            {
                topIndex = 8;
                bottomIndex = 0;
                break;
            }
        }
    }

    private void OnScrollRectValueChanged(Vector2 value)
    {
        var topItem = _items.First.Value;
        var lastItem = _items.Last.Value;

        if (_fitter.enabled)
        {
            _fitter.enabled = false;
            _group.enabled = false;

            foreach (var item in _items)
                item.SetAnchor(_anchorBottom, _anchorBottom);
        }

        if (_scrollRect.velocity.y > 0 && IsOutOfViewport(topItem.rect))
        {
            if (bottomIndex <= 0) return;
            
            _content.sizeDelta -= Vector2.up * (topItem.Height + _spacing);
            
            // topItem.Refresh(testData.messages[--bottomIndex]);
            // topItem.rect.position =
            //     lastItem.rect.position - Vector3.up * (topItem.Height + _spacing);

            StartCoroutine(OperationB());
            
            _items.AddLast(topItem);
            _items.RemoveFirst();
        
            --topIndex;
        }
        else if (_scrollRect.velocity.y < 0 && IsOutOfViewport(lastItem.rect, false))
        {
            if (topIndex >= testData.nums.Count) return;
            
            // lastItem.Refresh(testData.messages[topIndex]);
            // lastItem.rect.position = 
            //     topItem.rect.position + Vector3.up * (topItem.Height + _spacing);
            //
            // _content.sizeDelta += Vector2.up * (lastItem.Height + _spacing);

            StartCoroutine(OperationA());
        
            _items.AddFirst(lastItem);
            _items.RemoveLast();
            
            ++topIndex;
            ++bottomIndex;
        }
    }

    private IEnumerator OperationA()
    {
        var topItem = _items.First.Value;
        var lastItem = _items.Last.Value;
        
        lastItem.Refresh(testData.messages[topIndex]);

        yield return null;
        
        lastItem.rect.position = 
            topItem.rect.position + Vector3.up * (topItem.Height + _spacing);
            
        _content.sizeDelta += Vector2.up * (lastItem.Height + _spacing);
    }

    private IEnumerator OperationB()
    {
        var topItem = _items.First.Value;
        var lastItem = _items.Last.Value;
        
        topItem.Refresh(testData.messages[--bottomIndex]);

        yield return null;
        
        topItem.rect.position =
            lastItem.rect.position - Vector3.up * (topItem.Height + _spacing);
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
