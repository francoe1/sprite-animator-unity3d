using System;
using SpriteAnimatorRuntime;
using UnityEditor;
using UnityEngine;

namespace SpriteAnimatorEditor
{
    [CustomEditor(typeof(SpriteAnimatorController))]
    public class SpriteAnimatorControllerEditor : Editor
    {
        private SpriteAnimatorController m_current { get; set; }
        private SpriteAnimation[] m_animations { get; set; }
        private string[] m_animationName { get; set; }
        private int m_animationIndex { get; set; }

        private void OnEnable()
        {
            m_current = (SpriteAnimatorController)target;

            if (m_current.Data == null)
                m_current.Data = AssetDatabase.LoadAssetAtPath<SpriteAnimatorData>("Assets/Plugins/SpriteAnimator/Runtime/default.asset");

            
            if (m_current.Data == null)
            {
                m_current.Data = (SpriteAnimatorData)CreateInstance(typeof(SpriteAnimatorData));
                AssetDatabase.CreateAsset(m_current.Data, "Assets/Plugins/SpriteAnimator/Runtime/default.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        public override bool RequiresConstantRepaint()
        {
            return true;
        }

        public override void OnInspectorGUI()
        {

            if(m_current.Data == null)
            {
                EditorGUILayout.HelpBox("SpriteAnimator not initializate, open SpriteAnimatorWindows", MessageType.Warning);
                return;
            }

            GUILayout.Space(10);


            OnDrawSelector();   

            if(Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Animation State", MessageType.Info);

                if (m_current.Player != null && m_current.Player.Animation != null)
                {
                    GUILayout.BeginVertical("box");
                    {
                        GUILayout.Label("Animation:" + m_current.Player.Animation.Path);
                        GUILayout.Label("Frame:" + m_current.Player.FrameIndex);
                        GUILayout.Label("State:" + m_current.Player.State.ToString());
                        GUILayout.Label("Direction:" + m_current.Player.Direction.ToString());
                        GUILayout.Label("IsPlaying:" + m_current.Player.IsPlaying.ToString());

                        if (GUILayout.Button("Reset"))
                            m_current.Player.Reset();
                    }
                    GUILayout.EndVertical();                    
                }

                if (m_current.Player.Animation == null)
                {
                    EditorGUILayout.HelpBox("NULL animation", MessageType.Error);
                }
            }
        }

        private void OnDrawSelector()
        {
            UpdateInspectorData();
               
            EditorGUI.BeginChangeCheck();

            GUI.enabled = !Application.isPlaying;

            if (m_animations.Length > 0)
            {

                m_animationIndex = EditorGUILayout.Popup(m_animationIndex, m_animationName);
                if (EditorGUI.EndChangeCheck())
                {
                    m_current.GetComponent<SpriteRenderer>().sprite = m_animations[m_animationIndex].GetFirstFrameSprite();
                    m_current.SelectAnimationId = m_animations[m_animationIndex].Id;
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Empty animations library", MessageType.Warning);
                return;
            }

            GUI.enabled = true;

            if (GUILayout.Button("View in Animator") && m_animationIndex <= m_animationName.Length)
            {
                EditorWindow.GetWindow<SpriteAnimatorWindow>().Show(true);
                SpriteAnimatorWindow.Instance.SelectElementForID(m_animations[m_animationIndex].Id);
            }


            if (m_animationIndex <= m_animations.Length)
            {
                SpriteAnimation anim = m_animations[m_animationIndex];
                GUILayout.BeginVertical("box");
                {
                    GUILayout.Label("Available Pivots " + anim.Pivots.Length);
                    for (int i = 0; i < anim.Pivots.Length; i++)
                    {
                        GUILayout.Label("[" + i + "]" + anim.Pivots[i].Name);
                    }
                }
                GUILayout.EndVertical();
            }
        }

        private void UpdateInspectorData()
        {
            m_animations = new SpriteAnimation[0];
            m_animationName = new string[0];

            if (m_current.Data == null && m_current.Data.Animations.Length > 0)
                return;

            m_animations = m_current.Data.Animations;
            m_animationName = new string[m_animations.Length];
            for (int i = 0; i < m_animations.Length; i++)
            {
                m_animationName[i] = m_animations[i].Path;
                if (m_animations[i].Id == m_current.SelectAnimationId)
                    m_animationIndex = i;
            }
        }
    }
}
