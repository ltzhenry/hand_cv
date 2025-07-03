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
        if (HandManager.Instance == null || !HandManager.Instance.hasNewData) return;

        // 确保手的数量与数据匹配
        while (handSpheres.Count < HandManager.Instance.hands.Count)
        {
            var spheres = new GameObject[21];
            for (int i = 0; i < 21; i++)
            {
                spheres[i] = Instantiate(jointSpherePrefab);
                spheres[i].name = $"Hand{handSpheres.Count}_Joint{i}";
            }
            handSpheres.Add(spheres);

            var lineObj = new GameObject("LineRenderer_" + handSpheres.Count);
            var lr = lineObj.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startWidth = 0.02f;
            lr.endWidth = 0.02f;
            lr.positionCount = bonePairs.Length;
            lineRenderers.Add(lr);
        }

        for (int h = 0; h < HandManager.Instance.hands.Count; h++)
        {
            Vector3[] positions = HandManager.Instance.hands[h];
            for (int i = 0; i < 21; i++)
            {
                handSpheres[h][i].transform.position = positions[i];
            }
            for (int i = 0; i < bonePairs.Length; i++)
            {
                lineRenderers[h].SetPosition(i, positions[bonePairs[i]]);
            }
        }

        HandManager.Instance.hasNewData = false;
    }
}