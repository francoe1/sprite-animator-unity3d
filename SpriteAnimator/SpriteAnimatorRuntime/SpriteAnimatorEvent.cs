using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace SpriteAnimatorRuntime
{
    [RequireComponent(typeof(SpriteAnimatorController))]
    public class SpriteAnimatorEvent : MonoBehaviour
    {
        private SpriteAnimatorController m_controller { get; set; }

        public EndFrameUnityEvent EndFrameEvent;
        public FinishUnityEvent FinishEvent;

        private void Awake()
        {
            m_controller = GetComponent<SpriteAnimatorController>();
        }            

        private void Start()
        {
            m_controller.Player.PlayerUpdateHanlder = OnPlayerUpdateHanlder;
        }

        private void OnPlayerUpdateHanlder(SpriteAnimatorPlayer.PlayerState state)
        {
            switch(state)
            {
                case SpriteAnimatorPlayer.PlayerState.TimeOut:
                    EndFrameEvent.Invoke(m_controller.Player.FrameIndex);
                    break;

                case SpriteAnimatorPlayer.PlayerState.Finish:
                    FinishEvent.Invoke();
                    break;
            }
        }

        [Serializable]
        public class EndFrameUnityEvent : UnityEvent<int> { }
        [Serializable]
        public class FinishUnityEvent : UnityEvent { }
    }
}
