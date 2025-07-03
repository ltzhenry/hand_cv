using System;
using System.Collections.Generic;
using UnityEngine;

public static class JsonHelperMulti
{
    [Serializable]
    public class Landmark { public float x, y, z; }

    [Serializable]
    public class Hand
    {
        public Landmark[] landmarks;
    }

    [Serializable]
    public class HandsArray
    {
        public Hand[] hands;
    }

    public static List<List<Landmark>> ParseMultiHand(string json)
    {
        // Return empty list if input is null or empty
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogWarning("JsonHelperMulti: Input JSON is null or empty");
            return new List<List<Landmark>>();
        }

        try
        {
            // The incoming JSON is an array of arrays, so we need to convert it
            // to a format that Unity's JsonUtility can handle
            
            // Remove the outer brackets and split by hand
            string cleanJson = json.Trim();
            if (cleanJson.StartsWith("[") && cleanJson.EndsWith("]"))
            {
                cleanJson = cleanJson.Substring(1, cleanJson.Length - 2);
            }

            List<List<Landmark>> result = new List<List<Landmark>>();
            
            // Split the hands manually since JsonUtility struggles with List<List<>>
            List<string> handJsons = SplitHandArrays(cleanJson);
            
            foreach (string handJson in handJsons)
            {
                if (!string.IsNullOrEmpty(handJson.Trim()))
                {
                    try
                    {
                        // Parse each hand as an array of landmarks
                        string wrappedHandJson = "{\"landmarks\":" + handJson + "}";
                        Hand hand = JsonUtility.FromJson<Hand>(wrappedHandJson);
                        
                        if (hand != null && hand.landmarks != null)
                        {
                            List<Landmark> landmarks = new List<Landmark>(hand.landmarks);
                            result.Add(landmarks);
                            Debug.Log($"JsonHelperMulti: Successfully parsed hand with {landmarks.Count} landmarks");
                        }
                    }
                    catch (Exception handEx)
                    {
                        Debug.LogWarning($"JsonHelperMulti: Failed to parse individual hand: {handEx.Message}");
                    }
                }
            }
            
            Debug.Log($"JsonHelperMulti: Successfully parsed {result.Count} hands total");
            return result;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"JsonHelperMulti: Exception during JSON parsing: {e.Message}");
            return new List<List<Landmark>>();
        }
    }
    
    private static List<string> SplitHandArrays(string json)
    {
        List<string> hands = new List<string>();
        int bracketCount = 0;
        int start = 0;
        
        for (int i = 0; i < json.Length; i++)
        {
            if (json[i] == '[')
            {
                if (bracketCount == 0)
                    start = i;
                bracketCount++;
            }
            else if (json[i] == ']')
            {
                bracketCount--;
                if (bracketCount == 0)
                {
                    string hand = json.Substring(start, i - start + 1);
                    hands.Add(hand);
                }
            }
        }
        
        return hands;
    }
}
