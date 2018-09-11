using BW31.SP2D;
using FixMath.NET;


public partial class FBWorld
{

    ParticleContactResolver _onObstacleCollided(Particle particle, Obstacle obstacle, ParticleContact contact)
    {
        var ball = particle as NewBallParticle;
        if (ball != null)
        {
            ((FBBall)ball.tag).onCollided(contact);

            if (contact.tagI == (int)ArenaObstacle.DoorSide.kOutDoor)
            {
                return new PCR_Obstacle() { particle = ball, obstacle = obstacle, contact = contact, restitution = config.ballCollisionRestitution[0] };
            }

            PCR_Obstacle pcrObstracle = new PCR_Obstacle();
            pcrObstracle.particle = ball;
            pcrObstracle.obstacle = obstacle;
            pcrObstracle.contact = contact;

            if ((contact.tagI & (int)ArenaObstacle.DoorSide.kBack) == (int)ArenaObstacle.DoorSide.kBack)
            {
                //后面
                pcrObstracle.restitution = config.ballCollisionRestitution[1];
            }
            else if ((contact.tagI & (int)ArenaObstacle.DoorSide.kLeft) == (int)ArenaObstacle.DoorSide.kLeft)
            {
                //左侧边
                pcrObstracle.restitution = config.ballCollisionRestitution[2];
            }
            else if ((contact.tagI & (int)ArenaObstacle.DoorSide.kRight) == (int)ArenaObstacle.DoorSide.kRight)
            {
                //右侧边
                pcrObstracle.restitution = config.ballCollisionRestitution[3];
            }
            else if ((contact.tagI & (int)ArenaObstacle.DoorSide.kCeil) == (int)ArenaObstacle.DoorSide.kCeil)
            {
                //天花板
                pcrObstracle.restitution = config.ballCollisionRestitution[4];
            }
            return pcrObstracle;
        }
        return new PCR_Obstacle_DontMove_KeepVelocity() { particle = particle, obstacle = obstacle, contact = contact, restitution = config.ballCollisionRestitution_actorAndobstacle };
    }

    FixVector2 ajustActorBallCollideNormal(FixVector2 normal, FBActor actor, FixVector2 ballVelocity)
    {

        FixVector2 ajustedNormal = -normal;
        FixVector2 actorBallDirection = (actor.getPosition() - actor.world.ball.getPosition()).normalized;
        if (actor.particle.velocity.length > Fix64.Zero
            && FixVector2.dot(actorBallDirection, actor.direction) > Fix64.Cos(actor.configuration.maxBallActorCollideAngle))
        {
            ajustedNormal = actor.direction - ballVelocity.normalized;
            ajustedNormal = ajustedNormal.normalized;
        }

        return ajustedNormal;
    }

    ParticleContactResolver _onParticleCollided(Particle p1, Particle p2, ParticleContact contact)
    {
        var ball = p1 as NewBallParticle;
        if (ball != null)
        {
            if (_checkContact((FBBall)ball.tag, (FBActor)p2.tag))
            {
                //ball.setCollidedEventFlag();
                contact.normal = ajustActorBallCollideNormal(-contact.normal, (FBActor)p2.tag, ball.velocity);
                return new PCR_Obstacle() { particle = ball, contact = contact, restitution = config.ballCollisionRestitution_actorAndball };
            }
            return null;
        }
        ball = p2 as NewBallParticle;
        if (ball != null)
        {
            if (_checkContact((FBBall)ball.tag, (FBActor)p1.tag))
            {

                //ball.setCollidedEventFlag();
                contact.normal = ajustActorBallCollideNormal(contact.normal, (FBActor)p1.tag, ball.velocity);
                return new PCR_Obstacle() { particle = ball, contact = contact, restitution = config.ballCollisionRestitution_actorAndball };
            }
            return null;
        }

        FBActor actor = (FBActor)p1.tag;
        if (actor != null && actor.ignoreCollision)
        {
            return null;
        }
        FBActor actor2 = (FBActor)p2.tag;
        if (actor2 != null && actor2.ignoreCollision)
        {
            return null;
        }

        //碰撞数据保存
        actor.onActorCollided(actor2);
        actor2.onActorCollided(actor);

        return new PCR_Particle_DontMove_KeepVelocity() { p1 = p1, p2 = p2, contact = contact, restitution = config.ballCollisionRestitution_actorAndactor };
    }

    bool _checkContact(FBBall ball, FBActor actor)
    {
        // TODO:
        if (ball.get3DPosition().y > actor.configuration.bodyHeight)
        {
            return false;
        }

        if (ball.kicker == actor && actor.isKickingBall)
        {
            return false;
        }

        ball.onCollided(actor);
        return true;
    }
}