using System.Collections.Generic;
using UnityEngine;

class AnimatorParamParam
{
    public int nameHash;
    public string name;
    public AnimatorControllerParameterType type;
    public int intValue;
    public bool boolValue;
    public float floatValue;
};

class AnimatorRecord
{
    public AnimatorStateInfo stateInfo;
    public AnimatorParamParam[] parameters;
    public bool isHoldingBall;
    public Quaternion direction;
    public Vector3 position;
    public void clear()
    {
        parameters = null;
    }
    
}

//每一个逻辑帧所有的Animator状态记录
class LogicFrameAnimatorRecord
{
    public Dictionary<uint, AnimatorRecord> recordActors = new Dictionary<uint, AnimatorRecord>();
    public Vector3 ballPosition;
}
