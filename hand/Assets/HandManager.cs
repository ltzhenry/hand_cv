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
    public Vector3[] handPositions = new Vector3[21];
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
            var landmarks = JsonHelper2D.Parse(json);
            if (landmarks.Count == 21)
            {
                for (int i = 0; i < 21; i++)
                {
                    handPositions[i] = new Vector3(
                        (landmarks[i].x - 0.5f) * 12f,
                        -(landmarks[i].y - 0.5f) * 12f + 2f,
                        -landmarks[i].z * 12f);
                }
                hasNewData = true;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("❌ Failed to parse single-hand: " + e.Message);
        }

        client.BeginReceive(OnDataReceived, null);
    }
}
