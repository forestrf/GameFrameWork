﻿using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class CommandType
    {
        public const int EntityMove = 10;
    }

    public class EntityMoveCommand : Command
    {
        public int m_entity_id = 0;
        public Vector3FP m_destination;
        public EntityMoveCommand()
        {
            m_type = CommandType.EntityMove;
        }

        public override void Reset()
        {
            base.Reset();
            m_entity_id = 0;
            m_destination.MakeZero();
        }
    }
}