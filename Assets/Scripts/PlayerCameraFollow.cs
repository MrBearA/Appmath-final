using UnityEngine;

public class PlayerCameraFollow : MonoBehaviour
{
    // Target player position – set externally from EnhancedMeshGenerator
    private Vector3 playerPosition;
    public Vector3 offset = new Vector3(0, -5, -10);
    public float smoothSpeed = 0.125f;
    public bool useConstraints = true;
    public Vector2 xConstraint = new Vector2(-100f, 100f);
    public Vector2 yConstraint = new Vector2(-50f, 100f);
    public bool followX = true;
    public bool followY = true;
    public bool followZ = false;

    public void SetPlayerPosition(Vector3 position)
    {
        playerPosition = position;
    }

    void LateUpdate()
    {
        if (playerPosition == Vector3.zero)
            return;

        Vector3 desiredPosition = transform.position;
        if (followX) desiredPosition.x = playerPosition.x + offset.x;
        if (followY) desiredPosition.y = playerPosition.y + offset.y;
        if (followZ) desiredPosition.z = playerPosition.z + offset.z;

        if (useConstraints)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, xConstraint.x, xConstraint.y);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, yConstraint.x, yConstraint.y);
        }

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}
