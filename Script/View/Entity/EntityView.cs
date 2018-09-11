using UnityEngine;

class EntityView : MonoBehaviour
{
    public Vector3? startSamplePosition;
    public uint id { get; set; }

    public void setMaterialColor(Color color)
    {
        MeshRenderer mr = GetComponentInChildren<MeshRenderer>(false);
        if (mr != null)
        {
            mr.material.color = color;
        }
    }

    public Transform getChild(string subNode)
    {
        return this.transform.Find(subNode);
    }

    public virtual void setRotation(Quaternion rotation)
    {
        this.transform.localRotation = rotation;
    }

    public virtual void setScale(Vector3 scale)
    {
        this.transform.localScale = scale;
    }

    public virtual void lookAt(Vector3 dest)
    {

        Vector3 lookatPosition = this.getPosition() - dest;
        lookatPosition.y = 0;
        if (lookatPosition == Vector3.zero)
        {
            return;
        }
        Quaternion qt = Quaternion.LookRotation(lookatPosition);

        setRotation(qt);
    }

    protected virtual void Update()
    {
        if (positionDirty)
        {
            positionDirty = false;
            this.transform.localPosition = newposition;
        }

    }

    protected Vector3 newposition = Vector3.zero;
    protected bool positionDirty = false;
    public virtual void setPosition(Vector3 dest)
    {
        positionDirty = true;
        newposition = dest;
    }
    public void setPosition(Vector2 dest)
    {
        setPosition(new Vector3(dest.x, 0.0f, dest.y));
    }

    public Vector3 getPosition() { return newposition; }

    public virtual Vector3 getCenterPosition()
    {
        return transform.position;
    }

    public virtual void onCreate() { }


    public virtual AnimatorRecord createAnimatorRecord() { return null; }

    public virtual void restoreAnimatorRecord(AnimatorRecord record) { }
}