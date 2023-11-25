
using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

interface IControllable
{
    public void ChangeState(EnemyState state);
}
public enum EnemyState
{
    Patrolling,
    Chasing,
    Attacking
}

[RequireComponent(typeof(Animator), typeof(NavMeshAgent))]
public class EnemyControllerBase : MonoBehaviour, IControllable    
{
    private NavMeshAgent _agent;
    private Animator _animator;
    private Transform _target;
    public float moveSpeed = 2.0f;
    public bool canMove = true;
    public Transform target;
    [SerializeField] private float maximumAttackRange = 5.0f;
    [SerializeField] private bool doesAttack;
    [SerializeField] private bool lineOfSightToAttack = true;
    [SerializeField] protected bool moveWhileAttacking;
    [SerializeField] protected bool needsLineOfSightToMove = true;
    [SerializeField] private EnemyState enemyState = EnemyState.Patrolling;
    [SerializeField] private GameObject attackEffect;
    private int idleAnimationHash=Animator.StringToHash("Idle");
    private int walkAnimationHash=Animator.StringToHash("Walk");
    private int attackAnimationHash=Animator.StringToHash("Attack");
    private Rigidbody _enemyRigidbody;
    private bool isAttacking;
    private bool isIdle;
    private bool isMoving;
    [SerializeField]private Vector3[] _patrolPoints=new Vector3[3];
    
    private CancellationTokenSource _cancellationTokenSource;
    
    private void Awake()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        Initialize();
        FillArrayWithRandomNavmeshLocations(_patrolPoints,10);
        ChangeState(EnemyState.Patrolling);
    }
    
    private void Initialize()
    {
        _animator = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();
    }
    
    public void ChangeState(EnemyState state)
    {
        enemyState = state;
        StopAllCoroutines();
        switch (enemyState)
        {
            case EnemyState.Patrolling:
                Patrol().Forget();
                break;
            case EnemyState.Chasing:
                Chase().Forget();
                break;
            case EnemyState.Attacking:
                Attack().Forget();
                break;
        }
    }


    bool IsPlayerInRange()
    {
        return Vector3.Distance(transform.position, target.position) < maximumAttackRange;
    }

  
    private int _currentPatrolIndex = 0;

   
    private void OnDisable()
    {
        _cancellationTokenSource.Cancel();
    }

    async UniTaskVoid Patrol()
    {
        ChangeAnimationState(walkAnimationHash);

        while (true)
        {
            Vector3 targetPosition = _patrolPoints[_currentPatrolIndex];
            while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
            {
                _agent.SetDestination(targetPosition);
                await UniTask.Yield(_cancellationTokenSource.Token);
            }

            _currentPatrolIndex = (_currentPatrolIndex + 1) % _patrolPoints.Length;
            ChangeAnimationState(walkAnimationHash);

            if (IsPlayerInRange())
            {
                break;
            }
        }
    }
    async UniTaskVoid Chase()
    {
        _agent.destination = target.position;
        ChangeAnimationState(walkAnimationHash);
        await UniTask.WaitUntil(IsPlayerInRange, cancellationToken: _cancellationTokenSource.Token);
        ChangeState(!IsPlayerInRange() ? EnemyState.Attacking : EnemyState.Patrolling);
    }

    async UniTaskVoid Attack()
    {
        ChangeAnimationState(attackAnimationHash);
        await UniTask.WaitUntil(IsPlayerInRange, cancellationToken: _cancellationTokenSource.Token);
    }

    async UniTaskVoid MoveTo(Vector3 position)
    {
        _agent.destination = position;
        await UniTask.WaitUntil(IsPlayerInRange, cancellationToken: _cancellationTokenSource.Token);
    }
    private void FillArrayWithRandomNavmeshLocations(Vector3[] points, float radius)
    {
        for (int i = 0; i < points.Length; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere * radius;
            randomDirection += transform.position;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1))
            {
                points[i] = hit.position;
            }
        }
    }
    private void ChangeAnimationState(int animationHash)
    {
        _animator.SetBool(idleAnimationHash, false);
        _animator.SetBool(walkAnimationHash, false);
        _animator.SetBool(attackAnimationHash, false);
        _animator.SetBool(animationHash, true);
    }
    
   
}