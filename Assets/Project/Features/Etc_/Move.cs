using UnityEngine;

/// <summary>
/// 플레이어의 좌/우 움직임과 애니메이터 전환을 관리하는 코드
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private bool flipSprite = true;

    private Rigidbody rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private float horizontalInput;
    public bool canMove = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!canMove)
        {
            horizontalInput = 0f;
            animator.SetFloat("MoveSpeed", 0f); // 강제로 idle 모션
            return;
        }

        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (flipSprite)
            FlipSprite();

        animator.SetFloat("MoveSpeed", Mathf.Abs(horizontalInput));
    }

    void FixedUpdate()
    {
        if (!canMove)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y); // 이동도 정지
            return;
        }

        rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
    }

    private void FlipSprite()
    {
        if (horizontalInput > 0)
            spriteRenderer.flipX = false;
        else if (horizontalInput < 0)
            spriteRenderer.flipX = true;
    }

    /// <summary>
    /// 이동 불가 상태로 전환하고 애니메이션도 Idle로 고정함
    /// </summary>
    public void ForceStopMovementAndIdle()
    {
        canMove = false;
        horizontalInput = 0f;
        if (animator != null)
            animator.SetFloat("MoveSpeed", 0f);
        if (rb != null)
            rb.velocity = Vector2.zero;
    }

}
