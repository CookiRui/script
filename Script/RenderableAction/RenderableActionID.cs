//渲染对象的类型ID，ID只能往后加，不能修改
public enum RenderableActionID
{
    None = 0,

    #region Game Actions

    CreateWorldAction = 1,
    GameReadyAction = 3,
    GameBeginAction = 4,
    GoalAction = 5,
    ReplayBeginAction = 6,
    GameOverAction = 7,
    MainActorCreatedAction = 8,
    ProfilerAction = 10,
    ReplayEndAction = 11,

    #endregion

    #region Actor Actions

    CreateActorAction = 30,
    ActorMovingAction = 31,
    RotateAction = 32,
    TurningAction = 33,
    IdleAction = 34,
    RunAction = 35,
    OtherAction = 36,
    TurnAction = 37,
    DribbleAction = 38,
    FallAction = 39,
    PassBeginAction = 40,
    PassEndAction = 41,
    ShootBeginAction = 42,
    ShootBallReadyAction = 43,
    ChangeActorAnimatorSpeedAction = 44,
    SlideAction = 45,
    MovingToIdleAction = 46,
    ActorStandCatchingBallAction = 47,
    ActorStandCatchingBallBeginAction = 48,
    ActorAirCatchingBallAction = 49,
    ActorAirCatchingBallBeginAction = 50,

    ActorTigerCatchingBallBeginAction = 53,
    ActorMovingAction3D = 54,
    DoorKeeperCatchingBallAction = 55,
    DoorKeeperBeginToGetupAction = 56,
    DoorKeeperCatchingBallBeginAction = 57,
    SlowGetPassingBallAction = 58,
    ActorEndMoveAction = 59,
    WarmUpAction = 60,
    TauntAction = 61,
    CheerStandAction = 62,
    CheerUniqueAction = 63,
    DismayAction = 64,
    BeginHitAction = 65,
    EndHitAction = 66,
    HitCompletedAction = 67,

    #endregion

    #region Ball Actions
    BallMovingAction = 150,
    InstantBallMovingAction = 151,
    BallAttachAction = 152,
    BallDetachAction = 153,
    BallCollidedWallAction = 154,
    BallLandedAction = 155,
    BallPassAction = 156,
    BallShootAction = 157,
    CreateBallAction = 158,
    BallMovingPhysicalAction = 159,
    BallCollidedNetAction = 160,
    BallEnergyLevelChangedAction = 161,
    BallSlerpMoveAction = 162,
    BallLastDetachAction = 163,

    #endregion

    #region Animator Actions
    AnimatorBoolAction = 194,
    AnimatorTriggerAction = 195,
    AnimatorFloatAction = 196,
    AnimatorIntAction = 197,
    AnimatorScaleAction = 198,
    #endregion

    #region UI Actions

    AskBallAction = 200,
    RidiculeAction = 203,
    SettlementAction = 204,
    ShowOffAction = 206,
    UpdateCountdownAction = 207,
    UpdateMatchTimeAction = 208,
    UpdateScoreAction = 209,
    ShowReplayLogoAction = 210,
    GameInitAction = 211,

    #endregion
}