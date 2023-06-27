using UnityEngine;


public class Enemy : MonoBehaviour
{
    public float moveSpeed = 2.0f;
    public bool canMove = true;
    protected Rigidbody enemyRigidbody;

    
     public Transform target;
    [SerializeField] private  EnemyAttacker attacker;
    [SerializeField] private  float maximumAttackRange = 5.0f;
    [SerializeField] private bool doesAttack;
    [SerializeField] private bool lineOfSightToAttack = true;
    [SerializeField] protected bool moveWhileAttacking = false;
    [SerializeField] protected bool needsLineOfSightToMove = true;


    public enum ActionState { Idle, Moving, Attacking }

    [SerializeField] private ActionState actionState = ActionState.Idle;

    [SerializeField] private GameObject attackEffect;

    protected bool isIdle = false;
    protected bool isAttacking = false;
    protected bool isMoving = false;


  
    private void Start()
    {
        Setup();
    }


    private void LateUpdate()
    {
        HandleMovement();
        HandleActions();
        HandleAnimation();
    }

   
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

  
    protected virtual void HandleActions()
    {
        TryToAttack();
    }


    protected virtual void TryToAttack()
    {
        Debug.Log((target.position - transform.position).magnitude);
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

   
    protected virtual Vector3 CalculateDesiredMovement()
    {
        return transform.position;
    }

    protected virtual Quaternion CalculateDesiredRotation()
    {
        return transform.rotation;
    }

    public Animator animator;
    public string idleBooleanName;
    public string moveBooleanName;
    public string attackBooleanName;

    bool _hasIdleBoolean;
    bool _hasAttackBoolean;
    bool _hasMovingBoolean;

   
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


    private void SetState(ActionState action)
    {
        actionState = action;
        if (animator != null)
        {
            if (_hasIdleBoolean)
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

            if (_hasMovingBoolean)
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

            if (_hasAttackBoolean)
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

    void SetUpAnimator()
    {
        if (animator != null)
        {
            if (ContainsParam(animator, idleBooleanName, AnimatorControllerParameterType.Bool))
            {
                _hasIdleBoolean = true;
            }
            else if (idleBooleanName != "")
            {
                Debug.LogWarning("Enemy: " + name + " does not have an idle boolean by the name: " + idleBooleanName + "\n"
                    + "Make sure that the name on the script matches the parameter name in the animator");
            }

            if (ContainsParam(animator, moveBooleanName, AnimatorControllerParameterType.Bool))
            {
                _hasMovingBoolean = true;
            }
            else if (moveBooleanName != "")
            {
                Debug.LogWarning("Enemy: " + name + " does not have a move boolean by the name: " + moveBooleanName + "\n"
                    + "Make sure that the name on the script matches the parameter name in the animator");
            }

            if (ContainsParam(animator, attackBooleanName, AnimatorControllerParameterType.Bool))
            {
                _hasAttackBoolean = true;
            }
            else if (attackBooleanName != "")
            {
                Debug.LogWarning("Enemy: " + name + " does not have an attack boolean by the name: " + attackBooleanName + "\n"
                    + "Make sure that the name on the script matches the parameter name in the animator");
            }
        }
    }

   
    bool ContainsParam(Animator animator, string parameterName, AnimatorControllerParameterType type)
    {
        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.name == parameterName && type == parameter.type) return true;
        }
        return false;
    }

    public LayerMask hitWithLineOfSight;

   
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
