using System;
using System.Collections.Generic;
using UnityEngine;

public static class JsonHelper2D
{
    [Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }

    public static List<HandManager.Landmark> Parse(string json)
    {
        string newJson = "{\"Items\":" + json + "}";
        return new List<HandManager.Landmark>(JsonUtility.FromJson<Wrapper<HandManager.Landmark>>(newJson).Items);
    }
}
