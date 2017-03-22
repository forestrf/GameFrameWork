﻿using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class ManualConfigProvider : Singleton<ManualConfigProvider>, IConfigProvider
    {
        Dictionary<int, LevelData> m_level_data = new Dictionary<int, LevelData>();
        public Dictionary<int, ObjectTypeData> m_object_type_data = new Dictionary<int, ObjectTypeData>();
        public Dictionary<int, ObjectProtoData> m_object_proto_data = new Dictionary<int, ObjectProtoData>();

        private ManualConfigProvider()
        {
            InitLevelData();
            InitObjectTypeData();
            InitObjectProtoData();
        }

        public override void Destruct()
        {
        }

        #region IConfigProvider
        public LevelData GetLevelData(int id)
        {
            LevelData level_data = null;
            if (!m_level_data.TryGetValue(id, out level_data))
                return null;
            return level_data;
        }

        public ObjectTypeData GetObjectTypeData(int id)
        {
            ObjectTypeData type_data = null;
            if (!m_object_type_data.TryGetValue(id, out type_data))
                return null;
            return type_data;
        }

        public ObjectProtoData GetObjectProtoData(int id)
        {
            ObjectProtoData proto_data = null;
            if (!m_object_proto_data.TryGetValue(id, out proto_data))
                return null;
            return proto_data;
        }

        public AttributeData GetAttributeData(int id)
        {
            return null;
        }
        #endregion
        
        #region 手工配置
        void InitLevelData()
        {
            LevelData level_data = new LevelData();
            level_data.m_scene_name = "Scenes/zzw_test";
            m_level_data[1] = level_data;
        }

        void InitObjectTypeData()
        {
            //Player
            ObjectTypeData type_data = new ObjectTypeData();
            type_data.m_name = "EnvironmentPlayer";
            m_object_type_data[1] = type_data;

            type_data = new ObjectTypeData();
            type_data.m_name = "AIEnemyPlayer";
            m_object_type_data[2] = type_data;

            type_data = new ObjectTypeData();
            type_data.m_name = "LocalPlayer";
            m_object_type_data[3] = type_data;

            //Entity
            type_data = new ObjectTypeData();
            type_data.m_name = "Hero";
            ComponentData cd = new ComponentData();
            cd.m_component_type_id = ComponentTypeRegistry.CT_PositionComponent;
            cd.m_component_variables["ext_x"] = "500";
            cd.m_component_variables["ext_y"] = "500";
            cd.m_component_variables["ext_z"] = "500";
            cd.m_component_variables["visible"] = "True";
            type_data.m_components_data.Add(cd);

            cd = new ComponentData();
            cd.m_component_type_id = ComponentTypeRegistry.CT_LocomotorComponent;
            cd.m_component_variables["max_speed"] = "500";
            type_data.m_components_data.Add(cd);

            cd = new ComponentData();
            cd.m_component_type_id = ComponentTypeRegistry.CT_ModelComponent;
            type_data.m_components_data.Add(cd);

            m_object_type_data[101] = type_data;
        }

        void InitObjectProtoData()
        {
            ObjectProtoData proto_data = new ObjectProtoData();
            proto_data.m_name = "Cube";
            proto_data.m_component_variables["asset"] = "Objects/3D/zzw_cube";
            m_object_proto_data[101001] = proto_data;

            proto_data = new ObjectProtoData();
            proto_data.m_name = "Sphere";
            proto_data.m_component_variables["asset"] = "Objects/3D/zzw_sphere";
            m_object_proto_data[101002] = proto_data;
        }
        #endregion
    }
}