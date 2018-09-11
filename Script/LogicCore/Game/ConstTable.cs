using FixMath.NET;
//
public class ConstTable
{
    //超过此时间，认为是长传
    public static readonly Fix64 LongPassBallPressTime = (Fix64)0.15f;

    //大力射门的触发时间
    public static readonly Fix64 HardShootPressTime = (Fix64)0.5f;
    //超级射门的触发时间
    public static readonly Fix64 SuperShootPressTime = (Fix64)2.0f;

    //索引 0为慢速接传球 1为快速接传球
    public static readonly Fix64[] GetShortPasingBall_K1 = new Fix64[] { (Fix64)1f, (Fix64)1f };
	public static readonly Fix64[] GetShortPasingBall_K2 = new Fix64[] { (Fix64)7.333f, (Fix64)7.333f };
	public static readonly Fix64[] GetShortPasingBall_K3 = new Fix64[] { (Fix64)0.0322f, (Fix64)0.0322f };
    public static readonly Fix64[] GetShortPasingBall_K4 = new Fix64[] { (Fix64)0.5f, (Fix64)0.5f };

	public static readonly Fix64[] GetLongPasingBall_K1 = new Fix64[] { (Fix64)260.0f, (Fix64)260.0f };
	public static readonly Fix64[] GetLongPasingBall_K2 = new Fix64[] { (Fix64)0.0045f, (Fix64)0.0045f };

    //射门能进球的概率
    public static readonly Fix64[] ShootBall_GoalRate = new Fix64[] { (Fix64)0.0f, (Fix64)0.0f, (Fix64)0.0f, (Fix64)0.0f };
    //守门员失误时的误差距离
    public static readonly Fix64 GoallKeeperPretendGetBallMissDistance = (Fix64)(0.5f);

    //球员被攻击时的播放速度
    public static readonly Fix64 ActorAttackedSlowPlaySpeed = (Fix64)0.2f;
    //恢复1.0所需要的时间
    public static readonly Fix64 ActorAttackedResetTime = (Fix64)1.0f;
    //慢镜头持续时间
    public static readonly Fix64 ActorAttackedSlowPlayTime = (Fix64)1.0f;



    //球的ID
    public static readonly uint ballID = 0xFF;

    //调试参数
    public static readonly uint DebugStateAction = 0; //0无效果， 1饿虎捕食 2防御状态 3技能调试
}