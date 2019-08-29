using UnityEngine;
using UnityEditor;
using System;
using SpriteAnimatorEditor;

internal static class GUIPro
{
    public const int MAX_HEIGTH = 32;
    private const int INPUT_WIDTH_VALUE = 154;
    private const int INPUT_SPACE = 150;
    private static readonly GUIStyle m_defaultGUISkin = new GUIStyle();

    public static class Field
    {
        private static bool Toggle(Rect rect, bool value)
        {
            if (AvailableForDraw()) return value;
            rect.x += 38;
            float width = rect.width;
            width = 40;
            //background
            Rect rectBackground = new Rect(rect.x, rect.y + 6, width, rect.height - 12);
            GUI.Box(rectBackground, "", value ? SpriteAnimatorWindow.EditorResources.ToggleBackgroundTrue : SpriteAnimatorWindow.EditorResources.ToggleBackgroundFalse);

            Rect controlRect = new Rect(rect.x + (value ? width / 2 : 0), rect.y + 3, width / 2, rect.height - 6);
            GUI.Box(controlRect, "", SpriteAnimatorWindow.EditorResources.ToggleBackgroundControl);

            if (Event.current.button == 0 &&
                Event.current.type == EventType.MouseDown &&
                rectBackground.Contains(Event.current.mousePosition))
            {
                Event.current.Use();
                return !value;
            }

            return value;
        }

