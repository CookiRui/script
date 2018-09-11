using FixMath.NET;
using BW31.SP2D;
using Cratos;

public partial class FBActor {

    class Movement : State 
    {
        public static readonly State instance = new Movement();

        public override void update(FBActor actor, Fix64 deltaTime) 
        {
            if (checkCatchingBall(actor))
            {
                return;
            }

            actor.updateMovingState();

            // 如果不移动，则减速
            if (actor.m_movePower == Fix64.Zero) {
                actor.m_particle.dampingAcceleration =
                    actor.world.ball.owner != actor ?
                    actor.m_configuration.m1_stopDampingAcceleration :
                    actor.m_configuration.m1_stopDampingAcceleration_ball;
                return;
            }

            var moveDirection = actor.m_moveDirection;
            var tween = !processCatchBallHelper(actor, ref moveDirection);

            // 如果移动方向满足转身条件
            var acos = FixVector2.dot(actor.m_direction, moveDirection);
            if (acos <= actor.m_configuration.m2_minAngleCos) {
                Fix64[] minSpeedAndWaitTime = null;
                if (actor.world.ball.owner == actor) {
                    minSpeedAndWaitTime = actor.m_configuration.m2_minSpeedAndWaitTime_ball;
                    actor.m_stateSubState = 1;
                }
                else {
                    minSpeedAndWaitTime = actor.m_configuration.m2_minSpeedAndWaitTime;
                    actor.m_stateSubState = 0;
                }

                // 根据当前速度查询停留时间
                var speed = actor.m_particle.velocity.length;
                for (int i = 0; i < minSpeedAndWaitTime.Length; i += 2) {
                    if (speed > minSpeedAndWaitTime[i]) {
                        // 切换至转身状态
                        actor.m_direction = -actor.m_direction;
                        actor.ignoreDirectionSample = true;
                        actor.m_particle.velocity = FixVector2.kZero;
                        //actor.m_moving = false;
                        //actor.m_movingDirty = true;
                        actor.m_stateDataIndex = i;
                        actor.m_nextState = (actor.m_stateSubState == 0 ? TurnBack.instance : TurnBackWithBall.instance);
                        return;
                    }
                }
            }

            // 正常移动

            actor.m_particle.dampingAcceleration = Fix64.Zero;
            if (actor.world.ball.owner != actor) 
            {
                actor.m_particle.maxSpeed = actor.m_configuration.m1_maxSpeed;

                Fix64 angluarSpeed = actor.AIing ? actor.m_configuration.m1_angularSpeed_ai : actor.m_configuration.m1_angularSpeed;

                processRotation(actor, moveDirection, tween, deltaTime * angluarSpeed);
                actor.m_particle.addForce(actor.m_direction * (actor.m_configuration.m1_moveForce * actor.m_movePower));
            }
            else 
            {
                actor.m_particle.maxSpeed = actor.m_configuration.m1_maxSpeed_ball;
                Fix64 angluarSpeed = actor.AIing ? actor.m_configuration.m1_angluarSpeed_ball_ai : actor.m_configuration.m1_angluarSpeed_ball;
                processRotation(actor, moveDirection, tween, deltaTime * angluarSpeed);
                actor.m_particle.addForce(actor.m_direction * (actor.m_configuration.m1_moveForce_ball * actor.m_movePower));
            }
            //actor.m_particle.maxSpeed = actor.m_configuration.m1_maxSpeed;
            //FixVector2 force = actor.m_moveDirection * (actor.m_movePower * actor.m_configuration.m1_moveForce);
            //FixVector2 normalVelocity = actor.m_particle.velocity - actor.m_moveDirection * FixVector2.dot(actor.m_particle.velocity, actor.m_moveDirection);
            //Fix64 normalSpeed = normalVelocity.length;
            //if (normalSpeed > World.kEpsilon)
            //{
            //    var acc = normalSpeed / deltaTime;
            //    if (acc > actor.m_configuration.m1_normalDampingAcceleration)
            //    {
            //        acc = actor.m_configuration.m1_normalDampingAcceleration;
            //    }
            //    force -= normalVelocity * (acc * actor.m_particle.mass / normalSpeed);
            //}
            //actor.m_particle.addForce(force);
        }
        void processRotation(FBActor actor, FixVector2 moveDirection, bool tween, Fix64 maxAngle)
        {
            var speed = actor.m_particle.velocity.length;
            if (tween)
            {
                var cos = FixVector2.dot(actor.m_direction, moveDirection);
                var cos_max = Fix64.Cos(maxAngle);

                if (cos >= cos_max)
                {
                    actor.m_direction = moveDirection;
                }
                else
                {
                    var sin_max =
                        FixVector2.cross(actor.m_direction, moveDirection) >= Fix64.Zero ?
                        Fix64.Sin(maxAngle) : Fix64.Sin(-maxAngle);

                    actor.m_direction = new FixVector2()
                    {
                        x = FixVector2.dot(actor.m_direction, new FixVector2(cos_max, -sin_max)),
                        y = FixVector2.dot(actor.m_direction, new FixVector2(sin_max, cos_max))
                    };
                }
            }
            else
            {
                actor.m_direction = moveDirection;
            }

            actor.m_particle.velocity = actor.m_direction * speed;
        }
        private bool processCatchBallHelper(FBActor actor, ref FixVector2 moveDirection) {
            if (!actor.catchBallHelperEnabled || actor.world.ball.owner != null) {
                return false;
            }
            if (actor.world.ball.particleHeight > actor.m_configuration.maxCatchingBallHelperHeight)
            {
                return false;
            }
            var v = actor.world.ball.particlePosition - actor.m_particle.position;
            var sd = v.squareLength;
            if (sd > actor.m_configuration.catchBallHelper_Raidus * actor.m_configuration.catchBallHelper_Raidus) {
                return false;
            }
            var vu = v / Fix64.Sqrt(sd);
            if (FixVector2.dot(actor.m_direction, vu) < actor.m_configuration.catchBallHelper_MaxFanAngleCos) {
                return false;
            }
            if (FixVector2.dot(moveDirection, vu) < actor.m_configuration.catchBallHelper_MaxAngleCos) {
                return false;
            }
            if (actor.world.ball.particleVelocity.squareLength > actor.m_configuration.catchBallHelper_BallMaxSpeed.square) {
                return false;
            }

            moveDirection = vu;
            return true;
        }

