using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Kevin
{
    public class KevinAI : MonoBehaviour
    {
        private abstract class KevinState
        {
            protected readonly KevinAI AI;
            protected readonly KevinTarget[] Targets;

            protected KevinState(KevinAI ai)
            {
                AI = ai;
                Targets = FindObjectsOfType<KevinTarget>();
            }

            public virtual void OnStateEnd()
            { }
            
            public virtual void Update()
            { }

            public virtual void OnStateStart()
            { }
        }

        /**
         * Wanders around at nodes far away from player, fleeing if the light is on him.
         */
        private class WanderState : KevinState
        {
            private readonly KevinTarget[] _primeTargets = new KevinTarget[3];
            private float _aggressionLevel;

            public WanderState(KevinAI ai) : base(ai)
            { }

            public override void OnStateStart()
            {
                AI._agent.speed = AI.walkSpeed;
                _aggressionLevel = 0f;
                AI.StartCoroutine(UpdateTargetsLoop());
                AI.StartCoroutine(UpdateDestinationLoop());
            }

            public override void OnStateEnd()
            {
                AI.StopCoroutine(UpdateTargetsLoop());
                AI.StopCoroutine(UpdateDestinationLoop());
            }

            public override void Update()
            {
                if (AI.inLight)
                {
                    AI.SetState(new FleeState(AI, () => new WanderState(AI)));
                    return;
                }
                
                _aggressionLevel += Time.deltaTime * 0.05f * (Random.value + 0.5f);
                if (_aggressionLevel >= 1f)
                {
                    AI.SetState(new StalkState(AI, StalkState.AggressionLevel.GetNear));
                }
            }

            private IEnumerator UpdateTargetsLoop()
            {
                while (true)
                {
                    // set three targets (farthest from player)
                    Array.Sort(Targets, (a, b) =>
                    {
                        Transform camTransform;
                        return (int)Mathf.Sign(
                            (b.transform.position - (camTransform = AI._camera.transform).position).sqrMagnitude -
                            (a.transform.position - camTransform.position).sqrMagnitude);
                    });

                    for (var i = 0; i < 3; i++)
                        _primeTargets[i] = Targets[i];

                    yield return new WaitForSeconds(1);
                }
                // ReSharper disable once IteratorNeverReturns
            }
            
            private IEnumerator UpdateDestinationLoop()
            {
                while (true)
                {
                    var target = Mathf.FloorToInt(Random.value * 3f);
                    AI._agent.SetDestination(_primeTargets[target].transform.position);

                    yield return new WaitUntil(() => AI._agent.remainingDistance < 0.1f);
                    yield return new WaitForSeconds(Random.Range(4.5f, 10f));
                }
                // ReSharper disable once IteratorNeverReturns
            }
        }

        /**
         * Flees to the node farthest from the player, then switches to a supplied next state.
         */
        private class FleeState : KevinState
        {
            private readonly Func<KevinState> _nextState;

            public FleeState(KevinAI ai, Func<KevinState> nextState) : base(ai)
            { _nextState = nextState; }

            public override void OnStateStart()
            {
                AI._agent.speed = AI.runSpeed;
                AI.StartCoroutine(FleeRoutine());
            }
            
            public override void OnStateEnd()
            { AI.StopCoroutine(FleeRoutine()); }

            private IEnumerator FleeRoutine()
            {
                var maxTarget = Targets[0];
                {
                    var maxSqrDistance = 0f;
                    foreach (var target in Targets)
                    {
                        var distance = (target.transform.position - AI._camera.transform.position).sqrMagnitude;

                        if (distance <= maxSqrDistance) continue;
                        maxTarget = target;
                        maxSqrDistance = distance;
                    }
                }

                AI._agent.SetDestination(maxTarget.transform.position);

                yield return new WaitUntil(() => AI._agent.remainingDistance < 0.1f);
                
                AI.SetState(_nextState());
            }
        }

        /**
         * Attempts to get somewhat close to player, with varying responses to flashlight usage depending
         * on aggression level. Eventually rushes player.
         */
        private class StalkState : KevinState
        {
            public enum AggressionLevel
            {
                GetNear = 1,
                GetNearAndStop = 2,
                GetNearAndPounce = 3
            }

            private AggressionLevel _aggression;
            private float _subAggressionLevel;

            public StalkState(KevinAI ai, AggressionLevel aggression) : base(ai)
            {
                _aggression = aggression;
            }

            public override void OnStateStart()
            {
                _subAggressionLevel = 0;
                AI._agent.speed = AI.walkSpeed;
                AI.StartCoroutine(Stalk());
            }

            public override void OnStateEnd()
            {
                AI._agent.isStopped = false;
                AI.StopCoroutine(Stalk());
            }
            
            public override void Update()
            {
                // always target player
                AI._agent.SetDestination(AI._camera.transform.position);
                
                // flee if low aggression
                if (_aggression <= AggressionLevel.GetNear)
                {
                    if (AI.inLight)
                    {
                        AI._agent.isStopped = false;
                        AI.SetState(new FleeState(AI, () => new StalkState(AI, 0)));
                        return;
                    }
                }

                // rotate towards player if stood still
                if (AI._agent.velocity.sqrMagnitude < AI.walkSpeed * AI.walkSpeed * 0.5f)
                {
                    var vecToCamera = AI._camera.transform.position - AI.transform.position;
                    var angleToCamera = Mathf.Atan2(-vecToCamera.z, vecToCamera.x);
                    var yAngle = Mathf.LerpAngle(AI.transform.rotation.eulerAngles.y, angleToCamera,
                        4f * Time.deltaTime);

                    AI.transform.rotation = Quaternion.AngleAxis(yAngle, Vector3.up);
                }

                // up aggression level if seeing player but not in light
                if (AI.canSeePlayer && !AI.inLight)
                {
                    _subAggressionLevel += Time.deltaTime * 0.05f * (Random.value + 0.5f);
                    if (_subAggressionLevel >= 1f)
                    {
                        // up aggression by 1
                        _aggression = _aggression switch
                        {
                            AggressionLevel.GetNear => AggressionLevel.GetNearAndStop,
                            _ => AggressionLevel.GetNearAndPounce
                        };
                        _subAggressionLevel = 0f;
                    }
                }
            }

            private IEnumerator Stalk()
            {
                while (true)
                {
                    var minDistance = _aggression switch
                    {
                        AggressionLevel.GetNearAndPounce => 8f,
                        AggressionLevel.GetNearAndStop => 10f,
                        _ => 13f
                    };

                    // wait until close enough
                    yield return new WaitUntil(() => AI.canSeePlayer && AI._agent.remainingDistance <= minDistance);
                    AI._agent.isStopped = true;
                    
                    // wait until we reach aggression level 3, then pounce!
                    yield return new WaitUntil(() => !AI.inLight && _aggression == AggressionLevel.GetNearAndPounce);
                    AI._agent.isStopped = false;
                    AI.SetState(new PounceState(AI));
                }
            }
        }

        /**
         * Runs at player like a maniac and kills them
         */
        private class PounceState : KevinState
        {
            public PounceState(KevinAI ai) : base(ai)
            { }

            public override void OnStateStart()
            {
                AI._agent.speed = AI.runSpeed;
            }

            public override void Update()
            {
                AI._agent.SetDestination(AI._camera.transform.position);

                var sqrDistanceToPlayer = (AI._camera.transform.position - AI.headTransform.position).sqrMagnitude;
                var maxLightDistance = 6f;
                if (AI.inLight && sqrDistanceToPlayer < maxLightDistance * maxLightDistance)
                {
                    AI.SetState(new FleeState(AI, () => new WanderState(AI)));
                    return;
                }
                
                // TODO: kill player
            }
        }
        
        // parameters
        [SerializeField] private float walkSpeed = 2f;
        [SerializeField] private float runSpeed = 4f;

        [SerializeField] private Transform headTransform;
        [SerializeField] private float flashlightAngleDeg = 30f;
        [SerializeField] private float flashlightMaxDistance = 10f;
        [SerializeField] private LayerMask flashlightLayerMask;

        public float WalkSpeed => walkSpeed;
        public float RunSpeed => runSpeed;

        // references
        private NavMeshAgent _agent;
        private Camera _camera;
        private Transform _flashlight;

        // internal logic
        private KevinState _state;

        private void SetState(KevinState newState)
        {
            _state?.OnStateEnd();
            _state = newState;
            newState.OnStateStart();
        }

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _flashlight = FindObjectOfType<FlashlightFakeBounceLight>().transform;
            _camera = Camera.main;
        }

        private void Start()
        {
            SetState(new WanderState(this));
        }

        private void Update()
        {
            _state.Update();
        }

        private bool inLight
        {
            get
            {
                var vecFromFlashlight = headTransform.position - _flashlight.transform.position;

                if (Vector3.Dot(vecFromFlashlight.normalized, _flashlight.forward) <
                    Mathf.Cos(Mathf.Deg2Rad * flashlightAngleDeg))
                    return false;

                if (vecFromFlashlight.sqrMagnitude > flashlightMaxDistance * flashlightMaxDistance)
                    return false;

                var flashlightPos = _flashlight.position;
                var flashlightForward = _flashlight.forward;
                
                var ray = new Ray(flashlightPos + flashlightForward * 0.1f, flashlightForward);
                var maxRaycastDistance = (flashlightPos - headTransform.position).magnitude - 0.4f;
                return !Physics.Raycast(ray, maxRaycastDistance, flashlightLayerMask);
            }
        }

        private bool canSeePlayer
        {
            get
            {
                var vecToPlayer = _camera.transform.position - headTransform.position;
                var ray = new Ray(headTransform.position, vecToPlayer);
                var maxRaycastDistance = vecToPlayer.magnitude - 0.4f;
                return !Physics.Raycast(ray, maxRaycastDistance, flashlightLayerMask);
            }
        }
    }
}
