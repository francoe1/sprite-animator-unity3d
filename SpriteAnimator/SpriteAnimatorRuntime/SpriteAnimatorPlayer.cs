using System;
using UnityEngine;

namespace SpriteAnimatorRuntime
{
    public class SpriteAnimatorPlayer
    {
        public SpriteAnimation Animation { get; private set; }
        public PlayerState State { get; private set; }
        public SpriteAnimation.AnimationDirection Direction { get; private set; }
        private DateTime m_lastTime { get; set; }
        private int m_frameIndex { get; set; }
        private Sprite m_currentFrame { get; set; }
        public int SpeedMultipler { get { return (int)m_speedMultiple; } set { m_speedMultiple = Mathf.Clamp(value, 1, int.MaxValue); } }

        public int FrameIndex { get { return m_frameIndex; } }
        public Sprite FrameSprite { get { return m_currentFrame; } }
        public bool IsPlaying
        {
            get
            {
                return State != PlayerState.Stop && State != PlayerState.Pause && State != PlayerState.Finish;
            }
        }

        public delegate void PlayerUpdateDelegate(PlayerState state);
        public PlayerUpdateDelegate PlayerUpdateHanlder { private get; set; }
        private float m_speedMultiple = 1;

        public enum PlayerState
        {
            SetFrame,
            Update,
            NextFrame,
            BackFrame,
            Finish,
            Reset,
            TimeOut,
            FinishForward,
            FinishBackward,
            Pause,
            Stop,
            NonAnimation,
            SetFrameAffter,
        }

        public SpriteAnimatorPlayer()
        {
            Init();
        }

        public void PlayAnimation(SpriteAnimation spriteAnimation)
        {
            Animation = spriteAnimation;
            Reset();
            State = PlayerState.Update;
        }

        public SpriteAnimatorPlayer(SpriteAnimation animation)
        {
            Animation = animation;
            Init();
        }

        private void Init()
        {
            State = PlayerState.Reset;
            Update();
            State = PlayerState.Stop;
        }

        public void Update()
        {
            if (Animation == null)
            {
                State = PlayerState.NonAnimation;
                return;
            }

            if (Animation.FrameCount == 0)
                State = PlayerState.Stop;

            switch (State)
            {
                case PlayerState.Reset:
                    {
                        switch (Animation.Direction)
                        {
                            case SpriteAnimation.AnimationDirection.Backwar:
                                m_frameIndex = GetLastIndex();
                                break;

                            case SpriteAnimation.AnimationDirection.Forward:
                                m_frameIndex = 0;
                                break;

                            case SpriteAnimation.AnimationDirection.None:
                                m_frameIndex = 0;
                                Debug.LogWarning("The animation " + Animation.Name + " error in directión type");
                                break;
                        }
                    
                    
                        
                        State = PlayerState.SetFrame;
                        Direction = Animation.Direction;
                        m_lastTime = DateTime.Now;
                    }
                    break;

                case PlayerState.SetFrameAffter:
                    State = PlayerState.Update;
                    break;

                case PlayerState.SetFrame:       
                    if(ValidFrameIndex(m_frameIndex))
                        m_currentFrame = Animation.Frames[m_frameIndex].Sprite;
                    State = PlayerState.SetFrameAffter;
                    break;

                case PlayerState.Update:
                    {
                        if ((DateTime.Now - m_lastTime).TotalMilliseconds >
                            (Animation.TimePerFrame * GetCurrentFrameTime()))
                        {
                            State = PlayerState.TimeOut;
                            m_lastTime = DateTime.Now;
                        }
                    }
                    break;

                case PlayerState.TimeOut:
                    {
                        switch(Direction)
                        {
                            case SpriteAnimation.AnimationDirection.Forward:
                                State = PlayerState.NextFrame;
                                break;
                            case SpriteAnimation.AnimationDirection.Backwar:
                                State = PlayerState.BackFrame;
                                break;
                        }
                    }
                    break;

                case PlayerState.NextFrame:
                    {
                        m_frameIndex++;
                        if (m_frameIndex == Animation.FrameCount)
                            State = PlayerState.FinishForward;
                        else
                            State = PlayerState.SetFrame;

                        m_frameIndex = Mathf.Clamp(m_frameIndex, 0, Animation.FrameCount - 1);
                    }
                    break;

                case PlayerState.BackFrame:
                    {
                        m_frameIndex--;
                        if (m_frameIndex == -1)
                            State = PlayerState.FinishBackward;
                        else
                            State = PlayerState.SetFrame;

                        m_frameIndex = Mathf.Clamp(m_frameIndex, 0, Animation.FrameCount - 1);
                    }
                    break;                

                case PlayerState.FinishBackward:
                    {
                        m_frameIndex = 0;
                        switch (Animation.Type)
                        {
                            case SpriteAnimation.AnimationType.Loop:
                                {
                                    m_frameIndex = Animation.FrameCount - 1;
                                    State = PlayerState.SetFrame;
                                }
                                break;

                            case SpriteAnimation.AnimationType.PingPong:
                                {
                                    m_frameIndex++;
                                    Direction = SpriteAnimation.AnimationDirection.Forward;
                                    State = PlayerState.SetFrame;
                                }
                                break;

                            case SpriteAnimation.AnimationType.Normal:
                                {
                                    Direction = SpriteAnimation.AnimationDirection.None;
                                    State = PlayerState.Finish;
                                }
                                break;
                        }
                    }
                    break;

                case PlayerState.FinishForward:
                    {
                        m_frameIndex = Animation.FrameCount - 1;

                        switch (Animation.Type)
                        {
                            case SpriteAnimation.AnimationType.Loop:
                                {
                                    m_frameIndex = 0;                                    
                                    State = PlayerState.SetFrame;
                                }
                                break;

                            case SpriteAnimation.AnimationType.PingPong:
                                {
                                    m_frameIndex--;
                                    Direction = SpriteAnimation.AnimationDirection.Backwar;
                                    State = PlayerState.SetFrame;
                                }
                                break;

                            case SpriteAnimation.AnimationType.Normal:
                                {
                                    Direction = SpriteAnimation.AnimationDirection.None;
                                    State = PlayerState.Finish;
                                }
                                break;
                        }
                    }
                    break;

                case PlayerState.Finish:

                    break;
            }

            if (PlayerUpdateHanlder != null)
                PlayerUpdateHanlder.Invoke(State);
        }

