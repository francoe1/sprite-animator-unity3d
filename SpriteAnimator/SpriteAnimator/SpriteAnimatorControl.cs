using SpriteAnimatorRuntime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SpriteAnimatorEditor
{
    [Serializable]
    internal class SpriteAnimatorControl : ISerializationCallbackReceiver
    {
        private static SpriteAnimatorWindow Root { get; set; }
        private static Color GridColor = GUIResources.CreateColor(20, 20, 20, .5f);
        private SpriteAnimation m_animation { get; set; }
        private SpriteAnimatorPlayer m_player { get; set; }
        private Vector2 m_timelineScroll = Vector2.zero;
        private Vector2 m_settingScroll = Vector2.zero;
        private Vector2 m_pivotOriginalPos = Vector2.zero;

        private int m_currentDragIndexPivot = 0;
        private int m_currentDragIndexFrame = 0;
        private int m_currentTooltipIndexFrame = 0;

        internal bool AvailableAnimation => m_animation != null;

        [SerializeField]
        private byte[] m_serializeData = null;       

        private Stack<Sprite> m_cacheSpriteToInsert { get; set; } = new Stack<Sprite>();

        public SpriteAnimatorControl(SpriteAnimatorWindow window)
        {
            Root = window;
            m_currentDragIndexPivot = -1;
            m_currentDragIndexFrame = -1;
            m_currentTooltipIndexFrame = -1;
        }

        private void Update()
        {
            if (m_cacheSpriteToInsert.Count > 0)
            {
                Root.Snapshot("AddSprite");
                Root.ShowNotification(new GUIContent("Add sprite"));

                while (m_cacheSpriteToInsert.Count > 0)
                {
                    Sprite sprite = m_cacheSpriteToInsert.Pop();

                    if (sprite)
                    {
                        int index = m_animation.AddFrame(sprite);
                        m_player.GoToFrame(index);
                    }
                }
            }


            if (m_player != null)
            {
                m_player.Update();
            }
        }

        internal void Draw()
        {
            if (m_animation == null)
            {
                GUIPro.Elements.Header("Select or create animation", 35);
                return;
            }

            {
                //GUIPro.Elements.Header(m_animation.Path, 35);
                GUIStyle style = new GUIStyle(SpriteAnimatorWindow.EditorResources.Header0);
                style.alignment = TextAnchor.MiddleLeft;
                style.padding = new RectOffset(40, 0, 0, 0);
                GUILayout.Label(m_animation.Path.ToUpper(), style, GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(true), GUILayout.Height(30));
            }

            GUILayout.Space(2);

            GUIPro.Layout.Control(GUIPro.Layout.Direction.Horizontal, -1, -1, SpriteAnimatorWindow.EditorResources.Background1, () =>
            {
                GUI.SetNextControlName("Control");
                GUIPro.Layout.Control(GUIPro.Layout.Direction.Vertical, -1, -1, SpriteAnimatorWindow.EditorResources.Background1, () =>
                {
                    //Player
                    GUIPro.Layout.Control(GUIPro.Layout.Direction.Vertical, -1, -1, SpriteAnimatorWindow.EditorResources.Background0, DrawPlayer);
                    //Time Lines
                    GUIPro.Layout.Control(GUIPro.Layout.Direction.Vertical, -1, 200, SpriteAnimatorWindow.EditorResources.Background0, DrawTimeLine);
                });
                GUILayout.Space(2);
                //Tools
                GUI.SetNextControlName("ControlSetting");
                GUIPro.Layout.Control(GUIPro.Layout.Direction.Vertical, 300, -1, SpriteAnimatorWindow.EditorResources.Background0, DrawSetting);
            });
            Update();



            if (Event.current.type == EventType.MouseUp && (m_currentDragIndexFrame != -1 || m_currentDragIndexPivot != -1))
            {
                m_currentDragIndexFrame = -1;
                m_currentDragIndexPivot = -1;
            }

            if (Root.IsActiveWindows)
                DetectShortcutKey();
        }

        private void DetectShortcutKey()
        {
            string[] contextData = GUI.GetNameOfFocusedControl().Split('.');
            string context = contextData[0].ToLower();
            string value = contextData.Length > 1 ? contextData[1] : "";

            if (Event.current.type == EventType.KeyDown)
            {

                if (Event.current.keyCode == KeyCode.RightArrow)
                {
                    m_player.MoveNextFrame();
                    Event.current.Use();
                }

                if (Event.current.keyCode == KeyCode.LeftArrow)
                {
                    m_player.MoveBackFrame();
                    Event.current.Use();
                }

                if (Event.current.keyCode == KeyCode.Space)
                {
                    if (m_player.IsPlaying)
                        m_player.Pause();
                    else if (m_player.State == SpriteAnimatorPlayer.PlayerState.Pause)
                        m_player.Resume();
                    else if (m_player.State == SpriteAnimatorPlayer.PlayerState.Stop)
                        m_player.Reset();
                    Event.current.Use();
                }
            }
        }

        private void DrawSetting()
        {
            using (GUILayout.ScrollViewScope scope = new GUILayout.ScrollViewScope(m_settingScroll))
            {
                m_settingScroll = scope.scrollPosition;


                Root.Data.ShowAnimationOptions = GUIPro.Button.ContextFold(Root.Data.ShowAnimationOptions, "ANIMATION", "d_TimelineEditModeReplaceOFF", "d_TimelineEditModeReplaceON", () => Root.Snapshot("Show Animation Option"));
                if (Root.Data.ShowAnimationOptions)
                {
                    GUI.enabled = false;
                    GUIPro.Field.Draw("ID", m_animation.Id);
                    GUI.enabled = true;

                    m_animation.TimePerFrame = GUIPro.Field.Draw("Time Per Frame (ms)", m_animation.TimePerFrame, () => Root.Snapshot("Frame Time"));
                    EditorGUI.BeginChangeCheck();
                    SpriteAnimation.AnimationDirection dir = m_animation.Direction;
                    dir = GUIPro.Field.Draw("Direction:", m_animation.Direction, () => Root.Snapshot("Frame Direction"));
                    if (EditorGUI.EndChangeCheck() && dir == SpriteAnimation.AnimationDirection.None)
                        EditorUtility.DisplayDialog("error", "La animación debe tener un dirección", "ok");
                    else
                        m_animation.Direction = dir;

                    m_animation.Type = GUIPro.Field.Draw("Type", m_animation.Type, () => Root.Snapshot("Frame Type"));
                }

                Root.Data.ShowFrameOptions = GUIPro.Button.ContextFold(Root.Data.ShowFrameOptions, "FRAMES", "d_TimelineEditModeReplaceOFF", "d_TimelineEditModeReplaceON", () => Root.Snapshot("Show Frame Option"));
                if (Root.Data.ShowFrameOptions)
                {
                    if (m_animation.FrameCount > 0 && m_animation.FrameCount > m_player.FrameIndex)
                    {
                        m_animation.Frames[m_player.FrameIndex].Time = GUIPro.Field.Slider("Time", m_animation.Frames[m_player.FrameIndex].Time, 1f, 10, () => Root.Snapshot("Frame Lenght"));
                        m_animation.Frames[m_player.FrameIndex].Sprite = GUIPro.Field.Draw("Sprite", m_animation.Frames[m_player.FrameIndex].Sprite, () => Root.Snapshot("Frame Sprite"));
                    }

                    Root.Data.ShowPivots = GUIPro.Field.Draw("Show Pivots", Root.Data.ShowPivots);

                    if (Root.Data.ShowPivots)
                    {
                        if (GUIPro.Button.Alternative("Add Pivot")) m_animation.AddPivot();

                        Texture2D icon = SpriteAnimatorWindow.EditorResources.GetIcon("Delete");

                        if (m_animation.Pivots != null)
                        {
                            for (int i = 0; i < m_animation.Pivots.Length; i++)
                            {
                                if (m_animation.Pivots[i] == null)
                                {
                                    GUILayout.Label("Error");
                                    continue;
                                }

                                GUIStyle style = new GUIStyle(SpriteAnimatorWindow.EditorResources.Background2);
                                style.padding = new RectOffset(4, 4, 4, 4);

                                using (GUILayout.VerticalScope vScope = new GUILayout.VerticalScope(style))
                                {
                                    GUILayout.Label("Pivot " + i, SpriteAnimatorWindow.EditorResources.Field);

                                    m_animation.Pivots[i].Name = GUIPro.Field.Draw("Name", m_animation.Pivots[i].Name, () => Root.Snapshot("Privot Name"));
                                    m_animation.Pivots[i].Color = GUIPro.Field.Draw("Color", m_animation.Pivots[i].Color, () => Root.Snapshot("Privot Color"));

                                    if (GUILayout.Button("Remove", SpriteAnimatorWindow.EditorResources.ButtonNonMargin))
                                    {
                                        Root.Snapshot("RemovePivot");
                                        m_animation.RemovePivot(i);
                                        Event.current.Use();
                                    }
                                }
                            }
                        }
                    }
                }

                Root.Data.ShowRenderInfo = GUIPro.Button.ContextFold(Root.Data.ShowRenderInfo, "INFO", "d_TimelineEditModeReplaceOFF", "d_TimelineEditModeReplaceON", () => Root.Snapshot("Show Render Info"));
                if (Root.Data.ShowRenderInfo)
                {
                    GUI.enabled = false;
                    GUIPro.Field.Draw("Time", m_animation.Time);
                    GUIPro.Field.Draw("State", m_player.State);
                    GUIPro.Field.Draw("Frame", (m_player.FrameIndex + 1));
                    GUIPro.Field.Draw("Direcation", m_player.Direction);

                    GUI.enabled = true;
                }


                Root.Data.ShowRenderOptions = GUIPro.Button.ContextFold(Root.Data.ShowRenderOptions, "RENDER", "d_TimelineEditModeReplaceOFF", "d_TimelineEditModeReplaceON", () => Root.Snapshot("Show Render Setting"));
                if (Root.Data.ShowRenderOptions)
                {
                    Root.Data.ScaleSprite = GUIPro.Field.Slider("Render Scale", Root.Data.ScaleSprite, 1f, 100f, () => Root.Snapshot("Render Scale"));
                    Root.Data.ScalePivot = GUIPro.Field.Slider("Pivot Scale", Root.Data.ScalePivot, 1f, 100f, () => Root.Snapshot("Pivot Scale"));
                    Mathf.Clamp(Root.Data.ScaleSprite, .1f, 10);
                    Root.Data.PreviewBackgroundColor = GUIPro.Field.Draw("Background", Root.Data.PreviewBackgroundColor, () => Root.Snapshot("Render Color"));
                    Root.Data.DrawSpriteZone = GUIPro.Field.Draw("Sprite Zone", Root.Data.DrawSpriteZone, () => Root.Snapshot("Pivot SpriteZone"));
                }

            }
        }

        private void DrawPlayer()
        {
            GUIPro.Layout.Control(GUIPro.Layout.Direction.Vertical, -1, -1, SpriteAnimatorWindow.EditorResources.Background0, () =>
            {
                GUIPro.Layout.Control(GUIPro.Layout.Direction.Vertical, -1, -1, SpriteAnimatorWindow.EditorResources.Background0, () =>
                {
                    DrawSpriteRender(m_player.FrameSprite);
                });
                GUIPro.Layout.Control(GUIPro.Layout.Direction.Horizontal, -1, 30, SpriteAnimatorWindow.EditorResources.Background0, DrawPlayerBar);
            });
        }

        private void DrawPlayerBar()
        {
            GUILayout.FlexibleSpace();

            if (m_player == null)
                return;
            

            if (GUILayoutButtonIcon(SpriteAnimatorWindow.EditorResources.GetIcon("PlayLeft" + (m_player.IsPlaying ? "On" : "Off")),
                GUIStyle.none, 15,
                GUILayout.Width(30),
                GUILayout.ExpandHeight(true)))
            {
                m_player.MoveBackFrame();
            }

            if (GUILayoutButtonIcon(
                SpriteAnimatorWindow.EditorResources.GetIcon("Play" + (m_player.IsPlaying? "On" : "Off")),
               m_player.IsPlaying ? SpriteAnimatorWindow.EditorResources.Background1 : GUIStyle.none, 15,
                GUILayout.Width(30),
                GUILayout.Height(30)))
            {
                switch (m_player.State)
                {
                    case SpriteAnimatorPlayer.PlayerState.Pause:
                        m_player.Resume();
                        break;

                    case SpriteAnimatorPlayer.PlayerState.Update:
                        m_player.Pause();
                        break;

                    default:
                        m_player.Reset();
                        break;

                }
            }

            if(GUILayoutButtonIcon(SpriteAnimatorWindow.EditorResources.GetIcon("PlayRigth" + (m_player.IsPlaying ? "On" : "Off")),
                GUIStyle.none, 15,
                GUILayout.Width(30),
                GUILayout.ExpandHeight(true)))
            {
                m_player.MoveNextFrame();
            }

            GUILayout.FlexibleSpace();
        }

        private void DrawTimeLine()
        {
            using(GUILayout.HorizontalScope scope = new GUILayout.HorizontalScope("Timeline", SpriteAnimatorWindow.EditorResources.Field, GUILayout.ExpandWidth(true), GUILayout.Height(20)))            
            {
                if (GUILayout.Button("Order By name", SpriteAnimatorWindow.EditorResources.ButtonGreen, GUILayout.Height(20), GUILayout.Width(150)))
                {
                    Root.Snapshot("Order by Name");
                    m_animation.SetFrames(m_animation.Frames.OrderBy(x => x.Sprite.name).ToList());
                }
            }

            using (GUILayout.ScrollViewScope scrollScope = new GUILayout.ScrollViewScope(m_timelineScroll, false, false, GUILayout.Height(170)))
            {
                m_timelineScroll = scrollScope.scrollPosition;
                using(GUILayout.VerticalScope vScope = new GUILayout.VerticalScope(SpriteAnimatorWindow.EditorResources.TimeLine, GUILayout.ExpandWidth(true)))                 
                {
                    GUILayout.FlexibleSpace();
                    using (GUILayout.HorizontalScope hScope = new GUILayout.HorizontalScope())
                    {
                        for (int i = 0; i < m_animation.FrameCount; i++)
                        {
                            GUI.SetNextControlName("TimeLineSprite" + i);
                            GUILayout.Box("", GUIStyle.none, GUILayout.Width(80 * m_animation.Frames[i].Time), GUILayout.Height(100));
                            Rect rectElement = GUILayoutUtility.GetLastRect();
                            GUILayout.Space(10);
                            DrawTimeLineElement(m_animation.Frames[i], rectElement, m_player.FrameIndex == i, i);
                        }
                    }
                    GUILayout.FlexibleSpace();
                }
            }

            Rect dragAreaReact = GUILayoutUtility.GetLastRect();

            DropAreaGUI(dragAreaReact, (dragged_object) =>
            {
                if (dragged_object.Count > 0)
                {
                    if (dragged_object[0] is Texture2D)
                    {
                        dragged_object.ForEach(j =>
                        {
                            UnityEngine.Object[] sprites = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(j));

                            if (sprites != null && sprites.Length > 0)
                            {
                                sprites.ToList().ForEach(x =>
                                {
                                    Sprite sprite = x as Sprite;
                                    m_cacheSpriteToInsert.Push(sprite);
                                });
                            }
                        });
                    }
                    else if (dragged_object[0] is Sprite)
                    {
                        dragged_object.ForEach(x =>
                        {
                            Sprite sprite = x as Sprite;
                            m_cacheSpriteToInsert.Push(sprite);
                        });
                    }
                }

            }, typeof(Texture2D), typeof(Sprite));


            if (m_currentDragIndexFrame != -1 && m_animation.ContainFrameIndex(m_currentDragIndexFrame))
            {
                DrawSpriteRenderInternal(new Rect(Event.current.mousePosition.x - 80, Event.current.mousePosition.y - 80, 130, 80),
                                         m_animation.Frames[m_currentDragIndexFrame].Sprite, 1);
            }

            if (m_currentTooltipIndexFrame != -1 && m_animation.ContainFrameIndex(m_currentTooltipIndexFrame))
            {
                GUI.Label(new Rect(Event.current.mousePosition.x + 10, Event.current.mousePosition.y + 20, 130, 20),
                    m_animation.Frames[m_currentTooltipIndexFrame].Name, 
                    SpriteAnimatorWindow.EditorResources.Tooltip);
                if(Event.current.type == EventType.Repaint)
                    m_currentTooltipIndexFrame = -1;
            }
        }
        
        private void DrawTimeLineElement(SpriteAnimation.FrameInfo frame, Rect rect, bool active, int index)
        {
            EditorGUI.DrawRect(rect, active ? SpriteAnimatorWindow.EditorResources.Colors[2] : SpriteAnimatorWindow.EditorResources.Colors[1]);

            {
                Rect spriteRect = rect;
                spriteRect.width -= 20;
                spriteRect.x += 10;
                spriteRect.height -= 20;
                spriteRect.y += 20;
                DrawSpriteRenderInternal(spriteRect, frame.Sprite, 1);
            }

            GUI.Label(new Rect(rect.x, rect.y, rect.width, 20), 
                index + "-"+ frame.Time.ToString("F2") +  "["+ ((frame.Time * m_animation.TimePerFrame) / 60).ToString("F2") +"]",
                SpriteAnimatorWindow.EditorResources.LabelTimeLine);           
                        
            if(m_currentDragIndexFrame != -1)
            {
                Rect dropZone = new Rect(rect.x + 10, rect.y + 30, 64, 64);
                GUI.DrawTexture(dropZone, SpriteAnimatorWindow.EditorResources.GetIcon("SpriteArea"));

                if (Event.current.type == EventType.MouseUp && Root.IsActiveWindows)
                {
                    if (dropZone.Contains(Event.current.mousePosition))
                    {
                        if (m_currentDragIndexFrame != index)
                        {
                            Root.Snapshot("MoveFrame");
                            TimeLineChangeIndex(m_currentDragIndexFrame, index);
                        }
                        m_currentDragIndexFrame = -1;
                        Event.current.Use();                        
                    }
                }
            }
            else
            {
                if (rect.Contains(Event.current.mousePosition))
                {
                    m_currentTooltipIndexFrame = index;
                }
            }

            if (Event.current.type == EventType.MouseDrag &&
                rect.Contains(Event.current.mousePosition) &&
                m_currentDragIndexFrame == -1 && Root.IsActiveWindows)
            {
                m_currentDragIndexFrame = index;
                Event.current.Use();
            }           

            if (Event.current.type == EventType.MouseDown &&
                rect.Contains(Event.current.mousePosition) && Root.IsActiveWindows)
            {
                if (Event.current.button == 0)
                {
                    Root.Snapshot("Select Frame");                    
                    m_player.GoToFrame(index);
                    GUI.FocusControl("TimeLineSprite." + index);
                    Event.current.Use();
                }
                else if (Event.current.button == 1)
                {
                    Root.Snapshot("Select Frame");
                    m_player.GoToFrame(index);
                    GUI.FocusControl("TimeLineSprite." + index);
                    Root.MenuContext.Prepare();
                    Root.MenuContext.AddItem("Delete", () =>
                    {
                        Root.Snapshot("RemoveFrame");
                        m_animation.RemoveFrame(index);
                    });
                    Root.MenuContext.ShowAsContext();
                    Event.current.Use();
                }                
            }

            if (SpriteAnimatorWindow.ActiveContext != SpriteAnimatorWindow.Context.Control || !Root.IsActiveWindows) return;

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Delete)
            {
                Root.Snapshot("RemoveFrame");
                Root.ShowNotification(new GUIContent($"Remove frame {index}"));
                m_animation.RemoveFrame(index);
                Event.current.Use();
            }

        }

        private void TimeLineChangeIndex(int fromIndex, int toIndex)
        {
            Root.Snapshot("Move Frame");
            m_animation.ChangeFrameIndex(fromIndex, toIndex);
        }

        private bool GUILayoutButtonIcon(Texture icon, params GUILayoutOption[] options)
        {
            GUILayout.Label("", options);
            Rect rect = GUILayoutUtility.GetLastRect();

            GUI.Box(rect, "", SpriteAnimatorWindow.EditorResources.ButtonNonMargin);
            GUI.DrawTexture(rect, icon);

            if(Event.current.isMouse && Event.current.type == EventType.MouseDown && Event.current.button == 0 && 
                rect.Contains(Event.current.mousePosition) && Root.IsActiveWindows)
            {
                Event.current.Use();
                return true;
            }

            return false;
        }

        private bool GUILayoutButtonIcon(Texture icon, GUIStyle style, int size = 15, params GUILayoutOption[] options)
        {
            GUILayout.Label("", options);
            Rect rect = GUILayoutUtility.GetLastRect();

            GUI.Box(rect, "", style);

            Rect iconRect = rect;
            int scale = size;
            iconRect.width -= scale;
            iconRect.height -= scale;
            iconRect.x += (scale / 2) + 1;
            iconRect.y += (scale / 2) + 1;
            GUI.DrawTexture(iconRect, icon);

            if (Event.current.isMouse && Event.current.type == EventType.MouseDown && Event.current.button == 0 &&
                rect.Contains(Event.current.mousePosition) && Root.IsActiveWindows)
            {
                Event.current.Use();
                return true;
            }

            return false;
        }

        internal void SetAnimation(SpriteAnimation spriteAnimation)
        {
            m_animation = spriteAnimation;
            m_player = new SpriteAnimatorPlayer(m_animation);            
            m_player.MoveBackFrame();
        }

        private void DrawSpriteRender(Sprite sprite)
        {
            GUIPro.Layout.Control(GUIPro.Layout.Direction.Vertical, -1, -1, SpriteAnimatorWindow.EditorResources.Background1, null);
            Rect rect = GUILayoutUtility.GetLastRect();
            EditorGUI.DrawRect(rect, SpriteAnimatorWindow.EditorResources.Colors[3]);

            if (sprite == null)
            {
                GUI.Label(rect, "Frame empty", SpriteAnimatorWindow.EditorResources.Background3);
                return;
            }            

            Rect spriteRect = rect;
            spriteRect.width -= 50;
            spriteRect.height -= 30;
            spriteRect.x += 25;
            spriteRect.y += 25;

            spriteRect =  DrawSpriteRenderInternal(spriteRect, sprite, Root.Data.ScaleSprite / 100, Root.Data.DrawSpriteZone);

            GUI.Label(new Rect(rect.x, rect.y, rect.width, 20), $"Render {sprite.rect.size} | {Root.Data.ScaleSprite}%",  SpriteAnimatorWindow.EditorResources.Header0);

            if (Root.Data.ShowPivots)
            {
                DrawPivots(spriteRect);
            }
        }

       

        public static Rect DrawSpriteRenderInternal(Rect rect, Sprite sprite, float scale, bool spriteZone = true)
        {
            if (sprite == null)
            {
                GUI.Label(rect, "Empty", SpriteAnimatorWindow.EditorResources.Background3);
                return rect;
            }

            Rect spaceRect = rect;
            scale = Mathf.Clamp(scale, 0.1f, 1.0f);

            Rect textCoords = sprite.rect;
            textCoords.xMin /= sprite.texture.width;
            textCoords.xMax /= sprite.texture.width;
            textCoords.yMin /= sprite.texture.height;
            textCoords.yMax /= sprite.texture.height;
            
            Vector2 ratio = (spaceRect.size / sprite.rect.size);

            rect.size = sprite.rect.size;
            rect.size *= Mathf.Min(ratio.x, ratio.y);
            rect.size *= scale;
            rect.x += (spaceRect.width - rect.width) / 2;
            rect.y += (spaceRect.height - rect.height) / 2;

            if (spriteZone)
                EditorGUI.DrawRect(rect, SpriteAnimatorWindow.Instance.Data.PreviewBackgroundColor);

            GUI.DrawTextureWithTexCoords(rect, sprite.texture, textCoords);
            return rect;
        }

        private void DrawPivots(Rect rect)
        {
            try
            {
                SpriteAnimation.FrameInfo data = m_animation.Frames[m_player.FrameIndex];

                for (int i = 0; i < m_animation.Pivots.Length; i++)
                {
                    if (data.Pivots.Count - 1 < i)
                        break;

                    GUI.SetNextControlName("FramePivot." + i);
                    data.Pivots[i] = DrawPivotViewPlayer(i, data.Pivots[i], m_animation.Pivots[i].Name, rect);
                }
            }
            catch
            {
                return;
            }
        }

        private Vector2 DrawPivotViewPlayer(int index, Vector2 point, string name, Rect rect)
        {
            float posX = rect.width * point.x;
            float posY = rect.height * point.y;
            float pivotScale = 10 * (Root.Data.ScalePivot / 100);

            Rect pivotRect = new Rect(rect.x + posX - pivotScale / 2, rect.y + posY - pivotScale / 2, pivotScale, pivotScale);
            EditorGUI.DrawRect(pivotRect, m_animation.Pivots[index].Color);

            Rect labelRect = pivotRect;
            labelRect.height = 20;
            labelRect.width = 200;
            labelRect.x = rect.x + posX + 5;
            labelRect.y = rect.y + posY - 11;
            
            GUI.Label(labelRect, $"[{index}]{name}", SpriteAnimatorWindow.EditorResources.LabelPivot);            


            if (Event.current.type == EventType.MouseDown && pivotRect.Contains(Event.current.mousePosition) && Root.IsActiveWindows)
            {
                GUI.FocusControl("FramePivot" + index);
                m_pivotOriginalPos = point;
                m_currentDragIndexPivot = index;
                Event.current.Use();
            }
            else if (Event.current.type == EventType.MouseUp && Root.IsActiveWindows && m_currentDragIndexPivot != -1)
            {
                Vector2 newPos = m_animation.Frames[m_player.FrameIndex].Pivots[index];
                m_animation.Frames[m_player.FrameIndex].Pivots[index] = m_pivotOriginalPos;
                if (newPos != m_pivotOriginalPos) Root.Snapshot("MovePivot");
                m_animation.Frames[m_player.FrameIndex].Pivots[index] = newPos;
                m_currentDragIndexPivot = -1;
                Event.current.Use();
            }
            else if (m_currentDragIndexPivot == index && Event.current.type == EventType.MouseDrag && Root.IsActiveWindows)
            {
                point += Event.current.delta / new Vector2(rect.width, rect.height);
                Event.current.Use();
            }
            return new Vector2(Mathf.Clamp(point.x, 0, 1), Mathf.Clamp(point.y, 0, 1));
        }

        private static void DropAreaGUI(Rect rect, Action<List<UnityEngine.Object>> OnDrop, params Type[] validTypes)
        {
            Rect drop_area = rect;

            switch (Event.current.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!drop_area.Contains(Event.current.mousePosition)) break;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Move;

                    if (Event.current.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        List<UnityEngine.Object> objs = new List<UnityEngine.Object>();

                        foreach (UnityEngine.Object dragged_object in DragAndDrop.objectReferences)
                        {
                            if (validTypes.Contains(dragged_object.GetType()))
                            {
                                objs.Add(dragged_object);
                            }
                            else
                            {
                                string types = "";
                                validTypes.ToList().ForEach(x => types += Environment.NewLine + "-" + x.Name);
                                types = types.TrimEnd(',');
                                EditorUtility.DisplayDialog("Error", string.Format("No compatible type \n {0}", types), "ok!");
                            }
                        }
                        OnDrop.Invoke(objs);
                    }
                    break;
            }
        }

        internal byte[] Serialize()
        {

            using (MemoryStream mem = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(mem))
                {
                    writer.Write(m_timelineScroll.x);
                    writer.Write(m_timelineScroll.y);
                    writer.Write(m_settingScroll.x);
                    writer.Write(m_timelineScroll.y);
                    writer.Write(m_player == null ? 0 : m_player.FrameIndex);
                }

                return mem.ToArray();
            }
        }

        internal void Deserialize(byte[] bytes)
        {
            using (MemoryStream mem = new MemoryStream(bytes))
            {
                using (BinaryReader reader = new BinaryReader(mem))
                {
                    m_timelineScroll.x = reader.ReadSingle();
                    m_timelineScroll.y = reader.ReadSingle();
                    m_settingScroll.x = reader.ReadSingle();
                    m_settingScroll.y = reader.ReadSingle();
                    if (m_player != null)
                        m_player.GoToFrame(reader.ReadInt32());
                }
            }
        }

        public void OnBeforeSerialize()
        {
            m_serializeData = Serialize();
        }

        public void OnAfterDeserialize()
        {
            if (m_serializeData != null)  Deserialize(m_serializeData);
        }
    }
}
