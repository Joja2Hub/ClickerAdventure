using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public EnemyData enemyData;
    private PlayerStats player;
    private int currentHealth;
    private Coroutine attackCoroutine;

    private SpriteRenderer enemySprite => GetComponent<SpriteRenderer>();

    public delegate void EnemyDefeatedHandler(EnemyData data);
    public event EnemyDefeatedHandler OnDefeated;

    public void Initialize()
    {
        player = PlayerStats.Instance;
        currentHealth = enemyData.health;

        attackCoroutine = StartCoroutine(AttackPlayer());
    }

    private void OnMouseDown()
    {
        TakeDamage(player.currentDmg);
    }

    private void TakeDamage(int dmg)
    {
        currentHealth -= dmg;
        StartCoroutine(FlashRed());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator FlashRed()
    {
        enemySprite.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        enemySprite.color = Color.white;
    }

    private void Die()
    {
        if (attackCoroutine != null)
            StopCoroutine(attackCoroutine);

        if (QuestManager.Instance != null)
            QuestManager.Instance.RegisterEnemyKilled(enemyData);

        OnDefeated?.Invoke(enemyData);
        Destroy(gameObject);
    }

    private IEnumerator AttackPlayer()
    {
        while (true)
        {
            yield return new WaitForSeconds(enemyData.attackSpeed);
            player.TakeDamage(enemyData.damage);
        }
    }
}
