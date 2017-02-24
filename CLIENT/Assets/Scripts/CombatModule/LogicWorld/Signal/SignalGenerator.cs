﻿using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public interface ISignalGenerator
    {
        void AddListener(SignalType signal_type, SignalListenerContext listener_context);
        void RemoveListener(SignalType signal_type, int listener_id);
        void RemoveAllListeners();
        void SendSignal(SignalType signal_type, Signal signal);
        void NotifyGeneratorDestroy();
    }

    public abstract class SignalGenerator : ISignalGenerator
    {
        SortedDictionary<int, List<SignalListenerContext>> m_all_type_listeners;

        protected abstract LogicWorld GetLogicWorldForSignal();

        public void AddListener(SignalType signal_type_enum, SignalListenerContext listener_context)
        {
            int signal_type = (int)(signal_type_enum);
            if (m_all_type_listeners == null)
                m_all_type_listeners = new SortedDictionary<int, List<SignalListenerContext>>();
            List<SignalListenerContext> listeners;
            if (!m_all_type_listeners.TryGetValue(signal_type, out listeners))
            {
                listeners = new List<SignalListenerContext>();
                m_all_type_listeners[signal_type] = listeners;
            }
            for (int i = 0; i < listeners.Count; ++i)
            {
                if (listeners[i].m_listener_id == listener_context.m_listener_id)
                    return;
            }
            listeners.Add(listener_context);
        }

        public void RemoveListener(SignalType signal_type_enum, int listener_id)
        {
            int signal_type = (int)(signal_type_enum);
            if (m_all_type_listeners == null)
                return;
            List<SignalListenerContext> listeners;
            if (!m_all_type_listeners.TryGetValue(signal_type, out listeners))
                return;
            for (int i = 0; i < listeners.Count; ++i)
            {
                if (listeners[i].m_listener_id != listener_id)
                    continue;
                listeners.RemoveAt(i);
                return;
            }
        }

        public void RemoveAllListeners()
        {
            m_all_type_listeners.Clear();
        }

        public void SendSignal(SignalType signal_type_enum, Signal signal)
        {
            int signal_type = (int)(signal_type_enum);
            if (m_all_type_listeners == null)
                return;
            List<SignalListenerContext> listeners;
            if (!m_all_type_listeners.TryGetValue(signal_type, out listeners))
                return;
            int pre_count = listeners.Count;
            int left_count = pre_count;
            int index = 0;
            LogicWorld logic_world = GetLogicWorldForSignal();
            while (left_count > 0)
            {
                --left_count;
                SignalListenerContext context = listeners[index];
                ISignalListener listener = context.GetListener(logic_world);
                if (listener == null)
                    listeners.RemoveAt(index);
                else
                    listener.ReceiveSignal(this, signal_type_enum, signal);
                int cur_count = listeners.Count;
                if (cur_count < pre_count)
                    pre_count = cur_count;
                else
                    ++index;
            }
        }

        public void NotifyGeneratorDestroy()
        {
            if (m_all_type_listeners == null)
                return;
            LogicWorld logic_world = GetLogicWorldForSignal();
            var enumerator = m_all_type_listeners.GetEnumerator();
            while (enumerator.MoveNext())
            {
                List<SignalListenerContext> listeners = enumerator.Current.Value;
                for (int i = 0; i < listeners.Count; ++i)
                {
                    SignalListenerContext context = listeners[i];
                    ISignalListener listener = context.GetListener(logic_world);
                    if (listener != null)
                        listener.OnGeneratorDestroyed(this);
                }
            }
        }
    }
}