        //检测是否可以停到球
        public static bool checkCatchingBall(FBActor actor)
        {
            if (actor.world.ball.owner != null || actor.world.ball.willBeCatched)
                return false;

            //球速度不能为0也不能太大
            var ballSquareSpeed = actor.world.ball.particleVelocity.squareLength;
            if (ballSquareSpeed == Fix64.Zero || ballSquareSpeed > actor.configuration.scb_maxBallSpeed.square)
            {
                //Debuger.Log("Speed too big " + (float)ballSquareSpeed);
                return false;
            }
            //球员速度不能太快
            if (actor.particle.velocity.squareLength > actor.configuration.scb_maxActorSpeed.square)
            {
                //Debuger.Log("Actor too quick to get ball" + (float)actor.particle.velocity.squareLength);
                return false;
            }

            //求出足球方向和检测半径是否有交点
            Fix64 ballSpeedLength = actor.world.ball.particleVelocity.length;

            FixVector2 ballActor = actor.world.ball.getPosition() - actor.getPosition();
            Fix64 dot = FixVector2.dot(-ballActor, actor.world.ball.particleVelocity / ballSpeedLength);
            //方向相反
            if (dot <= Fix64.Zero)
            {
                //Debuger.Log("Negative Direction to getBall" + (float)ballActor.length);
                return false;
            }

            Fix64 d2 = ballActor.squareLength - dot.square;
            //无交点
            if (d2 > actor.configuration.scb_maxRadius)
            {
                //Debuger.Log("no intersect point" + (float)ballActor.length);
                return false; 
            }


            Fix64 b2 = actor.configuration.scb_maxRadius.square - d2;
            if (b2 <= Fix64.Zero)
            {
                //Debuger.Log("D1");
                return false;
            }

            Fix64 s = dot - Fix64.Sqrt(b2);
            if (s < Fix64.Zero)
            {
                //Debuger.Log("Ball in Range???" + (float)ballActor.length);
                return false;
            }

            FixVector2 normalizedBallActor = ballActor.normalized;

            FixVector2 bIntersectPoint = actor.world.ball.getPosition() + s * actor.world.ball.particleVelocity.normalized;

#if UNITY_EDITOR
            LogicEvent.fire("setIntersectPoint", bIntersectPoint);
#endif

            Fix64 t = Fix64.Zero;
            Fix64 dampingAcceleration = Fix64.Zero;
            bool canReachPoint = actor.world.ball.estimate(bIntersectPoint, out t, out dampingAcceleration);
            if (!canReachPoint)
            {
                //Debuger.Log("Can not read Point");
                return false;
            }

            if (t >= actor.configuration.scb_maxCathingTime)
            {
                //Debuger.Log("t > scb_maxCathingTime distance:" + (float)ballActor.length);
                return false;
            }
            //弹跳次数>1不处理            
            EstimateHeightInfo heightInfo = actor.world.ball.estimateHeight(t);
            if (heightInfo.landedTimes > 0)
            {
                //Debuger.Log("heightInfo.landedTimes > 0");
                return false;
            }

            //脚下停球
            if (heightInfo.destHeight <= actor.configuration.scb_catchingHeightLimit[0])
            {
                ////动画时间太短
                //if (t < actor.configuration.scb_catchingAniTime[0])
                //{
                //    Debuger.Log("t < actor.configuration.scb_catchingAniTime :t=" + (float)t);
                //    return false;
                //}

                //Debuger.Log("StandCatchingBall begin");

                FixVector2 faceTo = (bIntersectPoint - actor.getPosition()).normalized;

                FixVector2 target = actor.getPosition() + faceTo * actor.configuration.scb_catchingOffset[0];

                actor.world.ball.willCatchFreeBall(actor, target, t, actor.configuration.scb_catchingOffsetH[0], Fix64.FastAbs(dampingAcceleration));
                actor.m_stateVector = faceTo;
                actor.m_stateValue = t;
                actor.m_nextState = StandCatchingBall.instance;


                return true;
            }

            for (int i = 1; i < actor.configuration.scb_catchingHeightLimit.Length; ++i)
            {
                if (heightInfo.destHeight > actor.configuration.scb_catchingHeightLimit[i-1]
                    && heightInfo.destHeight <= actor.configuration.scb_catchingHeightLimit[i]
                    )
                {
                    //if (t < actor.configuration.scb_catchingAniTime[i])
                    //{
                    //    Debuger.Log("Air Cathing ball can not procced moveTime:t" + (float)t + " aniTime:" + (float)actor.configuration.scb_catchingAniTime[i]);
                    //    return false; 
                    //}

                    FixVector2 cPoint = actor.getPosition() + actor.configuration.scb_catchingOffset[i] * ballActor.normalized;

                    actor.world.ball.willCatchFreeBall(actor, cPoint, t, actor.configuration.scb_catchingOffsetH[i], Fix64.FastAbs(dampingAcceleration));
                    actor.m_stateVector = normalizedBallActor;
                    actor.m_stateValue = t;
                    actor.m_stateDataIndex = i;
                    actor.m_nextState = AirCatchingBall.instance;

                    return true;
                }
            }

            //Debuger.Log("can not get ball heightInfo.destHeight" + (float)heightInfo.destHeight);
            return false;

        }

