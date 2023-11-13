using EasyScrollViewer;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIMessageItem : MonoBehaviour, IScrollViewItem
{
    public float minHeight;
    private TextMeshProUGUI _textMesh;

    public RectTransform RectTrans { get; private set; }
    public float MinHeightOrWidth => minHeight;

    public ContentSizeFitter Fitter { get; private set; }

    private void Awake()
    {
        _textMesh = GetComponentInChildren<TextMeshProUGUI>();
        
        RectTrans = GetComponent<RectTransform>();
    }
    
    /// <summary>
    /// 刷新文本
    /// </summary>
    /// <param name="text">更新的内容</param>
    /// <param name="alignment">对齐方式，默认值为居中</param>
    public void Refresh(string text, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
    {
        _textMesh.text = text;
    }
        
    /// <summary>
    /// 刷新消息，如果发送者名字和本地玩家名字一样则向右对齐
    /// </summary>
    /// <param name="senderName">发送者的名字</param>
    /// <param name="message">发送的内容</param>
    public void Refresh(string senderName, string message)
    {
        Refresh($"[{senderName}]\n{message}");
    }
    
    public void Refresh(MessageData data)
    {
        Refresh(data.senderName, data.message);
    }
}

public struct MessageData
{
    public readonly string senderName;
    public readonly string message;

    public MessageData(string senderName, string message)
    {
        this.senderName = senderName;
        this.message = message;
    }
}
