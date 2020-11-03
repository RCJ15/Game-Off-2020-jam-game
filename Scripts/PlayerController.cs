using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //Movement
    public float moveSpeed;
    private float startMoveSpeed;
    public float jumpHeight;

    public float gravityScale;

    public bool isCrouching;
    public float crouchMoveSpeed;
    public float crouchMuscleForce;

    public float muscleForce;
    private float startMuscleForce;
    public _muscle headMuscle;

    //Legs
    public PlayerLeg leftLeg;
    public PlayerLeg rightLeg;

    public Transform leftFootPos;
    public Transform rightFootPos;

    public LayerMask groundLayer;
    public float checkGroundOffsetY;
    public Vector2 checkGroundSize;

    public bool leftFootOnGround;
    public bool rightFootOnGround;

    public bool touchingGround;

    //Walking
    public bool isWalking;
    public bool walkingRight;

    private Coroutine walkCoroutine;

    //Components
    public Animator eyesAnim;
    private Rigidbody2D rb;

    void Start()
    {
        //Get Rigidbody2D
        rb = GetComponent<Rigidbody2D>();

        //Set start variables
        startMuscleForce = muscleForce;
        startMoveSpeed = moveSpeed;

        //Set gravity
        rb.gravityScale = gravityScale;
        foreach (Rigidbody2D _rb in GetComponentsInChildren<Rigidbody2D>())
        {
            _rb.gravityScale = gravityScale;
        }
    }

    private float timeOneLegOnGround;
    private float timeInAir;
    void Update()
    {
        //Check if the feet are standing on ground
        leftFootOnGround = Physics2D.OverlapBox((Vector2)leftFootPos.position - new Vector2(0, checkGroundOffsetY), checkGroundSize, leftFootPos.eulerAngles.z, groundLayer);
        rightFootOnGround = Physics2D.OverlapBox((Vector2)rightFootPos.position - new Vector2(0, checkGroundOffsetY), checkGroundSize, rightFootPos.eulerAngles.z, groundLayer);

        //Increase a timer if only one foot is on ground
        if ((leftFootOnGround && !rightFootOnGround) || (rightFootOnGround && !leftFootOnGround))
        {
            timeOneLegOnGround += Time.deltaTime;
        } else if (timeOneLegOnGround != 0)
        {
            timeOneLegOnGround = 0;
        }

        //Increase a timer if the player is in the air
        if (!leftFootOnGround && !rightFootOnGround)
        {
            timeInAir += Time.deltaTime;
        } else if (timeInAir != 0)
        {
            timeInAir = 0;
        }

        //If any of the timers are over a specific amount of seconds, then make the player ragdoll
        if ((timeOneLegOnGround > 2 || timeInAir > 3) && muscleForce == startMuscleForce)
        {
            Ragdoll(1, true);
        }

        //Crouch
        if (Input.GetKeyDown(KeyCode.S) && muscleForce != 0 && touchingGround && !isCrouching)
        {
            isCrouching = true;
            muscleForce = crouchMuscleForce;
        }

        //Jump if on ground
        if (Input.GetKeyDown(KeyCode.Space) && muscleForce != 0)
        {
            if (leftFootOnGround)
            {
                leftLeg.topLeg.bone.AddForce(new Vector2(0, jumpHeight), ForceMode2D.Impulse);
                leftLeg.topLeg.restRotation = 45;
            }
            if (rightFootOnGround)
            {
                rightLeg.topLeg.bone.AddForce(new Vector2(0, jumpHeight), ForceMode2D.Impulse);
                rightLeg.topLeg.restRotation = -45;
            }

            StartCoroutine(Jump());
        }
    }

    private IEnumerator Jump()
    {
        yield return new WaitForSeconds(0.5f);

        leftLeg.topLeg.restRotation = 0;
        rightLeg.topLeg.restRotation = 0;
    }

    private bool startWalk;
    private bool turnRight;
    private void FixedUpdate()
    {
        //Check if any collider is touching ground
        touchingGround = false;
        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
        {
            if (Physics2D.IsTouchingLayers(col, groundLayer))
            {
                touchingGround = true;
                break;
            }
        }

        //Activate muslces to balance
        headMuscle.ActivateMuscle(muscleForce);
        leftLeg.ActivateMuslces(muscleForce);
        rightLeg.ActivateMuslces(muscleForce);

        //Get input and movement
        float input = Input.GetAxis("Horizontal");
        if (muscleForce != 0)
        {
            rb.velocity = new Vector2(input * moveSpeed, rb.velocity.y);
        }

        //Walking
        if (isWalking && !startWalk)
        {
            StartWalk();
            startWalk = true;
        }
        else if (!isWalking && startWalk)
        {
            EndWalk();
            startWalk = false;
        }

        isWalking = input > 0 || input < 0;
        walkingRight = input > 0;

        //Turn
        if (isWalking && walkingRight && turnRight)
        {
            TurnRight();
            turnRight = false;
        } else if (isWalking && !walkingRight && !turnRight)
        {
            TurnLeft();
            turnRight = true;
        }
    }

    private void TurnRight()
    {
        eyesAnim.SetTrigger("Right");

        leftFootPos.GetComponent<Animator>().SetTrigger("Right");
        rightFootPos.GetComponent<Animator>().SetTrigger("Right");
    }

    private void TurnLeft()
    {
        eyesAnim.SetTrigger("Left");

        leftFootPos.GetComponent<Animator>().SetTrigger("Left");
        rightFootPos.GetComponent<Animator>().SetTrigger("Left");
    }

    private void StartWalk()
    {
        walkCoroutine = StartCoroutine(walkCycle(walkingRight));
    }

    private void EndWalk()
    {
        if (walkCoroutine != null)
        {
            StopCoroutine(walkCoroutine);
        }
        leftLeg.ResetLeg();
        rightLeg.ResetLeg();
    }

    private IEnumerator walkCycle(bool startRight)
    {
        float distance = Vector2.Distance(new Vector2(0, Mathf.Abs(rb.velocity.x)), new Vector2(0, moveSpeed));
        if (startRight)
        {
            rightLeg.MoveUp(walkingRight, distance, rightFootOnGround);
        }
        else
        {
            leftLeg.MoveUp(walkingRight, distance, leftFootOnGround);
        }

        yield return new WaitForSeconds(0.5f);
        leftLeg.ResetLeg();
        rightLeg.ResetLeg();

        distance = Vector2.Distance(new Vector2(0, Mathf.Abs(rb.velocity.x)), new Vector2(0, moveSpeed));
        if (startRight)
        {
            leftLeg.MoveUp(walkingRight, distance, leftFootOnGround);
        }
        else
        {
            rightLeg.MoveUp(walkingRight, distance, rightFootOnGround);
        }

        yield return new WaitForSeconds(0.5f);
        leftLeg.ResetLeg();
        rightLeg.ResetLeg();

        walkCoroutine = StartCoroutine(walkCycle(walkingRight));
    }

    private Coroutine ragdollCoroutine;
    public void Ragdoll(float timeToRagdoll, bool waitUntilTouchingGround)
    {
        if (ragdollCoroutine != null)
        {
            StopCoroutine(ragdollCoroutine);
        }
        ragdollCoroutine = StartCoroutine(RagdollCoroutine(timeToRagdoll, waitUntilTouchingGround));
    }

    private IEnumerator RagdollCoroutine(float timeToRagdoll, bool waitUntilTouchingGround)
    {
        muscleForce = 0;
        yield return new WaitForSeconds(timeToRagdoll);
        if (waitUntilTouchingGround)
        {
            yield return new WaitUntil(() => touchingGround);
        }
        muscleForce = startMuscleForce;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
    }
#endif
}

[System.Serializable]
public class _muscle
{
    public Rigidbody2D bone;
    public float restRotation;

    public void ActivateMuscle(float force)
    {
        bone.MoveRotation(Mathf.LerpAngle(bone.rotation, restRotation, force * Time.fixedDeltaTime));
    }
}

[System.Serializable]
public class PlayerLeg
{
    public _muscle topLeg;
    public _muscle bottomLeg;
    public _muscle foot;

    public void ActivateMuslces(float force)
    {
        topLeg.ActivateMuscle(force);
        bottomLeg.ActivateMuscle(force);
        foot.ActivateMuscle(force);
    }

    public void MoveUp(bool walkingRight, float value, bool legOnGround)
    {
        if (value < 4f)
        {
            topLeg.bone.AddForce(new Vector2(0, 20), ForceMode2D.Impulse);
        }

        topLeg.restRotation = 60 * ((walkingRight) ? 1 : -1);

        if (legOnGround)
        {
            topLeg.bone.AddTorque(10 * ((walkingRight) ? 1 : -1), ForceMode2D.Impulse);
        }
    }

    public void ResetLeg()
    {
        topLeg.restRotation = 0;
    }
}
