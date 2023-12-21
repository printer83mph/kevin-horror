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

        private class WanderState : KevinState
        {
            private readonly KevinTarget[] _primeTargets = new KevinTarget[3];

            public WanderState(KevinAI ai) : base(ai)
            { }

            public override void OnStateStart()
            {
                AI._agent.speed = AI.walkSpeed;
                AI.StartCoroutine(UpdateTargetsLoop());
                AI.StartCoroutine(UpdateDestinationLoop());
            }

            public override void OnStateEnd()
            {
                AI.StopAllCoroutines();
            }

            public override void Update()
            {
                if (AI.inLight)
                    AI.SetState(new FleeState(AI, () => new WanderState(AI)));
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
            { AI.StopAllCoroutines(); }

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
    }
}
