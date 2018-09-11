using UnityEngine;

public enum RotateType
{
    /// <summary>
    /// 不旋转
    /// </summary>
    None,

    /// <summary>
    /// 地面滚动
    /// </summary>
    Roll,

    /// <summary>
    /// 抛物线
    /// </summary>
    Parabola,

    /// <summary>
    /// 弧线球
    /// </summary>
    ArcLine,

    /// <summary>
    /// 必杀技
    /// </summary>
    KillerSkill,
}

class BallRotator : MonoBehaviour
{
    public float radius { private get; set; }
    public float parabolaK { private get; set; }
    public float arclineK { private get; set; }

    [SerializeField]
    RotateType type;
    Vector3 direction;
    float angularVelocity;
    Vector3 rotateAxis;

    public void rotate(Vector3 from, Vector3 to)
    {
        if (type == RotateType.None) return;
        if (type == RotateType.Roll)
        {
            var diff = to - from;
            var d = new Vector3(diff.x, 0, diff.z);
            var dm = d.magnitude;
            if (d.magnitude > 1e-5f)
            {
                direction = d / dm;
            }
            angularVelocity = -diff.magnitude / radius * Mathf.Rad2Deg;
            rotateAxis = Vector3.Cross(direction, Vector3.up);
        }

        if (angularVelocity == 0) return;
        transform.rotation = Quaternion.AngleAxis(angularVelocity, rotateAxis) * transform.rotation;
    }

    public void clear()
    {
        direction = Vector3.zero;
        angularVelocity = 0;
        rotateAxis = Vector3.zero;
        type = RotateType.None;
    }

    public void parabola(Vector3 velocity)
    {
        type = RotateType.Parabola;
        var xz = new Vector3 { x = velocity.x, z = velocity.z };
        var xzMagnitude = xz.magnitude;
        var yMagnitude = Mathf.Abs(velocity.y);
        if (xzMagnitude == 0 || yMagnitude == 0)
        {
            angularVelocity = 0;
            return;
        }
        angularVelocity = -Mathf.Min(xzMagnitude / yMagnitude, yMagnitude / xzMagnitude) * parabolaK * Mathf.Rad2Deg;
        rotateAxis = Vector3.Cross(xz, Vector3.up);
    }

    public void arcline(float angle)
    {
        type = RotateType.ArcLine;
        angularVelocity = angle * arclineK * Mathf.Rad2Deg;
        rotateAxis = Vector3.up;
    }

    public void killerSkill(Vector3 axis, float angularVelocity)
    {
        type = RotateType.KillerSkill;
        rotateAxis = axis;
        this.angularVelocity = angularVelocity;
    }

    public void roll()
    {
        type = RotateType.Roll;
    }
}
