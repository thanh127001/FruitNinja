using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterController : MonoBehaviour
{
    [Header("Jump Settings")]
    [SerializeField] private float horizontalForce = 5f;
    [SerializeField] private float upwardForce = 2f;

    [Header("Climb Settings")]
    [SerializeField] private float upwardAcceleration = 5f;
    [SerializeField] private float wallStickForce = 2f;

    private Rigidbody2D rb;
    private Collider2D currentWall;
    private int verticalBorderLayer;

    private bool hasJumped;
    private bool jumpable;
    private bool isTouchingWall;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        verticalBorderLayer = LayerMask.NameToLayer("VerticalBorder");

        ResetCharacter();
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
            HandleGameStateChanged();
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void HandleGameStateChanged()
    {
        if (GameManager.Instance.State == GameState.Ready)
            ResetCharacter();
    }

    private void ResetCharacter()
    {
        hasJumped = false;
        jumpable = true;
        isTouchingWall = false;
        currentWall = null;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.position = Vector2.zero;
        rb.rotation = 0f;
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.State != GameState.Playing ||
            !hasJumped)
            return;

        // Apply continuous upward acceleration after the first jump.
        // Compensate gravity so the character keeps rising without a height limit.
        float gravityCompensation = -Physics2D.gravity.y * rb.gravityScale;
        float forceY = rb.mass * (upwardAcceleration + gravityCompensation);

        rb.AddForce(Vector2.up * forceY, ForceMode2D.Force);

        if (!isTouchingWall)
            return;

        if (currentWall == null)
        {
            isTouchingWall = false;
            return;
        }

        // Keep pushing toward the contacted wall.
        float direction = Mathf.Sign(
            currentWall.bounds.center.x - rb.position.x
        );

        rb.AddForce(
            Vector2.right * direction * wallStickForce,
            ForceMode2D.Force
        );
    }

    public void Jump()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.State != GameState.Playing ||
            !jumpable)
            return;

        float direction;

        if (!hasJumped)
        {
            // Random direction for the first jump.
            direction = Random.value < 0.5f ? -1f : 1f;
        }
        else if (isTouchingWall && currentWall != null)
        {
            // Jump away from the contacted wall.
            direction = Mathf.Sign(
                rb.position.x - currentWall.bounds.center.x
            );
        }
        else
        {
            return;
        }

        hasJumped = true;
        jumpable = false;

        isTouchingWall = false;
        currentWall = null;

        rb.linearVelocityX = 0f;

        rb.AddForce(
            new Vector2(
                direction * horizontalForce,
                upwardForce
            ),
            ForceMode2D.Impulse
        );
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.State != GameState.Playing ||
            !hasJumped ||
            !IsVerticalBorder(collision.collider))
            return;

        currentWall = collision.collider;
        isTouchingWall = true;
        jumpable = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider != currentWall)
            return;

        currentWall = null;
        isTouchingWall = false;
        jumpable = false;
    }

    private bool IsVerticalBorder(Collider2D collider)
    {
        return collider is BoxCollider2D &&
               collider.gameObject.layer == verticalBorderLayer;
    }
}
