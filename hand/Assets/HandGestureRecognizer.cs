// HandGestureRecognizer.cs
using UnityEngine;
using System.Collections.Generic;

public class HandGestureRecognizer : MonoBehaviour
{
    private bool triggered = false;
    private float cooldown = 1.5f;
    private float timer = 0f;
    
    // 手掌状态跟踪
    private bool wasHandOpen = false;
    private float handStateTimer = 0f;
    private float handStateMinTime = 0.5f; // 手掌张开最少保持时间

    // 手势触发记录
    private bool middleFingerTriggered = false;
    private bool thumbUpTriggered = false;
    private bool heartTriggered = false;
    private bool fistCloseTriggered = false;

    [Header("手势设置")]
    public float distanceThreshold = 3.0f;  // 可调节阈值
    
    [Header("重置选项")]
    public bool allowReset = true;  // 是否允许重置
    public KeyCode resetKey = KeyCode.R;  // 重置快捷键

    void Update()
    {
        // 检查重置按键
        if (allowReset && Input.GetKeyDown(resetKey))
        {
            ResetAllTriggers();
            return;
        }

        if (timer > 0f) timer -= Time.deltaTime;

        if (HandManager.Instance == null || HandManager.Instance.hands.Count == 0)
        {
            // 没有手部数据时重置状态
            wasHandOpen = false;
            handStateTimer = 0f;
            return;
        }

        Vector3[] lm = HandManager.Instance.hands[0];
        if (lm == null || lm.Length < 21) return;

        // 检测手掌状态
        bool isHandOpen = DetectOpenHand(lm);
        bool isHandClosed = DetectClosedFist(lm);

        // 处理手掌状态转换（握拳关闭浏览器）
        if (isHandOpen)
        {
            if (!wasHandOpen)
            {
                wasHandOpen = true;
                handStateTimer = 0f;
                Debug.Log("🖐️ 检测到手掌张开");
            }
            handStateTimer += Time.deltaTime;
        }
        else if (isHandClosed && wasHandOpen && handStateTimer >= handStateMinTime && !fistCloseTriggered)
        {
            // 从张开到握拳的转换（可无限触发）
            Debug.Log("✊ 手掌握拳！关闭所有浏览器并重置手势");
            CloseAllBrowsers();
            
            // 关闭浏览器后自动重置所有手势（包括握拳本身）
            middleFingerTriggered = false;
            thumbUpTriggered = false;
            heartTriggered = false;
            fistCloseTriggered = true; // 先设为true防止重复触发
            
            Debug.Log("🔄 握拳手势执行完毕，所有手势已重置可重新使用");
            
            wasHandOpen = false;
            handStateTimer = 0f;
            timer = cooldown; // 设置冷却时间
        }
        else if (!isHandOpen && !isHandClosed)
        {
            // 手掌既不是张开也不是握拳状态时，重置握拳状态（为下次握拳做准备）
            if (fistCloseTriggered)
            {
                fistCloseTriggered = false; // 重置握拳状态，可以再次使用
            }
            wasHandOpen = false;
            handStateTimer = 0f;
        }

        // 检测其他手势（只在没有冷却时间时且未触发过）
        if (timer <= 0f)
        {
            if (DetectMiddleFingerOnly(lm) && !middleFingerTriggered)
            {
                Debug.Log("🖕 中指手势识别成功（首次触发）");
                LaunchURL("https://pornhub.com");
                middleFingerTriggered = true;
                timer = cooldown;
            }
            else if (DetectThumbUp(lm) && !thumbUpTriggered)
            {
                Debug.Log("👍 拇指手势识别成功（首次触发）");
                // LaunchURL("https://hkuportal.hku.hk");
                thumbUpTriggered = true;
                timer = cooldown;
            }
            else if (DetectHeart(lm) && !heartTriggered)
            {
                Debug.Log("🫶 比心手势识别成功（首次触发）");
                LaunchURL("https://example.com/heart");
                heartTriggered = true;
                timer = cooldown;
            }
        }

        // 显示当前状态（每2秒一次）
        if (timer <= 0f && Time.time % 2f < 0.1f)
        {
            ShowGestureStatus();
        }
    }

    void ResetAllTriggers()
    {
        middleFingerTriggered = false;
        thumbUpTriggered = false;
        heartTriggered = false;
        fistCloseTriggered = false;
        
        wasHandOpen = false;
        handStateTimer = 0f;
        timer = 0f;
        
        Debug.Log("🔄 手动重置：所有手势已重置，可重新触发");
    }

    void ShowGestureStatus()
    {
        string status = "📊 手势状态: ";
        status += middleFingerTriggered ? "🖕已触发 " : "🖕可用 ";
        status += thumbUpTriggered ? "👍已触发 " : "👍可用 ";
        status += heartTriggered ? "🫶已触发 " : "🫶可用 ";
        
        // 握拳手势永远可用（除了冷却时间）
        if (timer > 0f)
        {
            status += $"✊冷却中({timer:F1}s) ";
        }
        else
        {
            status += "✊可用(关闭+重置) ";
        }
        
        if (allowReset)
        {
            status += $"| 按{resetKey}手动重置";
        }
        
        Debug.Log(status);
    }

