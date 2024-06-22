using UnityEngine;

public class parallax : MonoBehaviour
{
    [SerializeField] private Vector2 parallaxMultiplier;

    private Transform cameraTransform;
    private Vector2 lastCamPosition;

    private void Start()
    {
        cameraTransform = Camera.main.transform;
        lastCamPosition = cameraTransform.position;
    }

    private void LateUpdate() 
    {
        Vector2 deltaMovement = new Vector2(cameraTransform.position.x - lastCamPosition.x, cameraTransform.position.y - lastCamPosition.y);
        deltaMovement = deltaMovement * parallaxMultiplier;
        transform.position = new Vector2(transform.position.x + deltaMovement.x, transform.position.y + deltaMovement.y);
        lastCamPosition = cameraTransform.position;
    }
}
