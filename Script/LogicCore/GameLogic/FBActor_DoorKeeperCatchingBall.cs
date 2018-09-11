
using FixMath.NET;
using BW31.SP2D;

public partial class FBActor
{
    FixVector2 m_cathingBallStateMovingVelocity;
    Fix64 m_cathingBallStateVerticleSpeed;
    Fix64 m_cathingBallStateAniWaitTime;
    Fix64 m_heightWhenFalling;
    Fix64 m_timeElapsedWhenFalling;
    Fix64 m_glidingDamping;


    class DoorKeeperCatchingBall : State
    {
        public static readonly State instance = new DoorKeeperCatchingBall();

        enum SubState
        {
            kBeforeCatching,    
            kCatching,
            kBeforeFlyCathing,  //飞行模式下的Catching
            kFlyCatching,
            kBeforeFalling,     //下落前
            kFalling,           //下落
            kAfterFalling,      //下落后
            kAfterCatching,
        }

        public override bool canBreak(FBActor actor, State state)
        {
            return false;
        }

        //返回水平方向上的速度，并返回竖直方向的速度
        Fix64 _getActorVelocity(FBActor actor, Fix64 ajustTime, out FixVector2 moveSpeed )
        {
            Fix64 heightOffset = Fix64.Zero;
            FixVector2 moveTargetOffset = getMoveTargetOffset(actor, out heightOffset);

            Fix64 velocityValue = moveTargetOffset.length / ajustTime;

            moveSpeed = moveTargetOffset.normalized * velocityValue;
 
            //竖直方向速度
            Fix64 velocityValueVerticle = heightOffset / ajustTime;

            //Debuger.Log(
            //    "setActorVelocity moveTargetOffset:" + (UnityEngine.Vector2)moveTargetOffset
            //    + "actor should moveto :" + (UnityEngine.Vector2)(actor.getPosition() + moveTargetOffset)
                
            //    );

            return velocityValueVerticle;
        }

