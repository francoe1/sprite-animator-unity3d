using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpriteAnimatorRuntime
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(SpriteAnimatorEvent))]
    [AddComponentMenu("SpriteAnimator/SpriteAnimatorController")]
    public class SpriteAnimatorController : MonoBehaviour
    {
        [HideInInspector]
        public SpriteAnimatorData Data;
        [HideInInspector]
        public int SelectAnimationId = -1;
        public SpriteAnimatorPlayer Player { get; private set; }
        private SpriteRenderer m_renderer { get; set; }

        public List<AnimationPivotSync> PivotSync = new List<AnimationPivotSync>();


        private List<SpriteAnimation> m_cacheAnimations = new List<SpriteAnimation>();
                       
        private void Awake()
        {
            m_renderer = GetComponent<SpriteRenderer>();
            Player = new SpriteAnimatorPlayer();
            Data = Instantiate(Data);
        }

        private void Start()
        {
            if (SelectAnimationId != -1)
                Player.PlayAnimation(Data.GetAnimation(SelectAnimationId));
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
                    Vector2 pos = Player.GetPointOfPivot(pivot.PivotName);
                    if (m_renderer.flipX) pos.x *= -1;
                    if (m_renderer.flipY) pos.y *= -1;

                    if (pivot.IsLocal)
                        pivot.Transform.localPosition = pos;
                    else
                        pivot.Transform.position = transform.position + (Vector3)pos;
                }
            }
        }
        
        public void PlayAnimation(string path)
        {
            path = "Root/" + path;

            //SpriteAnimation anim = GetAnimation(path);
            SpriteAnimation anim = Data.GetAnimation(path);
            if (anim != null && Player.Animation != anim)
            {
                Player.PlayAnimation(anim);
                Player.Reset();
            }
        }

        private SpriteAnimation GetAnimation(string path)
        {
            for (int i = 0; i < m_cacheAnimations.Count; i++)
            {
                if (m_cacheAnimations[i] == null)
                {
                    m_cacheAnimations.RemoveAt(i);
                    continue;
                }

                if(m_cacheAnimations[i].Path.ToLower().Equals(path.ToLower()))
                {
                    return m_cacheAnimations[i];
                }
            }
           
            if (m_cacheAnimations.Count > 20) m_cacheAnimations.RemoveAt(0);

            SpriteAnimation anim = Data.GetAnimation(path);
            if (anim != null) m_cacheAnimations.Add(anim);
            return anim;
        }
    }

    [Serializable]
    public class AnimationPivotSync
    {
        public string PivotName = "";
        public Transform Transform;
        public bool IsLocal = true;
    }
}
