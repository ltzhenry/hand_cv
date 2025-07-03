using UnityEngine;

public class HandRenderer : MonoBehaviour
{
    public GameObject[] jointSpheres = new GameObject[21];
    private Vector3[] jointPositions = new Vector3[21];
    private LineRenderer[] lines;

    private readonly int[,] connections = new int[,]
    {
        {0,1}, {1,2}, {2,3}, {3,4},       // 拇指
        {0,5}, {5,6}, {6,7}, {7,8},       // 食指
        {5,9}, {9,10}, {10,11}, {11,12},  // 中指
        {9,13}, {13,14}, {14,15}, {15,16},// 无名指
        {13,17}, {17,18}, {18,19}, {19,20},// 小拇指
        {0,17}                             // 手掌边缘
    };

    void Start()
    {
        // 自动创建连线对象
        lines = new LineRenderer[connections.GetLength(0)];
        for (int i = 0; i < lines.Length; i++)
        {
            GameObject lineObj = new GameObject("Line_" + i);
            lineObj.transform.parent = this.transform;

            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startWidth = 0.02f;
            lr.endWidth = 0.02f;
            lr.positionCount = 2;
            lr.useWorldSpace = true;

            lines[i] = lr;
        }
    }

    void Update()
    {
        if (HandManager.Instance == null || !HandManager.Instance.hasNewData) return;

        for (int i = 0; i < 21; i++)
        {
            jointPositions[i] = HandManager.Instance.handPositions[i];
            if (jointSpheres[i] != null)
                jointSpheres[i].transform.position = jointPositions[i];
        }

        // 更新线段位置
        for (int i = 0; i < lines.Length; i++)
        {
            int a = connections[i, 0];
            int b = connections[i, 1];
            lines[i].SetPosition(0, jointPositions[a]);
            lines[i].SetPosition(1, jointPositions[b]);
        }

        HandManager.Instance.hasNewData = false;
    }
}