         //public static bool checkStandCatchingBall(FBActor actor) {
        //    if (actor.world.ball.owner != null || actor.world.ball.willBeCatched)
        //    {
        //        return false;
        //    }
        //    if (actor.m_particle.velocity.squareLength > actor.m_configuration.scb_maxSpeed.square)
        //    {
        //        return false;
        //    }
        //    var ballSquareSpeed = actor.world.ball.particleVelocity.squareLength;
        //    if (ballSquareSpeed == Fix64.Zero)
        //    {
        //        return false;
        //    }

        //    var s = actor.world.ball.particlePosition - actor.m_particle.position;
        //    var b = s.squareLength - actor.m_configuration.scb_offset.square;

        //    var c = FixVector2.dot(s, actor.world.ball.particleVelocity);
        //    var sigma = c.square - ballSquareSpeed * b;

        //    if (sigma < Fix64.Zero)
        //    {
        //        return false;
        //    }

        //    var t = -(c + Fix64.Sqrt(sigma));
        //    t /= ballSquareSpeed;
        //    if (t < actor.m_configuration.scb_minTime || t > actor.m_configuration.scb_maxTime) {
        //        return false;
        //    }

        //    var height = actor.world.ball.estimateHeight(t);
        //    if (height > actor.m_configuration.scb_maxBallHeight)
        //    {
        //        return false;
        //    }

