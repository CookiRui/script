using UnityEngine;

public class AnimatorStateCheck : StateMachineBehaviour {


    bool isState(string stateName, AnimatorStateInfo stateInfo)
    {
        int state = Animator.StringToHash(stateName);
        return state == stateInfo.fullPathHash;
    }

    float getPlayingTimeCursor(AnimatorStateInfo stateInfo)
    {
        return stateInfo.normalizedTime - (int)stateInfo.normalizedTime;
    }

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
    }

    //idleRandom为0时的动画比例
    public float normalIdlePercent = 0.5f;
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

		if (isState("Base Layer.run_free", stateInfo))
        {
            animator.SetFloat("runOffset", getPlayingTimeCursor(stateInfo));
        }
		else if (isState("Base Layer.idle_free", stateInfo)||isState("Base Layer.idle_holding", stateInfo))
        {
            if ((getPlayingTimeCursor(stateInfo) + 0.01f) > 1.0f)
            {

                float rand = Random.Range(0, 100) > (normalIdlePercent * 100) ? 1.0f : 0.0f;
                animator.SetFloat("idleRandom", rand);
            }
        }
       
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
		if (isState("Base Layer.run_holding", stateInfo))
        {
            animator.SetFloat("runOffset", 0);
        }
    }

    //public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    //{
    //    if (isState("runAndBrake_holding", stateMachinePathHash))
    //    {
    //        Debug.Log("runAndBrake_holding statemachine Enter");
    //    }
    //}
    //public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash, AnimatorControllerPlayable controller)
    //{
    //    if (isState("runAndBrake_holding", stateMachinePathHash))
    //    {
    //        Debug.Log("runAndBrake_holding statemachine Enter");
    //    }
    //}
    //public override void OnStateMachineExit(Animator animator, int stateMachinePathHash)
    //{

    //}
  
}
