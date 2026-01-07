using System.Collections;
using UnityEngine;

namespace Gameplay
{
    [RequireComponent(typeof(MovementController))]
    public class NPCMovement : MonoBehaviour
    {
        [Header("Wander Settings")]
        [SerializeField] private float wanderRange = 3f;
        [SerializeField] private float minWanderDistance = 1f;
        [SerializeField] private float minWaitTime = 2f;
        [SerializeField] private float maxWaitTime = 5f;
        [SerializeField] private float arrivalThreshold = 0.1f;

        [Header("Movement Settings")]
        [SerializeField] private float npcMoveSpeed = 3f;

        private MovementController _movementController;
        private NPC _npc;
        private Vector3 _startPosition;
        private Vector3 _targetPosition;
        private bool _isPaused;
        private bool _isMoving;
        private Coroutine _wanderCoroutine;
        private Coroutine _playerAcknowledgeCoroutine;

        private void Awake()
        {
            _movementController = GetComponent<MovementController>();
            _npc = GetComponent<NPC>();
            _startPosition = transform.position;

            ConfigureSpeed();
        }

        private void ConfigureSpeed()
        {
            if (_movementController != null)
            {
                _movementController.SetSpeed(npcMoveSpeed);
            }
        }

        private void OnEnable()
        {
            if (_npc != null)
            {
                _npc.OnInteractionStarted += HandleInteractionStarted;
                _npc.OnInteractionEnded += HandleInteractionEnded;
            }
            StartWandering();
        }

        private void OnDisable()
        {
            if (_npc != null)
            {
                _npc.OnInteractionStarted -= HandleInteractionStarted;
                _npc.OnInteractionEnded -= HandleInteractionEnded;
            }
            StopWandering();

            if (_playerAcknowledgeCoroutine != null)
            {
                StopCoroutine(_playerAcknowledgeCoroutine);
                _playerAcknowledgeCoroutine = null;
            }
        }

        private void HandleInteractionStarted()
        {
            _isPaused = true;
            _movementController.SetMoveInput(Vector2.zero);

            if (_playerAcknowledgeCoroutine != null)
            {
                StopCoroutine(_playerAcknowledgeCoroutine);
                _playerAcknowledgeCoroutine = null;
            }
        }

        private void HandleInteractionEnded()
        {
            _isPaused = false;
        }

        private void StartWandering()
        {
            if (_wanderCoroutine == null)
            {
                _wanderCoroutine = StartCoroutine(WanderCycle());
            }
        }

        private void StopWandering()
        {
            if (_wanderCoroutine != null)
            {
                StopCoroutine(_wanderCoroutine);
                _wanderCoroutine = null;
            }
            _movementController.SetMoveInput(Vector2.zero);
        }

        private IEnumerator WanderCycle()
        {
            yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));

            while (enabled)
            {
                if (_isPaused)
                {
                    yield return null;
                    continue;
                }

                _targetPosition = PickRandomTarget();
                yield return MoveToTarget();

                if (!_isPaused)
                {
                    var waitTime = Random.Range(minWaitTime, maxWaitTime);
                    yield return new WaitForSeconds(waitTime);
                }
            }
        }

        private Vector3 PickRandomTarget()
        {
            Vector3 newTarget;
            int attempts = 0;
            const int maxAttempts = 10;

            do
            {
                newTarget = _startPosition + (Vector3)(Random.insideUnitCircle * wanderRange);
                attempts++;
            }
            while (Vector3.Distance(transform.position, newTarget) < minWanderDistance && attempts < maxAttempts);

            return newTarget;
        }

        private Vector3 PickTargetAwayFrom(Vector3 avoidPosition)
        {
            var awayDirection = (transform.position - avoidPosition).normalized;
            var preferredDirection = (Vector2)awayDirection + Random.insideUnitCircle * 0.3f;

            var newTarget = _startPosition + (Vector3)(preferredDirection.normalized * wanderRange * Random.Range(0.5f, 1f));

            if (Vector3.Distance(_startPosition, newTarget) > wanderRange)
            {
                newTarget = _startPosition + (Vector3)(preferredDirection.normalized * wanderRange);
            }

            return newTarget;
        }

        private IEnumerator MoveToTarget()
        {
            _isMoving = true;

            while (_isMoving && !_isPaused)
            {
                var direction = (_targetPosition - transform.position);

                if (direction.magnitude <= arrivalThreshold)
                {
                    _isMoving = false;
                    _movementController.SetMoveInput(Vector2.zero);
                    yield break;
                }

                _movementController.SetMoveInput(direction.normalized);
                yield return null;
            }

            _movementController.SetMoveInput(Vector2.zero);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                if (_playerAcknowledgeCoroutine != null)
                {
                    StopCoroutine(_playerAcknowledgeCoroutine);
                }
                _playerAcknowledgeCoroutine = StartCoroutine(AcknowledgePlayer(collision.transform));
            }
            else if (_isMoving && !_isPaused)
            {
                _targetPosition = PickRandomTarget();
            }
        }

        private IEnumerator AcknowledgePlayer(Transform player)
        {
            var wasPaused = _isPaused;
            _isPaused = true;
            _movementController.SetMoveInput(Vector2.zero);

            var directionToPlayer = (player.position - transform.position).normalized;
            _movementController.SetLastMoveInput(directionToPlayer);

            yield return new WaitForSeconds(0.5f);

            if (!wasPaused)
            {
                _targetPosition = PickTargetAwayFrom(player.position);
                _isPaused = false;
            }

            _playerAcknowledgeCoroutine = null;
        }

        private void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            Gizmos.color = Color.yellow;
            var origin = Application.isPlaying ? _startPosition : transform.position;
            Gizmos.DrawWireSphere(origin, wanderRange);

            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawWireSphere(origin, minWanderDistance);

            if (Application.isPlaying && _isMoving)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, _targetPosition);
                Gizmos.DrawSphere(_targetPosition, 0.1f);
            }
#endif
        }
    }
}
