using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float patrolSpeed = 2f;
    public Vector3 patrolDirection = Vector3.right;
    public float leftLimit = -5f;
    public float rightLimit = 5f;
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        transform.Translate(patrolDirection * patrolSpeed * Time.deltaTime);

        // Reverse direction when reaching patrol bounds
        if (transform.position.x > startPosition.x + rightLimit ||
            transform.position.x < startPosition.x + leftLimit)
        {
            patrolDirection = -patrolDirection;
        }
    }

    public void DestroyEnemy()
    {
        // Optional: trigger explosion effects or score increments here
        Destroy(gameObject);
    }
}
