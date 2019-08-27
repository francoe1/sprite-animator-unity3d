using UnityEngine;
using UnityEditor;
using System;
using SpriteAnimatorEditor;

internal static class SpriteAnimatorEditorExtencion
{
    public const int MAX_HEIGTH = 32;
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

            GUILayout.BeginHorizontal(GUILayout.Height(20));
            {
                GUILayout.Label(text.ToUpper(), SpriteAnimatorWindow.EditorResources.Field, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                Rect rect = GUILayoutUtility.GetLastRect();
                rect.x += rect.width - 84;
                rect.height -= 8;
                rect.y += 4;
                rect.width = 80;
                value = Toggle(rect, value);
            }
            GUILayout.EndHorizontal();
            return value;
        }

        public static Color ColorBox(string text, Color value)
        {
            if (AvailableForDraw()) return value;
            GUILayout.BeginHorizontal(GUILayout.Height(20));
            {
                GUILayout.Label(text.ToUpper(), SpriteAnimatorWindow.EditorResources.Field, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                Rect rect = GUILayoutUtility.GetLastRect();
                rect.x += rect.width - 204;
                rect.height -= 14;
                rect.y += 7;
                rect.width = 200;
                value = EditorGUI.ColorField(rect, value);
            }
            GUILayout.EndHorizontal();
            return value;
        }

        public static Vector3 ColorBox(string text, Vector3 value)
        {
            if (AvailableForDraw()) return value;
            GUILayout.BeginHorizontal(GUILayout.Height(20));
            {
                GUILayout.Label(text.ToUpper(), SpriteAnimatorWindow.EditorResources.Field, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                Rect rect = GUILayoutUtility.GetLastRect();
                rect.x += rect.width - 204;
                rect.height -= 14;
                rect.y += 7;
                rect.width = 200;
                value = EditorGUI.Vector3Field(rect, "", value);
            }
            GUILayout.EndHorizontal();
            return value;
        }

        public static string TextBox(string text, string value)
        {
            if (AvailableForDraw()) return value;
            GUILayout.BeginHorizontal(GUILayout.Height(20));
            {
                GUILayout.Label(text.ToUpper(), SpriteAnimatorWindow.EditorResources.Field, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                Rect rect = GUILayoutUtility.GetLastRect();
                rect.x += rect.width - 204;
                rect.height -= 8;
                rect.y += 4;
                rect.width = 200;
                value = GUI.TextField(rect, value, SpriteAnimatorWindow.EditorResources.FieldValue);
            }
            GUILayout.EndHorizontal();
            return value;
        }

        public static float NumberBox(string text, float value)
        {
            if (AvailableForDraw()) return value;
            GUILayout.BeginHorizontal(GUILayout.Height(20));
            {
                GUILayout.Label(text.ToUpper(), SpriteAnimatorWindow.EditorResources.Field, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                Rect rect = GUILayoutUtility.GetLastRect();
                rect.x += rect.width - 204;
                rect.height -= 8;
                rect.y += 4;
                rect.width = 200;
                value = EditorGUI.FloatField(rect, value, SpriteAnimatorWindow.EditorResources.FieldValue);
            }
            GUILayout.EndHorizontal();
            return value;
        }

        public static Enum Enum(string text, System.Enum value)
        {
            if (AvailableForDraw()) return value;
            GUILayout.BeginHorizontal(GUILayout.Height(20));
            {
                GUILayout.Label(text.ToUpper(), SpriteAnimatorWindow.EditorResources.Field, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                Rect rect = GUILayoutUtility.GetLastRect();
                rect.x += rect.width - 204;
                rect.height -= 8;
                rect.y += 4;
                rect.width = 200;
                value = EditorGUI.EnumPopup(rect, value, SpriteAnimatorWindow.EditorResources.FieldEnum);

                if(GUI.enabled)
                    GUI.DrawTexture(new Rect(rect.x + 178, rect.y + 3, rect.height - 6, rect.height - 6), SpriteAnimatorWindow.EditorResources.GetIcon("ListEnum"));
            }
            GUILayout.EndHorizontal();
            return value;
        }

        public static float Slider (string text, float value, float min, float max)
        {
            if (AvailableForDraw()) return value;
            GUILayout.BeginHorizontal(GUILayout.Height(20));
            {
                GUILayout.Label(text.ToUpper(), SpriteAnimatorWindow.EditorResources.Field, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                Rect rect = GUILayoutUtility.GetLastRect();
                rect.x += rect.width - 204;
                rect.height -= 8;
                rect.y += 4;
                rect.width = 200;

                GUI.Box(rect, "", SpriteAnimatorWindow.EditorResources.FieldValue);

                rect.width -= 10;
                rect.x += 8;
                rect.height -= 5;
                rect.y += 2.5f;
                value = EditorGUI.Slider(rect, "", value, min, max);
            }
            GUILayout.EndHorizontal();
            return value;
        }

        public static UnityEngine.Object ObjectField(string text, UnityEngine.Object value, Type type)
        {
            if (AvailableForDraw()) return value;
            GUILayout.BeginHorizontal(GUILayout.Height(20));
            {
                GUILayout.Label(text.ToUpper(), SpriteAnimatorWindow.EditorResources.Field, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                Rect rect = GUILayoutUtility.GetLastRect();
                rect.x += rect.width - 204;
                rect.x += 0;
                rect.height -= 16;
                rect.y += 8;
                rect.width = 200;

                value = EditorGUI.ObjectField(rect, value, type, true);


                GUIContent icon = EditorGUIUtility.ObjectContent(null, type);
                Rect iconRect = new Rect(rect.x + 184, rect.y - 2, 20, 20);
                GUI.Box(iconRect,"", SpriteAnimatorWindow.EditorResources.Field);
                GUI.DrawTexture(iconRect, EditorGUIUtility.IconContent("winbtn_mac_max").image);
                if (icon != null)
                    GUI.DrawTexture(new Rect(rect.x - 30, rect.y - 2, 20, 20), icon.image);
            }
            GUILayout.EndHorizontal();

            return value;//EditorGUILayout.ObjectField(text, obj, type, true);
        }

        public static T Draw<T>(string text, T value)
        {
            if(IsType(typeof(T), typeof(int)))
            {
                int v = (int)NumberBox(text, (int)Convert.ChangeType(value, typeof(T)));
                return (T)Convert.ChangeType(v, typeof(T));
            }

            if (IsType(typeof(T), typeof(bool)))
            {
                bool v = Toggle(text, (bool)Convert.ChangeType(value, typeof(T)));
                return (T)Convert.ChangeType(v, typeof(T));
            }

            if (IsType(typeof(T), typeof(Color)))
            {
                Color v = ColorBox(text, (Color)Convert.ChangeType(value, typeof(T)));
                return (T)Convert.ChangeType(v, typeof(T));
            }

            if (IsType(typeof(T), typeof(float)))
            {
                float v = NumberBox(text, (float)Convert.ChangeType(value, typeof(T)));
                return (T)Convert.ChangeType(v, typeof(T));
            }

            if (IsType(typeof(T), typeof(Single)))
            {
                float v = NumberBox(text, (float)Convert.ChangeType(value, typeof(T)));
                return (T)Convert.ChangeType(v, typeof(T));
            }

            if (IsType(typeof(T), typeof(uint)))
            {
                uint v = (uint)NumberBox(text, (uint)Convert.ChangeType(value, typeof(T)));
                return (T)Convert.ChangeType(v, typeof(T));
            }

            if (IsType(typeof(T), typeof(string)))
            {
                string v = TextBox(text, (string)Convert.ChangeType(value, typeof(T)));
                return (T)Convert.ChangeType(v, typeof(T));
            }

            if (typeof(T).IsEnum)
            {
                Enum v = Enum(text, (Enum)Convert.ChangeType(value, typeof(T)));
                return (T)Convert.ChangeType(v, typeof(T));
            }

            if (typeof(UnityEngine.Object).IsAssignableFrom(typeof(T)))
            {
                UnityEngine.Object v = ObjectField(text, (UnityEngine.Object)Convert.ChangeType(value, typeof(T)), typeof(T));
                return (T)Convert.ChangeType(v, typeof(T));
            }

            GUILayout.Label(text + " > " + typeof(T), SpriteAnimatorWindow.EditorResources.Field);

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
            try
            {
                switch (direction)
                {
                    case Direction.Vertical:
                        {
                            GUILayout.BeginVertical(style,
                                (heigth == -1) ? GUILayout.ExpandHeight(true) : GUILayout.Height(heigth),
                                (width == -1) ? GUILayout.ExpandWidth(true) : GUILayout.Width(width));
                            if (draw != null) draw.Invoke();
                            GUILayout.EndVertical();
                        }
                        break;
                    case Direction.Horizontal:
                        {
                            GUILayout.BeginHorizontal(style,
                                (heigth == -1) ? GUILayout.ExpandHeight(true) : GUILayout.Height(heigth),
                                (width == -1) ? GUILayout.ExpandWidth(true) : GUILayout.Width(width));
                            if (draw != null) draw.Invoke();
                            GUILayout.EndHorizontal();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                if (SpriteAnimatorWindow.DEV_MODE)  Debug.LogException(ex);
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
    
    public static bool AvailableForDraw()
    {
        return false;
    }
}