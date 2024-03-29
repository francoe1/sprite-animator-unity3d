﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SpriteAnimatorRuntime
{
    [Serializable]
    public class SpriteAnimatorData : ScriptableObject, ISerializationCallbackReceiver
    {
        public static SpriteAnimatorData Instance { get; private set; }
        
        [HideInInspector, System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public byte[] TreeViewData = new byte[0];
        [HideInInspector, System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public byte[] EditorData = new byte[0];
        [HideInInspector, System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public bool ShowAnimationTree = true;
        [HideInInspector, System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public bool ShowAnimationSetting = true;
        [HideInInspector, System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public float ScaleSprite = 1;
        [HideInInspector, System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public float ScalePivot = 1;
        [HideInInspector, System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public Color PreviewBackgroundColor = Color.blue;
        [HideInInspector, System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
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
            for(int i = 0; i < m_animations.Count; i++)
                if (m_animations[i].Path.ToLower().Equals(path.ToLower())) return m_animations[i];
            return null;
        }

        public void Clear()
        {
            m_animations.Clear();
        }

        private void Reset()
        {
            Instance = this;
        }

        public void OnBeforeSerialize()
        {
            m_lastSave = DateTime.Now.ToString();
        }

        public void OnAfterDeserialize()
        {

        }

        public void CopyAnimation(int originId, int destineId)
        {
            SpriteAnimation animOr = GetAnimation(originId);
            SpriteAnimation animDs = GetAnimation(destineId);
            if (animOr == null || animDs == null) return;

            animDs.Copy(animOr);
        }
    }    
}
