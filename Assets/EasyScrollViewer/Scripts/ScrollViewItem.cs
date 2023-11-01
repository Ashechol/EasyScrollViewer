using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScrollViewItem : MonoBehaviour
{
    private TextMeshProUGUI _textMesh;
    public RectTransform rect;

    public string Text => _textMesh.text;

    public float Height => rect.rect.height;

    private Vector3 _lastPosition;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        _textMesh = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void Refresh(string text)
    {
        _textMesh.text = text;
        // LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        LayoutRebuilder.MarkLayoutForRebuild(rect);
    }

    public void SetAnchor(Vector2 min, Vector2 max)
    {
        _lastPosition = rect.position;

        rect.anchorMin = min;
        rect.anchorMax = max;

        rect.position = _lastPosition;
    }

    public void RecordPosition() => _lastPosition = rect.position;
    public void RestorePosition() => rect.position = _lastPosition;
}
