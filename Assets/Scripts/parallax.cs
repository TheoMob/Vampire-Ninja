using UnityEngine;

public class parallax : MonoBehaviour
{
    [SerializeField] private float parallaxSpeed;
    private Transform cameraTarget;
    private Vector2 initialPosition;
    private void Awake()
    {
        cameraTarget = GameObject.FindGameObjectWithTag("MainCamera").transform;
        initialPosition = transform.position;
    }

    private void Update()
    {
        float distance = cameraTarget.position.x * parallaxSpeed;
        transform.position = new Vector2( initialPosition.x + distance, transform.position.y);
    }
}
