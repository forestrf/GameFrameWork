﻿using System.Collections;
using System.Collections.Generic;
using System;
using BaseUtil;
namespace Combat
{
    public partial class CommandType
    {
        public const int Invalid = 0;
        public const int SyncTurnDone = 1;
        public const int RandomTest = 2;
    }

    public class Command : IRecyclable
    {
        [ProtoBufAttribute(Index = 1)] 
        public long m_player_pstid = -1;
        [ProtoBufAttribute(Index = 2)]
        public int m_type = CommandType.Invalid;
        [ProtoBufAttribute(Index = 3)]
        public int m_syncturn = -1;

        public long PlayerPstid
        {
            get { return m_player_pstid; }
            set { m_player_pstid = value; }
        }
        public int Type
        {
            get { return m_type; }
        }
        public int SyncTurn
        {
            get { return m_syncturn; }
            set { m_syncturn = value; }
        }

        public virtual void Reset()
        {
            m_player_pstid = -1;
            m_type = CommandType.Invalid;
            m_syncturn = -1;
        }
        ////具体数据是什么，可以固定为几个int；或者提供序列化接口，以下只是随便写写
        //public int Serialize(char[] buff, int index)
        //{
        //    return 0;
        //}
        //public int Unserialize(char[] buff, int index)
        //{
        //    return 0;
        //}
    }

    public class SyncTurnDoneCommand : Command
    {
        public SyncTurnDoneCommand()
        {
            m_type = CommandType.SyncTurnDone;
        }
    }

    public class RandomTestCommand : Command
    {
        [ProtoBufAttribute(Index = 1)]
        public int m_random = 0;

        public RandomTestCommand()
        {
            m_type = CommandType.RandomTest;
        }

        public override void Reset()
        {
            base.Reset();
            m_random = 0;
        }
    }
}
