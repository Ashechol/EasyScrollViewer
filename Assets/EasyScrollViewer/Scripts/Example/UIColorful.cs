using System.Collections;
using System.Collections.Generic;
using EasyScrollViewer;
using UnityEngine;
using UnityEngine.UI;

public class UIColorful : MonoBehaviour, IScrollViewItem
{
    private Image _img;
    
    public Vector3 LastPosition { get; set; }

    public RectTransform RectTrans { get; private set; }

    public ContentSizeFitter Fitter { get; private set; }
    
    private void Awake()
    {
        _img = GetComponentInChildren<Image>();
        RectTrans = GetComponent<RectTransform>();
        Fitter = GetComponent<ContentSizeFitter>();
    }
    
    public void Refresh(ScrollViewItemData data)
    {
        if (data is ColorfulData colorfulData)
        {
            _img.color = colorfulData.color;
        }
    }
}

public class ColorfulData : ScrollViewItemData
{
    public Color color;

    public ColorfulData(Color color)
    {
        this.color = color;
    }
}
