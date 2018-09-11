using FixMath.NET;
using BW31.SP2D;
using System.Collections.Generic;

public class ArenaObstacle : Obstacle
{
    // value\bit  0        1       2-3
    //   0        OutDoor  FieldA   Back
    //   1        InDoor   FieldB   Left
    //   2                         Right
    //   3
    [System.Flags]
    public enum DoorSide : int
    {
        kOutDoor = 0,
        kDoor = 1,
        kFieldA = 0 << 1,
        kFieldB = 1 << 1,
        kBack = 0 << 2,
        kLeft = 1 << 2,
        kRight = 2 << 2,
        kCeil = 3 << 2,

        kFieldMask = 1 << 1,
        kSideMask = 3 << 2
    }

    //public ArenaObstacle() { }
    //public ArenaObstacle(FixVector2 mainExtent, FixVector2 doorExtent, Fix64 doorHeight)
    //{
    //    build(mainExtent, doorExtent, doorHeight);
    //}

    public void build(FixVector2 mainExtent, FixVector2 doorExtent, Fix64 doorHeight, Fix64 doorSlopeExtent)
    {
        m_edges.Clear();
        m_rangeEdges.Clear();
        m_points.Clear();
        //m_mainExtent = mainExtent;
        //m_doorExtent = doorExtent;


        /*
                     *************************************************m_edges[1]*********************************************************
                     *                                                                                                                  *
                     *                                                                                                                  *
               m_rangeEdges[1]                                                                                                    m_rangeEdges[0]
                     *                                                                                                                  *
                     *                                                                                                                  *
                     *                                                                                                                  *
    *m_rangeEdges[6]**m_points[3]                                                                                            m_points[0]**m_rangeEdges[4]* 
    *       *        *                                                                                                                  *       *        * 
    *       *        *                                                                                                                  *       *        * 
    *       *        *                                                                                                                  *       *        * 
    *   m_edges[3] m_ballEdges[1]                                                                                            m_ballEdges[0] m_edges[2]  dynamicSlopeEdge
    *       *        *                                                                                                                  *       *        * 
    *       *        *                                                                                                                  *       *        * 
    *       *        *                                                                                                                  *       *        * 
    *       *        *                                                                                                                  *       *        * 
    *m_rangeEdges[7]**m_points[2]                                                                                            m_points[1]**m_rangeEdges[5]* 
                     *                                                                                                                  *
                     *                                                                                                                  *
                     *                                                                                                                  *
               m_rangeEdges[3]                                                                                                    m_rangeEdges[1]
                     *                                                                                                                  *
                     *                                                                                                                  *
                     *************************************************m_edges[0]*********************************************************
                                                                                                 
         */


        //  3  0
        //  2  1
        FixVector2 point = new FixVector2(mainExtent.x, doorExtent.y);
        m_points.Add(point);
        m_points.Add(point.inverseY);
        m_points.Add(-point);
        m_points.Add(point.inverseX);

        //     1
        //  3     2
        //     0
        Fix64 d = mainExtent.x + doorExtent.x * (Fix64)2;
        m_edges.Add(new Edge() { normal = FixVector2.kUp, d = mainExtent.y });
        m_edges.Add(new Edge() { normal = FixVector2.kDown, d = mainExtent.y });
        m_edges.Add(new Edge() { normal = FixVector2.kLeft, d = d });
        m_edges.Add(new Edge() { normal = FixVector2.kRight, d = d });

        // 1     0
        m_ballEdges.Add(new Edge() { normal = FixVector2.kLeft, d = mainExtent.x });
        m_ballEdges.Add(new Edge() { normal = FixVector2.kRight, d = mainExtent.x });

        //    2      0
        //  6          4
        //
        //  7          5
        //    3      1
        Fix64 r = mainExtent.y - doorExtent.y;
        m_rangeEdges.Add(new RangeEdge() { root = mainExtent, normal = FixVector2.kLeft, d = mainExtent.x, r = r });
        m_rangeEdges.Add(new RangeEdge() { root = mainExtent.inverseY, normal = FixVector2.kLeft, d = mainExtent.x, r = r });
        m_rangeEdges.Add(new RangeEdge() { root = mainExtent.inverseX, normal = FixVector2.kRight, d = mainExtent.x, r = r });
        m_rangeEdges.Add(new RangeEdge() { root = -mainExtent, normal = FixVector2.kRight, d = mainExtent.x, r = r });

        var r1 = doorExtent.x + doorSlopeExtent;
        point.x += r1;
        m_rangeEdges.Add(new RangeEdge() { root = point, normal = FixVector2.kDown, d = doorExtent.y, r = r1 });
        m_rangeEdges.Add(new RangeEdge() { root = point.inverseY, normal = FixVector2.kUp, d = doorExtent.y, r = r1 });
        m_rangeEdges.Add(new RangeEdge() { root = point.inverseX, normal = FixVector2.kDown, d = doorExtent.y, r = r1 });
        m_rangeEdges.Add(new RangeEdge() { root = -point, normal = FixVector2.kUp, d = doorExtent.y, r = r1 });

        m_doorHeight = doorHeight;
        this.doorExtent = doorExtent;
        this.doorSlopeExtent = doorSlopeExtent;
    }

