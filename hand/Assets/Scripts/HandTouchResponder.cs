using UnityEngine;

public class HandTouchResponder : MonoBehaviour
{
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
        rend.material.color = Color.white;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name.StartsWith("Hand"))
        {
            rend.material.color = Color.red;  // 遇到手时变色
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.name.StartsWith("Hand"))
        {
            rend.material.color = Color.white;  // 离开后恢复
        }
    }
}
