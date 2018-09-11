
using System.Collections.Generic;

namespace RAL {
    public class LogicFrame {
        public int frameId;
        public float offset;

        public PhysicsFrame[] physicsFrames;

        public void release(RenderActionGenerator generator) {
            for (int i = 0; i < physicsFrames.Length; ++i) {
                for (int j = 0; j < physicsFrames[i].actions.Length; ++j) {
                    generator.releaseRenderAction(physicsFrames[i].actions[j]);
                }
            }
        }

        public void dump()
        {
            Debuger.Log("LogicFrame " + frameId + " actionsList");
            for (int i = 0; i < physicsFrames.Length; ++i) {
                for (int j = 0; j < physicsFrames[i].actions.Length; ++j)
                {
                    Debuger.Log("RenderAction:" + physicsFrames[i].actions[j].ToString() );
                }
            }
        }
    }

    public struct PhysicsFrame {
        public RenderAction[] actions;
    }


    public class LogicFrameQueue {

        public RenderActionGenerator renderActionGenerator { get { return m_generator; }  }

        public void reset() {
            m_firstMsg = true;
            m_lastFrameId = -1;
            while (m_queue.Count != 0) {
                m_queue.Dequeue().release(m_generator);
            }
        }

        public void push(float time, PhysicsFrame[] physicsFrames) {
            if (m_firstMsg) {
                //UnityEngine.Debug.Log(UnityEngine.Time.renderedFrameCount);
                m_firstMsg = false;
                m_lastMsgTime = time;
                m_lastFrameId = 0;
                m_queue.Enqueue(new LogicFrame() {
                    frameId = 0,
                    offset = 0,
                    physicsFrames = physicsFrames
                });
            }
            else {
                m_queue.Enqueue(new LogicFrame() {
                    frameId = ++m_lastFrameId,
                    offset = time - m_lastMsgTime - 1f / FrameSync.LOGIC_FPS,
                    physicsFrames = physicsFrames
                });
                m_lastMsgTime = time;
            }
        }

        public bool pop(int maxFrameId, out LogicFrame msg) {
            msg = null;
            if (m_queue.Count == 0) {
                return false;
            }
            var _msg = m_queue.Peek();
            if (_msg.frameId <= maxFrameId) {
                msg = m_queue.Dequeue();
                return true;
            }
            return false;
        }

        public int nextFrameId { get { return m_lastFrameId + 1; } }


        private Queue<LogicFrame> m_queue = new Queue<LogicFrame>();

        bool m_firstMsg = false;
        private float m_lastMsgTime = 0;
        private int m_lastFrameId = -1;
        private RenderActionGenerator m_generator = new RenderActionGenerator();
    }
}
