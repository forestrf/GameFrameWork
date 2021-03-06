﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Combat
{
    public partial class ModelComponent : RenderEntityComponent
    {
        //配置数据
        string m_asset_name;
        string m_bodyctrl_path;
        string m_headctrl_path;
        //运行数据
        GameObject m_unity_go;
        Transform m_bodyctrl_tr;
        GameObject m_bodyctrl_obj;
        Transform m_headctrl_tr;
        GameObject m_headctrl_obj;
        PositionComponent m_position_component;
        Vector3 m_last_position = Vector3.zero;
        PredictLogicComponent m_predict_component = null;

        #region GETTER
        public GameObject GetUnityGameObject()
        {
            return m_unity_go;
        }

        public Vector3 GetCurrentPosition()
        {
            return m_last_position;
        }

        public void SetPredictComponent(PredictLogicComponent predict_component)
        {
            m_predict_component = predict_component;
        }

        public void SetBaseAngle(float angle)
        {
            m_bodyctrl_tr.localEulerAngles = new Vector3(0, angle, 0);
            //Turn(new Vector3(0, angle, 0), 0.2f);
        }

        public void Turn(Vector3 vec, float duration)
        {
            iTween.RotateTo(m_bodyctrl_obj, iTween.Hash("rotation", vec, "time", duration, "easetype", "linear"));
        }
        #endregion

        #region 初始化/销毁
        public override void InitializeComponent()
        {
            ObjectProtoData proto_data = ParentObject.GetCreationContext().m_proto_data;
            if (proto_data == null)
                return;
            var dic = proto_data.m_component_variables;
            if (dic == null)
                return;
            string asset;
            if (dic.TryGetValue("asset", out asset))
                m_asset_name = asset;
            CreateModel();
        }

        void CreateModel()
        {
            m_unity_go = UnityResourceManager.Instance.CreateGameObject(m_asset_name);
            if (m_unity_go == null)
                return;

            if (m_bodyctrl_path != null)
                m_bodyctrl_tr = m_unity_go.transform.FindChild(m_bodyctrl_path);
            else
                m_bodyctrl_tr = m_unity_go.transform;
            m_bodyctrl_obj = m_bodyctrl_tr.gameObject;

            if (m_headctrl_path != null)
                m_headctrl_tr = m_unity_go.transform.FindChild(m_headctrl_path);
            if (m_headctrl_tr != null)
                m_headctrl_obj = m_headctrl_tr.gameObject;

            Entity logic_entity = GetLogicEntity();
            m_position_component = logic_entity.GetComponent(PositionComponent.ID) as PositionComponent;
            if (m_position_component != null)
            {
                m_last_position = RenderWorld.Vector3FP_To_Vector3(m_position_component.CurrentPosition);
                m_bodyctrl_tr.localPosition = m_last_position;
                m_bodyctrl_tr.localEulerAngles = new Vector3(0, (float)m_position_component.BaseAngle, 0);
                if (m_headctrl_tr != null)
                    m_bodyctrl_tr.localEulerAngles = new Vector3(0, (float)m_position_component.HeadAngle, 0);
            }

            UnityObjectBinding binding = m_bodyctrl_tr.gameObject.GetComponent<UnityObjectBinding>();
            if (binding == null)
                binding = m_bodyctrl_tr.gameObject.AddComponent<UnityObjectBinding>();
            binding.EntityID = logic_entity.ID;
        }

        protected override void OnDestruct()
        {
            if (m_unity_go != null)
            {
                UnityResourceManager.Instance.RecycleGameObject(m_asset_name, m_unity_go);
                m_unity_go = null;
            }
        }
        #endregion

        public bool UpdatePosition()
        {
            if (m_unity_go == null)
            {
                LogWrapper.LogError("ModelComponent.UpdatePosition, Object has already been destroyed！！！！！！！！！");
                return false; //ZZWTODO
            }
            m_last_position = RenderWorld.Vector3FP_To_Vector3(m_position_component.CurrentPosition);
            if (m_predict_component != null)
                m_predict_component.OnLogicUpdatePosition(m_last_position - m_bodyctrl_tr.localPosition);
            m_bodyctrl_tr.localPosition = m_last_position;
            return true;
        }

        public void UpdateAngle()
        {
            if (m_position_component.BaseRotatable)
                m_bodyctrl_tr.localEulerAngles = new Vector3(0, (float)m_position_component.BaseAngle, 0);
            else if (m_headctrl_tr != null)
                m_headctrl_tr.localEulerAngles = new Vector3(0, (float)m_position_component.HeadAngle, 0);
        }
        
        public override void Show(bool is_show)
        {
            if (m_unity_go != null)
                m_unity_go.SetActive(is_show);
        }
    }
}