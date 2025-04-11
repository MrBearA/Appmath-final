using UnityEngine;
using System.Collections;

public class FireballPowerup : MonoBehaviour
{
    [Header("Powerup Effects")]
    [Tooltip("Number of extra lives granted by this powerup.")]
    public int extraLives = 1;
    [Tooltip("Duration of player invincibility (seconds).")]
    public float invincibilityDuration = 5f;

    [Header("Fireball Settings")]
    [Tooltip("Prefab for the fireball to be spawned.")]
    public GameObject fireballPrefab;
    [Tooltip("Speed at which the fireball travels.")]
    public float fireballSpeed = 10f;
    [Tooltip("Direction in which the fireball will travel.")]
    public Vector3 fireballDirection = Vector3.right;

    void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object has the tag "Player"
        if (other.CompareTag("Player"))
        {
            // Get the player's controller script (assumed to be attached to the player)
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                // Grant an extra life and activate temporary invincibility
                playerController.AddLife(extraLives);
                playerController.SetInvincible(invincibilityDuration);
            }

            // Spawn the fireball at the player's current position
            if (fireballPrefab != null)
            {
                GameObject fb = Instantiate(fireballPrefab, other.transform.position, Quaternion.identity);
                Fireball fbScript = fb.GetComponent<Fireball>();
                if (fbScript != null)
                {
                    fbScript.speed = fireballSpeed;
                    fbScript.direction = fireballDirection;
                }
            }

            // Destroy the powerup object after collection
            Destroy(gameObject);
        }
    }
}
