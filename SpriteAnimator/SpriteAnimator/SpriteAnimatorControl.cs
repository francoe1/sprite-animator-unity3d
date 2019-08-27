using SpriteAnimatorRuntime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SpriteAnimatorEditor
{
    internal class SpriteAnimatorControl
    {
        private SpriteAnimatorWindow Root { get; set; }
        private SpriteAnimation m_animation { get; set; }
        private SpriteAnimatorPlayer m_player { get; set; }
        private Vector2 m_timelineScroll { get; set; }
        private Rect m_spriteRect { get; set; }

        private Vector2 m_settingScroll { get; set; }
        private Vector2 m_pivotZoneOffset { get; set; }
        private Matrix4x4 m_guimatrixDefault { get; set; }

        private Vector2 nonClipOffset;
        private float scale { get; set; }
        private int m_currentDragIndexPivot { get; set; }
        private int m_currentDragIndexFrame { get; set; }
        private int m_currentTooltipIndexFrame { get; set; }

        private Stack<Sprite> m_cacheSpriteToInsert { get; set; } = new Stack<Sprite>();

        public KeyCode NextFrame = KeyCode.RightArrow;
        public KeyCode BackFrame = KeyCode.LeftArrow;

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
                Root.ShowNotification(new GUIContent("Add sprite"));
            }

            while(m_cacheSpriteToInsert.Count > 0)
            {
                Sprite sprite = m_cacheSpriteToInsert.Pop();

                if (sprite)
                {
                    int index = m_animation.AddFrame(sprite);
                    m_player.GoToFrame(index);
                }
            }


            if (m_player != null)
            {
                m_player.Update();

                if (m_player.State == SpriteAnimatorPlayer.PlayerState.NextFrame ||
                    m_player.State == SpriteAnimatorPlayer.PlayerState.BackFrame)
                    Root.Repaint();
            }
        }

        internal void Draw()
        {
            if (m_animation == null)
            {
                SpriteAnimatorEditorExtencion.Elements.Header("Select or create animation", 35);
                return;
            }
            else
            {
                SpriteAnimatorEditorExtencion.Elements.Header(m_animation.Path, 35);

                GUILayout.Space(2);

                SpriteAnimatorEditorExtencion.Layout.Control(SpriteAnimatorEditorExtencion.Layout.Direction.Horizontal, -1, -1, SpriteAnimatorWindow.EditorResources.Background1, () =>
                {
                    SpriteAnimatorEditorExtencion.Layout.Control(SpriteAnimatorEditorExtencion.Layout.Direction.Vertical, -1, -1, SpriteAnimatorWindow.EditorResources.Background1, () =>
                    {
                        //Player
                        SpriteAnimatorEditorExtencion.Layout.Control(SpriteAnimatorEditorExtencion.Layout.Direction.Vertical, -1, -1, SpriteAnimatorWindow.EditorResources.Background0, DrawPlayer);
                        //Time Lines
                        SpriteAnimatorEditorExtencion.Layout.Control(SpriteAnimatorEditorExtencion.Layout.Direction.Vertical, -1, 200, SpriteAnimatorWindow.EditorResources.Background0, DrawTimeLine);
                    });
                    GUILayout.Space(2);
                    //Tools
                    SpriteAnimatorEditorExtencion.Layout.Control(SpriteAnimatorEditorExtencion.Layout.Direction.Vertical, 300, -1, SpriteAnimatorWindow.EditorResources.Background0, DrawSetting);
                });
                Root.Repaint();
                Update();
            }

            if(Event.current.type == EventType.MouseUp && (m_currentDragIndexFrame != -1 || m_currentDragIndexPivot != -1))
            {
                m_currentDragIndexFrame = -1;
                m_currentDragIndexPivot = -1;
            }

            if(Root.IsActiveWindows)
                DetectShortcutKey();
        }

        private void DetectShortcutKey()
        {
            if (Event.current.type == EventType.KeyDown)
            {
                if(Event.current.keyCode == NextFrame)
                {
                    m_player.MoveNextFrame();
                    Event.current.Use();
                }

                if (Event.current.keyCode == BackFrame)
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
            m_settingScroll = GUILayout.BeginScrollView(m_settingScroll);

            EditorGUI.BeginChangeCheck();

            if (GUILayout.Button("ANIMATION OPTIONS", SpriteAnimatorWindow.EditorResources.SeparatorField, GUILayout.ExpandWidth(true), GUILayout.Height(30)))
                Root.Data.ShowAnimationOptions = !Root.Data.ShowAnimationOptions;

            if (Root.Data.ShowAnimationOptions)
            {
                GUI.enabled = false;
                SpriteAnimatorEditorExtencion.Field.Draw("ID", m_animation.Id);
                GUI.enabled = true;                

                m_animation.TimePerFrame = SpriteAnimatorEditorExtencion.Field.Draw("Time Per Frame (ms)", m_animation.TimePerFrame);
                EditorGUI.BeginChangeCheck();
                SpriteAnimation.AnimationDirection dir = m_animation.Direction;
                dir = SpriteAnimatorEditorExtencion.Field.Draw("Direction:", m_animation.Direction);
                if (EditorGUI.EndChangeCheck() && dir == SpriteAnimation.AnimationDirection.None)
                    EditorUtility.DisplayDialog("error", "La animación debe tener un dirección", "ok");
                else
                    m_animation.Direction = dir;

                m_animation.Type = SpriteAnimatorEditorExtencion.Field.Draw("Type", m_animation.Type);
            }

            if (GUILayout.Button("FRAME OPTIONS", SpriteAnimatorWindow.EditorResources.SeparatorField, GUILayout.ExpandWidth(true), GUILayout.Height(30)))
                Root.Data.ShowFrameOptions = !Root.Data.ShowFrameOptions;

            if (Root.Data.ShowFrameOptions)
            {
                if (m_animation.FrameCount > 0 && m_animation.FrameCount > m_player.FrameIndex)
                {
                    m_animation.Frames[m_player.FrameIndex].Time = SpriteAnimatorEditorExtencion.Field.Slider("Time", m_animation.Frames[m_player.FrameIndex].Time, 1f, 10);
                    m_animation.Frames[m_player.FrameIndex].Sprite = (Sprite)SpriteAnimatorEditorExtencion.Field.ObjectField("Sprite", m_animation.Frames[m_player.FrameIndex].Sprite, typeof(Sprite));
                }

                Root.Data.ShowPivots = SpriteAnimatorEditorExtencion.Field.Draw("Show Pivots", Root.Data.ShowPivots);

                if (Root.Data.ShowPivots)
                {
                    if (SpriteAnimatorEditorExtencion.Button.Alternative("Add Pivot"))
                        m_animation.AddPivot();

                    {
                        Texture2D icon = SpriteAnimatorWindow.EditorResources.GetIcon("Delete");
                        for (int i = 0; i < m_animation.Pivots.Length; i++)
                        {
                            m_animation.Pivots[i].Name = SpriteAnimatorEditorExtencion.Field.Draw("Pivot " + i, m_animation.Pivots[i].Name);
                            m_animation.Pivots[i].Color = SpriteAnimatorEditorExtencion.Field.Draw("Pivot " + i, m_animation.Pivots[i].Color);

                            Rect rectPivot = GUILayoutUtility.GetLastRect();
                            rectPivot = new Rect(rectPivot.x + rectPivot.width - 230, rectPivot.y + 5, 20, 20);

                            GUI.DrawTexture(rectPivot, icon);
                            if (rectPivot.Contains(Event.current.mousePosition) &&
                                Event.current.type == EventType.MouseDown && Root.IsActiveWindows)
                            {
                                m_animation.RemovePivot(i);
                                Event.current.Use();
                                continue;
                            }
                        }
                    }
                }
            }
            
            if (GUILayout.Button("RENDER INFO", SpriteAnimatorWindow.EditorResources.SeparatorField, GUILayout.ExpandWidth(true), GUILayout.Height(30)))
                Root.Data.ShowRenderInfo = !Root.Data.ShowRenderInfo;

            if (Root.Data.ShowRenderInfo)
            {
                GUI.enabled = false;
                SpriteAnimatorEditorExtencion.Field.Draw("Time", m_animation.Time);
                SpriteAnimatorEditorExtencion.Field.Draw("State", m_player.State);
                SpriteAnimatorEditorExtencion.Field.Draw("Frame", (m_player.FrameIndex + 1));
                SpriteAnimatorEditorExtencion.Field.Draw("Direcation", m_player.Direction);

                GUI.enabled = true;
            }


            if (GUILayout.Button("RENDER OPTIONS", SpriteAnimatorWindow.EditorResources.SeparatorField, GUILayout.ExpandWidth(true), GUILayout.Height(30)))
                Root.Data.ShowRenderOptions = !Root.Data.ShowRenderOptions;

            if (Root.Data.ShowRenderOptions)
            {
                Root.Data.ScaleSprite = SpriteAnimatorEditorExtencion.Field.Slider("Render Scale", Root.Data.ScaleSprite, 0.1f, 20f);
                Root.Data.ScalePivot = SpriteAnimatorEditorExtencion.Field.Slider("Pivot Scale", Root.Data.ScalePivot, 1f, 30f);
                //Root.Data.ScaleSprite = SpriteAnimatorEditorExtencion.Field.Draw("Scale", Root.Data.ScaleSprite);
                Mathf.Clamp(Root.Data.ScaleSprite, .1f, 10);
                Root.Data.PreviewBackgroundColor = SpriteAnimatorEditorExtencion.Field.Draw("Background", Root.Data.PreviewBackgroundColor);
                Root.Data.DrawSpriteZone = SpriteAnimatorEditorExtencion.Field.Draw("Sprite Zone", Root.Data.DrawSpriteZone);
                Root.Data.AutoScale = SpriteAnimatorEditorExtencion.Field.Draw("Auto Scale", Root.Data.AutoScale);
            }
            GUILayout.EndScrollView();


            if(EditorGUI.EndChangeCheck())
            {
                Root.Snapshot("options");
                Root.Repaint();
                Root.Save();
            }
        }

        private void DrawPlayer()
        {
            SpriteAnimatorEditorExtencion.Layout.Control(SpriteAnimatorEditorExtencion.Layout.Direction.Vertical, -1, -1, SpriteAnimatorWindow.EditorResources.Background0, () =>
            {
                SpriteAnimatorEditorExtencion.Layout.Control(SpriteAnimatorEditorExtencion.Layout.Direction.Vertical, -1, -1, SpriteAnimatorWindow.EditorResources.Background0, () =>
                {
                    SpriteAnimatorEditorExtencion.Elements.Header("Render (" + m_spriteRect.width 
                        + " x " + m_spriteRect.height + ")"
                        + scale.ToString("F1") + "x", 30);  
                    
                    DrawSpriteRender(m_player.FrameSprite);
                });
                SpriteAnimatorEditorExtencion.Layout.Control(SpriteAnimatorEditorExtencion.Layout.Direction.Horizontal, -1, 30, SpriteAnimatorWindow.EditorResources.Background0, DrawPlayerBar);
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
            GUILayout.BeginHorizontal("Timeline", SpriteAnimatorWindow.EditorResources.Field, GUILayout.ExpandWidth(true), GUILayout.Height(20));
            {
                if (GUILayout.Button("Order By name", SpriteAnimatorWindow.EditorResources.ButtonGreen, GUILayout.Height(20), GUILayout.Width(150)))
                {
                    Root.Snapshot("Order by Name");
                    m_animation.SetFrames(m_animation.Frames.OrderBy(x => x.Sprite.name).ToList());
                }
            }
            GUILayout.EndHorizontal();

            m_timelineScroll = GUILayout.BeginScrollView(m_timelineScroll, false, false, GUILayout.Height(150));
            {
                GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.BeginHorizontal();
                    {                        
                        for (int i = 0; i < m_animation.FrameCount; i++)
                        {
                            GUILayout.Box("", GUIStyle.none, GUILayout.Width(80 * m_animation.Frames[i].Time), GUILayout.Height(130));
                            Rect rectElement = GUILayoutUtility.GetLastRect();
                            GUILayout.Space(2);
                            DrawElementInTimeLine(m_animation.Frames[i], rectElement, m_player.FrameIndex == i, i);                           
                        }
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndScrollView();

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
                DrawSpriteTimeLine(m_animation.Frames[m_currentDragIndexFrame].Sprite,
                    new Rect(Event.current.mousePosition.x - 80, Event.current.mousePosition.y - 80, 130, 80));
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
        
        private void DrawElementInTimeLine(SpriteAnimation.FrameInfo frame, Rect rect, bool active, int index)
        {
            EditorGUI.DrawRect(rect, active ? SpriteAnimatorWindow.EditorResources.Colors[2] : SpriteAnimatorWindow.EditorResources.Colors[1]);
            DrawSpriteTimeLine(frame.Sprite, rect);

            GUI.Label(new Rect(rect.x, rect.y, rect.width, 20), index + "-"+
                frame.Time.ToString("F2") +
                "["+ ((frame.Time * m_animation.TimePerFrame) / 60).ToString("F2") +"]", SpriteAnimatorWindow.EditorResources.LabelTimeLine);

            

            
            if(m_currentDragIndexFrame != -1)
            {
                Rect dropZone = new Rect(rect.x + 10, rect.y + 30, 64, 64);
                GUI.DrawTexture(dropZone, SpriteAnimatorWindow.EditorResources.GetIcon("SpriteArea"));

                if (Event.current.type == EventType.MouseUp && Root.IsActiveWindows)
                {
                    if (dropZone.Contains(Event.current.mousePosition))
                    {
                        if (m_currentDragIndexFrame != index)
                            TimeLineChangeIndex(m_currentDragIndexFrame, index);
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
                    m_player.GoToFrame(index);
                    Event.current.Use();
                }
                else if (Event.current.button == 1)
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Remove"), false, () =>
                    {
                        m_animation.RemoveFrame(index);
                        Root.Repaint();
                    });

                    menu.ShowAsContext();
                }                
            }

            /*
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Delete)
            {
                Root.ShowNotification(new GUIContent($"Remove frame {index}"));
                m_animation.RemoveFrame(index);
                Event.current.Use();
            }*/

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
            Root.Snapshot("Set Animation");
            m_animation = spriteAnimation;
            m_player = new SpriteAnimatorPlayer(m_animation);            
            m_player.MoveBackFrame();
        }

        private void DrawSpriteRender(Sprite sprite)
        {
            SpriteAnimatorEditorExtencion.Layout.Control(SpriteAnimatorEditorExtencion.Layout.Direction.Vertical, -1, -1, SpriteAnimatorWindow.EditorResources.Background1, null);
            Rect rect = GUILayoutUtility.GetLastRect();
            Rect nonClipRect = rect;

            EditorGUI.DrawRect(rect, Root.Data.PreviewBackgroundColor);
            
            if (sprite == null)
            {
                GUI.Label(rect, "Clip is Empty", SpriteAnimatorWindow.EditorResources.Background3);
                return;
            }

            Matrix4x4 matrix = GUI.matrix;

            if (Event.current.type == EventType.Repaint)
            {

                GUI.BeginGroup(rect);                
                int factorScaleX = Mathf.FloorToInt(rect.width / sprite.rect.width);
                int factorScaleY = Mathf.FloorToInt(rect.height / sprite.rect.height);
                float factorTarget = Mathf.Min(factorScaleX, factorScaleY);
                
                scale = (Root.Data.AutoScale) ? factorTarget: Root.Data.ScaleSprite;

                scale = Mathf.Clamp(scale, 0.01f, float.PositiveInfinity);

                GUIUtility.ScaleAroundPivot(Vector2.one * scale, new Vector2(rect.width / 2, rect.height / 2));

                Rect spriteOriginalRect = sprite.rect;
                nonClipOffset = Vector2.zero;

                rect.x = rect.y = 0;
                Texture2D texture = sprite.texture;
                spriteOriginalRect.xMin /= texture.width;
                spriteOriginalRect.xMax /= texture.width;
                spriteOriginalRect.yMin /= texture.height;
                spriteOriginalRect.yMax /= texture.height;

                float height = sprite.rect.height;
                float width = sprite.rect.width;

                /*
                if ((factorIsVertical && height * scale > rect.height) || (!factorIsVertical && width * scale > rect.width))
                {
                    GUI.matrix = matrix;
                    GUI.Label(rect, "Error, use auto-scale option", SpriteAnimatorWindow.EditorResources.Background3);
                }
                else*/
                {
                    float y = (rect.height - height) / 2;
                    float x = (rect.width - width) / 2;

                    Rect spriteRect = new Rect(x, y, width, height);

                    nonClipOffset.x = (nonClipRect.width - (width * scale)) / 2;
                    nonClipOffset.y = (nonClipRect.height - (height * scale)) / 2;
                    
                    if (Root.Data.DrawSpriteZone)
                        EditorGUI.DrawRect(spriteRect, new Color(0, 0, 0, .05f));

                    GUI.DrawTextureWithTexCoords(spriteRect, texture, spriteOriginalRect);
                    m_spriteRect = spriteRect;
                    GUI.matrix = matrix;
                }
                GUI.EndGroup();
            }

            GUI.Label(new Rect(nonClipRect.x, nonClipRect.y, nonClipRect.width, 20),
                nonClipRect.width + "x" + nonClipRect.height + sprite.rect.size,
                SpriteAnimatorWindow.EditorResources.Background5);

            if (Root.Data.ShowPivots)
            {
                Rect pivotRect = m_spriteRect;
                pivotRect.position = nonClipRect.position;
                pivotRect.position += nonClipOffset;
                pivotRect.size *= scale;
                DrawPivots(pivotRect);
            }
        }

        private void DrawSpriteTimeLine(Sprite sprite, Rect rect)
        {
            if (sprite == null)
            {
                rect.y += 40;
                rect.height = 30;
                GUI.Label(rect, "Empty", SpriteAnimatorWindow.EditorResources.Background3);
                return;
            }

            Rect positionRect = sprite.rect;
            Texture2D texture = sprite.texture;
            positionRect.xMin /= texture.width;
            positionRect.xMax /= texture.width;
            positionRect.yMin /= texture.height;
            positionRect.yMax /= texture.height;

            float height = 64;
            float width = 64;
            
            float y = rect.y + 30;
            float x = rect.x + (rect.width - width) / 2;
            Rect spriteRect = new Rect(x, y, width, height);

            EditorGUI.DrawRect(spriteRect, Root.Data.PreviewBackgroundColor);
            GUI.DrawTextureWithTexCoords(spriteRect, texture, positionRect);
        }

        private void DrawPivots(Rect rect)
        {
            //EditorGUI.DrawRect(rect, new Color(255, 0, 0, .5f));

            try
            {
                SpriteAnimation.FrameInfo data = m_animation.Frames[m_player.FrameIndex];

                for (int i = 0; i < m_animation.Pivots.Length; i++)
                {
                    if (data.Pivots.Count - 1 < i)
                        break;

                    data.Pivots[i] = DrawPivotInGUI(i, data.Pivots[i], m_animation.Pivots[i].Name, rect);
                }
            }
            catch
            {
                return;
            }
        }

        private Vector2 DrawPivotInGUI(int index, Vector2 point, string name, Rect rect)
        {
            Rect controlRect = new Rect(rect.x + (point.x * scale), rect.y + (point.y * scale), 
                Root.Data.ScalePivot * scale, Root.Data.ScalePivot * scale);

            EditorGUI.DrawRect(controlRect, m_animation.Pivots[index].Color);
            GUIStyle styleLabel = SpriteAnimatorWindow.EditorResources.LabelPivot;
            styleLabel.fontSize = (int)controlRect.height - 5;
            GUI.Label(new Rect(controlRect.x + controlRect.width, controlRect.y, 15 * scale, controlRect.height), name + point, styleLabel);

            if (Event.current.type == EventType.MouseDown && controlRect.Contains(Event.current.mousePosition) && Root.IsActiveWindows)
            {
                m_currentDragIndexPivot = index;
                Event.current.Use();
            }
            else if (Event.current.type == EventType.MouseUp && Root.IsActiveWindows)
            {
                m_currentDragIndexPivot = -1;
            }
            else if (m_currentDragIndexPivot == index && Event.current.type == EventType.MouseDrag && Root.IsActiveWindows)
            {
                point += (Event.current.delta / scale);
                point.x = Mathf.Clamp(point.x, 0, m_player.FrameSprite.rect.width - 2);
                point.y = Mathf.Clamp(point.y, 0, m_player.FrameSprite.rect.height - 2);
                Event.current.Use();
            }         
            return point;
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
    }
}
