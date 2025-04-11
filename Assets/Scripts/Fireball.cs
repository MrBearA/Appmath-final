using UnityEngine;

public class Fireball : MonoBehaviour
{
    [Tooltip("Speed at which the fireball travels.")]
    public float speed = 10f;
    [Tooltip("Direction of travel for the fireball.")]
    public Vector3 direction = Vector3.right;

    void Update()
    {
        // Move the fireball in the specified direction
        transform.position += direction * speed * Time.deltaTime;
    }

    // Ensure your prefab has a Collider with "Is Trigger" checked, and optionally a Rigidbody (set to Kinematic).
    void OnTriggerEnter(Collider other)
    {
        // If the fireball collides with an enemy, destroy the enemy and the fireball.
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.DestroyEnemy();   // Ensure your Enemy script has a DestroyEnemy() method.
            Destroy(gameObject);
        }
    }
}