        //    var ballLocalTarget = actor.world.ball.particleVelocity * t;
        //    actor.world.ball.willCatchBall(actor, actor.world.ball.particlePosition + ballLocalTarget, t);
        //    actor.m_stateVector = (s + ballLocalTarget).normalized;
        //    actor.m_stateValue = t;
        //    actor.m_nextState = StandCatchingBall.instance;

        //    return true;
        //}

        //public static bool checkChestCatchingBall(FBActor actor) {
        //    if (actor.world.ball.owner != null || actor.world.ball.willBeCatched) {
        //        return false;
        //    }
        //    if (actor.m_particle.velocity.squareLength > actor.m_configuration.ccb_maxSpeed.square) {
        //        return false;
        //    }
        //    var ballSquareSpeed = actor.world.ball.particleVelocity.squareLength;
        //    if (ballSquareSpeed == Fix64.Zero) {
        //        return false;
        //    }

        //    var s = actor.world.ball.particlePosition - actor.m_particle.position;
        //    var b = s.squareLength - actor.m_configuration.ccb_offset.square;

        //    var c = FixVector2.dot(s, actor.world.ball.particleVelocity);
        //    var sigma = c.square - ballSquareSpeed * b;

        //    if (sigma < Fix64.Zero || (-c + Fix64.Sqrt(sigma)) <= Fix64.Zero) {
        //        return false;
        //    }



        //    var ballSpeed = Fix64.Sqrt(ballSquareSpeed);
        //    var d = s.length;
        //    var t = (d - actor.m_configuration.ccb_offset2) / ballSpeed;
        //    if (t < actor.m_configuration.ccb_minTime || t > actor.m_configuration.ccb_maxTime)
        //    {
        //        return false;
        //    }

        //    var h = actor.world.ball.estimateHeight(t);
        //    if (h <= actor.m_configuration.scb_maxBallHeight || h > actor.m_configuration.ccb_maxBallHeight) {
        //        return false;
        //    }

        //    actor.m_stateVector = s / d;
        //    actor.m_stateValue = t;
        //    actor.m_nextState = ChestCatchingBall.instance;

        //    actor.world.ball.willCatchBall(actor, actor.m_particle.position + actor.m_stateVector * actor.m_configuration.ccb_offset2, t, actor.m_configuration.ccb_ballHeight);

        //    return true;
        //}
            
        //public static bool checkHeadCatchingBall(FBActor actor)
        //{
        //    if (actor.world.ball.owner != null || actor.world.ball.willBeCatched)
        //    {
        //        return false;
        //    }
        //    if (actor.m_particle.velocity.squareLength > actor.m_configuration.hcb_maxSpeed.square)
        //    {
        //        return false;
        //    }
        //    var ballSquareSpeed = actor.world.ball.particleVelocity.squareLength;
        //    if (ballSquareSpeed == Fix64.Zero)
        //    {
        //        return false;
        //    }

        //    var s = actor.world.ball.particlePosition - actor.m_particle.position;
        //    var b = s.squareLength - actor.m_configuration.hcb_offset.square;

        //    var c = FixVector2.dot(s, actor.world.ball.particleVelocity);
        //    var sigma = c.square - ballSquareSpeed * b;

        //    if (sigma < Fix64.Zero || (-c + Fix64.Sqrt(sigma)) <= Fix64.Zero)
        //    {
        //        return false;
        //    }



        //    var ballSpeed = Fix64.Sqrt(ballSquareSpeed);
        //    var d = s.length;
        //    var t = (d - actor.m_configuration.hcb_offset2) / ballSpeed;
        //    if (t < actor.m_configuration.hcb_minTime || t > actor.m_configuration.hcb_maxTime)
        //    {
        //        return false;
        //    }

