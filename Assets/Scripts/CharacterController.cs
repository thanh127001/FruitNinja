using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterController : MonoBehaviour
{
    [Header("Jump Settings")]
    [SerializeField] private float horizontalForce = 5f;
    [SerializeField] private float upwardForce = 2f;

    [Header("Wall Settings")]
    [SerializeField] private float wallStickForce = 2f;
    [SerializeField] private float climbForce = 12f;
    [SerializeField] private float targetY = 3f;

    [Header("Anchor Settings")]
    [SerializeField] private float anchorStrength = 30f;
    [SerializeField] private float anchorDamping = 10f;

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
        if (GameManager.Instance != null &&
            GameManager.Instance.State == GameState.Ready)
        {
            ResetCharacter();
        }
    }

    private void ResetCharacter()
    {
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
            !isTouchingWall)
            return;

        if (currentWall == null)
        {
            isTouchingWall = false;
            return;
        }

        // Keep character attached to wall.
        float direction = Mathf.Sign(
            currentWall.bounds.center.x - rb.position.x
        );

        rb.AddForce(
            Vector2.right * direction * wallStickForce,
            ForceMode2D.Force
        );

        // Apply vertical force to reach and hold targetY.
        float distance = targetY - rb.position.y;

        float gravityCompensation =
            -Physics2D.gravity.y * rb.gravityScale * rb.mass;

        float verticalForce;

        if (distance > 0f)
        {
            // Climb toward targetY.
            verticalForce = climbForce;
        }
        else
        {
            // Hold near targetY using a damped spring.
            verticalForce =
                gravityCompensation +
                distance * anchorStrength -
                rb.linearVelocityY * anchorDamping;
        }

        rb.AddForce(
            Vector2.up * verticalForce,
            ForceMode2D.Force
        );
    }

    public void Jump()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.State != GameState.Playing ||
            !jumpable)
            return;

        jumpable = false;

        float direction;

        if (isTouchingWall && currentWall != null)
        {
            direction = Mathf.Sign(
                rb.position.x - currentWall.bounds.center.x
            );
        }
        else if (rb.position.x < 0f)
        {
            direction = 1f;
        }
        else if (rb.position.x > 0f)
        {
            direction = -1f;
        }
        else
        {
            direction = Random.value < 0.5f ? -1f : 1f;
        }

        isTouchingWall = false;
        currentWall = null;

        rb.linearVelocityX = 0f;

        // No upward impulse at or above targetY.
        float verticalForce = rb.position.y < targetY
            ? upwardForce
            : 0f;

        rb.AddForce(
            new Vector2(direction * horizontalForce, verticalForce),
            ForceMode2D.Impulse
        );
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.State != GameState.Playing)
            return;

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
