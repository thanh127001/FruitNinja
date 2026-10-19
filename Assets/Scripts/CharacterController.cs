using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterController : MonoBehaviour
{
    [Header("Jump Settings")]
    [SerializeField] private float horizontalForce = 5f;
    [SerializeField] private float upwardForce = 2f;

    private Rigidbody2D rb;
    private bool jumpable = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Jump()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.State != GameState.Playing ||
            !jumpable)
            return;

        jumpable = false;

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
        if (collision.collider is BoxCollider2D &&
            collision.gameObject.layer == LayerMask.NameToLayer("VerticalBorder"))
        {
            jumpable = true;
        }
    }
}
