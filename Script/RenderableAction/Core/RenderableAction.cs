using System;
using Cratos;

namespace RAL
{
    //����ʾ�Ķ���������
    public abstract class RenderAction : IRecycleObject, ISerialize
    {
        public readonly RenderableActionID typeID;
        protected BitStream _objectID = new BitStream(1);    //����ID
        public virtual bool recycleable { get { return true; } }

        public RenderAction()
        {
            typeID = (RenderableActionID)(Enum.Parse(typeof(RenderableActionID), GetType().Name));
        }

        public virtual void init(uint objID)
        {
            this.objectID = objID;
        }

        public virtual uint objectID
        {
            get
            {
                return _objectID.getBit(0, 7);
            }
            set
            {
                _objectID.setBit(0, 7, value);
            }
        }

        //=============����====================
        public virtual void onGetNew()
        {
            //��������
            objectID = 0;
        }


        //==============���л�
        public virtual void serialize(BytesStream stream)
        {
            stream.Write(_objectID.data);
        }
        public virtual void unserialize(BytesStream stream)
        {
            _objectID.data = stream.ReadBytes(1);
        }
    };

    public abstract class InstantAction : RenderAction { }
    public abstract class ContinuousAction : RenderAction { }
}