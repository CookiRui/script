using UnityEngine;

public class AnimatorViewer : MonoBehaviour
{

    Animator animator;
    // Use this for initialization
    void Start()
    {
        animator = this.GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnGUI()
    {

        if (GUI.Button(new Rect(200, 0, 100, 50), "转身"))
        {
            animator.SetTrigger("turn");
        }
        if (GUI.Button(new Rect(200, 50, 100, 50), "跑步"))
        {
            animator.SetInteger("state", 1);
        }
        if (GUI.Button(new Rect(200, 150, 100, 50), "走路"))
        {
            animator.SetInteger("state", 0);
        }
        if (GUI.Button(new Rect(200, 200, 100, 50), "持球"))
        {
            animator.SetBool("keepingBall", true);
        }
        if (GUI.Button(new Rect(200, 250, 100, 50), "不持球"))
        {
            animator.SetBool("keepingBall", false);
        }

        if (GUI.Button(new Rect(200, 300, 100, 50), "传球"))
        {
            animator.SetBool("pass", true);
        }
        if (GUI.Button(new Rect(200, 350, 100, 50), "不传球"))
        {
            animator.SetBool("pass", false);
        }

        if (GUI.Button(new Rect(200, 400, 100, 50), "铲倒"))
        {
            animator.SetTrigger("fall");
        }
        if (GUI.Button(new Rect(200, 450, 100, 50), "盘带"))
        {
            animator.SetTrigger("dribble");
        }

        //if (GUI.Slider(new Rect(200, 0, 100, 50), 0, 100, 0, 100))
        //{
        //    animator.SetBool("movingSpeed", true);
        //}

    }
}
