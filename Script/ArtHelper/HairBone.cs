using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class HairBone : MonoBehaviour {

    public enum DistanceType {
        kFixed,
        kRope,
        kSpring
    }

    public float damping = 2f;
    public float gravity = 10f;
    [Range(0, 1)]
    public float inertia = 1;

    public DistanceType distanceType = DistanceType.kFixed;
    public float linearRelaxation, linearRelaxation2;
    public float minDistance, maxDistance;
    public float linearBounce;

    [Range(0, 180)]
    public float angularLimit = 15;

    [Range(0, 180)]
    public float angularRelaxation = 0;

    public float angularBounce = 0;
    public Quaternion angularRotation = Quaternion.identity;

    public float originDistance { get { return m_originDistance; } }

    // Use this for initialization
    private void Start () {
        m_velocity = Vector3.zero;
        m_lastParentPosition = transform.parent.position;
        m_position = transform.position;
        var localPos = transform.position - transform.parent.position;
        m_originDistance = localPos.magnitude;
        if (m_originDistance > Vector3.kEpsilon) {
            m_direction = localPos / m_originDistance;
        }
        m_forwardRotation = Quaternion.LookRotation(transform.localPosition.normalized);
        var q = _calculateRotation();
        m_originRotation = Quaternion.Inverse(q) * transform.rotation;
       

        m_parent = transform.parent.GetComponent<HairBone>();
        if (m_parent != null) {
            m_parent.m_children.Add(this);
        }
        
    }

    private void OnDestroy() {
#if UNITY_EDITOR
        onEditorStop();
#endif
    }

    private void LateUpdate () {
#if UNITY_EDITOR
        if (Application.isPlaying)
#endif
        {
            updateBone(Time.deltaTime);
        }

	}

    public void updateBone(float deltaTime) {
        if (!isActiveAndEnabled) {
            return;
        }
        if (m_parent != null && m_parent.isActiveAndEnabled) {
            return;
        }
        _update(deltaTime);
    }

    private void _update(float deltaTime) {

        var parentVelocity = (transform.parent.position - m_lastParentPosition) / deltaTime;
        m_lastParentPosition = transform.parent.position;

        // process movement
        //var oldLocalPos = m_position - transform.parent.position;
        var newLocalPos = m_position + m_velocity * deltaTime - transform.parent.position;

        m_velocity += Vector3.down * (gravity * deltaTime);

        var distance = newLocalPos.magnitude;
        var newDirection = (distance > Vector3.kEpsilon ? newLocalPos / distance : m_direction);


        var parentForward = transform.parent.rotation * angularRotation * Vector3.forward;
        float angle = Mathf.Acos(Mathf.Clamp(Vector3.Dot(newDirection, parentForward), -1, 1)) * Mathf.Rad2Deg;
        if (angle > angularLimit) {
            if (angularLimit <= Vector3.kEpsilon) {
                newDirection = parentForward;
                var parentUpward = angularRotation * transform.parent.up;
                m_velocity = m_velocity.equalDirectionalLength(parentUpward, Vector3.Dot(parentVelocity, parentUpward) * inertia);
            }
            else {
                var axis = Vector3.Cross(parentForward, newDirection).normalized;
                newDirection = Quaternion.AngleAxis(angularLimit, axis) * parentForward;
                axis = Vector3.Cross(newDirection, axis).normalized;
                m_velocity = m_velocity.greaterEuqalDirectionalLength(axis, Vector3.Dot(parentVelocity, axis) * inertia);
            }
            newLocalPos = newDirection * distance;
        }
        else if (angle > angularRelaxation) {
            var axis = Vector3.Cross(parentForward, newDirection).normalized;
            axis = Vector3.Cross(newDirection, axis).normalized;
            m_velocity += axis * ((angle - angularRelaxation) * angularBounce * deltaTime);
        }


        // calc new rotation by direction.
        m_direction = newDirection;
        transform.rotation = _calculateRotation() * m_originRotation;


        if (distanceType == DistanceType.kFixed) {
            if (Mathf.Abs(distance - m_originDistance) > Vector3.kEpsilon) {
                newLocalPos = newDirection * m_originDistance;
                m_velocity = m_velocity.equalDirectionalLength(newDirection, Vector3.Dot(parentVelocity, newDirection) * inertia);
            }
        }
        else if (distanceType == DistanceType.kRope) {
            if (distance > m_originDistance) {
                newLocalPos = m_direction * m_originDistance;        
                m_velocity += m_direction * ((linearRelaxation - m_originDistance) * linearBounce * deltaTime);
                m_velocity = m_velocity.lessEuqalDirectionalLength(m_direction, Vector3.Dot(parentVelocity, m_direction) * inertia);
            }
            else if (distance > linearRelaxation) {
                m_velocity += m_direction * ((linearRelaxation - distance) * linearBounce * deltaTime);
            }
        }
        else if (distanceType == DistanceType.kSpring) {
            var diff = distance - m_originDistance;
            if (diff < 0) {
                diff = -diff;
                if (diff > minDistance) {
                    var d = m_originDistance - minDistance;
                    newLocalPos = m_direction * d;
                    m_velocity += m_direction * (minDistance * linearBounce * deltaTime);
                    m_velocity = m_velocity.greaterEuqalDirectionalLength(m_direction, Vector3.Dot(parentVelocity, m_direction) * inertia);
                }
                else if (diff > linearRelaxation2) {
                    m_velocity += m_direction * ((diff - linearRelaxation2) * linearBounce * deltaTime);
                }
            }
            else {
                if (diff > maxDistance) {
                    var d = m_originDistance + maxDistance;
                    newLocalPos = m_direction * d;
                    m_velocity += m_direction * ((linearRelaxation - maxDistance) * linearBounce * deltaTime);
                    m_velocity = m_velocity.lessEuqalDirectionalLength(m_direction, Vector3.Dot(parentVelocity, m_direction) * inertia);
                }
                else if (diff > linearRelaxation) {
                    m_velocity += m_direction * ((linearRelaxation - diff) * linearBounce * deltaTime);
                }
            }

        }

        var k = 1 - deltaTime * damping;
        if (k <= 0) {
            m_velocity = Vector3.zero;
        }
        else {
            m_velocity *= k;
        }

        m_position = transform.parent.position + newLocalPos;
        //transform.localPosition = newLocalPos;
        transform.position = m_position;


        foreach (var bone in m_children) {
            bone._update(deltaTime);
        }
    }

    Quaternion _calculateRotation() {
        var parentUp = transform.parent.rotation * m_forwardRotation * Vector3.up;
        Vector3 up;
        var right = Vector3.Cross(parentUp, m_direction);
        var m = right.magnitude;
        if (m > Vector3.kEpsilon) {
            up = Vector3.Cross(m_direction, right / m).normalized;
        }
        else {
            up = Vector3.Cross(m_direction, transform.parent.right).normalized;
        }
        return Quaternion.LookRotation(m_direction, up);
    }

    private Vector3 m_lastParentPosition;
    private Vector3 m_position = Vector3.one;
    private Vector3 m_direction = Vector3.forward;
    private Vector3 m_velocity = Vector3.one;
    private Quaternion m_originRotation;
    private Quaternion m_forwardRotation;
    private float m_originDistance;
    private HairBone m_parent = null;
    private List<HairBone> m_children = new List<HairBone>();

#if UNITY_EDITOR
    private void OnEnable() {
        m_listNode = allBones.AddLast(this);
    }

    private void OnDisable() {
        allBones.Remove(m_listNode);
        onEditorStop();
    }

    public void onEditorRun() {
        m_localPosition = transform.localPosition;
        m_localRotation = transform.localRotation;
        m_localScale = transform.localScale;
        Start();
        m_running = true;
    }

    public void onEditorStop() {
        if (m_running) {
            m_running = false;
            transform.localPosition = m_localPosition;
            transform.localRotation = m_localRotation;
            transform.localScale = m_localScale;
            m_children.Clear();
        }
    }

    private Vector3 m_localPosition = Vector3.zero;
    private Quaternion m_localRotation = Quaternion.identity;
    private Vector3 m_localScale = Vector3.one;
    private bool m_running = false;

    private LinkedListNode<HairBone> m_listNode;
    public static LinkedList<HairBone> allBones = new LinkedList<HairBone>();
#endif
}


public static class Vector3Extend {
    public static Vector3 equalDirectionalLength(this Vector3 v, Vector3 direction, float length) {
        var d = Vector3.Dot(v, direction);
        var t = v - direction * d;
        return t + direction * length;
    }

    public static Vector3 lessEuqalDirectionalLength(this Vector3 v, Vector3 direction, float length) {
        var d = Vector3.Dot(v, direction);
        if (d <= length) {
            return v;
        }
        var t = v - direction * d;
        return t + direction * length;
    }

    public static Vector3 greaterEuqalDirectionalLength(this Vector3 v, Vector3 direction, float length) {
        var d = Vector3.Dot(v, direction);
        if (d >= length) {
            return v;
        }
        var t = v - direction * d;
        return t + direction * length;
    }
}