using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    public static HandManager Instance;

    private UdpClient client;
    public List<Vector3[]> hands = new List<Vector3[]>();
    public bool hasNewData = false;

    [Serializable]
    public class Landmark { public float x, y, z; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        client = new UdpClient(9999);
        client.BeginReceive(OnDataReceived, null);
        Debug.Log("✅ HandManager listening on port 9999");
    }

    void OnDataReceived(IAsyncResult ar)
    {
        IPEndPoint ep = new IPEndPoint(IPAddress.Any, 9999);
        byte[] data = client.EndReceive(ar, ref ep);
        string json = Encoding.UTF8.GetString(data);

        try
        {
            var handsRaw = JsonHelperMulti.ParseMultiHand(json);
            hands.Clear();

            foreach (var hand in handsRaw)
            {
                if (hand.Count == 21)
                {
                    Vector3[] handPositions = new Vector3[21];
                    for (int i = 0; i < 21; i++)
                    {
                        handPositions[i] = new Vector3(
                            (hand[i].x - 0.5f) * 12f, //左右
                            -(hand[i].y - 0.5f) * 12f + 2f, //上下
                            -hand[i].z * 12f
                        );
                    }
                    hands.Add(handPositions);
                }
            }

            if (hands.Count > 0) hasNewData = true;
        }
        catch (Exception e)
        {
            Debug.LogWarning("❌ Failed to parse multi-hand: " + e.Message);
        }

        client.BeginReceive(OnDataReceived, null);
    }
}
