using UnityEngine;

public class AnimationPlayer : MonoBehaviour
{
    private Animation _animation;

    private void Awake()
    {
        _animation = GetComponent<Animation>();
    }

    public void Play()
    {
        _animation.Play();
    }
}
