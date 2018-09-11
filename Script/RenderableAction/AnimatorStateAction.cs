using Cratos;

namespace RAL
{
    public class IdleAction : InstantAction { }

    public class RunAction : InstantAction { }

    public class MovingToIdleAction : InstantAction { }

    public class OtherAction : InstantAction { }

    public class AnimatorScaleAction : InstantAction
    {
        public float animatorScale;
        public void init(float animatorScale)
        {
            this.animatorScale = animatorScale;
        }

        public override void serialize(BytesStream stream)
        {
            base.serialize(stream);
            stream.Write(animatorScale);

        }
        public override void unserialize(BytesStream stream)
        {
            base.unserialize(stream);
            animatorScale = stream.ReadSingle();
        }
    }
}