        public static bool Toggle(string text, bool value)
        {
            if (AvailableForDraw()) return value;

            using (GUILayout.HorizontalScope scope = new GUILayout.HorizontalScope(GUILayout.Height(20)))
            {
                GUILayout.Label(text.ToUpper(), SpriteAnimatorWindow.EditorResources.Field, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                Rect rect = GUILayoutUtility.GetLastRect();
                rect.x += rect.width - 84;
                rect.height -= 8;
                rect.y += 4;
                rect.width = 80;
                value = Toggle(rect, value);
            }
            
            return value;
        }

        public static Color ColorBox(string text, Color value)
        {
            if (AvailableForDraw()) return value;

            using (GUILayout.HorizontalScope scope = new GUILayout.HorizontalScope(GUILayout.Height(20)))
            {
                GUILayout.Label(text.ToUpper(), SpriteAnimatorWindow.EditorResources.Field, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                Rect rect = GUILayoutUtility.GetLastRect();
                rect.x += rect.width - INPUT_WIDTH_VALUE;
                rect.height -= 14;
                rect.y += 7;
                rect.width = INPUT_SPACE;
                value = EditorGUI.ColorField(rect, value);
            }
            return value;
        }

        public static Vector3 Vector3(string text, Vector3 value)
        {
            if (AvailableForDraw()) return value;
            using (GUILayout.HorizontalScope scope = new GUILayout.HorizontalScope(GUILayout.Height(20)))
            {
                GUILayout.Label(text.ToUpper(), SpriteAnimatorWindow.EditorResources.Field, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                Rect rect = GUILayoutUtility.GetLastRect();
                rect.x += rect.width - INPUT_WIDTH_VALUE;
                rect.height -= 14;
                rect.y += 7;
                rect.width = INPUT_SPACE;
                value = EditorGUI.Vector3Field(rect, "", value);
            }
            return value;
        }

        public static string TextBox(string text, string value)
        {
            if (AvailableForDraw()) return value;
            using (GUILayout.HorizontalScope scope = new GUILayout.HorizontalScope(GUILayout.Height(20)))
            {
                GUILayout.Label(text.ToUpper(), SpriteAnimatorWindow.EditorResources.Field, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                Rect rect = GUILayoutUtility.GetLastRect();
                rect.x += rect.width - INPUT_WIDTH_VALUE;
                rect.height -= 8;
                rect.y += 4;
                rect.width = INPUT_SPACE;
                value = GUI.TextField(rect, value, SpriteAnimatorWindow.EditorResources.FieldValue);
            }
            return value;
        }

        public static float NumberBox(string text, float value)
        {
            if (AvailableForDraw()) return value;
            using (GUILayout.HorizontalScope scope = new GUILayout.HorizontalScope(GUILayout.Height(20)))
            {
                GUILayout.Label(text.ToUpper(), SpriteAnimatorWindow.EditorResources.Field, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                Rect rect = GUILayoutUtility.GetLastRect();
                rect.x += rect.width - INPUT_WIDTH_VALUE;
                rect.height -= 8;
                rect.y += 4;
                rect.width = INPUT_SPACE;
                value = EditorGUI.FloatField(rect, value, SpriteAnimatorWindow.EditorResources.FieldValue);
            }
            return value;
        }

        public static Enum Enum(string text, Enum value)
        {
            if (AvailableForDraw()) return value;
            using (GUILayout.HorizontalScope scope = new GUILayout.HorizontalScope(GUILayout.Height(20)))
            {
                GUILayout.Label(text.ToUpper(), SpriteAnimatorWindow.EditorResources.Field, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                Rect rect = GUILayoutUtility.GetLastRect();
                rect.x += rect.width - INPUT_WIDTH_VALUE;
                rect.height -= 8;
                rect.y += 4;
                rect.width = INPUT_SPACE;
                value = EditorGUI.EnumPopup(rect, value, SpriteAnimatorWindow.EditorResources.FieldEnum);

                if(GUI.enabled)
                    GUI.DrawTexture(new Rect(rect.x + 178, rect.y + 3, rect.height - 6, rect.height - 6), SpriteAnimatorWindow.EditorResources.GetIcon("ListEnum"));
            }
            return value;
        }

        public static float Slider (string text, float value, float min, float max, Action onChange = null)
        {
            if (AvailableForDraw()) return value;

            float initialValue = value;
            using (GUILayout.HorizontalScope scope = new GUILayout.HorizontalScope(GUILayout.Height(20)))
            {
                GUILayout.Label(text.ToUpper(), SpriteAnimatorWindow.EditorResources.Field, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                Rect rect = GUILayoutUtility.GetLastRect();
                rect.x += rect.width - INPUT_WIDTH_VALUE;
                rect.height -= 8;
                rect.y += 4;
                rect.width = INPUT_SPACE;

                rect.width -= 10;
                rect.x += 8;
                rect.height -= 5;
                rect.y += 2.5f;
                value = EditorGUI.Slider(rect, "", value, min, max);
            }
            if (initialValue != value) onChange?.Invoke();
            return value;
        }

        public static UnityEngine.Object ObjectField(string text, UnityEngine.Object value, Type type)
        {
            if (AvailableForDraw()) return value;
            using (GUILayout.HorizontalScope scope = new GUILayout.HorizontalScope(GUILayout.Height(20)))
            {
                GUILayout.Label(text.ToUpper(), SpriteAnimatorWindow.EditorResources.Field, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                Rect rect = GUILayoutUtility.GetLastRect();
                rect.x += rect.width - INPUT_WIDTH_VALUE;
                rect.x += 0;
                rect.height -= 16;
                rect.y += 8;
                rect.width = INPUT_SPACE;

                value = EditorGUI.ObjectField(rect, value, type, true);


                GUIContent icon = EditorGUIUtility.ObjectContent(null, type);
                Rect iconRect = new Rect(rect.x + 184, rect.y - 2, 20, 20);
                GUI.Box(iconRect,"", SpriteAnimatorWindow.EditorResources.Field);
                GUI.DrawTexture(iconRect, EditorGUIUtility.IconContent("winbtn_mac_max").image);
                if (icon != null)
                    GUI.DrawTexture(new Rect(rect.x - 30, rect.y - 2, 20, 20), icon.image);
            }
            return value;
        }

        public static T Draw<T>(string text, T value, Action onChange = null)
        {
            T initialValue = value;
            if(IsType(typeof(T), typeof(int)))
            {
                int v = (int)NumberBox(text, (int)Convert.ChangeType(value, typeof(T)));
                value = (T)Convert.ChangeType(v, typeof(T));
            }
            else if (IsType(typeof(T), typeof(bool)))
            {
                bool v = Toggle(text, (bool)Convert.ChangeType(value, typeof(T)));
                value = (T)Convert.ChangeType(v, typeof(T));
            }
            else if (IsType(typeof(T), typeof(Color)))
            {
                Color v = ColorBox(text, (Color)Convert.ChangeType(value, typeof(T)));
                value = (T)Convert.ChangeType(v, typeof(T));
            }
            else if (IsType(typeof(T), typeof(float)))
            {
                float v = NumberBox(text, (float)Convert.ChangeType(value, typeof(T)));
                value = (T)Convert.ChangeType(v, typeof(T));
            }
            else if (IsType(typeof(T), typeof(Single)))
            {
                float v = NumberBox(text, (float)Convert.ChangeType(value, typeof(T)));
                value = (T)Convert.ChangeType(v, typeof(T));
            }
            else if (IsType(typeof(T), typeof(uint)))
            {
                uint v = (uint)NumberBox(text, (uint)Convert.ChangeType(value, typeof(T)));
                value = (T)Convert.ChangeType(v, typeof(T));
            }
            else if (IsType(typeof(T), typeof(string)))
            {
                string v = TextBox(text, (string)Convert.ChangeType(value, typeof(T)));
                value = (T)Convert.ChangeType(v, typeof(T));
            }
            else if (typeof(T).IsEnum)
            {
                Enum v = Enum(text, (Enum)Convert.ChangeType(value, typeof(T)));
                value = (T)Convert.ChangeType(v, typeof(T));
            }
            else if (typeof(UnityEngine.Object).IsAssignableFrom(typeof(T)))
            {
                UnityEngine.Object v = ObjectField(text, (UnityEngine.Object)Convert.ChangeType(value, typeof(T)), typeof(T));
                value = (T)Convert.ChangeType(v, typeof(T));
            }
            else
            {
                GUILayout.Label(text + " > " + typeof(T), SpriteAnimatorWindow.EditorResources.Field);
            }           

            if (!value.Equals(initialValue)) onChange?.Invoke();
            return value;
        }
        
        public static bool IsType(Type type, params Type[] types)
        {
            for(int i = 0; i < types.Length; i++)
            {
                if (type == types[i])
                    return true;
            }
            return false;
        }
    }

    public static class Button
    {
        public static bool Normal(string text)
        {
            return GUILayout.Button(text.ToUpper(), SpriteAnimatorWindow.EditorResources.ButtonDefault);
        }

        public static bool Alert(string text)
        {
            return GUILayout.Button(text.ToUpper(), SpriteAnimatorWindow.EditorResources.ButtonAlert);
        }

        public static bool Alternative(string text)
        {
            return GUILayout.Button(text.ToUpper(), SpriteAnimatorWindow.EditorResources.ButtonAlternative);
        }

        public static bool ContextFold(bool value, string text, string icon_enable, string icon_disable, Action onChange = null)
        {
            GUILayout.Label("", SpriteAnimatorWindow.EditorResources.SeparatorField, GUILayout.ExpandWidth(true), GUILayout.Height(30));
            GUIContent icon = EditorGUIUtility.IconContent(value ? icon_enable : icon_disable);
            Rect rect = GUILayoutUtility.GetLastRect();
            Rect rectInput = rect;
            GUI.Label(new Rect(rect.x + 25, rect.y + 5, rect.width, rect.height), text);
            GUI.Label(new Rect(rect.x + 5, rect.y + 6, 30, 30), icon);

            if (Event.current.button == 0 &&
                Event.current.type == EventType.MouseDown &&
                rectInput.Contains(Event.current.mousePosition))
            {
                Event.current.Use();
                onChange?.Invoke();
                return !value;
            }

            return value;
        }
    }

    public static class Layout
    {
        public enum Direction
        {
            Vertical,
            Horizontal
        }

        /// <summary>
        /// Set -1 for use 100% of space
        /// </summary>
        /// <param name="width"></param>
        /// <param name="heigth"></param>
        public static void Control(Direction direction, int width, int heigth, GUIStyle style, Action draw)
        {
            if (style == null)
                style = m_defaultGUISkin;

            GUILayoutOption[] options = new GUILayoutOption[]
            {
                (heigth == -1) ? GUILayout.ExpandHeight(true) : GUILayout.Height(heigth),
                (width == -1) ? GUILayout.ExpandWidth(true) : GUILayout.Width(width)
            };

            switch (direction)
            {
                case Direction.Vertical:
                    {
                        using (GUILayout.VerticalScope scope = new GUILayout.VerticalScope(style, options))
                        {
                            if (draw != null) draw.Invoke();
                        }
                    }
                    break;
                case Direction.Horizontal:
                    {
                        using (GUILayout.HorizontalScope scope = new GUILayout.HorizontalScope(style, options))
                        {
                            if (draw != null) draw.Invoke();
                        }
                    }
                    break;
            }
        }

        public static void ControlCenter(Action draw)
        {
            using (GUILayout.VerticalScope vscope = new GUILayout.VerticalScope())
            {
                GUILayout.FlexibleSpace();
                using (GUILayout.HorizontalScope hscope = new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    draw?.Invoke();
                    GUILayout.FlexibleSpace();
                }
                GUILayout.FlexibleSpace();
            }
        }

    }
    
    public static class Elements
    {
        public static void Header(string text, int heigth)
        {
            GUILayout.Label(text.ToUpper(), SpriteAnimatorWindow.EditorResources.Header0, GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(true), GUILayout.Height(heigth));
        }
    }
    
    public static class Tools
    {
        internal static void DrawGrid(Rect rect, int widht, int height, int size, Color color)
        {
            for (int x = 0; x < widht; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    DrawLine(new Vector2(rect.x, rect.y + (size * y)), new Vector2(rect.x + rect.width, rect.y + (size * y)), color, 1);
                    DrawLine(new Vector2(rect.x + (size * x), rect.y), new Vector2(rect.x + (size * x), rect.y + rect.height), color, 2);
                }
            }
        }

        internal static void DrawLine(Vector2 start, Vector2 end, Color color, float width)
        {
            if (start.y == end.y)
            {
                Rect rect = new Rect(start.x, start.y, end.x - start.x, width);
                EditorGUI.DrawRect(rect, color);
            }
            else
            {
                Rect rect = new Rect(start.x, start.y, width, end.y - start.y);
                EditorGUI.DrawRect(rect, color);
            }
        }
    }

    public static bool AvailableForDraw()
    {
        return false;
    }
}