﻿using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    enum ObjectClassType
    {
        General = 0,
        Player,
        Entity,
        Ability,
        Effect,
        RenderEntity,
    }

    public abstract class Object : SignalGenerator, ILogicOwnerInfo, IDestruct, IExpressionVariableProvider
    {
        protected ObjectCreationContext m_context;
        protected SortedDictionary<int, Component> m_components = new SortedDictionary<int, Component>();
        protected bool m_is_delete_pending = false;

        #region 初始化、销毁
        public void Destruct()
        {
            NotifyGeneratorDestroyAndRemoveAllListeners();
            PreDestruct();
            var enumerator = m_components.GetEnumerator();
            while (enumerator.MoveNext())
            {
                Component component = enumerator.Current.Value;
                component.Destruct();
            }
            m_components.Clear();
            OnDestruct();
            //ZZWTODO 名字没取好
            if (OwnContext())
                m_context.Destruct();
        }

        protected virtual bool OwnContext()
        {
            return true;
        }

        protected virtual void PreDestruct()
        {
        }

        protected virtual void OnDestruct()
        {
        }

        public void Resurrect()
        {
            if (!m_is_delete_pending)
                return;
            m_is_delete_pending = false;
            var enumerator = m_components.GetEnumerator();
            while (enumerator.MoveNext())
            {
                Component component = enumerator.Current.Value;
                component.OnResurrect();
            }
        }
        #endregion

        #region GETTER
        public ObjectCreationContext GetCreationContext()
        {
            return m_context;
        }

        public int ID
        {
            get { return m_context.m_object_id; }
        }

        public string Name
        {
            get { return m_context.m_name; }
        }

        public FixPoint CreationTime
        {
            get { return m_context.m_creation_time; }
        }

        public bool IsLocal
        {
            get { return m_context.m_is_local; }
        }

        public bool IsAI
        {
            get { return m_context.m_is_ai; }
        }

        public bool DeletePending
        {
            get { return m_is_delete_pending; }
            set
            {
                if (m_is_delete_pending == value)
                    return;
                m_is_delete_pending = value;
                if (m_is_delete_pending)
                {
                    var enumerator = m_components.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        Component component = enumerator.Current.Value;
                        component.OnDeletePending();
                    }
                    PositionComponent position_component = GetComponent(PositionComponent.ID) as PositionComponent;
                    if (position_component != null)
                        position_component.ClearSpace();
                }
            }
        }
        #endregion

        #region SignalGenerator
        protected override LogicWorld GetLogicWorldForSignal()
        {
            return m_context.m_logic_world;
        }
        #endregion

        #region ILogicOwnerInfo
        public LogicWorld GetLogicWorld()
        {
            return m_context.m_logic_world;
        }
        public FixPoint GetCurrentTime()
        {
            return m_context.m_logic_world.CurrentTime;
        }
        public int GetOwnerObjectID()
        {
            return m_context.m_owner_id;
        }
        public virtual Object GetOwnerObject()
        {
            return null;
        }
        public virtual int GetOwnerPlayerID()
        {
            return 0;
        }
        public virtual Player GetOwnerPlayer()
        {
            return null;
        }
        public virtual int GetOwnerEntityID()
        {
            return 0;
        }
        public virtual Entity GetOwnerEntity()
        {
            return null;
        }
        #endregion

        #region 初始化
        public void InitializeObject(ObjectCreationContext context)
        {
            m_context = context;
            PreInitializeObject(context);
            InitializeComponents(context);
            PostInitializeObject(context);
        }

        protected virtual void PreInitializeObject(ObjectCreationContext context)
        {
        }

        void InitializeComponents(ObjectCreationContext context)
        {
            List<ComponentData> components_data = context.m_type_data.m_components_data;
            for (int i = 0; i < components_data.Count; ++i)
                AddComponent(components_data[i]);

            if (context.m_proto_data != null)
            {
                var attributes = context.m_proto_data.m_attributes;
                if (attributes != null && attributes.Count > 0)
                {
                    AttributeManagerComponent cmp = GetComponent(AttributeManagerComponent.ID) as AttributeManagerComponent;
                    if (cmp != null)
                    {
                        var enumerator = attributes.GetEnumerator();
                        while (enumerator.MoveNext())
                            cmp.SetAttributeBaseValue(enumerator.Current.Key, enumerator.Current.Value);
                    }
                }

                var skills = context.m_proto_data.m_skills;
                if (skills != null && skills.Count > 0)
                {
                    SkillManagerComponent cmp = GetComponent(SkillManagerComponent.ID) as SkillManagerComponent;
                    if (cmp != null)
                    {
                        var enumerator = skills.GetEnumerator();
                        while (enumerator.MoveNext())
                            cmp.AddSkill(enumerator.Current.Key, enumerator.Current.Value);
                    }
                }
            }

            for (int i = 0; i < components_data.Count; ++i)
            {
                Component component = GetComponent(components_data[i].m_component_type_id);
                if (component == null)
                    continue;
                component.InitializeComponent();
            }

            for (int i = 0; i < components_data.Count; ++i)
            {
                Component component = GetComponent(components_data[i].m_component_type_id);
                if (component == null)
                    continue;
                component.OnObjectCreated();
            }
        }

        void InitializeComponentVariables(ObjectTypeData type_data)
        {
            if (type_data == null)
                return;
            List<ComponentData> components_data = type_data.m_components_data;
            for (int i = 0; i < components_data.Count; ++i)
            {
                Dictionary<string, string> variables = components_data[i].m_component_variables;
                if (variables == null || variables.Count == 0)
                    continue;
                Component component = GetComponent(components_data[i].m_component_type_id);
                if (component == null)
                    continue;
                component.InitializeVariable(variables);
            }
        }

        protected virtual void PostInitializeObject(ObjectCreationContext context)
        {
        }

        public Component AddComponent(ComponentData component_data)
        {
            int component_type_id = component_data.m_component_type_id;
            if (!IsSuitableComponent(component_type_id))
                return null;
            Component component = ComponentTypeRegistry.CreateComponent(component_type_id);
            if (component == null)
                return null;
            component.ParentObject = this;
            component.ComponentTypeID = component_type_id;
            m_components[component_type_id] = component;
            if (component_data.m_component_variables != null)
                component.InitializeVariable(component_data.m_component_variables);
            return component;
        }

        public T AddComponent<T>() where T : Component
        {
            int component_type_id = ComponentTypeRegistry.ComponentType2ID(typeof(T));
            Component component = AddComponent(component_type_id);
            return component as T;
        }

        public Component AddComponent(int component_type_id)
        {
            if (GetComponent(component_type_id) != null)
                return null;
            ComponentData component_data = new ComponentData();
            component_data.m_component_type_id = component_type_id;
            return AddComponent(component_data);
        }

        protected virtual bool IsLogicObject()
        {
            return true;
        }

        protected virtual bool IsSuitableComponent(int component_type_id)
        {
            return ComponentTypeRegistry.IsLogicComponent(component_type_id);
        }
        #endregion

        #region Components
        public T GetComponent<T>(int component_type_id) where T : Component
        {
            // PositionComponent cmp = object.GetComponent<PositionComponent>(PositionComponent.ID);
            Component component;
            if (!m_components.TryGetValue(component_type_id, out component))
                return null;
            return component as T;
        }

        public T GetComponent<T>() where T : Component
        {
            // PositionComponent cmp = object.GetComponent<PositionComponent>();
            int component_type_id = ComponentTypeRegistry.ComponentType2ID(typeof(T));
            Component component;
            if (!m_components.TryGetValue(component_type_id, out component))
                return null;
            return component as T;
        }

        public Component GetComponent(System.Type type)
        {
            // PositionComponent cmp = object.GetComponent(typeof(PositionComponent)) as PositionComponent;
            int component_type_id = ComponentTypeRegistry.ComponentType2ID(type);
            Component component;
            m_components.TryGetValue(component_type_id, out component);
            return component;
        }

        public Component GetComponent(int component_type_id)
        {
            // PositionComponent cmp = object.GetComponent(PositionComponent.ID) as PositionComponent;
            Component component;
            m_components.TryGetValue(component_type_id, out component);
            return component;
        }

        public Component GetComponent(string component_name)
        {
            // PositionComponent cmp = object.GetComponent("PositionComponent") as PositionComponent;
            int component_type_id = (int)CRC.Calculate(component_name);
            Component component;
            m_components.TryGetValue(component_type_id, out component);
            return component;
        }

        public void GetInterfaceInComponents<T>(ref List<T> interfaceList) where T : class
        {
            if (interfaceList == null)
                interfaceList = new List<T>();
            var enumerator = m_components.GetEnumerator();
            while (enumerator.MoveNext())
            {
                Component component = enumerator.Current.Value;
                T it = component as T;
                if (it != null)
                {
                    interfaceList.Add(it);
                }
            }
        }
        #endregion

        #region Variable
        public FixPoint GetVariable(ExpressionVariable variable, int index)
        {
            int vid = variable[index];
            if (index == variable.MaxIndex)
            {
                return ObjectUtil.GetVariable(this, vid);
            }
            else if (vid == ExpressionVariable.VID_LevelTable)
            {
                return GetLogicWorld().GetConfigProvider().GetLevelBasedNumber(variable[index + 1], ObjectUtil.GetLevel(this));
            }
            else if (vid == ExpressionVariable.VID_Attribute)
            {
                AttributeManagerComponent cmp = GetComponent(AttributeManagerComponent.ID) as AttributeManagerComponent;
                if (cmp != null)
                    return cmp.GetVariable(variable, index + 1);
            }
            else if (vid == ExpressionVariable.VID_Object)
            {
                Object owner_object = GetOwnerObject();
                if (owner_object != null)
                    return owner_object.GetVariable(variable, index + 1);
            }
            else if (vid == ExpressionVariable.VID_Entity)
            {
                Object owner_entity = GetOwnerEntity();
                if (owner_entity != null)
                    return owner_entity.GetVariable(variable, index + 1);
            }
            else if (vid == ExpressionVariable.VID_Player)
            {
                Object owner_player = GetOwnerPlayer();
                if (owner_player != null)
                    return owner_player.GetVariable(variable, index + 1);
            }
            else if (vid == ExpressionVariable.VID_Master)
            {
                Object master = GetOwnerEntity();
                SummonedEntityComponent summoned_component = GetComponent(SummonedEntityComponent.ID) as SummonedEntityComponent;
                if (summoned_component != null)
                {
                    Entity entity = GetLogicWorld().GetEntityManager().GetObject(summoned_component.MasterID);
                    if (entity != null)
                        master = entity;
                }
                return master.GetVariable(variable, index + 1);
            }
            else if (vid == ExpressionVariable.VID_UltimateMaster)
            {
                Object master = GetOwnerEntity();
                SummonedEntityComponent summoned_component = GetComponent(SummonedEntityComponent.ID) as SummonedEntityComponent;
                while (summoned_component != null)
                {
                    Entity entity = GetLogicWorld().GetEntityManager().GetObject(summoned_component.MasterID);
                    if (entity == null)
                        break;
                    master = entity;
                    summoned_component = entity.GetComponent(SummonedEntityComponent.ID) as SummonedEntityComponent;
                }
                return master.GetVariable(variable, index + 1);
            }
            Object owner = GetOwnerObject();
            if (owner != null)
                return owner.GetVariable(variable, index);
            else
                return FixPoint.Zero;
        }
        #endregion
    }
}