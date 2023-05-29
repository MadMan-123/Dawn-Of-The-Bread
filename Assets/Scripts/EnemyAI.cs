using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;


//code written by Daniel Cunningham

//requires components
[RequireComponent(typeof(EnemyState))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{

    //Variables
    [SerializeField] WeaponHandler _handler;
    [SerializeField] Rigidbody2D RigidBodyCache;
    [SerializeField] private float fWalkWaitTime = 5f;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float fSpeed = 1.5f;
    [SerializeField] private float fAttackDistance = 2f;
    [SerializeField] private float fCheckDistance = 5f;

    //calls the enemy state script
    [SerializeField] private EnemyState _CurrentState;
    //checks if boss using editor.
    [SerializeField] private bool bIsBoss;
    //rigidbody
    private Rigidbody2D RigidbodyCache;

    //Direction
    private Vector2 Dir;
    //Ignores a layer
    public LayerMask IgnoreLayer;
    



    // Update is called once per frame

    private void Start()
    {
        //sets state
        _CurrentState.SetIdle();
        //checking what direction to move
        StartCoroutine(ChangeDir(0));

        //List of drag and gravity applied to enemy.
        if (TryGetComponent<Rigidbody2D>(out RigidbodyCache))
        {
            RigidBodyCache.gravityScale = 0;
            RigidBodyCache.angularDrag = 1;
            RigidBodyCache.drag = 8;
            
        }

        
        
    }
    void Update()
    {
        //check sphere
        Collider2D[] colliders = Physics2D.OverlapCircleAll(
            transform.position,
            fCheckDistance
        );


        //if player is in sphere start moving towards position until at certain distance 
        //attack 
        //todo: Clean up the nested if mess
        Collider2D PlayerCache = colliders.ToList().Find(x => x.CompareTag("Player"));
        if (PlayerCache)
        {
            _CurrentState.SetChase();
            if ((PlayerCache.transform.position - transform.position).sqrMagnitude < fAttackDistance * fAttackDistance)
            {
                _CurrentState.SetAttack();
                if ((PlayerCache.transform.position - transform.position).sqrMagnitude <
                    fAttackDistance / 2 * fAttackDistance / 2)
                {
                    Dir = Vector2.zero;
                }
            }
        }
        else
        {
            //if there is no player walk around
            _CurrentState.SetIdle();
        }


        //goes through the states and changes depending on what has been activated.
        switch (_CurrentState.GetState())
        {
            case State.Idle:
                IdleWalk();
                break;
            case State.Chasing:
                ChasePlayer(PlayerCache.transform.position);
                break;
            case State.Attacking:
                Attack(PlayerCache.transform.position);
                break;
        }


        rb.AddForce(Dir * (fSpeed * Time.deltaTime), ForceMode2D.Impulse);
        Debug.DrawRay(transform.position, Dir * 3f);


    }

    void IdleWalk()
    {
        //if something is in front of enemy then it moves away

        RaycastHit2D raycast = Physics2D.Raycast(transform.position, Dir, 3f, ~IgnoreLayer);
        if (raycast.collider && raycast.collider.transform != transform)
        {
            StartCoroutine(ChangeDir(fWalkWaitTime));
        }
    }

    private void ChasePlayer(Vector2 PlayerPos)
    {//chases the player around
        Dir = (PlayerPos - new Vector2(transform.position.x, transform.position.y)).normalized;
    }


    IEnumerator ChangeDir(float fWait)
    {
        Dir = Vector2.zero;
        //wait for x amount of time 
        yield return new WaitForSeconds(fWait);
        //pick random direction
        Vector2 vRandDir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        Dir = vRandDir;

    }
    void Attack(Vector2 PlayerPos)
    {
        //attacks transforming it's postion to be where the player is.
       _handler.GetCurrentWeapon().Dir = (PlayerPos - new Vector2(transform.position.x, transform.position.y)).normalized;
        _handler.HandleAttack(0);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, fCheckDistance);
    }
}
