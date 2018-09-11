using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class RecorderSprite
{
    public string name;
    public string atlas;
    public bool compressed;
    public SelectionState state;
    public static implicit operator bool(RecorderSprite exists) { return !exists.isNull(); }
}

public enum SelectionState
{
    None,
    Highlighted,
    Pressed,
    Disabled
}
