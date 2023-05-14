using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuAnimationController : MonoBehaviour
{
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayButtonAnimate()
    {
        animator.SetBool("isPlay", !animator.GetBool("isPlay"));
    }

    public void PlayButtonReturn()
    {
        animator.SetBool("isPlay", false);
    }

    public void HelpButtonAnimate()
    {
        animator.SetBool("isHelp", !animator.GetBool("isHelp"));
    }

    public void HelpButtonReturn()
    {
        animator.SetBool("isHelp", false);
    }

    public void OptionsButtonAnimate()
    {
        animator.SetBool("isOptions", !animator.GetBool("isOptions"));
    }

    public void OptionsButtonReturn()
    {
        animator.SetBool("isOptions", false);
    }
}
