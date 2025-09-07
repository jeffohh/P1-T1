using System.Collections;
using UnityEngine;

public class UIAnimationController : MonoBehaviour
{
    public Animator animator;

    void Start()
    {
        StartCoroutine(PlayIntroThenIdle());
    }

    IEnumerator PlayIntroThenIdle()
    {
        animator.Play("MainMenuIntro");

        // Wait for the intro animation to finish
        float introLength = GetClipLength("MainMenuIntro");
        yield return new WaitForSeconds(introLength);

        // Play the idle animation
        animator.Play("MainMenuIdle");
    }

    float GetClipLength(string clipName)
    {
        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
                return clip.length;
        }

        Debug.LogWarning("Clip not found: " + clipName);
        return 0f;
    }
}
