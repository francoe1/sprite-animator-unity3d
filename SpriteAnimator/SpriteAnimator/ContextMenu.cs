using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpriteAnimatorEditor
{
    internal class ContextMenu
    {
        public bool IsShow { get; set; }

        private List<Item> m_items = new List<Item>();
        private int m_height = 30;
        private int m_index = -1;
        private Vector2 m_position = Vector2.zero;

        public ContextMenu()
        {
            m_items = new List<Item>();
        }

        public void AddItem(string name, Action callback)
        {
            m_items.Add(new Item {
                Name = name,
                Callback = callback,
                Rect = new Rect(0, (m_height * m_items.Count), 150, m_height)
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
            m_position = Event.current.mousePosition;
            m_position.y += m_height * .65f;
            m_position.x += 10;
            for (int i = 0; i < m_items.Count; i++)
            {
                m_items[i].Rect.x += m_position.x;
                m_items[i].Rect.y += m_position.y;
            }
            IsShow = true;
            Event.current.Use();
        }

        internal void Repaint()
        {
            if(m_index != -1 && IsShow)
            {
                if(m_items[m_index].Callback != null)
                    m_items[m_index].Callback.Invoke();
                m_index = -1;
                IsShow = false;
            }
        }

        internal void EventDetected()
        {
            if (!IsShow)
                return;

            for(int i = 0; i < m_items.Count; i++)
            {
                if(m_items[i].Rect.Contains(Event.current.mousePosition) && 
                    Event.current.type == EventType.MouseDown && 
                    Event.current.button == 0)
                {
                    m_index = i;
                    Event.current.Use();
                    return;
                }
            }

            if((Event.current.type == EventType.MouseDown || Event.current.type == EventType.KeyDown) && IsShow)
            {
                IsShow = false;
            }
        }

        internal void Draw()
        {
            if (IsShow)
            {
                if(m_items.Count > 0)
                {
                    Rect rect = new Rect(m_items[0].Rect.x - m_height, m_items[0].Rect.y, m_height, m_height);
                    rect.width -= 16;
                    rect.height -= 16;
                    rect.x += 20;
                    rect.y += 8;
                    GUI.DrawTexture(rect, SpriteAnimatorWindow.EditorResources.GetIcon("ArrowLeft"));
                }

                for (int i = 0; i < m_items.Count; i++)
                {
                    GUI.Button(m_items[i].Rect, m_items[i].Name, SpriteAnimatorWindow.EditorResources.ButtonMenuContext);
                }
            }
        }

        internal void Prepare()
        {
            m_index = -1;
            m_items.Clear();
        }

        internal void AddSeparator()
        {
        }

        internal void DropDown(Rect rect)
        {
            m_position = new Vector2(rect.x, rect.y);
            for (int i = 0; i < m_items.Count; i++)
            {
                m_items[i].Rect.x += m_position.x;
                m_items[i].Rect.y += m_position.y;
            }
            IsShow = true;
        }

        internal void GUIEvent()
        {
            if (IsShow && Event.current.type == EventType.Repaint)
                Repaint();
            else
                EventDetected();
        }
    }
}
