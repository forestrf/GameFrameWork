﻿using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class EntityManager : ObjectManager<Entity>
    {
        public EntityManager(LogicWorld logic_world)
            : base(logic_world, IDGenerator.ENTITY_FIRST_ID)
        {
        }

        protected override Entity CreateObjectInstance(ObjectCreationContext context)
        {
            return new Entity();
        }

        protected override void AfterObjectCreated(Entity entity)
        {
            Player player = entity.GetOwnerPlayer();
            if (player != null)
                player.AddEntity(entity);
            PositionComponent position_component = entity.GetComponent(PositionComponent.ID) as PositionComponent;
            if (position_component != null && position_component.Visible)
                m_logic_world.AddSimpleRenderMessage(RenderMessageType.CreateEntity, entity.ID);
        }

        protected override void PreDestroyObject(Entity entity)
        {
            entity.DeletePending = true;
            Player player = entity.GetOwnerPlayer();
            player.RemoveEntity(entity);
            m_logic_world.AddSimpleRenderMessage(RenderMessageType.DestroyEntity, entity.ID);
        }
    }
}