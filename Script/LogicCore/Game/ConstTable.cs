using FixMath.NET;
//
public class ConstTable
{
    //������ʱ�䣬��Ϊ�ǳ���
    public static readonly Fix64 LongPassBallPressTime = (Fix64)0.15f;

    //�������ŵĴ���ʱ��
    public static readonly Fix64 HardShootPressTime = (Fix64)0.5f;
    //�������ŵĴ���ʱ��
    public static readonly Fix64 SuperShootPressTime = (Fix64)2.0f;

    //���� 0Ϊ���ٽӴ��� 1Ϊ���ٽӴ���
    public static readonly Fix64[] GetShortPasingBall_K1 = new Fix64[] { (Fix64)1f, (Fix64)1f };
	public static readonly Fix64[] GetShortPasingBall_K2 = new Fix64[] { (Fix64)7.333f, (Fix64)7.333f };
	public static readonly Fix64[] GetShortPasingBall_K3 = new Fix64[] { (Fix64)0.0322f, (Fix64)0.0322f };
    public static readonly Fix64[] GetShortPasingBall_K4 = new Fix64[] { (Fix64)0.5f, (Fix64)0.5f };

	public static readonly Fix64[] GetLongPasingBall_K1 = new Fix64[] { (Fix64)260.0f, (Fix64)260.0f };
	public static readonly Fix64[] GetLongPasingBall_K2 = new Fix64[] { (Fix64)0.0045f, (Fix64)0.0045f };

    //�����ܽ���ĸ���
    public static readonly Fix64[] ShootBall_GoalRate = new Fix64[] { (Fix64)0.0f, (Fix64)0.0f, (Fix64)0.0f, (Fix64)0.0f };
    //����Աʧ��ʱ��������
    public static readonly Fix64 GoallKeeperPretendGetBallMissDistance = (Fix64)(0.5f);

    //��Ա������ʱ�Ĳ����ٶ�
    public static readonly Fix64 ActorAttackedSlowPlaySpeed = (Fix64)0.2f;
    //�ָ�1.0����Ҫ��ʱ��
    public static readonly Fix64 ActorAttackedResetTime = (Fix64)1.0f;
    //����ͷ����ʱ��
    public static readonly Fix64 ActorAttackedSlowPlayTime = (Fix64)1.0f;



    //���ID
    public static readonly uint ballID = 0xFF;

    //���Բ���
    public static readonly uint DebugStateAction = 0; //0��Ч���� 1������ʳ 2����״̬ 3���ܵ���
}