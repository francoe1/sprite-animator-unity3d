using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SpriteAnimatorRuntime
{
    [Serializable]
    public class SpriteAnimation
    {
        public int Id { get { return m_id; } }
        public int TimePerFrame { get { return m_timePerSecond; } set { m_timePerSecond = Mathf.Clamp(value, 10, int.MaxValue); } }
        public FrameInfo[] Frames { get { return m_frames.ToArray(); } }
        public Pivot[] Pivots { get { return m_pivots.ToArray(); } }
        public int FrameCount { get { return m_frames.Count; } }        
        public string Path { get { return (string.IsNullOrEmpty(m_path))? "" : m_path; } }

        public string Name = "new animation";        
        public AnimationType Type = AnimationType.Normal;
        public AnimationDirection Direction = AnimationDirection.Forward;

        [SerializeField]
        private int m_timePerSecond = 100;
        [SerializeField]
        private int m_id = 0;
        [SerializeField]
        private List<FrameInfo> m_frames = new List<FrameInfo>();
        [SerializeField]
        private List<Pivot> m_pivots = new List<Pivot>();
        [SerializeField]
        private string m_path = "";

        public SpriteAnimation(int id)
        {
            m_id = id;
        }

        public float Time
        {
            get
            {
                switch (Type)
                {
                    case AnimationType.Normal: return (TimePerFrame * m_frames.Count) / 1000f;
                    case AnimationType.PingPong: return ((TimePerFrame * m_frames.Count) / 1000f) * 2;
                    case AnimationType.Loop: return float.PositiveInfinity;
                }

                return 0;
            }
        }

        public enum AnimationType : int
        {
            Normal,
            PingPong,
            Loop,
        }

        public enum AnimationDirection : int
        {
            Forward,
            Backwar,
            None,
        }       

        public FrameInfo GetFirstFrame()
        {
            if (FrameCount == 0)
                return null;
            return m_frames[0];
        }

        public Sprite GetFirstFrameSprite()
        {
            if (FrameCount == 0)
                return null;
            return m_frames[0].Sprite;
        }

        public override string ToString()
        {
            return Name;
        }

        public int AddFrame(Sprite sprite)
        {
            if (Application.isPlaying)
                throw new Exception("AddFrame is not available in runtime");

            m_frames.Add(new FrameInfo (m_pivots.Count) { Sprite = sprite });
            return m_frames.Count - 1;
        }

        public void InsertFrame(int index, Sprite sprite)
        {
            if (Application.isPlaying)
                throw new Exception("Insert is not available in runtime");

            m_frames.Insert(index, new FrameInfo(m_pivots.Count) { Sprite = sprite });
        }

        public void RemoveFrame(int index)
        {
            if (Application.isPlaying)
                throw new Exception("RemoveAt is not available in runtime");
            m_frames.RemoveAt(index);
        }

        public void SetFrames(IList<FrameInfo> frames)
        {
            if (Application.isPlaying)
                throw new Exception("SetFrames is not available in runtime");

            m_frames.Clear();
            m_frames.AddRange(frames);
        }

        public void AddPivot()
        {
            if (Application.isPlaying)
                throw new Exception("AddPivot is not available in runtime");

            m_pivots.Add(new Pivot(m_pivots.Count));
            for(int i = 0; i < m_frames.Count; i++)
                m_frames[i].Pivots.Add(Vector2.zero);
        }

        public void RemovePivot(int index)
        {
            if (Application.isPlaying)
                throw new Exception("RemovePivot is not available in runtime");

            m_pivots.RemoveAt(index);
            for (int i = 0; i < m_frames.Count; i++)
                m_frames[i].Pivots.RemoveAt(index);
        }

        public Vector2 GetPivotPointInFrame(int indexFrame, int indexPivot)
        {
            if (m_frames.Count <= indexFrame || indexFrame < 0 || indexPivot >= Pivots.Length ||indexPivot < 0)
            {
                throw new Exception("Error in GetPivotPointInFrame");
            }

            if (m_frames.Count <= indexFrame || indexFrame < 0)
            {
                throw new Exception("IndexFrame Error in GetPivotPointInFrame");
            }

            Vector2 point = m_frames[indexFrame].Pivots[indexPivot];
            point.y = -point.y;
            point.x *= 1f / Frames[indexFrame].Width;
            point.y *= 1f / Frames[indexFrame].Height;
            point.x -= .5f;
            point.y += .5f;
            return point;
        }

        public Vector2 GetPivotPointInFrame(int indexFrame, string name)
        {
            Pivot pivot = m_pivots.Where(x => x.Name == name).Take(1).SingleOrDefault();
            if(pivot == null)
            {
                throw new Exception("Pivot " +  name + " no exist in " + Name);
            }

            if(m_frames.Count <= indexFrame ||indexFrame < 0)
            {
                throw new Exception("IndexFrame Error in GetPivotPointInFrame");
            }

            Vector2 point = m_frames[indexFrame].Pivots[pivot.Index];
            point.y = -point.y;
            point.x *= 1f / Frames[indexFrame].Width;
            point.y *= 1f / Frames[indexFrame].Height;
            point.x -= .5f;
            point.y += .5f;
            return point;
        }

        public void SetPath(string path)
        {
            if (Application.isPlaying)
                throw new Exception("AddFrame is not available in runtime");
            m_path = path;
        }

        public void ChangeFrameIndex(int fromIndex, int toIndex)
        {
            if (Application.isPlaying)
                throw new Exception("ChangeFrame is not available in runtime");

            FrameInfo fromFrame = m_frames[fromIndex];
            m_frames.RemoveAt(fromIndex);
            m_frames.Insert(toIndex, fromFrame);
        }

        public bool HasPivot(string name)
        {
            return m_pivots.Count(x => x.Name == name) == 1;
        }

        public bool ContainFrameIndex(int index)
        {
            return m_frames.Count > index;
        }

        [Serializable]
        public class FrameInfo
        {
            public Sprite Sprite;
            public List<Vector2> Pivots = new List<Vector2>();

            public float Time { get { return m_time; } set { m_time = Mathf.Clamp(value, 1f, 10); } }
            public string Name { get { return Sprite == null? "null": Sprite.name; } set { Sprite.name = value; } }
            public float Width { get { return Sprite == null ? 0 : Sprite.rect.width; } }
            public float Height { get { return Sprite == null ? 0 : Sprite.rect.height; } }
                       

            [SerializeField]
            private float m_time = 1;

            public FrameInfo(int length)
            {
                Pivots = new List<Vector2>(new Vector2[length]);
            }
        }

        [Serializable]
        public class FrameEvent
        {
            public string Name;
            public TriggerType Type;

            public enum TriggerType : int
            {
                Enter,
                Exit
            }
        }

        [Serializable]
        public class Pivot
        {
            public int Index;
            public string Name;
            public Color Color = Color.red;

            public Pivot(int index)
            {
                Index = index;
            }

            public override bool Equals(object obj)
            {
                return obj.ToString() == Name;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

    }
}
