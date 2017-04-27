﻿using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class SkillManagerComponent : EntityComponent, ISignalListener
    {
        LocomotorComponent m_locomotor_cmp;
        SignalListenerContext m_listener_context;
        int m_default_skill_cfgid = 0;
        int m_default_skill_id = 0;
        SortedDictionary<int, int> m_skill_cfgid2id = new SortedDictionary<int, int>();  //ZZWTODO CRCID?
        int m_move_block_count = 0;
        int m_active_block_count = 0;
        List<int> m_active_skill_ids = new List<int>();

        #region 初始化/销毁
        public void AddSkill(int cfgid, bool default_skill = false)
        {
            if (default_skill)
                m_default_skill_cfgid = cfgid;
            m_skill_cfgid2id[cfgid] = -1;
        }

        protected override void PostInitializeComponent()
        {
            SkillManager skill_manager = GetLogicWorld().GetSkillManager();
            var enumerator = m_skill_cfgid2id.GetEnumerator();
            while(enumerator.MoveNext())
            {
                int skill_cfgid = enumerator.Current.Key;
                //ZZWTODO 技能配置
                ObjectTypeData type_data = null;
                ObjectCreationContext object_context = new ObjectCreationContext();
                object_context.m_logic_world = GetLogicWorld();
                object_context.m_object_type_id = skill_cfgid;
                object_context.m_type_data = type_data;
                object_context.m_owner_id = ParentObject.ID;
                Skill skill = skill_manager.CreateObject(object_context);
                m_skill_cfgid2id[skill_cfgid] = skill.ID;
                if (skill_cfgid == m_default_skill_cfgid)
                    m_default_skill_id = skill.ID;
            }

            m_locomotor_cmp = ParentObject.GetComponent(LocomotorComponent.ID) as LocomotorComponent;
            m_listener_context = SignalListenerContext.CreateForEntityComponent(GetLogicWorld().GenerateSignalListenerID(), ParentObject.ID, m_component_type_id);
            ParentObject.AddListener(SignalType.StartMoving, m_listener_context);
        }

        protected override void OnDestruct()
        {
            m_locomotor_cmp = null;
            ParentObject.RemoveListener(SignalType.StartMoving, m_listener_context.ID);
            SignalListenerContext.Recycle(m_listener_context);
            m_listener_context = null;
        }
        #endregion

        #region ISignalListener
        public void ReceiveSignal(ISignalGenerator generator, int signal_type, System.Object signal = null)
        {
            switch (signal_type)
            {
            case SignalType.StartMoving:
                OnMovementStart();
                break;
            default:
                break;
            }
        }

        private void OnMovementStart()
        {
            SkillManager skill_manager = GetLogicWorld().GetSkillManager();
            var enumerator = m_active_skill_ids.GetEnumerator();
            while(enumerator.MoveNext())
            {
                Skill skill = skill_manager.GetObject(enumerator.Current);
                if(skill != null)
                {
                    SkillDefinitionComponent def_cmp = skill.GetSkillDefinitionComponent();
                    if (def_cmp.DeactivateWhenMoving)
                        skill.Interrupt();
                }
            }
        }

        public void OnGeneratorDestroyed(ISignalGenerator generator)
        {
        }
        #endregion

        public Skill GetDefaultSkill()
        {
            return GetLogicWorld().GetSkillManager().GetObject(m_default_skill_id);
        }

        public Skill GetSkill(int skill_cfgid)
        {
            int skill_id;
            if (m_skill_cfgid2id.TryGetValue(skill_cfgid, out skill_id))
                return GetLogicWorld().GetSkillManager().GetObject(skill_id);
            else
                return null;
        }

        public bool CanActivateSkill()
        {
            if (!IsEnable())
                return false;
            return m_active_block_count == 0;
        }

        public void OnSkillActivated(Skill skill)
        {
            SkillDefinitionComponent def_cmp = skill.GetSkillDefinitionComponent();
            if(def_cmp.BlocksMovementWhenActive)
            {
                ++m_move_block_count;
                if(m_move_block_count == 1)
                {
                    if (m_locomotor_cmp != null)
                        m_locomotor_cmp.Disable();
                }
            }
            if (def_cmp.BlocksOtherSkillsWhenActive)
                ++m_active_block_count;
            m_active_skill_ids.Add(skill.ID);
        }

        public void OnSkillDeactivated(Skill skill)
        {
            SkillDefinitionComponent def_cmp = skill.GetSkillDefinitionComponent();
            if (def_cmp.BlocksMovementWhenActive)
            {
                --m_move_block_count;
                if(m_move_block_count == 0)
                {
                    if (m_locomotor_cmp != null)
                        m_locomotor_cmp.Enable();
                }
            }
            if (def_cmp.BlocksOtherSkillsWhenActive)
                --m_active_block_count;
            m_active_skill_ids.Remove(skill.ID);
        }

        public bool IsMoveAllowed()
        {
            return m_move_block_count == 0;
        }
    }
}
