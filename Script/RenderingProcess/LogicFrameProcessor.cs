
using System;
using UnityEngine;

namespace RenderingProcess {
    public class LogicFrameProcessor {

        public float timeScale { get { return m_timeScale; } }

        public RAL.LogicFrameQueue logicFrameQueue = null;

        public void reset()
        {
            m_firstFrame = true;
            m_time = 0;
            m_maxFrameId = -1;

            m_totalTimeOffset = 0;
            m_blocked = false;
            m_delayTime = 0;
            m_timeScale = 1;

            if (m_currentFrame != null) {
                m_currentFrame.release(logicFrameQueue.renderActionGenerator);
                m_currentFrame = null;
            }
            m_currentPhysicsIndex = -1;
        }

        public void update() {
            float time = 0;
            if (m_firstFrame) {
                m_firstFrame = false;
                UnityEngine.Debug.Log(UnityEngine.Time.renderedFrameCount);
                m_time = 0;
                m_maxFrameId = -1;
            }
            else {
                time = _advance();
            }

            int maxFrameId = (int)(time * FrameSync.LOGIC_FPS);
            if (maxFrameId == m_maxFrameId) {
                m_time = time;
                _processCurrentFrame(m_time - m_maxFrameId / (float)FrameSync.LOGIC_FPS);
            }
            else {
                int msgCount = 0;

                RAL.LogicFrame msg;
                while (logicFrameQueue.pop(maxFrameId, out msg))
                {
                    _processNewFrame(msg, time - msg.frameId / (float)FrameSync.LOGIC_FPS);
                    ++msgCount;

                    if (m_blocked) {
                        m_blocked = false;
                    }
                    else {
                        m_totalTimeOffset += msg.offset;
                    }
                }

                if (msgCount != 0) {
                    m_maxFrameId = maxFrameId;
                    m_time = time;
                    
                }
                else {
                    
                    if (!m_blocked) {
                        m_time = (m_maxFrameId + 1) / (float)FrameSync.LOGIC_FPS;
                        m_blocked = true;
                        m_totalTimeOffset = 0;
                    }
                    m_delayTime += time - m_time;
                    if (m_delayTime > 0.2f) {
                        m_delayTime = 0.2f;
                    }
                }
            }
        }

        private float _advance() {
            if (m_delayTime > 0) {
                m_delayTime -= Time.unscaledDeltaTime * Mathf.Max(m_delayTime, 0.2f);
                if (m_delayTime < 0) {
                    m_delayTime = 0;
                }
            }
            float scale = 1f;
            float offset = m_totalTimeOffset + m_delayTime;

            if (offset <= 0) {
                var a = -offset / Time.unscaledDeltaTime;
                if (a > 0.5f) {
                    scale = 1.5f;
                    m_totalTimeOffset += Time.unscaledDeltaTime * 0.5f;
                }
                else {
                    m_totalTimeOffset = 0;
                    scale += a;
                }
            }
            m_timeScale = scale;
            return m_time + Time.unscaledDeltaTime * scale;
        }

        private void _processCurrentFrame(float cursor) {
            if (m_currentFrame == null) {
                return;
            }
            _process(ref m_currentFrame, ref m_currentPhysicsIndex, cursor);
        }

        private void _processNewFrame(RAL.LogicFrame frame, float cursor) {
            if (m_currentFrame != null) {
                for (int i = m_currentPhysicsIndex; i < m_currentFrame.physicsFrames.Length; ++i) {
                    for (int j = 0; j < m_currentFrame.physicsFrames[i].actions.Length; ++j) {
                        _doRenderActionDone(m_currentFrame.physicsFrames[i].actions[j]);
                    }
                }
                _doLogicFrameEnd(m_currentFrame);
                m_currentFrame = null;
                m_currentPhysicsIndex = -1;
            }
            if (frame != null) {
                m_currentFrame = frame;
                m_currentPhysicsIndex = 0;
                _doLogicFrameBegin(frame);
                _process(ref m_currentFrame, ref m_currentPhysicsIndex, cursor);
            }
        }

        private void _process(ref RAL.LogicFrame frame, ref int physicsIndex, float cursor) {
            int nowIndex = (int)(cursor * FrameSync.LOGIC_PHYSICS_FPS);
            if (nowIndex >= frame.physicsFrames.Length) {
                for (int i = physicsIndex; i < m_currentFrame.physicsFrames.Length; ++i) {
                    for (int j = 0; j < frame.physicsFrames[i].actions.Length; ++j) {
                        _doRenderActionDone(frame.physicsFrames[i].actions[j]);
                    }
                }
                physicsIndex = -1;
                _doLogicFrameEnd(frame);
                frame = null;
            }
            else {
                for (int i = physicsIndex; i < nowIndex; ++i) {
                    for (int j = 0; j < frame.physicsFrames[i].actions.Length; ++j) {
                        _doRenderActionDone(frame.physicsFrames[i].actions[j]);
                    }
                }
                physicsIndex = nowIndex;
                var progress = cursor * FrameSync.LOGIC_PHYSICS_FPS - nowIndex;
                for (int i = 0; i < frame.physicsFrames[nowIndex].actions.Length; ++i) {
                    _doRenderActionProgress(frame.physicsFrames[nowIndex].actions[i], progress);
                }
            }
        }

        private void _doRenderActionProgress(RAL.RenderAction ra, float progress) {
            RenderActionProcessorFactory.instance.doProgress(ra, progress);
        }

        private void _doRenderActionDone(RAL.RenderAction ra) {
            RenderActionProcessorFactory.instance.doDone(ra);
        }

        private void _doLogicFrameBegin(RAL.LogicFrame frame) {

            if (onLogicFrameBegin != null)
                onLogicFrameBegin(frame);
        }

        private void _doLogicFrameEnd(RAL.LogicFrame frame) {
            if (onLogicFrameEnd != null)
                onLogicFrameEnd(frame);

        }


        public Action<RAL.LogicFrame> onLogicFrameBegin = null;
        public Action<RAL.LogicFrame> onLogicFrameEnd = null;

        bool m_isReplaying = false;
        bool m_firstFrame = true;
        float m_time = 0;
        int m_maxFrameId = -1;

        float m_totalTimeOffset = 0;
        bool m_blocked = false;
        float m_delayTime = 0;
        float m_timeScale = 1;

        RAL.LogicFrame m_currentFrame = null;
        int m_currentPhysicsIndex = -1;
    }
}