        public override void enter(FBActor actor)
        {
            Fix64 animationTime = actor.configuration.dkcb_animationCathingTime[actor.m_stateDataIndex];
            
            //足球飞行时间
            Fix64 ballFlyTime = actor.m_stateValue;

            Debuger.Log("DoorKeeperCathingBall Begin zoneIndex:" + actor.m_stateDataIndex);
            //a\b区的情况
            if (actor.m_stateDataIndex == 0 || actor.m_stateDataIndex == 1)
            {
                actor.m_stateInt = 1;//能拿到球
                if (animationTime < ballFlyTime)
                {
                    Debuger.Log("ab区 begin kBeforeCatching");
                    actor.m_stateSubState = (int)SubState.kBeforeCatching;
                    actor.m_timer = ballFlyTime - animationTime;
                }
                else
                {

                    Debuger.Log("ab区 begin kCatching");

                    //在BallFlyTime时间内移动到目标                    
                    _getActorVelocity(actor, ballFlyTime, out actor.m_cathingBallStateMovingVelocity);
                    actor.particle.dampingAcceleration = Fix64.Zero;

                    actor.m_stateSubState = (int)SubState.kCatching;
                    actor.m_timer = ballFlyTime;

                    //开始播放接球动画                
                    actor.world.onDoorKeeperCatchingBall(actor,
                        actor.m_stateDataIndex,
                        FixVector2.cross(actor.direction, actor.m_stateVector - actor.getPosition()) < Fix64.Zero);    

                }
            }
            else
            {                   
                Fix64 heightOffset = Fix64.Zero;
                FixVector2 moveTargetOffset = getMoveTargetOffset(actor, out heightOffset);
                Fix64 moveDistance = moveTargetOffset.length;

                Fix64 actorMoveTime = Fix64.Zero;
                if (actor.configuration.dkcb_cathingBallMovingVolocity[actor.m_stateDataIndex].x != Fix64.Zero)
                    actorMoveTime = moveDistance / actor.configuration.dkcb_cathingBallMovingVolocity[actor.m_stateDataIndex].x;

                Fix64 actorMoveTimeVerticle = Fix64.Zero;
                if (actor.configuration.dkcb_cathingBallMovingVolocity[actor.m_stateDataIndex].y != Fix64.Zero)
                    actorMoveTimeVerticle = heightOffset / actor.configuration.dkcb_cathingBallMovingVolocity[actor.m_stateDataIndex].y;

                Fix64 actorMoveTimeAjusted = actorMoveTime > actorMoveTimeVerticle ? actorMoveTime : actorMoveTimeVerticle;

                //不能拿到球
                if (actorMoveTimeAjusted > ballFlyTime)
                {
                    //直接移动
                    Fix64 verticleSpeed = _getActorVelocity(actor, actorMoveTimeAjusted, out actor.m_cathingBallStateMovingVelocity);
                    actor.particle.dampingAcceleration = Fix64.Zero;

                    actor.m_stateSubState = (int)SubState.kFlyCatching;
                    //腾空时垂直方向上的速度
                    actor.m_cathingBallStateVerticleSpeed = verticleSpeed;
                    actor.m_stateInt = 0;//不能拿到球
                    actor.m_timer = actorMoveTimeAjusted;

                    //拿球动画
                    actor.world.onDoorKeeperCatchingBall(
                        actor,
                        actor.m_stateDataIndex,
                        FixVector2.cross(actor.direction, actor.m_stateVector - actor.getPosition()) < Fix64.Zero);

                    Debuger.Log(" can not get ball begin kFlyCatching");


                }
                else
                {
                    //动画时间比较长，直接开始做动画，并且开始移动,把球弹开
                    if( animationTime > ballFlyTime )
                    {
                        Fix64 verticleSpeed = _getActorVelocity(actor, ballFlyTime, out actor.m_cathingBallStateMovingVelocity);
                        actor.particle.dampingAcceleration = Fix64.Zero;

                        actor.m_stateSubState = (int)SubState.kFlyCatching;
                        actor.m_cathingBallStateVerticleSpeed = verticleSpeed;
                        actor.m_stateInt = 2;//把球弹开
                        actor.m_timer = ballFlyTime;

                        //拿球动画
                        actor.world.onDoorKeeperCatchingBall(
                            actor,
                            actor.m_stateDataIndex,
                            FixVector2.cross(actor.direction, actor.m_stateVector - actor.getPosition()) < Fix64.Zero);


                        //Debuger.Log(" can colide ball begin kFlyCatching frameNumber: " + actor.world.world.frameCount);


                    }
                    else//动画时间比较短，接住球，先等待一段时间，然后播动画，做位移
                    {
                        //
                        Fix64 animationWaitTime = animationTime > actorMoveTimeAjusted ? animationTime : actorMoveTimeAjusted;
                        actor.m_stateSubState = (int)SubState.kBeforeFlyCathing;
                        actor.m_timer = ballFlyTime - animationWaitTime;
                        actor.m_cathingBallStateAniWaitTime = animationWaitTime;
                        Debuger.Log(" can get ball kBeforeFlyCathing");
                    }
                }
            }

            actor.world.onDoorKeeperCatchingBallReady(actor, actor.m_stateInt == 1);
        }

        public override void update(FBActor actor, Fix64 deltaTime)
        {
            switch ((SubState)actor.m_stateSubState)
            {
                case SubState.kBeforeCatching:
                    _beforeCatching(actor, deltaTime);
                    break;
                case SubState.kBeforeFlyCathing:
                    _beforeFlyCatching(actor, deltaTime);
                    break;
                case SubState.kFlyCatching:
                    _flyCatching(actor, deltaTime);
                    break;
                case SubState.kBeforeFalling:
                    _beforeFalling(actor, deltaTime);
                    break;
                case SubState.kFalling:
                    _falling(actor, deltaTime);
                    break;
                case SubState.kAfterFalling:
                    _afterFalling(actor, deltaTime);
                    break;
                case SubState.kCatching:
                    _Catching(actor, deltaTime);
                    break;
                case SubState.kAfterCatching:
                    _afterCatching(actor, deltaTime);
                    break;
            }
        }
        void _Catching(FBActor actor, Fix64 deltaTime)
        {
            actor.m_timer -= deltaTime;

            if (actor.m_timer <= Fix64.Zero)
            {
                //Debuger.Log("_Catching end frameNumber:" + actor.world.world.frameCount);
                actor.particle.velocity = FixVector2.kZero;
                actor.m_stateSubState = (int)SubState.kAfterCatching;
                actor.m_timer = actor.m_configuration.dkcb_afterCathingWaitingTime[actor.m_stateDataIndex];

                //能拿到球的情况下在球的运动差值完成后冻结球
                actor.world.ball.willBeFreezed();
                actor.world.ball.transferTarget = actor;

                //播放球员起身动画
                actor.world.onDoorKeeperBeginToGetup(actor);
                return;
            }

            actor.particle.velocity = actor.m_cathingBallStateMovingVelocity;

        }

