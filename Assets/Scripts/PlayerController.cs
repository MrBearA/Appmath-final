using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Player Health Settings")]
    public int maxHP = 100;
    private int currentHP;

    private bool isInvincible = false;
    private bool hasFireballAbility = false;


    void Start()
    {
        currentHP = maxHP;
    }

    public int GetCurrentHP()
    {
        return currentHP;
    }

    public void AddLife(int extra)
    {
        currentHP += (extra * 10);  // For example, each extra life adds 10 HP.
        currentHP = Mathf.Min(currentHP, maxHP);
        Debug.Log("Extra life! HP: " + currentHP);
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible) return;

        currentHP -= damage;
        Debug.Log("Player took damage. HP: " + currentHP);
        if (currentHP <= 0)
        {
            Debug.Log("Game Over!");
            // Trigger game over logic here.
        }
    }

    public void SetInvincible(float duration)
    {
        if (!isInvincible)
            StartCoroutine(InvincibilityCoroutine(duration));
    }

    private IEnumerator InvincibilityCoroutine(float duration)
    {
        isInvincible = true;
        Debug.Log("Player is invincible.");
        // (Optional: change player appearance)
        yield return new WaitForSeconds(duration);
        isInvincible = false;
        Debug.Log("Invincibility expired.");
    }

    public void ActivateFireballAbility(float duration)
    {
        if (!hasFireballAbility)
            StartCoroutine(FireballAbilityCoroutine(duration));
    }

    private IEnumerator FireballAbilityCoroutine(float duration)
    {
        hasFireballAbility = true;
        Debug.Log("Fireball ability activated.");
        // (Optional: show UI indicator)
        yield return new WaitForSeconds(duration);
        hasFireballAbility = false;
        Debug.Log("Fireball ability expired.");
    }

    public bool IsInvincible() { return isInvincible; }
    public bool HasFireballAbility() { return hasFireballAbility; }
}
