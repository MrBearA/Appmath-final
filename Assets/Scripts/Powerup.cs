using UnityEngine;
using System.Collections;

public enum PowerupType { ExtraLife, Invincibility, Fireball }

public class Powerup : MonoBehaviour
{
    [Header("Powerup Effect Settings")]
    public int extraLifeAmount = 1;
    public float invincibilityDuration = 5f;
    public float fireballAbilityDuration = 10f; // Duration for fireball ability

    private PowerupType myType;

    void Start()
    {
        // Randomize a powerup type (0, 1, or 2)
        int typeIndex = Random.Range(0, 3);
        myType = (PowerupType)typeIndex;
        Debug.Log("Powerup generated as: " + myType);

        // Change color for visual feedback:
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            switch (myType)
            {
                case PowerupType.ExtraLife:
                    renderer.material.color = Color.green;
                    break;
                case PowerupType.Invincibility:
                    renderer.material.color = Color.yellow;
                    break;
                case PowerupType.Fireball:
                    renderer.material.color = Color.red;
                    break;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController pc = other.GetComponent<PlayerController>();
            if (pc != null)
            {
                switch (myType)
                {
                    case PowerupType.ExtraLife:
                        pc.AddLife(extraLifeAmount);
                        break;
                    case PowerupType.Invincibility:
                        pc.SetInvincible(invincibilityDuration);
                        break;
                    case PowerupType.Fireball:
                        pc.ActivateFireballAbility(fireballAbilityDuration);
                        break;
                }
            }
            Destroy(gameObject);
        }
    }
}
