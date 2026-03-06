using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SceneResetConfig
{
    public string sceneName;
    public List<ObjectState> objectStates;
    public SceneResetData resetData;
}

[System.Serializable]
public class ObjectState
{
    public string objectPath;
    public bool isActive;
}

[System.Serializable]
public class SceneResetData
{
    public string defaultSelectedButton;
}