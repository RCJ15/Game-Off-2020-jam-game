using Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //Movement
    public bool canMove;
    public float moveSpeed;
    private float startMoveSpeed;
    public bool canJump;
    public float jumpHeight;

    public bool facingRight;

    public float gravityScale;

    //Crouching
    public bool canCrouch;
    public bool isCrouching;
    public float crouchMoveSpeed;
    public float crouchMuscleForce;
    public float crouchRestRotation;

    //Muscles
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

    //Feet
    public bool leftFootOnGround;
    public bool rightFootOnGround;

    public float leftFootTimeInAir;
    public float rightFootTimeInAir;

    public float feetCheckDist;

    private Vector2 leftFootNormal;
    private Vector2 rightFootNormal;
    private Vector2 feetBeetweenNormal;

    public bool onSlope;
    public bool onSteepSlope;
    [Range(0, 1)]
    public float slopeAngleLimit;

    public bool touchingGround;

    //Walking
    public bool isWalking;
    public bool walkingRight;

    private Coroutine walkCoroutine;

    public PhysicsMaterial2D idleMat;
    public PhysicsMaterial2D movingMat;

    //Dialog
    public bool inDialog;

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

        facingRight = true;

        inDialog = false;
        canMove = true;
        canJump = true;
        canCrouch = true;

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
        //Set the in between feet normal value
        feetBeetweenNormal = new Vector2(MathExtension.GetInBetween(leftFootNormal.x, rightFootNormal.x), MathExtension.GetInBetween(leftFootNormal.y, rightFootNormal.y));
        
        //Slopes
        onSlope = Mathf.Abs(feetBeetweenNormal.x) > 0.2f;
        onSteepSlope = Mathf.Abs(feetBeetweenNormal.x) > 0.5f;

        //Check if the feet are standing on ground
        leftFootOnGround = Physics2D.OverlapBox((Vector2)leftFootPos.position - new Vector2(0, checkGroundOffsetY), checkGroundSize, leftFootPos.eulerAngles.z, groundLayer);
        rightFootOnGround = Physics2D.OverlapBox((Vector2)rightFootPos.position - new Vector2(0, checkGroundOffsetY), checkGroundSize, rightFootPos.eulerAngles.z, groundLayer);

        //If feet are not on ground then increase a timer
        if (!leftFootOnGround) { leftFootTimeInAir += Time.deltaTime; } else { leftFootTimeInAir = 0; }
        if (!rightFootOnGround) { rightFootTimeInAir += Time.deltaTime; } else { rightFootTimeInAir = 0; }

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

        if (onSteepSlope && !(Mathf.Abs(feetBeetweenNormal.x) < 0.8f))
        {
            Ragdoll(1, true);
        }

        //Crouch
        if (Input.GetKeyDown(KeyCode.S) && muscleForce != 0 && touchingGround && !isCrouching && canCrouch)
        {
            isCrouching = true;
            muscleForce = crouchMuscleForce;
            moveSpeed = crouchMoveSpeed;

            updateCrouch();
        }

        if ((Input.GetKeyUp(KeyCode.S) && isCrouching) || !canCrouch)
        {
            isCrouching = false;
            muscleForce = startMuscleForce;
            moveSpeed = startMoveSpeed;

            rightLeg.topLeg.restRotation = 0;
            leftLeg.topLeg.restRotation = 0;
        }

        //Jump if on ground
        if (Input.GetKeyDown(KeyCode.Space) && muscleForce != 0 && canJump)
        {
            if ((leftFootOnGround || (leftFootTimeInAir < 0.5f && timeOneLegOnGround > 0))
                && (rightFootOnGround || (rightFootTimeInAir < 0.5f && timeOneLegOnGround > 0)))
            {
                leftLeg.topLeg.bone.velocity = new Vector2(jumpHeight, jumpHeight) * leftFootNormal.normalized;
                rightLeg.topLeg.bone.velocity = new Vector2(jumpHeight, jumpHeight) * rightFootNormal.normalized;

                leftLeg.topLeg.restRotation = 45;
                rightLeg.topLeg.restRotation = -45;

                inJump = true;
                StartCoroutine(Jump());
            }
        }

        //Update feet rotation
        Transform leftFoot = leftLeg.foot.bone.transform;
        Transform rightFoot = rightLeg.foot.bone.transform;

        RaycastHit2D hit = Physics2D.Raycast(leftFoot.position, Vector2.down, feetCheckDist, groundLayer);
        if (hit)
        {
            leftFootNormal = new Vector2(Mathf.Clamp(hit.normal.x, -slopeAngleLimit, slopeAngleLimit), hit.normal.y);
            leftLeg.foot.restRotation = Quaternion.FromToRotation(Vector3.up, leftFootNormal).eulerAngles.z;
        } else { leftLeg.foot.restRotation = 0; }

        hit = Physics2D.Raycast(rightFoot.position, Vector2.down, feetCheckDist, groundLayer);
        if (hit)
        {
            rightFootNormal = new Vector2(Mathf.Clamp(hit.normal.x, -slopeAngleLimit, slopeAngleLimit), hit.normal.y);
            rightLeg.foot.restRotation = Quaternion.FromToRotation(Vector3.up, rightFootNormal).eulerAngles.z;
        }
        else { rightLeg.foot.restRotation = 0; }

        //Set correct physics material
        leftLeg.foot.bone.sharedMaterial = isWalking ? movingMat : idleMat;
        rightLeg.foot.bone.sharedMaterial = isWalking ? movingMat : idleMat;

        //Set rest rotation for all muslces
        headMuscle.restRotation = Quaternion.FromToRotation(Vector3.up, feetBeetweenNormal).eulerAngles.z;

        if (!isWalking && !isCrouching && !inJump)
        {
            //Left Leg
            leftLeg.topLeg.restRotation = Quaternion.FromToRotation(Vector3.up, leftFootNormal).eulerAngles.z;
            leftLeg.bottomLeg.restRotation = Quaternion.FromToRotation(Vector3.up, leftFootNormal).eulerAngles.z;

            //Right Leg
            rightLeg.topLeg.restRotation = Quaternion.FromToRotation(Vector3.up, rightFootNormal).eulerAngles.z;
            rightLeg.bottomLeg.restRotation = Quaternion.FromToRotation(Vector3.up, rightFootNormal).eulerAngles.z;
        }
    }

    private bool inJump;
    private IEnumerator Jump()
    {
        yield return new WaitForSeconds(0.5f);
        inJump = false;
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

        //Activate muscles to balance
        headMuscle.ActivateMuscle(muscleForce);
        leftLeg.ActivateMuslces(muscleForce);
        rightLeg.ActivateMuslces(muscleForce);

        //Get input and movement
        float input = Input.GetAxis("Horizontal");
        if (!canMove)
        {
            input = 0;
        }

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

    #region Dialog
    public void StartDialog()
    {
        inDialog = true;

        canMove = false;
        canJump = false;
        canCrouch = false;
    }

    public void EndDialog()
    {
        inDialog = false;

        canMove = true;
        canJump = true;
        canCrouch = true;
    }
    #endregion

    #region Turning
    private void TurnRight()
    {
        facingRight = true;

        updateCrouch();

        eyesAnim.SetTrigger("Right");

        leftFootPos.GetComponent<Animator>().SetTrigger("Right");
        rightFootPos.GetComponent<Animator>().SetTrigger("Right");
    }

    private void TurnLeft()
    {
        facingRight = false;

        updateCrouch();

        eyesAnim.SetTrigger("Left");

        leftFootPos.GetComponent<Animator>().SetTrigger("Left");
        rightFootPos.GetComponent<Animator>().SetTrigger("Left");
    }
    #endregion

    private void updateCrouch()
    {
        rightLeg.topLeg.restRotation = crouchRestRotation * (facingRight ? -1 : 1);
        leftLeg.topLeg.restRotation = crouchRestRotation * (facingRight ? -1 : 1);
    }

    #region Walking
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
        leftLeg.ResetLeg(isCrouching, crouchRestRotation, facingRight);
        rightLeg.ResetLeg(isCrouching, crouchRestRotation, facingRight);
    }

    private IEnumerator walkCycle(bool startRight)
    {
        float distance = Vector2.Distance(new Vector2(0, Mathf.Abs(rb.velocity.x)), new Vector2(0, moveSpeed));
        if (startRight)
        {
            rightLeg.MoveUp(walkingRight, distance, rightFootOnGround, isCrouching, rightFootNormal, timeInAir, onSlope);
        }
        else
        {
            leftLeg.MoveUp(walkingRight, distance, leftFootOnGround, isCrouching, leftFootNormal, timeInAir, onSlope);
        }

        yield return new WaitForSeconds(0.5f);
        leftLeg.ResetLeg(isCrouching, crouchRestRotation, facingRight);
        rightLeg.ResetLeg(isCrouching, crouchRestRotation, facingRight);

        distance = Vector2.Distance(new Vector2(0, Mathf.Abs(rb.velocity.x)), new Vector2(0, moveSpeed));
        if (startRight)
        {
            leftLeg.MoveUp(walkingRight, distance, leftFootOnGround, isCrouching, leftFootNormal, timeInAir, onSlope);
        }
        else
        {
            rightLeg.MoveUp(walkingRight, distance, rightFootOnGround, isCrouching, rightFootNormal, timeInAir, onSlope);
        }

        yield return new WaitForSeconds(0.5f);
        leftLeg.ResetLeg(isCrouching, crouchRestRotation, facingRight);
        rightLeg.ResetLeg(isCrouching, crouchRestRotation, facingRight);

        walkCoroutine = StartCoroutine(walkCycle(walkingRight));
    }
    #endregion

    #region Ragdoll
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
    #endregion

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

    public void MoveUp(bool walkingRight, float value, bool legOnGround, bool isCrouching, Vector2 normal, float timeInAir, bool onSlope)
    {
        if ((value < 4f || onSlope) && timeInAir < 0.5f)
        {
            float force = 20;
            topLeg.bone.AddForce(new Vector2(force * (normal.normalized.x * 2), force * (normal.normalized.x + 1)), ForceMode2D.Impulse);
        }

        topLeg.restRotation = 60 * (walkingRight ? 1 : -1);
        if (isCrouching)
        {
            topLeg.restRotation -= -35 * (walkingRight ? 1 : -1);
        }

        if (legOnGround)
        {
            topLeg.bone.AddTorque(10 * (walkingRight ? 1 : -1), ForceMode2D.Impulse);
        }
    }

    public void ResetLeg(bool isCrouching, float crouchRotation, bool facingRight)
    {
        if (isCrouching)
        {
            topLeg.restRotation = crouchRotation * (facingRight ? -1 : 1);
        }
        else
        {
            topLeg.restRotation = 0;
        }
    }
}