    public uint collisionLayerMask { get { return 0x1; } }

    public void fillParticleContacts(Particle particle, List<ParticleContact> contacts)
    {
        ParticleContact contact = default(ParticleContact);
        for (int i = 0; i < 2; ++i)
        {
            if (particle.intersect(m_edges[i].normal, m_edges[i].d, ref contact))
            {
                contacts.Add(contact);
            }
        }

        for (int i = 0; i < 4; ++i)
        {
            int b = i * 2;
            if (intersectTwoRangeEdgesAndPoint(particle, s_rangEdgeIndices[b], s_rangEdgeIndices[b + 1], s_rangEdgeIndices[b], ref contact))
            {
                contacts.Add(contact);
            }
        }

        var ball = particle as NewBallParticle;
        if (ball != null)
        {
            //球
            fillBallContacts(ball, contacts);
        }
        else
        {
            //人
            if (((FBActor)particle.tag).configuration.bodyHeight >= m_doorHeight)
            {
                //人比球门高
                for (int i = 0; i < m_ballEdges.Count; ++i)
                {
                    if (particle.intersect(m_ballEdges[i].normal, m_ballEdges[i].d, ref contact))
                    {
                        contacts.Add(contact);
                    }
                }
            }
            else
            {
                for (int i = 2; i < m_edges.Count; ++i)
                {
                    if (particle.intersect(m_edges[i].normal, m_edges[i].d, ref contact))
                    {
                        switch (i)
                        {
                            case 2: contact.tagI = (int)(DoorSide.kDoor | DoorSide.kFieldA | DoorSide.kBack); break;
                            case 3: contact.tagI = (int)(DoorSide.kDoor | DoorSide.kFieldB | DoorSide.kBack); break;
                        }
                        contacts.Add(contact);
                    }
                }
            }
        }
    }

    void fillBallContacts(NewBallParticle ball, List<ParticleContact> contacts)
    {
        ParticleContact contact = default(ParticleContact);
        if (ball.height >= m_doorHeight)
        {
            for (int i = 0; i < m_ballEdges.Count; ++i)
            {
                if (ball.intersect(m_ballEdges[i].normal, m_ballEdges[i].d, ref contact))
                {
                    contacts.Add(contact);
                }
            }
        }
        else
        {
            #region jlx 2017.05.15-log:添加动态计算碰撞球门斜边

            var dynamicSlopeEdge = calculateSlopeEdge(ball.position);
            if (ball.intersect(dynamicSlopeEdge.normal, dynamicSlopeEdge.d, ref contact))
            {
                contact.tagI = (int)(
                                    DoorSide.kDoor |
                                    DoorSide.kBack |
                                    (ball.position.x > Fix64.Zero ? DoorSide.kFieldA : DoorSide.kFieldB));
                contacts.Add(contact);
            }

            #endregion
        }
    }

    static readonly int[] s_rangEdgeIndices = new int[] { 2, 6, 0, 4, 3, 7, 1, 5 };

