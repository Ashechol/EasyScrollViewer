using EasyScrollViewer;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIMessageItem : MonoBehaviour, IScrollViewItem
{
    private TextMeshProUGUI _textMesh;
    
    public Vector3 LastPosition { get; set; }

    public RectTransform RectTrans { get; private set; }

    public ContentSizeFitter Fitter { get; private set; }

    private void Awake()
    {
        _textMesh = GetComponentInChildren<TextMeshProUGUI>();
        
        RectTrans = GetComponent<RectTransform>();
        Fitter = GetComponent<ContentSizeFitter>();
    }
    
    /// <summary>
    /// 刷新文本
    /// </summary>
    /// <param name="text">更新的内容</param>
    /// <param name="alignment">对齐方式，默认值为居中</param>
    public void Refresh(string text, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
    {
        _textMesh.text = text;
            
        // 在下一帧更新
        LayoutRebuilder.MarkLayoutForRebuild(RectTrans);
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
    
    public void Refresh(ScrollViewItemData data)
    {
        if (data is MessageData messageData)
        {
            Refresh(messageData.senderName, messageData.message);
        }
    }
}

public class MessageData: ScrollViewItemData
{
    public string senderName;
    public string message;

    public MessageData(string senderName, string message)
    {
        this.senderName = senderName;
        this.message = message;
    }
}
