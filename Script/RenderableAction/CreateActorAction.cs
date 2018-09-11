using System.Collections.Generic;
using UnityEngine;
using Cratos;

namespace RAL
{
    public class CreateActorAction : InstantAction
    {
        public Vector2 position;
        public FBTeam team;
        public float[] runAnimiationNormalSpeeds;
        public float height;
        public bool gk;
        public string avatarName;
        private static readonly int maxAvatarPartNum = 5; //最多5个部件
        public Dictionary<string, string> avatarPart = new Dictionary<string, string>();
        public string name;
        public FiveElements element;
        public uint roleId;

        public void init(uint id,
                        uint roleId,
                        FBTeam team,
                        string name,
                        Vector2 position,
                        bool gk,
                        FiveElements element,
                        float height,
                        float[] runAnimiationNormalSpeeds)
        {
            base.init(id);
            this.roleId = roleId;
            this.team = team;
            this.name = name;
            this.position = position;
            this.gk = gk;
            this.element = element;
            this.height = height;
            this.runAnimiationNormalSpeeds = runAnimiationNormalSpeeds;
            switch (roleId)
            {
                case 1:
                     avatarName = "P0102_01";
                     avatarPart["000"] = "P0102_01_1_01";
                    break;
                case 2:
                    avatarName = "P0504_01";
                    avatarPart["000"] = "P0504_01_1_01";
                    break;
                case 3:
                    avatarName = "P0307_01";
                    avatarPart["000"] = "P0307_01_1_01";
                    break;
                case 4:
                    avatarName = "GK0701_01";
                    avatarPart["000"] = "GK0701_01_1_01";
                    break;
                case 5:
                    avatarName = "GK0701_01";
                    avatarPart["000"] = "GK0701_01_1_01";
                    break;
                case 6:
                    avatarName = "GK0701_01";
                    avatarPart["000"] = "GK0701_01_1_01";
                    break;
            }
        }

        public override void serialize(BytesStream stream)
        {
            base.serialize(stream);
            stream.Write((byte)team);
            stream.write(position);
            stream.Write(height);
            for (int i = 0; i < runAnimiationNormalSpeeds.Length; ++i)
            {
                stream.Write(runAnimiationNormalSpeeds[i]);
            }

            //avatar相关 begin
            stream.WriteStringByte(avatarName);
            int index = 0;
            foreach (var dict in avatarPart)
            {
                stream.WriteStringByte(dict.Key);
                stream.WriteStringByte(dict.Value);
                index++;
            }

            for (int i = index; i < maxAvatarPartNum; i++)
            {
                stream.WriteStringByte("");
                stream.WriteStringByte("");
            }
            //avatar相关 end

            stream.Write(gk);
            stream.WriteStringByte(name);
            stream.Write((byte)element);
            stream.Write(roleId);
        }
        public override void unserialize(BytesStream stream)
        {
            base.unserialize(stream);
            team = (FBTeam)stream.ReadByte();
            position = stream.readVector2();
            height = stream.ReadSingle();
            for (int i = 0; i < runAnimiationNormalSpeeds.Length; ++i)
            {
                runAnimiationNormalSpeeds[i] = stream.ReadSingle();
            }

            //avatar相关 begin
            avatarName = stream.ReadString();
            for (int i = 0; i < maxAvatarPartNum; i++)
            {
                avatarPart.Add(stream.ReadString(), stream.ReadString());
            }
            //avatar相关 end

            gk = stream.ReadBoolean();
            name = stream.ReadString();
            element = (FiveElements)stream.ReadByte();
            roleId = stream.ReadUInt32();
        }
    }

    public class MainActorCreatedAction : InstantAction
    {
        public FBTeam team;
        public void init(uint playerID, FBTeam team)
        {
            base.init(playerID);
            this.team = team;
        }

        public override void serialize(BytesStream stream)
        {
            base.serialize(stream);
            stream.Write((byte)team);
        }
        public override void unserialize(BytesStream stream)
        {
            base.unserialize(stream);
            team = (FBTeam)stream.ReadByte();
        }
    }
}