    void LaunchURL(string url)
    {
#if UNITY_STANDALONE_OSX
        System.Diagnostics.Process.Start("open", url);
#elif UNITY_STANDALONE_WIN
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("cmd", "/c start " + url) { CreateNoWindow = true });
#else
        Application.OpenURL(url);
#endif
    }

    void CloseAllBrowsers()
    {
#if UNITY_STANDALONE_OSX
        // macOS: 关闭常见浏览器
        try
        {
            System.Diagnostics.Process.Start("osascript", "-e 'tell application \"Safari\" to quit'");
            System.Diagnostics.Process.Start("osascript", "-e 'tell application \"Google Chrome\" to quit'");
            System.Diagnostics.Process.Start("osascript", "-e 'tell application \"Firefox\" to quit'");
            Debug.Log("🍎 macOS: 尝试关闭所有浏览器");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("关闭浏览器时出错: " + e.Message);
        }
#elif UNITY_STANDALONE_WIN
        // Windows: 关闭常见浏览器进程
        try
        {
            System.Diagnostics.Process.Start("taskkill", "/f /im chrome.exe");
            System.Diagnostics.Process.Start("taskkill", "/f /im firefox.exe");
            System.Diagnostics.Process.Start("taskkill", "/f /im msedge.exe");
            System.Diagnostics.Process.Start("taskkill", "/f /im safari.exe");
            Debug.Log("🪟 Windows: 尝试关闭所有浏览器");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("关闭浏览器时出错: " + e.Message);
        }
#else
        Debug.Log("💻 当前平台不支持关闭浏览器功能");
#endif
    }

    // 公共方法：手动重置特定手势
    public void ResetGesture(string gestureName)
    {
        switch (gestureName.ToLower())
        {
            case "middle":
            case "middlefinger":
                middleFingerTriggered = false;
                Debug.Log("🔄 中指手势已重置");
                break;
            case "thumb":
            case "thumbup":
                thumbUpTriggered = false;
                Debug.Log("🔄 拇指手势已重置");
                break;
            case "heart":
                heartTriggered = false;
                Debug.Log("🔄 比心手势已重置");
                break;
            case "fist":
            case "close":
                fistCloseTriggered = false;
                wasHandOpen = false;
                handStateTimer = 0f;
                Debug.Log("🔄 握拳手势状态已重置（握拳本身无限可用）");
                break;
            default:
                Debug.LogWarning("⚠️ 未知手势类型: " + gestureName);
                break;
        }
    }

    bool IsFingerExtended(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 dir1 = (b - a).normalized;
        Vector3 dir2 = (c - b).normalized;
        float angle = Vector3.Angle(dir1, dir2);
        return angle < 25f;
    }

    bool IsClose(Vector3 a, Vector3 b, float threshold = 0.05f)
    {
        return Vector3.Distance(a, b) < threshold;
    }

    bool DetectOpenHand(Vector3[] lm)
    {
        // 检测所有手指都伸展（张开手掌）
        return
            IsFingerExtended(lm[1], lm[2], lm[4]) &&   // 拇指伸展
            IsFingerExtended(lm[5], lm[6], lm[8]) &&   // 食指伸展
            IsFingerExtended(lm[9], lm[10], lm[12]) && // 中指伸展
            IsFingerExtended(lm[13], lm[14], lm[16]) && // 无名指伸展
            IsFingerExtended(lm[17], lm[18], lm[20]);  // 小指伸展
    }

    bool DetectClosedFist(Vector3[] lm)
    {
        // 检测所有手指都弯曲（握拳）
        return
            !IsFingerExtended(lm[1], lm[2], lm[4]) &&   // 拇指弯曲
            !IsFingerExtended(lm[5], lm[6], lm[8]) &&   // 食指弯曲
            !IsFingerExtended(lm[9], lm[10], lm[12]) && // 中指弯曲
            !IsFingerExtended(lm[13], lm[14], lm[16]) && // 无名指弯曲
            !IsFingerExtended(lm[17], lm[18], lm[20]);  // 小指弯曲
    }

    bool DetectMiddleFingerOnly(Vector3[] lm)
    {
        return
            IsFingerExtended(lm[9], lm[10], lm[12]) && // 中指直
            !IsFingerExtended(lm[5], lm[6], lm[8]) &&  // 食指弯
            !IsFingerExtended(lm[13], lm[14], lm[16]) && // 无名弯
            !IsFingerExtended(lm[17], lm[18], lm[20]) && // 小指弯
            !IsFingerExtended(lm[1], lm[2], lm[4]); // 拇指弯
    }

    bool DetectThumbUp(Vector3[] lm)
    {
        return
            IsFingerExtended(lm[1], lm[2], lm[4]) && // 拇指直
            !IsFingerExtended(lm[5], lm[6], lm[8]) &&  // 食指弯
            !IsFingerExtended(lm[9], lm[10], lm[12]) && // 中指弯
            !IsFingerExtended(lm[13], lm[14], lm[16]) && // 无名弯
            !IsFingerExtended(lm[17], lm[18], lm[20]); // 小指弯
    }

    bool DetectHeart(Vector3[] lm)
    {
        return
            IsClose(lm[8], lm[4]) &&  // 食指和拇指靠近
            !IsFingerExtended(lm[9], lm[10], lm[12]) && // 中指弯
            !IsFingerExtended(lm[13], lm[14], lm[16]); // 无名弯
    }
}
