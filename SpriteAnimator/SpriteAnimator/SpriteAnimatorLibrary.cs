using SpriteAnimatorRuntime;
using System;
using System.Timers;
using UnityEditor;
using UnityEngine;

namespace SpriteAnimatorEditor
{
    [Serializable]
    internal class SpriteAnimatorLibrary
    {
        private SpriteAnimatorWindow Root = null;
        [SerializeField]
        private AnimationPreview[] m_preview = null;
        [SerializeField]
        private bool[] m_previewFilter = null;
        [SerializeField]
        private int m_previewSize;
        [SerializeField]
        private int m_previewMargin;
        [SerializeField]
        private Vector2 m_scroll ;
        [SerializeField]
        private int m_scrollHeightFix ;
        [SerializeField]
        private string m_searchText = "";
        [SerializeField]
        private Rect m_rectPreviewZone;
        [SerializeField]
        private Rect m_searchRect;

        private SpriteAnimation[] m_cacheAnimations;
       

        public SpriteAnimatorLibrary(SpriteAnimatorWindow spriteAnimatorWindow)
        {
            m_previewSize = 150;
            m_previewMargin = 5;
            Root = spriteAnimatorWindow;
            m_preview = new AnimationPreview[0];
        }

        private void Update()
        {
            if (Event.current.type != EventType.Layout) return;
            for (int i = 0; i < m_preview.Length; i++)
            {
                if (m_preview[i].Player.State == SpriteAnimatorPlayer.PlayerState.Finish)
                    m_preview[i].Player.Reset();
                m_preview[i].Player.Update();
            }
        }

        public void SetAnimation(SpriteAnimation[] animations)
        {
            m_cacheAnimations = animations;
        }

        private void RefreshAnimationData()
        {
            m_preview = new AnimationPreview[m_cacheAnimations.Length];
            m_previewFilter = new bool[m_cacheAnimations.Length];

            for (int i = 0; i < m_cacheAnimations.Length; i++)
            {
                m_preview[i] = new AnimationPreview(m_cacheAnimations[i]);
                m_previewFilter[i] = true;
            }

            m_cacheAnimations = null;
            m_scrollHeightFix = 0;
        }

        public void Draw()
        {
            GUIPro.Elements.Header("Sprite Animator Library", 30);

            if (Event.current.type == EventType.Layout && m_cacheAnimations != null)
            {
                RefreshAnimationData();
            }

            if (m_preview.Length == 0)
            {
                return;
            }

            Update();

            GUILayout.Label("", GUILayout.ExpandWidth(true), GUILayout.Height(20));
            m_searchRect = GUILayoutUtility.GetLastRect();
                        
            EditorGUI.BeginChangeCheck();
            m_searchText = GUI.TextField(m_searchRect, m_searchText, SpriteAnimatorWindow.EditorResources.LabelSearch);

            GUI.DrawTexture(new Rect(m_searchRect.x + 5, m_searchRect.y + 2, 15, 15),
            SpriteAnimatorWindow.EditorResources.GetIcon("Search"));

            if (EditorGUI.EndChangeCheck())
            {
                for(int i = 0; i < m_preview.Length; i++)
                {
                    if(m_preview[i].Player.Animation.Name.ToLower().Contains(m_searchText.ToLower()))
                    {
                        m_previewFilter[i] = true;
                    }
                    else
                    {
                        m_previewFilter[i] = false;
                    }
                }
            }

            m_scroll = GUILayout.BeginScrollView(m_scroll, SpriteAnimatorWindow.EditorResources.Background0);    
            
            GUILayout.BeginVertical();
            {
                GUILayout.Label("", GUILayout.Height(0), GUILayout.ExpandWidth(true));
                m_rectPreviewZone = GUILayoutUtility.GetLastRect();
                m_rectPreviewZone.y -= 2;
                int cols = (int)m_rectPreviewZone.width / m_previewSize;
                int x = 0;
                int y = 0;

                for (int i = 0; i < m_preview.Length; i++)
                {
                    if (!m_previewFilter[i])
                        continue;

                    Rect itemRect = new Rect(m_rectPreviewZone.x + ((m_previewSize + m_previewMargin) * x),
                    m_rectPreviewZone.y + ((m_previewSize + m_previewMargin) * y) + m_previewMargin,
                    m_previewSize,
                    m_previewSize);

                    DrawOnGUISprite(m_preview[i].Player, itemRect);

                    x++;
                    if(x == cols)
                    {                        
                        y++;
                        x = 0;
                        m_scrollHeightFix = y;
                    }
                }
            }
            GUILayout.EndVertical();
            GUILayout.Box("", GUIStyle.none, GUILayout.Height((m_scrollHeightFix + 1) * (m_previewSize + m_previewMargin)));
            GUILayout.EndScrollView();
        }

        private void DrawOnGUISprite(SpriteAnimatorPlayer player, Rect rect)
        {
            if(Event.current.type == EventType.MouseDown && Event.current.button == 0 && 
                rect.Contains(Event.current.mousePosition))
            {
                Root.SelectElementForID(player.Animation.Id);
                Event.current.Use();
            }


            Sprite aSprite = player.FrameSprite;
            Rect originalRect = rect;
            EditorGUI.DrawRect(rect, Root.Data.PreviewBackgroundColor);
            rect.height -= 20;

            if (aSprite == null)
            {
                GUI.Box(rect, "Empty Clip");
            }
            else
            {
                GUIPro.Layout.ControlCenter(() =>
                {
                    Rect spriteRect = rect;
                    spriteRect.height -= 40;
                    spriteRect.y += 40;
                    SpriteAnimatorControl.DrawSpriteRenderInternal(spriteRect, aSprite, .8f, false);
                });
            }

            GUI.Box(new Rect(originalRect.x, originalRect.y, originalRect.width, 40), player.Animation.Name, SpriteAnimatorWindow.EditorResources.Background3);

            if(originalRect.Contains(Event.current.mousePosition))
            {
                Rect infoRect = new Rect(originalRect.x, originalRect.y + 40, originalRect.width, originalRect.height - 40);
                GUI.Box(infoRect
                    , "Frames:" + player.Animation.FrameCount + "\n" + 
                      "Time:" + player.Animation.Time + "\n" + 
                      "Dir:" + player.Animation.Direction + "\n" + 
                      "Type:" + player.Animation.Type
                    , SpriteAnimatorWindow.EditorResources.LibraryInfo);



                EditorGUIUtility.AddCursorRect(new Rect(Event.current.mousePosition, Vector2.one * 15), MouseCursor.Link);
            }
        }

        [Serializable]
        public class AnimationPreview
        {
            public SpriteAnimatorPlayer Player = null;

            public AnimationPreview(SpriteAnimation animation)
            {
                Player = new SpriteAnimatorPlayer(animation);
                Player.Reset();
            }
        }
    }
}
