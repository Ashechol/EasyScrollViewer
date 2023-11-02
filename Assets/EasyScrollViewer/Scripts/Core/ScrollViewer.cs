using System;
using System.Collections;
using System.Collections.Generic;
using EasyScrollViewer;
using UnityEngine;
using UnityEngine.UI;

public class ScrollViewer : MonoBehaviour
{
    public GameObject itemGameObject;
    
    private readonly List<IScrollViewItem> _items = new();
    private readonly List<ScrollViewItemData> _datas = new();

    private ScrollRect _scrollRect;
    private RectTransform _content;
    private ContentSizeFitter _fitter;
    private LayoutGroup _group;
    
    private float _spacing;
    private int _frontIndex;
    private int _backIndex;
    public float itemMinHeight;
    
    private readonly Vector3[] _corners = new Vector3[4];
    
    private static readonly Vector2 bottom = new Vector2(0.5f, 0);
    private static readonly Vector2 top = new Vector2(0.5f, 1);

    private void Awake()
    {
        _scrollRect = GetComponent<ScrollRect>();
        _content = _scrollRect.content;

        _group = _content.GetComponent<VerticalLayoutGroup>();
        _fitter = _content.GetComponent<ContentSizeFitter>();
        _spacing = _content.GetComponent<VerticalLayoutGroup>().spacing;
            
        _scrollRect.onValueChanged.AddListener(OnScrollRectValueChanged);

        itemGameObject = _content.GetChild(0).gameObject;
    }

    private int _index;
    public float r, g, b;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            AddItem(new ColorfulData(new Color(r, g, b)));

            r += 0.01f;
            g += 0.02f;
            b += 0.03f;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            AddItem(new MessageData("Ash", $"Hello There {_index++}"));
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            AddItem(new MessageData("Ash", $"Hello There\nHi {_index++}"));
        }
    }
    
    private void Start()
    {
        Initialize();
    }
        
    /// <summary>
    /// 初始化
    /// </summary>
    private void Initialize()
    {
        _frontIndex = 0;
        _backIndex = 0;
            
        if (itemMinHeight == 0)
            Debug.LogWarning("[UIChatViewer]: 请设置列表每项的最小大小");
            
        var maxNum = Math.Ceiling(_scrollRect.viewport.rect.height / (itemMinHeight + _spacing)) + 1;
            
        // 预加载列表项
        _items.Add(itemGameObject.GetComponent<IScrollViewItem>());
        for (var i = 1; i < maxNum; ++i)
        {
            var item = Instantiate(itemGameObject, _content).GetComponent<IScrollViewItem>();
            item.SetName($"Item {i.ToString()}");
            item.SetActive(false);
            _items.Add(item);
        }
        itemGameObject.SetActive(false);
    }
    
    /// <summary>
    /// 添加消息
    /// </summary>
    /// <param name="messageInfo">消息信息</param>
    public void AddItem(ScrollViewItemData data)
    {
        _datas.Add(data);
            
        if (_backIndex < _items.Count)
        {
            _items[_backIndex].SetActive(true);
            _items[_backIndex++].Refresh(data);
                
            DoInNextFrame(() =>
            {
                var amount = _content.rect.height - _scrollRect.viewport.rect.height;
                _content.anchoredPosition = Vector2.up * (amount > 0 ? amount : 0);
            });
        }
        else if (_backIndex == _datas.Count - 1)
        {
            if (_fitter.enabled)
            {
                _fitter.enabled = false;
                _group.enabled = false;

                foreach (var item in _items)
                {
                    item.SetAnchor(top, top);
                    item.SetPivot(top);
                    item.Fitter.enabled = true;
                }
            }
                
            ReuseFront(_backIndex++);
            _frontIndex++;
                
            DoInNextFrame(() =>
            {
                var amount = _content.rect.height - _scrollRect.viewport.rect.height;
                _content.anchoredPosition = Vector2.up * (amount > 0 ? amount : 0);
            });
        }
    }
    
    /// <summary>
    /// 复用 Content 当前第一个子物体
    /// </summary>
    /// <param name="index">复用后对应的数据下标</param>
    private void ReuseFront(int index)
    {
        var front = _content.GetChild(0).GetComponent<IScrollViewItem>();
        var back = _content.GetChild(_items.Count - 1).GetComponent<IScrollViewItem>();
            
        front.Refresh(_datas[index]);
            
        DoInNextFrame(() =>
        {
            front.RectTrans.anchoredPosition = back.RectTrans.anchoredPosition - Vector2.up * (back.Height + _spacing);
            front.RectTrans.SetAsLastSibling();
            _content.sizeDelta += Vector2.up * (front.Height + _spacing);
        });
    }
    
    /// <summary>
    /// 复用 Content 当前最后一个子物体
    /// </summary>
    /// <param name="index">复用后对应的数据下标</param>
    private void ReuseBack(int index)
    {
        var front = _content.GetChild(0).GetComponent<IScrollViewItem>();
        var back = _content.GetChild(_items.Count - 1).GetComponent<IScrollViewItem>();
            
        _content.sizeDelta -= Vector2.up * (back.Height + _spacing);
        back.Refresh(_datas[index]);
            
        DoInNextFrame(() =>
        {
            back.RectTrans.anchoredPosition = front.RectTrans.anchoredPosition + Vector2.up * (back.Height + _spacing);
            back.RectTrans.SetAsFirstSibling();
        });
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
            
        _scrollRect.viewport.GetWorldCorners(_corners);
        var viewportTop = _corners[1].y;
        var viewportBottom = _corners[0].y;

        return checkTop ? itemBottom > viewportTop : itemTop < viewportBottom;
    }
    
    private void OnScrollRectValueChanged(Vector2 value)
    {
        var front = _content.GetChild(0).GetComponent<IScrollViewItem>();
        var back = _content.GetChild(_items.Count - 1).GetComponent<IScrollViewItem>();
            
        if (_scrollRect.velocity.y > 0 && _backIndex < _datas.Count && IsOutOfViewport(front.RectTrans))
        {
            ReuseFront(_backIndex++);
            ++_frontIndex;
        }
        else if (_scrollRect.velocity.y < 0 && _frontIndex > 0 && IsOutOfViewport(back.RectTrans, false))
        {
            ReuseBack(--_frontIndex);
            --_backIndex;
        }
    }

    
    /// <summary>
    /// 下一帧执行操作
    /// </summary>
    /// <param name="operation">操作委托</param>
    private void DoInNextFrame(Action operation)
    {
        StartCoroutine(Operation());
        return;

        IEnumerator Operation()
        {
            yield return null;
            operation?.Invoke();
        }
    }
}
