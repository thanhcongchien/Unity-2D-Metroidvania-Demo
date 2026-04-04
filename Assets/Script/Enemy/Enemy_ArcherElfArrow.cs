using UnityEngine;

public class Enemy_ArcherElfArrow : MonoBehaviour, ICounterable
{
    [SerializeField] private LayerMask whatIsTarget;

    private Collider2D col;
    private Rigidbody2D rb;
    private EntityCombat combat;
    private Animator anim;

    public bool CanBeCountered => true;

    public void SetupArrow(float xVelocity, EntityCombat combat)
    {
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        rb.velocity = new Vector2(xVelocity, 0);
        this.combat = combat;

        if(rb.velocity.x<0)
            transform.Rotate(0, 180, 0);

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & whatIsTarget) != 0)
        {
            combat.PerformAttackOnTarget(collision.transform);
            StuckIntoTarget(collision.transform);
        }
    }
    private void StuckIntoTarget(Transform target)
    {
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        col.enabled= false;
        anim.enabled = false;

        transform.parent = target;

        Destroy(gameObject, 3);
    }

    public void HandleCounter()
    {
        rb.velocity = new Vector2(rb.velocity.x * -1, 0);
        transform.Rotate(0, 180, 0);

        int enemyLayer =LayerMask.NameToLayer("Enemy");

        whatIsTarget |= (1 << enemyLayer);
    }
}
