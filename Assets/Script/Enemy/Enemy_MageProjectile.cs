using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_MageProjectile : MonoBehaviour
{
    private EntityCombat combat;
    private Rigidbody2D rb;
    private Collider2D col;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private float physicalDamage;
    private float elementalDamage;
    private ElementType element;
    private ElementalEffectData effectData;


    [SerializeField] private float arcHeight = 2f;
    [SerializeField] private LayerMask whatCanCollideWith;
    [SerializeField] private Color fireColor = new Color(1f, .45f, .12f, 1f);
    [SerializeField] private Color iceColor = new Color(.42f, .75f, 1f, 1f);
    [SerializeField] private Color lightningColor = new Color(1f, .92f, .12f, 1f);

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        anim = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }
    
    public void SetupProjectile(Transform target, EntityCombat combat, float physicalDamage,
        float elementalDamage, ElementType element, ElementalEffectData effectData)
    {
        this.combat = combat;
        this.physicalDamage = physicalDamage;
        this.elementalDamage = elementalDamage;
        this.element = element;
        this.effectData = effectData;

        anim.enabled = false;
        ApplyElementVisual();

        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector2 velocity=CalculateBallisticVelocity(transform.position, target.position);
        rb.velocity = velocity;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & whatCanCollideWith) != 0)
        {
            combat.PerformCustomAttackOnTarget(collision.transform, physicalDamage, elementalDamage, element, effectData);

            rb.velocity = Vector2.zero;
            rb.gravityScale = 0;
            anim.enabled = true;
            col.enabled = false;
            Destroy(gameObject, 2);
        }
    }

    private void ApplyElementVisual()
    {
        if (spriteRenderer == null)
            return;

        switch (element)
        {
            case ElementType.Fire:
                spriteRenderer.color = fireColor;
                break;
            case ElementType.Ice:
                spriteRenderer.color = iceColor;
                break;
            case ElementType.Lightning:
                spriteRenderer.color = lightningColor;
                break;
            default:
                spriteRenderer.color = Color.white;
                break;
        }
    }
    private Vector2 CalculateBallisticVelocity(Vector2 start, Vector2 end)
    {
        // Get effective gravity based on global gravity and this Rigidbody's gravity scale
        float gravity = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);

        // Calculate vertical and horizontal displacemnt 
        float displacementY = end.y - start.y;
        float displacementX = end.x - start.x;

        float peakHieght = Mathf.Max(arcHeight, end.y - start.y + .1f); // Ensure arc is always above

        // Time to reaach the top of the arc
        float timeToApex = Mathf.Sqrt(2 * peakHieght / gravity);

        // Time to fall from the top of arc to the target
        float timeFromApex = Mathf.Sqrt(2 * (peakHieght - displacementY) / gravity);

        // Total flight time = up + down
        float totalTime = timeToApex + timeFromApex;

        // Initial vertical velocity to reach the arc height
        float velocityY = Mathf.Sqrt(2 * gravity * peakHieght);

        // Initial horizontal velocity to cover distance in total flight time
        float velocityX = displacementX / totalTime;

        // Return combined velocity
        return new Vector2(velocityX, velocityY);
    }
}
