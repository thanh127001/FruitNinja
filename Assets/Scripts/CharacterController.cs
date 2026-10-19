using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterController : MonoBehaviour
{
    [Header("Jump Settings")]
    [SerializeField] private float horizontalForce = 5f;
    [SerializeField] private float upwardForce = 2f;

    [Header("Wall Climb Settings")]
    [SerializeField] private float wallStickForce = 2f;
    [SerializeField] private float climbForce = 12f;
    [SerializeField] private float targetY = 3f;

    private Rigidbody2D rb;

    private bool jumpable = true;
    private bool isTouchingWall = false;

    private Collider2D currentWall;
    private int verticalBorderLayer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        verticalBorderLayer = LayerMask.NameToLayer("VerticalBorder");
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.State != GameState.Playing ||
            !isTouchingWall)
            return;

        if (currentWall == null)
        {
            isTouchingWall = false;
            return;
        }

        // Push the character toward the contacted wall.
        float direction = Mathf.Sign(
            currentWall.bounds.center.x - transform.position.x
        );

        rb.AddForce(
            Vector2.right * direction * wallStickForce,
            ForceMode2D.Force
        );

        // Climb only while below the target height.
        if (rb.position.y < targetY)
        {
            rb.AddForce(
                Vector2.up * climbForce,
                ForceMode2D.Force
            );
        }
    }

    public void Jump()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.State != GameState.Playing ||
            !jumpable)
            return;

        jumpable = false;

        // Jump away from the wall if currently attached.
        float direction;

        if (isTouchingWall && currentWall != null)
        {
            direction = Mathf.Sign(
                transform.position.x - currentWall.bounds.center.x
            );
        }
        else if (transform.position.x < 0f)
        {
            direction = 1f;
        }
        else if (transform.position.x > 0f)
        {
            direction = -1f;
        }
        else
        {
            direction = Random.value < 0.5f ? -1f : 1f;
        }

        // Stop applying wall forces immediately.
        isTouchingWall = false;
        currentWall = null;

        rb.linearVelocity = Vector2.zero;

        float verticalForce = rb.position.y < targetY
            ? upwardForce
            : 0f;

        Vector2 force = new Vector2(
            direction * horizontalForce,
            verticalForce
        );

        rb.AddForce(force, ForceMode2D.Impulse);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsVerticalBorder(collision.collider))
            return;

        currentWall = collision.collider;
        isTouchingWall = true;
        jumpable = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider == currentWall)
        {
            currentWall = null;
            isTouchingWall = false;
        }
    }

    private bool IsVerticalBorder(Collider2D collider)
    {
        return collider is BoxCollider2D &&
               collider.gameObject.layer == verticalBorderLayer;
    }
}