        void _flyCatching(FBActor actor, Fix64 deltaTime)
        {
            actor.m_timer -= deltaTime;

            actor.particle.velocity = actor.m_cathingBallStateMovingVelocity;
            actor.height += actor.m_cathingBallStateVerticleSpeed * deltaTime;

            if (actor.m_timer <= Fix64.Zero)
            {
                //Debuger.Log("_flyCatching end frameNumber:" + actor.world.world.frameCount);
                if (actor.m_stateInt == 0)//接不到球
                {
                    //Debuger.Log("flyCathing can not get ball ");
                }
                else if (actor.m_stateInt == 1)//接到球
                {
                    //Debuger.Log("_flyCatching get ball");
                    //能拿到球的情况下在球的运动差值完成后冻结球
                    actor.world.ball.willBeFreezed();
                    actor.world.ball.transferTarget = actor;
                }
                else if (actor.m_stateInt == 2)//弹开
                {
                    //Debuger.Log("_flyCatching will collide ball");
                    actor.world.ball.collide(actor.direction, actor.world.config.ballCollisionRestitution_actorAndball);
                }
                if (actor.m_stateInt == 1)
                {
                    actor.m_stateSubState = (int)SubState.kBeforeFalling;
                    actor.m_timer = actor.configuration.flyCatchingBallFreezingTime;
                    //Debuger.Log("before Falling: needtime:" + (float)actor.m_timer);
                }
                else
                {
                    actor.m_stateSubState = (int)SubState.kFalling;
                    actor.m_heightWhenFalling = actor.height;
                    actor.m_timeElapsedWhenFalling = Fix64.Zero;

                    Fix64 time2 = new Fix64(2) * actor.height / actor.configuration.fallingAcceleration;
                    if (time2 < Fix64.Zero)
                    {
                        time2 = Fix64.Zero;
                    }
                    actor.m_timer = Fix64.Sqrt(time2);
                    //Debuger.Log("begin Falling: needtime:" + (float)actor.m_timer);
                }

                return;
            }
        }

        //开始坠落
        void _beforeFalling(FBActor actor, Fix64 deltaTime)
        {
            actor.m_timer -= deltaTime;
            if (actor.m_timer <= Fix64.Zero)
            {
                actor.m_stateSubState = (int)SubState.kFalling;
                actor.m_heightWhenFalling = actor.height;
                actor.m_timeElapsedWhenFalling = Fix64.Zero;

                Fix64 time2 = new Fix64(2) * actor.height / actor.configuration.fallingAcceleration;
                if (time2 < Fix64.Zero)
                {
                    time2 = Fix64.Zero;
                }
                actor.m_timer = Fix64.Sqrt(time2);
                //Debuger.Log("begin Falling: needtime:" + (float)actor.m_timer);

            }
            actor.particle.velocity = actor.m_cathingBallStateMovingVelocity;
        }
        //开始坠落
        void _falling(FBActor actor, Fix64 deltaTime)
        {
            actor.m_timer -= deltaTime; 

            actor.m_timeElapsedWhenFalling += deltaTime;
            actor.height = actor.m_heightWhenFalling - (Fix64)0.5f * actor.configuration.fallingAcceleration * actor.m_timeElapsedWhenFalling * actor.m_timeElapsedWhenFalling;
            if (actor.height < Fix64.Zero)
                actor.height = Fix64.Zero;

             if (actor.m_timer <= Fix64.Zero)
             {
                 actor.m_stateSubState = (int)SubState.kAfterFalling;
                 actor.m_timer = actor.m_configuration.dkcb_afterFallingGlideTime[actor.m_stateDataIndex];
                 if (actor.m_configuration.dkcb_afterFallingGlideTime[actor.m_stateDataIndex] > Fix64.Zero)
                 {
                     actor.m_glidingDamping = actor.m_cathingBallStateMovingVelocity.length / actor.m_configuration.dkcb_afterFallingGlideTime[actor.m_stateDataIndex];
                 }
                 actor.height = Fix64.Zero;
                 //播放球员起身动画
                 actor.world.onDoorKeeperBeginToGetup(actor);
                 //Debuger.Log("begin getup: frameID:" + actor.world.world.frameCount);
                 return;
             }
             actor.particle.velocity = actor.m_cathingBallStateMovingVelocity;

        }

