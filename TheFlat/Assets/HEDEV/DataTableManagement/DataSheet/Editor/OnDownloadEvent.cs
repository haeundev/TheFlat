using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class OnDownloadEventParam
{
    public string SheetName;
    public string RawJson;
    public string ObjectJson;
}

public class OnDownloadEvent : ScriptableObject
{
    public UnityEvent<OnDownloadEventParam> downloadEvent;
}
