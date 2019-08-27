using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpriteAnimatorRuntime
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(SpriteAnimatorEvent))]
    [AddComponentMenu("SpriteAnimator/Controller")]
    public class SpriteAnimatorController : MonoBehaviour
    {
        [HideInInspector]
        public SpriteAnimatorData Data;
        [HideInInspector]
        public int SelectAnimationId = -1;
        public SpriteAnimatorPlayer Player { get; private set; }
        private SpriteRenderer m_renderer { get; set; }

        public List<AnimationPivotSync> PivotSync = new List<AnimationPivotSync>();
                       
        private void Awake()
        {
            m_renderer = GetComponent<SpriteRenderer>();
            Player = new SpriteAnimatorPlayer();
            Data = Instantiate(Data);
        }

        private void Update()
        {
            Player.Update();
            switch(Player.State)
            {
                case SpriteAnimatorPlayer.PlayerState.SetFrameAffter:
                        m_renderer.sprite = Player.FrameSprite;
                    break;
            }

            foreach(AnimationPivotSync pivot in PivotSync)
            {
                if (Player.HasPivot(pivot.PivotName))
                {
                    if (pivot.WorldPosition)
                    {
                        pivot.Transform.position = transform.position + (Vector3)Player.GetPointOfPivot(pivot.PivotName);
                    }
                    else
                    {
                        pivot.Transform.localPosition = Player.GetPointOfPivot(pivot.PivotName);
                    }
                }
            }
        }
        
        public void PlayAnimation(string path)
        {
            path = "Root/" + path;

            SpriteAnimation anim = Data.GetAnimation(path);
            if (anim != null && Player.Animation != anim)
            {
                Player.PlayAnimation(anim);
                Player.Reset();
            }
        }
    }

    [Serializable]
    public class AnimationPivotSync
    {
        public string PivotName = "";
        public Transform Transform;
        public bool WorldPosition;
    }
}
