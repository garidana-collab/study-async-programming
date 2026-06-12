using UnityEngine;

public class MyRotatingCube : MonoBehaviour
{
    [Header("회전 설정")]
    [Tooltip("초당 회전 속도 (도 단위)")]
    [SerializeField]
    private float rotationSpeed = 100f;

    private void Update()
    {
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }
}