        void _afterFalling(FBActor actor, Fix64 deltaTime)
        {
             actor.m_timer -= deltaTime;
             if (actor.m_timer <= Fix64.Zero)
             {
                 actor.particle.velocity = FixVector2.kZero;
                 actor.height = Fix64.Zero;
                 actor.m_stateSubState = (int)SubState.kAfterCatching;
                 actor.m_timer = actor.m_configuration.dkcb_afterCathingWaitingTime[actor.m_stateDataIndex];
                 return;
             }

             actor.particle.velocity = actor.m_cathingBallStateMovingVelocity.normalized * (actor.m_glidingDamping * actor.m_timer);

        }
        void _beforeFlyCatching(FBActor actor, Fix64 deltaTime)
        {
            actor.m_timer -= deltaTime;
            if (actor.m_timer <= Fix64.Zero)
            {
                //Debuger.Log("_beforeFlyCatching end begin move frameNumber:" + actor.world.world.frameCount);

                actor.m_timer = actor.m_cathingBallStateAniWaitTime;
                if (actor.m_stateBool && (actor.m_stateDataIndex == 3 || actor.m_stateDataIndex == 4))
                    actor.m_stateInt = 0;//接不到球                            
                else
                    actor.m_stateInt = 1;//接到球


                Fix64 verticleSpeed = _getActorVelocity(actor, actor.m_timer, out actor.m_cathingBallStateMovingVelocity);
                actor.particle.dampingAcceleration = Fix64.Zero;
                actor.m_stateSubState = (int)SubState.kFlyCatching;
                actor.m_cathingBallStateVerticleSpeed = verticleSpeed;
                //开始播放接球动画                
                actor.world.onDoorKeeperCatchingBall(actor, 
                    actor.m_stateDataIndex,
                    FixVector2.cross(actor.direction, actor.m_stateVector - actor.getPosition()) < Fix64.Zero);    

                return;
            }
        }
        void _beforeCatching(FBActor actor, Fix64 deltaTime)
        {
            actor.m_timer -= deltaTime;
            if (actor.m_timer <= Fix64.Zero)
            {
                Debuger.Log("beforeCatching end begin move");
                actor.m_timer = actor.configuration.dkcb_animationCathingTime[actor.m_stateDataIndex];
                actor.m_stateSubState = (int)SubState.kCatching;

                //移动球员位置到目标点
                _getActorVelocity(actor, actor.m_timer, out actor.m_cathingBallStateMovingVelocity);
                actor.particle.dampingAcceleration = Fix64.Zero;

                //开始播放接球动画
                actor.world.onDoorKeeperCatchingBall(actor,
                    actor.m_stateDataIndex,
                    FixVector2.cross(actor.direction, actor.m_stateVector - actor.getPosition()) < Fix64.Zero);    

                return;
            }
        }

        void _afterCatching(FBActor actor, Fix64 deltaTime)
        {
            actor.m_timer -= deltaTime;

            if (actor.m_timer <= Fix64.Zero)
            {
                actor.setToMovementState();
            }
        }

        public override void leave(FBActor actor)
        {

        }

        private FixVector2 getMoveTargetOffset(FBActor actor, out Fix64 heightOffset )
        {
            FixVector2 moveTaregetOffset = FixVector2.kZero;
            Fix64 moveHeight = Fix64.Zero;
            FixVector2 offset = actor.configuration.dkcb_cathingOffset[actor.m_stateDataIndex];

            FixVector2 targetDirection = actor.m_stateVector - actor.getPosition();
            Fix64 length = targetDirection.length;
            targetDirection = targetDirection / length;
            moveTaregetOffset = targetDirection * (length - offset.x);
            //Debuger.Log("getMoveTargetOffset configOffset:" + (UnityEngine.Vector2)offset 
            //    + " targetPosition:" + (UnityEngine.Vector2)actor.m_stateVector
            //    + " actorPosition:" + (UnityEngine.Vector2)actor.getPosition());

            heightOffset = actor.m_stateValue2 - offset.y;
            if (heightOffset <= Fix64.Zero)
                heightOffset = Fix64.Zero;

            return moveTaregetOffset;
        }
    }

}