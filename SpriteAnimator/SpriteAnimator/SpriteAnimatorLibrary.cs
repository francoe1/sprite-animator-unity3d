using SpriteAnimatorRuntime;
using System.Timers;
using UnityEditor;
using UnityEngine;

namespace SpriteAnimatorEditor
{
    internal class SpriteAnimatorLibrary
    {
        private SpriteAnimatorWindow Root { get; set; }
        private AnimationPreview[] m_preview { get; set; }
        private bool[] m_previewFilter { get; set; }
        private int m_previewSize { get; set; }
        private int m_previewMargin { get; set; }
        private Vector2 m_scroll { get; set; }
        private int m_scrollHeightFix  { get; set; }
        private string m_searchText = "";

        public SpriteAnimatorLibrary(SpriteAnimatorWindow spriteAnimatorWindow)
        {
            m_previewSize = 150;
            m_previewMargin = 5;
            Root = spriteAnimatorWindow;
            m_preview = new AnimationPreview[0];
            Timer timer = new Timer();
            timer.Interval = 10;
            timer.Elapsed += (o, e) => Update();
            timer.Start();
        }

        private void Update()
        {
            bool isRepain = false;

            for (int i = 0; i < m_preview.Length; i++)
            {
                if (m_preview[i].Player.State == SpriteAnimatorPlayer.PlayerState.Finish)
                    m_preview[i].Player.Reset();

                m_preview[i].Player.Update();
                if (!isRepain && (m_preview[i].Player.State == SpriteAnimatorPlayer.PlayerState.NextFrame ||
                    m_preview[i].Player.State == SpriteAnimatorPlayer.PlayerState.BackFrame))
                {
                    Root.Repaint();
                    isRepain = true;
                }
            }
        }

        public void SetAnimation(SpriteAnimation[] animations)
        {
            m_preview = new AnimationPreview[animations.Length];
            m_previewFilter = new bool[animations.Length];

            for (int i = 0; i < animations.Length; i++)
            {
                m_preview[i] = new AnimationPreview(animations[i]);
                m_previewFilter[i] = true;
            }

            m_scrollHeightFix = 0;            
        }


        public void Draw()
        {
            SpriteAnimatorEditorExtencion.Elements.Header("Sprite Animator Library", 35);
            if (m_preview.Length == 0)
            {
                return;
            }

            GUILayout.Label("", GUILayout.ExpandWidth(true), GUILayout.Height(20));
            Rect searchRect = GUILayoutUtility.GetLastRect();
                        
            EditorGUI.BeginChangeCheck();
            m_searchText = GUI.TextField(searchRect, m_searchText, SpriteAnimatorWindow.EditorResources.LabelSearch);

            GUI.DrawTexture(new Rect(searchRect.x + 5, searchRect.y + 2, 15, 15),
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
                Rect rect = GUILayoutUtility.GetLastRect();
                rect.y -= 2;
                int cols = (int)rect.width / m_previewSize;
                int x = 0;
                int y = 0;

                for (int i = 0; i < m_preview.Length; i++)
                {
                    if (!m_previewFilter[i])
                        continue;

                    Rect itemRect = new Rect(rect.x + ((m_previewSize + m_previewMargin) * x),
                    rect.y + ((m_previewSize + m_previewMargin) * y) + m_previewMargin,
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
                Rect c = aSprite.rect;

                if (Event.current.type == EventType.Repaint)
                {
                    var tex = aSprite.texture;
                    c.xMin /= tex.width;
                    c.xMax /= tex.width;
                    c.yMin /= tex.height;
                    c.yMax /= tex.height;

                    if (rect.width > rect.height)
                        rect.width = rect.height;
                    else
                        rect.height = rect.width;

                    rect.width = Mathf.Clamp(rect.width, 64, 150);
                    rect.height = Mathf.Clamp(rect.height, 64, 150);

                    float y = 20 + (originalRect.height - rect.height) / 2;
                    float x = 20 + (originalRect.width - rect.width) / 2;

                    Rect spriteRect = new Rect(rect.x + x, rect.y + y, rect.width - 40, rect.height - 40);
                    EditorGUI.DrawRect(spriteRect, new Color(0, 0, 0, .05f));
                    GUI.DrawTextureWithTexCoords(spriteRect, tex, c);
                }
            }

            GUI.Box(new Rect(originalRect.x, originalRect.y, originalRect.width, 20), player.Animation.Name, SpriteAnimatorWindow.EditorResources.Background3);

            if(originalRect.Contains(Event.current.mousePosition))
            {
                Rect infoRect = new Rect(originalRect.x, originalRect.y + 20, originalRect.width, originalRect.height - 20);
                GUI.Box(infoRect
                    , "Frames:" + player.Animation.FrameCount + "\n" + 
                      "Time:" + player.Animation.Time + "\n" + 
                      "Dir:" + player.Animation.Direction + "\n" + 
                      "Type:" + player.Animation.Type
                    , SpriteAnimatorWindow.EditorResources.LibraryInfo);



                EditorGUIUtility.AddCursorRect(new Rect(Event.current.mousePosition, Vector2.one * 15), MouseCursor.Link);
            }
        }


        public class AnimationPreview
        {
            public SpriteAnimatorPlayer Player { get; set; }

            public AnimationPreview(SpriteAnimation animation)
            {
                Player = new SpriteAnimatorPlayer(animation);
                Player.Reset();
            }
        }
    }
}
