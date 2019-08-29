using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SpriteAnimatorEditor
{
    internal class ContextMenu
    {
        public bool IsShow { get; set; }

        private List<Item> m_items = new List<Item>();
        private int m_height = 30;
        private int m_width = 150;
        private Vector2 m_position = Vector2.zero;
        private Vector2 m_mousePosition = Vector2.zero;
        private bool m_isClosing = false;

        private bool m_mouseFix = false;

        public ContextMenu()
        {
            m_items = new List<Item>();
        }

        public void AddItem(string name, Action callback)
        {
            m_items.Add(new Item {
                Name = name,
                Callback = callback,
                Rect = new Rect(0, (m_height * m_items.Count), m_width, m_height)
            });
        }

        public class Item
        {
            public string Name { get; internal set; }
            public Action Callback { get; internal set; }
            internal Rect Rect;
        }

        public void ShowAsContext()
        {
            m_mouseFix = true;
            m_position = m_mousePosition;   
            IsShow = true;
            Event.current.Use();
        }
        
        internal void UpdateMousePosition()
        {
            if (Event.current.type == EventType.MouseDown)
            {
                m_mousePosition = Event.current.mousePosition;
                m_isClosing = true;
            }
        }
        
        internal void Draw()
        {
            if (IsShow)
            {
                if (m_mouseFix)
                {
                    if (Event.current.type == EventType.MouseUp)
                    {
                        m_mousePosition = Event.current.mousePosition;
                        m_mouseFix = false;
                        m_isClosing = false;
                    }
                    return;
                }

                Rect rect = new Rect();
                rect.width = m_width;
                rect.height = m_height * m_items.Count;
                rect.position = m_position;
                rect.y -= m_height / 2;
                rect.x += 20;

                EditorGUI.DrawRect(rect, Color.red);

                if (m_items.Count > 0)
                {
                    Rect rectIcon = new Rect(rect.x - m_height,rect.y, m_height, m_height);
                    rectIcon.width -= 16;
                    rectIcon.height -= 16;
                    rectIcon.x += 20;
                    rectIcon.y += 8;
                    GUI.DrawTexture(rectIcon, SpriteAnimatorWindow.EditorResources.GetIcon("ArrowLeft"));
                }

                for (int i = 0; i < m_items.Count; i++)
                {
                    rect.height = m_height;
                    GUI.Button(rect, m_items[i].Name, SpriteAnimatorWindow.EditorResources.ButtonMenuContext);

                    if (rect.Contains(Event.current.mousePosition) && m_isClosing)
                    {
                        m_items[i].Callback?.Invoke();
                        IsShow = false;
                    }
                    rect.y += m_height;
                }
                if (m_isClosing) IsShow = false;
            }       
        }

        internal void Prepare()
        {
            m_items.Clear();
        }

        internal void AddSeparator()
        {
        }

        internal void DropDown(Vector2 position)
        {
            m_position = position;
            IsShow = true;
        }
    }
}
