using System.Collections.Generic;
using UnityEngine;

namespace SpriteAnimatorRuntime
{    
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(SpriteAnimatorEvent))]
    [AddComponentMenu("SpriteAnimator/Controller")]
    public class SpriteAnimatorController : MonoBehaviour
    {
        public SpriteAnimatorData Data;
        public int SelectAnimationId = -1;
        
        public SpriteAnimatorPlayer Player { get; private set; }
        private SpriteRenderer m_renderer { get; set; }

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
        }
        
        public void PlayAnimation(string path)
        {
            path = "Root/" + path;
            SpriteAnimation anim = Data.GetAnimation(path);
            if (anim != null && Player.Animation != anim)
                Player.PlayAnimation(anim);
            else if (anim == null)
                Player.PlayAnimation(anim); 
        }
    }
}
