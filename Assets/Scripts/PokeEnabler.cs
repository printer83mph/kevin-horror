using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class PokeEnabler : MonoBehaviour
{
    [SerializeField] private InputActionReference triggerInputAction;
    [SerializeField] private InputActionReference gripInputAction;
    
    private XRPokeInteractor _pokeInteractor;

    private void Awake()
    {
        _pokeInteractor = GetComponent<XRPokeInteractor>();
    }

    private void Update()
    {
        var isPointing = triggerInputAction.action.ReadValue<float>() < 0.1f &&
                         gripInputAction.action.ReadValue<float>() > 0.9f;

        if (isPointing != _pokeInteractor.enabled)
            _pokeInteractor.enabled = isPointing;
    }
}