    public IEnumerator<CastHit> cast(Ray ray, Fix64 width)
    {
        Fix64 t1, t2;
        CastHit hit;
        hit.obstacle = this;
        hit.particle = null;
        for (int i = 0; i < m_edges.Count; ++i)
        {
            if (ray.cast(m_edges[i].normal, m_edges[i].d - width, out t1) && t1 >= -width)
            {
                hit.distance = t1;
                hit.normal = m_edges[i].normal;
                yield return hit;
            }
        }
        for (int i = 0; i < m_rangeEdges.Count; ++i)
        {
            if (ray.cast(m_rangeEdges[i].normal, m_rangeEdges[i].d - width, m_rangeEdges[i].root, m_rangeEdges[i].r, out t1) && t1 >= -width)
            {
                hit.distance = t1;
                hit.normal = m_rangeEdges[i].normal;
                yield return hit;
            }
        }
        for (int i = 0; i < m_points.Count; ++i)
        {
            if (ray.cast(m_points[i], width, out t1, out t2) && t1 >= -width)
            {
                hit.distance = t1;
                hit.normal = (ray * t1 - m_points[i]).normalized;
                yield return hit;
            }
        }
    }



    bool intersectTwoRangeEdgesAndPoint(Particle particle, int re0, int re1, int pt, ref ParticleContact contact)
    {
        ParticleContact c0 = default(ParticleContact), c1 = default(ParticleContact);
        if (m_rangeEdges[re0].intersect(particle, ref c0))
        {
            if (m_rangeEdges[re1].intersect(particle, ref c1))
            {
                if (c0.penetration < c1.penetration)
                {
                    contact = c0;
                    setDoorSide(re0, ref contact);
                }
                else
                {
                    contact = c1;
                    setDoorSide(re1, ref contact);
                }
            }
            else
            {
                contact = c0;
                setDoorSide(re0, ref contact);
            }
            return true;
        }
        if (m_rangeEdges[re1].intersect(particle, ref contact))
        {
            setDoorSide(re1, ref contact);
            return true;
        }
        return particle.intersect(m_points[pt], ref contact);
    }

    void setDoorSide(int index, ref ParticleContact contact)
    {
        switch (index)
        {
            case 4: contact.tagI = (int)(DoorSide.kDoor | DoorSide.kFieldB | DoorSide.kLeft); break;
            case 5: contact.tagI = (int)(DoorSide.kDoor | DoorSide.kFieldB | DoorSide.kRight); break;
            case 6: contact.tagI = (int)(DoorSide.kDoor | DoorSide.kFieldA | DoorSide.kRight); break;
            case 7: contact.tagI = (int)(DoorSide.kDoor | DoorSide.kFieldA | DoorSide.kLeft); break;
        }
    }

    Edge calculateSlopeEdge(FixVector2 point)
    {
        var slopeEdge = m_edges[point.x > Fix64.Zero ? 2 : 3];
        var ratio = Fix64.Clamp(point.y / m_doorHeight, Fix64.Zero, Fix64.One);
        slopeEdge.d += doorSlopeExtent * (Fix64)2 * (Fix64.One - ratio);
        return slopeEdge;
    }


    struct Edge
    {
        public FixVector2 normal;
        public Fix64 d;
    }

    struct RangeEdge
    {
        public FixVector2 normal;
        public FixVector2 root;
        public Fix64 d;
        public Fix64 r;

        public bool intersect(Particle particle, ref ParticleContact contact)
        {
            return particle.intersect(root, normal, d, r, ref contact);
        }
    }

    //FixVector2 m_mainExtent = FixVector2.kZero;
    //FixVector2 m_doorExtent = FixVector2.kZero;
    Fix64 m_doorHeight = Fix64.MaxValue;
    List<Edge> m_edges = new List<Edge>(4);
    List<Edge> m_ballEdges = new List<Edge>(2);
    List<RangeEdge> m_rangeEdges = new List<RangeEdge>(8);
    List<FixVector2> m_points = new List<FixVector2>(4);
    FixVector2 doorExtent;
    Fix64 doorSlopeExtent;
}