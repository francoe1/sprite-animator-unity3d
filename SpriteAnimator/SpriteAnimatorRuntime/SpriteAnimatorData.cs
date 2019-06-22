using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SpriteAnimatorRuntime
{
    [Serializable]
    public class SpriteAnimatorData : ScriptableObject
    {
        public static SpriteAnimatorData Instance { get; private set; }

        [HideInInspector]
        public byte[] TreeViewData = new byte[0];
        [HideInInspector]
        public float ScaleSprite = 1;
        [HideInInspector]
        public float ScalePivot = 1;
        [HideInInspector]
        public Color PreviewBackgroundColor = Color.blue;
        [HideInInspector]
        public bool AutoScale = false;
        [HideInInspector]
        public bool DrawSpriteZone = false;

        

        [SerializeField, HideInInspector]
        private int m_lastId = 0;
        [SerializeField]
        private List<SpriteAnimation> m_animations = new List<SpriteAnimation>();
        [SerializeField, HideInInspector]
        private string m_lastSave = "";

        public int LastId { get { return m_lastId; } }
        public string LastSave { get { return m_lastSave; } }
        public SpriteAnimation[] Animations { get { return m_animations.ToArray(); } }

        [HideInInspector]
        public bool ShowAnimationOptions = true;
        [HideInInspector]
        public bool ShowFrameOptions = true;
        [HideInInspector]
        public bool ShowRenderInfo = true;
        [HideInInspector]
        public bool ShowRenderOptions = true;
        [HideInInspector]
        public bool ShowPivots = true;
        [HideInInspector]
        public bool ShowEvents = false;

        public SpriteAnimation CreateAnimation()
        {
            if (Application.isPlaying)
                throw new Exception("CreateAnimation is not available in runtime");
            m_lastId++;
            SpriteAnimation anim = new SpriteAnimation(m_lastId);
            m_animations.Add(anim);
            return anim;
        }

        public void RemoveAnimation(int id)
        {
            if (Application.isPlaying)
                throw new Exception("Create Animation is not available in runtime");
            m_animations.RemoveAll(x => x.Id == id);
        }

        public SpriteAnimation GetAnimation(int id)
        {
            return m_animations.Where(x => x.Id == id).SingleOrDefault();
        }

        public SpriteAnimation GetAnimation(string path)
        {
            return m_animations.Where(x => x.Path.ToLower() == path.ToLower()).SingleOrDefault();
        }

        public void Clear()
        {
            m_animations.Clear();
        }

        private void Reset()
        {
            Instance = this;
        }

        public void Save()
        {
            m_lastSave = DateTime.Now.ToString();
        }
    }    
}
