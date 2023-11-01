using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScrollViewItem : MonoBehaviour
{
    private TextMeshProUGUI _textMesh;

    public RectTransform rect;

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
    }

    public void ReadyStay()
    {
        _lastPosition = rect.transform.position;
    }

    public void Stay()
    {
        rect.transform.position = _lastPosition;
    }
}
