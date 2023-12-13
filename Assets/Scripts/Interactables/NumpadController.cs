using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Interactables
{
    public class NumpadController : MonoBehaviour
    {
        [SerializeField] private XRSimpleInteractable[] buttons;

        private void Update()
        {
            // TODO: move button if being pressed, add to text, trigger event if everything typed in (correctly or incorrectly)
        }
    }
}
