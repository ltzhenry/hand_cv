using UnityEngine;
using System.Collections.Generic;

public class HandRenderer : MonoBehaviour
{
    public GameObject jointSpherePrefab;
    private List<GameObject[]> handSpheres = new List<GameObject[]>();
    private List<LineRenderer> lineRenderers = new List<LineRenderer>();

    private readonly int[] bonePairs = new int[] {
        0,1, 1,2, 2,3, 3,4,
        0,5, 5,6, 6,7, 7,8,
        0,9, 9,10, 10,11, 11,12,
        0,13, 13,14, 14,15, 15,16,
        0,17, 17,18, 18,19, 19,20
    };

    void Update()
    {
        if (HandManager.Instance == null) return;

        int currentHandCount = HandManager.Instance.hands.Count;

        // 删除多余的手部对象
        while (handSpheres.Count > currentHandCount)
        {
            int lastIndex = handSpheres.Count - 1;
            
            // 删除最后一只手的所有关节球体
            GameObject[] lastHandSpheres = handSpheres[lastIndex];
            for (int i = 0; i < lastHandSpheres.Length; i++)
            {
                if (lastHandSpheres[i] != null)
                {
                    Destroy(lastHandSpheres[i]);
                }
            }
            handSpheres.RemoveAt(lastIndex);

            // 删除对应的线渲染器
            if (lastIndex < lineRenderers.Count)
            {
                if (lineRenderers[lastIndex] != null)
                {
                    Destroy(lineRenderers[lastIndex].gameObject);
                }
                lineRenderers.RemoveAt(lastIndex);
            }

            Debug.Log($"🗑️ 删除了第{lastIndex + 1}只手的渲染对象");
        }

        // 创建不足的手部对象
        while (handSpheres.Count < currentHandCount)
        {
            var spheres = new GameObject[21];
            for (int i = 0; i < 21; i++)
            {
                spheres[i] = Instantiate(jointSpherePrefab);
                spheres[i].name = $"Hand{handSpheres.Count}_Joint{i}";
                var rb = spheres[i].AddComponent<Rigidbody>();
                rb.useGravity = false;
                rb.isKinematic = true;

                spheres[i].AddComponent<SphereCollider>();
            }
            handSpheres.Add(spheres);

            var lineObj = new GameObject("LineRenderer_" + handSpheres.Count);
            var lr = lineObj.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startWidth = 0.02f;
            lr.endWidth = 0.02f;
            lr.positionCount = bonePairs.Length;
            lineRenderers.Add(lr);

            Debug.Log($"➕ 创建了第{handSpheres.Count}只手的渲染对象");
        }

        // 更新手部位置（只有在有新数据时）
        if (HandManager.Instance.hasNewData && currentHandCount > 0)
        {
            for (int h = 0; h < currentHandCount; h++)
            {
                Vector3[] positions = HandManager.Instance.hands[h];
                
                // 更新关节球体位置
                for (int i = 0; i < 21; i++)
                {
                    if (handSpheres[h][i] != null)
                    {
                        handSpheres[h][i].transform.position = positions[i];
                    }
                }
                
                // 更新骨骼连线
                if (h < lineRenderers.Count && lineRenderers[h] != null)
                {
                    for (int i = 0; i < bonePairs.Length; i++)
                    {
                        lineRenderers[h].SetPosition(i, positions[bonePairs[i]]);
                    }
                }
            }

            HandManager.Instance.hasNewData = false;
        }
    }

    // 清理所有手部对象
    void OnDestroy()
    {
        ClearAllHands();
    }

    public void ClearAllHands()
    {
        // 删除所有手部球体
        foreach (var hand in handSpheres)
        {
            if (hand != null)
            {
                foreach (var sphere in hand)
                {
                    if (sphere != null)
                    {
                        Destroy(sphere);
                    }
                }
            }
        }
        handSpheres.Clear();

        // 删除所有线渲染器
        foreach (var lr in lineRenderers)
        {
            if (lr != null)
            {
                Destroy(lr.gameObject);
            }
        }
        lineRenderers.Clear();

        Debug.Log("🧹 清理了所有手部渲染对象");
    }
}