        public void MoveNextFrame()
        {
            if (Animation == null)
                return;

            m_frameIndex++;
            if (m_frameIndex > Animation.FrameCount - 1)
                m_frameIndex = Animation.FrameCount - 1;

            if (Animation.Frames.Length > 0)
                m_currentFrame = Animation.Frames[m_frameIndex].Sprite;
            State = PlayerState.Stop;
        }

        public void MoveBackFrame()
        {
            if (Animation == null)
                return;

            m_frameIndex--;
            if (m_frameIndex < 0)
                m_frameIndex = 0;

            if(Animation.Frames.Length > 0)
                m_currentFrame = Animation.Frames[m_frameIndex].Sprite;
            State = PlayerState.Stop;
        }

        public void Reset()
        {
            State = PlayerState.Reset;
        }

        public void Pause()
        {
            if (State == PlayerState.Update)
                State = PlayerState.Pause;
        }

        public void Stop()
        {
            m_currentFrame = null;
            State = PlayerState.Stop;
        }

        public void Resume()
        {
            if (State == PlayerState.Pause)
                State = PlayerState.SetFrame;                
            else
                Debug.LogWarning("Use Resume() cuando la animación esta en pausa, no usar en " + State);
        }

        public void GoFirtFrame()
        {            
            m_frameIndex = 0;
            m_lastTime = DateTime.Now;
            State = PlayerState.SetFrame;
        }

        public void GoLastFrame()
        {
            m_frameIndex = GetLastIndex();
            m_lastTime = DateTime.Now;
            State = PlayerState.SetFrame;
        }

        public int GetLastIndex()
        {
            if(State == PlayerState.NonAnimation)
                return 0;
            if (Animation.FrameCount > 0)
                return Animation.FrameCount - 1;
            return 0;
        }

        public bool ValidFrameIndex(int index)
        {
            if (Animation == null)
                return false;

            if (index < 0)
                return false;

            if (index >= Animation.FrameCount)
                return false;


            return true;
        }

        public void GoToFrame(int index)
        {
            if (!ValidFrameIndex(index))
                return;

            m_frameIndex = index;

            if (Animation.Frames.Length > 0)
                m_currentFrame = Animation.Frames[m_frameIndex].Sprite;
        }

        public float GetCurrentFrameTime()
        {
            if (!ValidFrameIndex(FrameIndex))
                return 0;

            return Animation.Frames[m_frameIndex].Time / SpeedMultipler;
        }

        public Vector2 GetPointOfPivot(int pivot)
        {
            return Animation.GetPivotPointInFrame(FrameIndex, pivot);
        }

        public Vector2 GetPointOfPivot(string pivot)
        {
            return Animation.GetPivotPointInFrame(FrameIndex, pivot);
        }

        public bool HasPivot(string name)
        {
            if (Animation == null) return false;
            return Animation.HasPivot(name);
        }
    }
}
