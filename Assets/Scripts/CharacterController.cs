using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterController : MonoBehaviour
{
    [Header("Jump Settings")]
    [SerializeField] private float horizontalForce = 5f;
    [SerializeField] private float upwardForce = 2f;

    private Rigidbody2D rb;
    private bool canJump = true;
    private bool hasLeftBorder = true;
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
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }
    }

    private void HandleGameStateChanged()
    {
        GameState state = GameManager.Instance.State;

        if (state == GameState.Ready)
        {
            canJump = true;
            hasLeftBorder = true;
            rb.linearVelocity = Vector2.zero;
        }
    }

    public void Jump()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.State != GameState.Playing ||
            !canJump)
            return;

        canJump = false;
        hasLeftBorder = false;

        float direction;

        if (transform.position.x < 0f)
            direction = 1f;
        else if (transform.position.x > 0f)
            direction = -1f;
        else
            direction = Random.value < 0.5f ? -1f : 1f;

        rb.linearVelocity = Vector2.zero;

        Vector2 force = new Vector2(
            direction * horizontalForce,
            upwardForce
        );

        rb.AddForce(force, ForceMode2D.Impulse);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.State != GameState.Playing)
            return;

        if (hasLeftBorder && IsVerticalBorder(collision))
            canJump = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (IsVerticalBorder(collision))
            hasLeftBorder = true;
    }

    private bool IsVerticalBorder(Collision2D collision)
    {
        return collision.collider is BoxCollider2D &&
               collision.gameObject.layer == verticalBorderLayer;
    }
}
