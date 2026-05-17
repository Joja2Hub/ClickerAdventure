using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public EnemyData enemyData;
    private PlayerStats player;
    private int currentHealth;
    private int maxHealth;
    private Coroutine attackCoroutine;
    private Coroutine hitPulseCoroutine;
    private Transform healthFill;
    private Sprite healthBarSprite;
    private bool isDead;
    private Vector3 baseScale;

    private SpriteRenderer enemySprite => GetComponent<SpriteRenderer>();

    public delegate void EnemyDefeatedHandler(EnemyData data);
    public event EnemyDefeatedHandler OnDefeated;

    public void Initialize()
    {
        player = PlayerStats.Instance;
        baseScale = transform.localScale;
        maxHealth = Mathf.Max(1, enemyData.health);
        currentHealth = maxHealth;
        CreateHealthBar();
        UpdateHealthBar();

        attackCoroutine = StartCoroutine(AttackPlayer());
    }

    private void OnMouseDown()
    {
        TakeDamage(player.currentDmg);
    }

    public void TakeDamage(int dmg)
    {
        if (isDead)
            return;

        currentHealth = Mathf.Max(0, currentHealth - dmg);
        DamageNumberPopup.Show(transform.position, dmg);
        UpdateHealthBar();
        StartCoroutine(FlashRed());
        PlayHitPulse();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator FlashRed()
    {
        enemySprite.color = Color.red;
        yield return new WaitForSeconds(0.08f);
        if (isDead)
            yield break;

        enemySprite.color = Color.white;
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;
        if (attackCoroutine != null)
            StopCoroutine(attackCoroutine);

        if (QuestManager.Instance != null)
            QuestManager.Instance.RegisterEnemyKilled(enemyData);

        OnDefeated?.Invoke(enemyData);
        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        Collider2D enemyCollider = GetComponent<Collider2D>();
        if (enemyCollider != null)
            enemyCollider.enabled = false;

        Color startColor = enemySprite.color;
        Vector3 startScale = transform.localScale;
        const float duration = 0.28f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            enemySprite.color = Color.Lerp(startColor, new Color(1f, 1f, 1f, 0f), eased);
            transform.localScale = Vector3.Lerp(startScale, startScale * 1.22f, eased);
            yield return null;
        }

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

    private void PlayHitPulse()
    {
        if (hitPulseCoroutine != null)
            StopCoroutine(hitPulseCoroutine);

        hitPulseCoroutine = StartCoroutine(HitPulseRoutine());
    }

    private IEnumerator HitPulseRoutine()
    {
        const float duration = 0.12f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float wave = Mathf.Sin(t * Mathf.PI);
            transform.localScale = baseScale * (1f + 0.09f * wave);
            yield return null;
        }

        if (!isDead)
            transform.localScale = baseScale;
    }

    private void CreateHealthBar()
    {
        Sprite sprite = GetHealthBarSprite();
        float yOffset = GetSpriteTopOffset() + 0.25f;

        Transform root = new GameObject("HealthBar").transform;
        root.SetParent(transform, false);
        root.localPosition = new Vector3(0f, yOffset, 0f);

        SpriteRenderer background = CreateBarPart(root, "Background", sprite, new Color(0.12f, 0.04f, 0.04f, 0.88f), 11);
        background.transform.localScale = new Vector3(1.15f, 0.12f, 1f);

        SpriteRenderer fill = CreateBarPart(root, "Fill", sprite, new Color(0.22f, 0.9f, 0.28f, 0.95f), 12);
        fill.transform.localScale = new Vector3(1.05f, 0.07f, 1f);
        healthFill = fill.transform;
    }

    private SpriteRenderer CreateBarPart(Transform parent, string name, Sprite sprite, Color color, int sortingOrder)
    {
        GameObject part = new GameObject(name);
        part.transform.SetParent(parent, false);

        SpriteRenderer renderer = part.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingLayerID = enemySprite.sortingLayerID;
        renderer.sortingOrder = enemySprite.sortingOrder + sortingOrder;

        return renderer;
    }

    private void UpdateHealthBar()
    {
        if (healthFill == null)
            return;

        float ratio = Mathf.Clamp01((float)currentHealth / maxHealth);
        healthFill.localScale = new Vector3(1.05f * ratio, 0.07f, 1f);
        healthFill.localPosition = new Vector3(-0.525f * (1f - ratio), 0f, 0f);
    }

    private float GetSpriteTopOffset()
    {
        SpriteRenderer spriteRenderer = enemySprite;
        if (spriteRenderer == null)
            return 1f;

        return spriteRenderer.bounds.extents.y / Mathf.Max(0.0001f, transform.lossyScale.y);
    }

    private Sprite GetHealthBarSprite()
    {
        if (healthBarSprite != null)
            return healthBarSprite;

        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        healthBarSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        return healthBarSprite;
    }
}
