using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class AnimalMovement : MonoBehaviour {
    [Header("Physics Settings")]
    public float mass = 5f;
    public float initialSpeed = 3f;
    public float moveDirection = 1f; // 1 = right, -1 = left

    private Rigidbody2D rb;
    private bool hasCollided = false;

    public Rigidbody2D Rb => rb;
    public bool HasCollided => hasCollided;

    void Awake() {
        rb = GetComponent<Rigidbody2D>();
        rb.mass = mass;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void Start() {
        rb.linearVelocity = new Vector2(moveDirection * initialSpeed, rb.linearVelocity.y);
    }

    // 第一次碰到
    private void OnTriggerEnter2D(Collider2D collision) {
        HandleCollision(collision);
    }

    // 已经贴着，速度改变后重新检查
    private void OnTriggerStay2D(Collider2D collision) {
        HandleCollision(collision);
    }

    private void HandleCollision(Collider2D collision) {
        if (hasCollided) return;

        AnimalMovement other = collision.gameObject.GetComponent<AnimalMovement>();
        if (other == null || other.hasCollided) return;

        // 只处理「朝向对方」的碰撞，避免同向的误触发
        // 如果两者速度差显示没有相向运动，跳过
        float relativeVelocity = rb.linearVelocity.x - other.rb.linearVelocity.x;
        bool movingTowardsEachOther = (transform.position.x < other.transform.position.x && relativeVelocity > 0)
                                   || (transform.position.x > other.transform.position.x && relativeVelocity < 0);

        if (!movingTowardsEachOther) return;

        hasCollided = true;
        other.hasCollided = true;

        float m1 = rb.mass;
        float v1 = rb.linearVelocity.x;
        float m2 = other.rb.mass;
        float v2 = other.rb.linearVelocity.x;

        float vFinal = (m1 * v1 + m2 * v2) / (m1 + m2);

        rb.linearVelocity = new Vector2(vFinal, rb.linearVelocity.y);
        other.rb.linearVelocity = new Vector2(vFinal, other.rb.linearVelocity.y);

        Debug.Log($"[碰撞] {name} (m={m1}, v={v1:F2}) + {other.name} (m={m2}, v={v2:F2}) → vFinal={vFinal:F2}");

        StartCoroutine(ResetCollisionFlag());
        other.StartCoroutine(other.ResetCollisionFlag());
    }

    IEnumerator ResetCollisionFlag() {
        yield return new WaitForSeconds(0.15f);
        hasCollided = false;
    }
}