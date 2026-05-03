using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using static UnityEngine.GraphicsBuffer;
using TMPro;

public class BlockController : MonoBehaviour
{
    private InputAction playerMovement;
    private InputAction rotateCW;
    private InputAction rotateCCW;
    private InputAction moveLeft, moveRight;

    [Header("Movement Settings")]
    public float moveSpeed = 3f;          
    public float fallSpeed = 2f;
    public float fastFallMultiplier = 3f;
    public float horizontalMovementScale = 0.5f;

    [Header("References")]
    public BlockSpawner spawner;        
    public DropIndicator dropIndicator;
    public Transform heightLine;

    [Header("UI")]
    public TMP_Text heightText;

    public bool isControllable { get; private set; } = true;
    private Rigidbody2D theRB;

    private Vector2 moveDirection;
    private float _horizontalInput;
    private float _verticalInput;

    private float targetHeightY = float.MinValue;
    public float heightLineMoveSpeed = 2f;  

    private void Start() {
        playerMovement = InputSystem.actions.FindAction("Move");
        rotateCW = InputSystem.actions.FindAction("RotateCW");
        rotateCCW = InputSystem.actions.FindAction("RotateCCW");
        moveLeft = InputSystem.actions.FindAction("Left");
        moveRight = InputSystem.actions.FindAction("Right");

        theRB = GetComponent<Rigidbody2D>();
        theRB.gravityScale = 0f;


    }

    private void Update() {
        if (!isControllable) return;

        HandleHorizontalInput();
        ApplyFalling();
        HandleRotation();
        FollowIndicator();
        //quantumHorizontalMovement();
        //UpdateHeightLine();
    }

    void HandleHorizontalInput() {
        moveDirection = playerMovement.ReadValue<Vector2>();

        _horizontalInput = moveDirection.x;
        _verticalInput = moveDirection.y;

        theRB.linearVelocity = new Vector2(_horizontalInput * moveSpeed, theRB.linearVelocity.y);
    }

    void quantumHorizontalMovement()
    {

        if(moveLeft.triggered) 
        {
            theRB.transform.position += Vector3.left * horizontalMovementScale;
        }
        if (moveRight.triggered)
        {
            theRB.transform.position += Vector3.right * horizontalMovementScale;
        }
    }

    void ApplyFalling() {
        float currentFallSpeed = fallSpeed;
        if (_verticalInput < 0)
            currentFallSpeed = fallSpeed * fastFallMultiplier;


        theRB.linearVelocity = new Vector2(theRB.linearVelocity.x, -currentFallSpeed);
    }
    
    void HandleRotation() {
        if (rotateCW.WasPressedThisFrame())
            TryRotate(-90f);

        if (rotateCCW.WasPressedThisFrame())
            TryRotate(90f);
    }
    

    void TryRotate(float angle) {
        // 1. Rotate
        transform.Rotate(0f, 0f, angle);

        // Wall Kick offest try : no move, Left, Right, Left*2, Right*2, Up 
        Vector2[] kicks = new Vector2[]
        {
        Vector2.zero,
        Vector2.left,
        Vector2.right,
        Vector2.left * 2,
        Vector2.right * 2,
        Vector2.up,
        };

        foreach (Vector2 kick in kicks) {
            transform.position += (Vector3)kick;

            if (!IsOverlapping())
                return; // if offset no overlapping just Rotate

            // this offset overlapping try next 
            transform.position -= (Vector3)kick;
        }

        // all offset wrong 
        transform.Rotate(0f, 0f, -angle);
    }

    bool IsOverlapping() {
        Collider2D[] children = GetComponentsInChildren<Collider2D>();

        foreach (Collider2D col in children) {
            Collider2D[] hits = Physics2D.OverlapBoxAll(
                col.bounds.center,
                col.bounds.size * 0.9f,  
                0f
            );

            foreach (Collider2D hit in hits) {
                if (hit.transform.IsChildOf(transform)) continue;
                if (hit.transform == transform) continue;

                return true;
            }
        }

        return false;
    }

    void FollowIndicator() {
        if (dropIndicator == null) return;

        // indicator follow the block 
        dropIndicator.transform.position = new Vector3(
            transform.position.x,
            dropIndicator.transform.position.y,
            dropIndicator.transform.position.z
        );
    }

    private void OnCollisionEnter2D(Collision2D other) {
        GameObject root = other.transform.root.gameObject;

        if (root.CompareTag("Block") || root.CompareTag("Board")) {
            LockBlock();
        }
    }

    public void LockBlock() {
        if (!isControllable) return; 

        isControllable = false;

        theRB.gravityScale = 1f;                          
        theRB.constraints = RigidbodyConstraints2D.None;  
        theRB.linearVelocity = Vector2.zero;              

        Debug.Log("Touch block");
        if (dropIndicator != null)
            dropIndicator.DetachBlock();
        dropIndicator = null;

        FindHighestBlock();

        // next block
        if (spawner != null) {
            Debug.Log("Next Block");
            spawner.SpawnNextBlock();
        }

    }

    void FindHighestBlock() {
        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");

        float highest = float.MinValue;

        foreach (GameObject block in blocks) {
            Collider2D[] cols = block.GetComponentsInChildren<Collider2D>();
            foreach (Collider2D col in cols) {
                float top = col.bounds.max.y;
                if (top > highest)
                    highest = top;
            }
        }

        if (highest > float.MinValue) {
            Debug.Log("heightest: " + highest);

            if (heightLine != null) {
                StartCoroutine(MoveHeightLine(highest));
            } else {
                Debug.LogWarning("heightLine no setting");
            }
        }
    }

    private IEnumerator MoveHeightLine(float targetY) {
        Vector3 target = new Vector3(heightLine.position.x, targetY, heightLine.position.z);

        while (Vector3.Distance(heightLine.position, target) > 0.01f) {
            heightLine.position = Vector3.MoveTowards(
                heightLine.position,
                target,
                2f * Time.deltaTime  
            );
            UpdateHeightText();  
            yield return null;
        }

        heightLine.position = target; 
    }

    void UpdateHeightText() {
        if (heightText == null) return;

        float baseY = -2.27f;  // height line y value
        float currentHeight = heightLine.position.y - baseY;
        heightText.text = currentHeight.ToString("F1") + "m";  
    }

    void UpdateHeightLine() {
        if (heightLine == null || targetHeightY == float.MinValue) return;

        Vector3 target = new Vector3(
            heightLine.position.x,
            targetHeightY,
            heightLine.position.z
        );

        heightLine.position = Vector3.MoveTowards(
            heightLine.position,
            target,
            heightLineMoveSpeed * Time.deltaTime
        );
    }


}