        //    var h = actor.world.ball.estimateHeight(t);
        //    if (h <= actor.m_configuration.hcb_maxBallHeight || h > actor.m_configuration.hcb_maxBallHeight)
        //    {
        //        return false;
        //    }

        //    actor.m_stateVector = s / d;
        //    actor.m_stateValue = t;
        //    actor.m_nextState = HeadCatchingBall.instance;

        //    actor.world.ball.willCatchBall(actor, actor.m_particle.position + actor.m_stateVector * actor.m_configuration.hcb_offset2, t, actor.m_configuration.hcb_ballHeight);

        //    return true;
        //}

        public override void enter(FBActor actor) {
            //jlx 2017.03.27-log: 修复：铲球、射门结束动作、传球结束动作之后，状态机没有正确流转下去
            //actor.m_movingDirty = true;
            //actor._updateMovingState();

            actor.updateMovingState();
        }
        public override void leave(FBActor actor) {
            //Debuger.Log("Leve MovementState change to otherState");
            //actor.world.onActorLeaveMovement(actor);

            actor._setMovingState(MovingState.kAction);
        }
    }

    class TurnBack : State {
        public static readonly State instance = new TurnBack();

        public override bool canBreak(FBActor actor, State state) {
            return state != PassBall.instance
                    && state != ShootBall.instance
                    && state != Sliding.instance
                    && state != instance
                    && state != Skilling.instance;
        }

        public override void enter(FBActor actor) {
            actor.m_timer = actor.m_configuration.m2_minSpeedAndWaitTime[actor.m_stateDataIndex + 1];
            actor.world.onTurnBack(actor);
        }

        public override void update(FBActor actor, Fix64 deltaTime) {
            actor.m_particle.velocity = FixVector2.kZero;
            actor.m_timer -= deltaTime;
            if (actor.m_timer <= Fix64.Zero) {
                actor.m_nextState = TurnBackMoving.instance;
            }
        }
    }

    class TurnBackWithBall : State {
        public static readonly State instance = new TurnBackWithBall();

        public override bool canBreak(FBActor actor, State state) {
            return state != ShootBall.instance
                    && state != PassBall.instance
                    && state != Sliding.instance
                    && state != instance
                    && state != Skilling.instance;
        }

        public override void enter(FBActor actor) {
            actor.m_timer = actor.m_configuration.m2_minSpeedAndWaitTime_ball[actor.m_stateDataIndex + 1];
            actor.world.onTurnBack(actor);
        }

        public override void update(FBActor actor, Fix64 deltaTime) {
            actor.m_particle.velocity = FixVector2.kZero;
            actor.m_timer -= deltaTime;
            if (actor.m_timer <= Fix64.Zero) {
                actor.m_nextState = TurnBackMoving.instance;
            }
        }

    }

    class TurnBackMoving : State {

        public static readonly State instance = new TurnBackMoving();

        public override bool canBreak(FBActor actor, State state) {
            return state != ShootBall.instance
                    && state != PassBall.instance
                    && state != Sliding.instance
                    && state != instance;
        }

        public override void enter(FBActor actor) {
            actor.m_timer = actor.m_configuration.m2_movingTime;
            actor._setMovingState(MovingState.kMoving);
        }

        public override void update(FBActor actor, Fix64 deltaTime) {
            actor.m_particle.dampingAcceleration = Fix64.Zero;
            if (actor.world.ball.owner == actor) {
                actor.m_particle.maxSpeed = actor.m_configuration.m1_maxSpeed_ball;
                actor.m_particle.addForce(actor.m_direction * actor.m_configuration.m1_moveForce_ball);
            }
            else {
                actor.m_particle.maxSpeed = actor.m_configuration.m1_maxSpeed;
                actor.m_particle.addForce(actor.m_direction * actor.m_configuration.m1_moveForce);
            }
            actor.m_timer -= deltaTime;
            if (actor.m_timer <= Fix64.Zero) {
                actor.setToMovementState();
                //actor.m_nextState = Movement.instance;
            }
        }

        public override void leave(FBActor actor) {

        }

    }

}