using System.Diagnostics;
using UnityEngine;

public class GestureController : MonoBehaviour
{
    [Header("Gesture Settings")]
    public bool enableGestureRecognition = true;
    public string targetURL = "https://www.google.com";
    
    private bool actionTriggered = false;

    void Update()
    {
        if (!enableGestureRecognition) return;
        if (HandManager.Instance == null || HandManager.Instance.hands.Count == 0) return;

        Vector3[] hand = HandManager.Instance.hands[0];

        if (IsHandOpen(hand))
        {
            if (!actionTriggered)
            {
                OpenBrowser(targetURL);
                actionTriggered = true;
            }
        }
        else
        {
            actionTriggered = false; // 可重复触发
        }
    }

    bool IsHandOpen(Vector3[] hand)
    {
        // 判断是否张开手（所有指尖比指根远）
        int[] tips = { 4, 8, 12, 16, 20 }; // 拇指到小指的指尖
        int[] baseJoints = { 2, 5, 9, 13, 17 }; // 对应的基础关节

        int openCount = 0;
        for (int i = 0; i < 5; i++)
        {
            if (Vector3.Distance(hand[tips[i]], hand[0]) > Vector3.Distance(hand[baseJoints[i]], hand[0]))
                openCount++;
        }

        return openCount >= 4; // 至少 4 指张开
    }

    void OpenBrowser(string url)
    {
        try
        {
#if UNITY_EDITOR
            UnityEngine.Application.OpenURL(url);
#elif UNITY_STANDALONE_WIN
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
#elif UNITY_STANDALONE_OSX
            Process.Start("open", url);
#else
            UnityEngine.Application.OpenURL(url);
#endif
            UnityEngine.Debug.Log("✅ 打开浏览器：" + url);
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError("❌ 无法打开浏览器: " + e.Message);
        }
    }

    // 公共方法，可以从其他脚本调用
    public void TriggerAction(string url = null)
    {
        string urlToOpen = url ?? targetURL;
        OpenBrowser(urlToOpen);
    }

    // 设置目标URL
    public void SetTargetURL(string newURL)
    {
        targetURL = newURL;
    }

    // 启用/禁用手势识别
    public void SetGestureRecognition(bool enabled)
    {
        enableGestureRecognition = enabled;
        UnityEngine.Debug.Log($"手势识别已{(enabled ? "启用" : "禁用")}");
    }
} 