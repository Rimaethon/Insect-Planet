using UnityEngine;

/// <summary>
/// Base class for enemies
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("The base movement speed of the enemy.")]
    public float moveSpeed = 2.0f;
    [Tooltip("Whether the enemy can move.")]
    public bool canMove = true;
    // The rigidbody to be used to move this enemy
    protected Rigidbody enemyRigidbody = null;

    [Header("Behavior Settings")]
    [Tooltip("The target to move, aim, and/or shoot at.")]
    public Transform target = null;
    [Tooltip("The shooter component that this enemy will use to attack, if it attacks")]
    public EnemyAttacker attacker = null;
    [Tooltip("The maximum distance from the target that this enemy will begin to attack")]
    public float maximumAttackRange = 5.0f;
    [Tooltip("Whether or not this enemy will attack")]
    public bool doesAttack = false;
    [Tooltip("Whether this enemy requires line of sight to it's target to take action against it")]
    public bool lineOfSightToAttack = true;
    [Tooltip("Whether or not this enemy will move while it attacks")]
    public bool moveWhileAttacking = false;
    [Tooltip("Whether or not this enemy needs line of sight to move")]
    public bool needsLineOfSightToMove = true;

    /// <summary>
    /// Enum to help track the action state of this enemy
    /// </summary>
    public enum ActionState { Idle, Moving, Attacking }

    [Tooltip("The state of this enemy with respect to actions it can perform.")]
    public ActionState actionState = ActionState.Idle;

    [Header("Effects")]
    [Tooltip("The effect to create when attacking")]
    public GameObject attackEffect;

    // booleans to track whether the enemy is in a certain state before setting the action state which changes the animation
    protected bool isIdle = false;
    protected bool isAttacking = false;
    protected bool isMoving = false;


    /// <summary>
    /// Description:
    /// Standard Unity function called once before the first Update call
    /// Input: 
    /// none
    /// Return: 
    /// void (no return)
    /// </summary>
    private void Start()
    {
        // When this script starts up, set it up
        Setup();
    }

    /// <summary>
    /// Description:
    /// Standard Unity function called once every frame
    /// Input: 
    /// none
    /// Return: 
    /// void (no return)
    /// </summary>
    private void LateUpdate()
    {
        // Every frame, determine the correct movement, rotation, actions, etc. and cause the enemy to act accordingly
        HandleMovement();
        HandleActions();
        HandleAnimation();
    }

    /// <summary>
    /// Description:
    /// Sets up this enemy component
    /// Input: 
    /// none
    /// Return: 
    /// void (no return)
    /// </summary>
    protected virtual void Setup()
    {
        enemyRigidbody = GetComponent<Rigidbody>();
        if (attacker == null)
        {
            attacker = GetComponent<EnemyAttacker>();
        }
        if (target == null && GameObject.FindGameObjectWithTag("Player") != null)
        {
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }
        SetUpAnimator();
    }

    /// <summary>
    /// Description:
    /// Handles the desired movement of this enemy
    /// Input: 
    /// none
    /// Return: 
    /// void (no return)
    /// </summary>
    protected virtual void HandleMovement()
    {
        // If move while attack is set to true, we can move while attacking. Otherwise we just need to check isAttacking for
        // whether or not we move
        bool attackMove = moveWhileAttacking == true || isAttacking == false;

        bool hasLineOfSight = true;

        if (needsLineOfSightToMove)
        {
            hasLineOfSight = HasLineOfSight();
        }

        if (canMove && enemyRigidbody != null && attackMove && hasLineOfSight)
        {
            Vector3 desiredMovement = CalculateDesiredMovement();
            Quaternion desiredRotation = CalculateDesiredRotation();

            enemyRigidbody.velocity = Vector3.zero;
            enemyRigidbody.angularVelocity = Vector3.zero;

            enemyRigidbody.MovePosition(desiredMovement);
            enemyRigidbody.MoveRotation(desiredRotation);

            isMoving = true;
        }
        else if (isAttacking)
        {
            isMoving = false;
            isIdle = false;
        }
        // if we are not attacking or moving we are idle
        else
        {
            if (enemyRigidbody != null)
            {
                enemyRigidbody.velocity = Vector3.zero;
                enemyRigidbody.angularVelocity = Vector3.zero;
            }

            isMoving = false;
            isIdle = true;
        }
    }

    /// <summary>
    /// Description:
    /// Handles the desired actions of this enemy
    /// Input: 
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    protected virtual void HandleActions()
    {
        TryToAttack();
    }

    /// <summary>
    /// Description:
    /// Tries to attack using references to an attacker script
    /// Input: 
    /// none
    /// Return: 
    /// void (no return)
    /// </summary>
    protected virtual void TryToAttack()
    {
        if (doesAttack && attacker != null && target != null && (target.position - transform.position).magnitude < maximumAttackRange)
        {
            if (!lineOfSightToAttack || (lineOfSightToAttack && HasLineOfSight()))
            {
                isAttacking = attacker.Attack();
                if (isAttacking && attackEffect != null)
                {
                    Instantiate(attackEffect, transform.position, Quaternion.identity, null);
                }
            }
            else
            {
                isAttacking = false;
            }
        }
        else
        {
            isAttacking = false;
        }
    }

    /// <summary>
    /// Descriptions:
    /// Calculates the movement that this enemy should make.
    /// Made virtual to be overridden by inheriting classes
    /// Input: none
    /// Return: 
    /// Vector3
    /// </summary>
    /// <returns>Vector3: The desired movement of this enemy</returns>
    protected virtual Vector3 CalculateDesiredMovement()
    {
        return transform.position;
    }

    /// <summary>
    /// Description:
    /// Calculates the rotation that this enemy should rotate to
    /// Made virtual to be overridden by inheriting classes
    /// Input:
    /// none
    /// Return: 
    /// Quaternion
    /// </summary>
    /// <returns>Quaternion: The desired rotation of this enemy</returns>
    protected virtual Quaternion CalculateDesiredRotation()
    {
        return transform.rotation;
    }

    [Header("Animation")]
    [Tooltip("The animator which plays back this enemies animations")]
    public Animator animator;
    [Tooltip("The name of the boolean in the animator that moves to the idle state")]
    public string idleBooleanName;
    [Tooltip("The name of the boolean in the animator that moves to the moving state")]
    public string moveBooleanName;
    [Tooltip("The name of the boolean in the animator that moves to the attacking state")]
    public string attackBooleanName;

    // Whether or not the attached animator has an idle boolean
    bool hasIdleBoolean = false;
    // Whether or not the attached animator has an attack boolean
    bool hasAttackBoolean = false;
    // Whether or not the attached animator has a moving boolean
    bool hasMovingBoolean = false;

    /// <summary>
    /// Description:
    /// Handles determination of what animation booleans to set
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    protected void HandleAnimation()
    {
        if (isAttacking)
        {
            SetState(ActionState.Attacking);
        }
        else if (isMoving)
        {
            SetState(ActionState.Moving);
        }
        else if (isIdle)
        {
            SetState(ActionState.Idle);
        }
    }

    /// <summary>
    /// Description:
    /// Sets the state of this enemy and sets the animator accordingly
    /// Input: 
    /// none
    /// Return: 
    /// void (no return)
    /// </summary>
    protected void SetState(ActionState action)
    {
        actionState = action;
        if (animator != null)
        {
            if (hasIdleBoolean)
            {
                if (actionState == ActionState.Idle)
                {
                    animator.SetBool(idleBooleanName, true);
                }
                else
                {
                    animator.SetBool(idleBooleanName, false);
                }
            }

            if (hasMovingBoolean)
            {
                if (actionState == ActionState.Moving)
                {
                    animator.SetBool(moveBooleanName, true);
                }
                else
                {
                    animator.SetBool(moveBooleanName, false);
                }
            }

            if (hasAttackBoolean)
            {
                if (actionState == ActionState.Attacking)
                {
                    animator.SetBool(attackBooleanName, true);
                }
                else
                {
                    animator.SetBool(attackBooleanName, false);
                }
            }
        }
    }

    /// <summary>
    /// Description:
    /// Sets up the animator for this enemy if it has one
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    void SetUpAnimator()
    {
        if (animator != null)
        {
            if (ContainsParam(animator, idleBooleanName, AnimatorControllerParameterType.Bool))
            {
                hasIdleBoolean = true;
            }
            else if (idleBooleanName != "")
            {
                Debug.LogWarning("Enemy: " + name + " does not have an idle boolean by the name: " + idleBooleanName + "\n"
                    + "Make sure that the name on the script matches the parameter name in the animator");
            }

            if (ContainsParam(animator, moveBooleanName, AnimatorControllerParameterType.Bool))
            {
                hasMovingBoolean = true;
            }
            else if (moveBooleanName != "")
            {
                Debug.LogWarning("Enemy: " + name + " does not have a move boolean by the name: " + moveBooleanName + "\n"
                    + "Make sure that the name on the script matches the parameter name in the animator");
            }

            if (ContainsParam(animator, attackBooleanName, AnimatorControllerParameterType.Bool))
            {
                hasAttackBoolean = true;
            }
            else if (attackBooleanName != "")
            {
                Debug.LogWarning("Enemy: " + name + " does not have an attack boolean by the name: " + attackBooleanName + "\n"
                    + "Make sure that the name on the script matches the parameter name in the animator");
            }
        }
    }

    /// <summary>
    /// Description:
    /// Checks to see if Unity's animator has a parameter by a name or not
    /// Useful because Unity does not have an existing way to do this
    /// Obtained from: https://answers.unity.com/questions/571414/is-there-a-way-to-check-if-an-animatorcontroller-h.html
    /// By users: 
    /// https://answers.unity.com/users/273357/dev6-rc.html
    /// https://answers.unity.com/users/111582/thexdd.html
    /// Expanded to include a type check
    /// Input:
    /// Animator animator, string parameterName
    /// </summary>
    /// <param name="animator">The animator to look through</param>
    /// <param name="parameterName">The parameter name to look for</param>
    /// <returns>bool: Whether or not the parameter exists</returns>
    bool ContainsParam(Animator animator, string parameterName, AnimatorControllerParameterType type)
    {
        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.name == parameterName && type == parameter.type) return true;
        }
        return false;
    }

    [Header("Line of Sight Settings")]
    [Tooltip("The layers that can be hit when checking line of sight")]
    public LayerMask hitWithLineOfSight;

    /// <summary>
    /// Description:
    /// Checks if this enemy has line of sight to it's target
    /// Input: 
    /// none
    /// Return: 
    /// bool
    /// </summary>
    /// <returns>bool: Whether or not this enemy has line of sight to it's target.</returns>
    protected virtual bool HasLineOfSight()
    {
        if (target != null)
        {
            RaycastHit hit = new RaycastHit();
            Ray ray = new Ray(transform.position, target.position - transform.position);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, hitWithLineOfSight))
            {
                if (hit.transform == target || target.IsChildOf(hit.transform))
                {
                    return true;
                }
            }
        }
        return false;
    }
}
