using EasyScrollViewer;
using UnityEngine;
using UnityEngine.UI;

public class UIColorful : MonoBehaviour, IScrollViewItem
{
    public float minHeight;
    private Image _img;

    public RectTransform RectTrans { get; private set; }
    public float MinHeightOrWidth => minHeight;

    public ContentSizeFitter Fitter { get; private set; }
    
    private void Awake()
    {
        _img = GetComponentInChildren<Image>();
        RectTrans = GetComponent<RectTransform>();
        Fitter = GetComponent<ContentSizeFitter>();
    }
    
    public void Refresh(ColorfulData data)
    {
        _img.color = data.color;
    }
}

public struct ColorfulData
{
    public Color color;

    public ColorfulData(Color color)
    {
        this.color = color;
    }
}
