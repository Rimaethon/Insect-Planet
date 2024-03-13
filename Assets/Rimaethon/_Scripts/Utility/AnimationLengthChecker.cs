using UnityEngine;

public class AnimationLengthChecker : MonoBehaviour
{
    [SerializeField] private Animator anim;

    void Update()
    {
        Debug.Log((anim.GetCurrentAnimatorStateInfo(0).normalizedTime%anim.GetCurrentAnimatorStateInfo(0).length)/Time.deltaTime);
    }
}
