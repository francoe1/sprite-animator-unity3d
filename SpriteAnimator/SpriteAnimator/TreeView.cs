using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace SpriteAnimatorEditor
{
    [Serializable]
    internal class TreeView : ISerializationCallbackReceiver
    {        
        internal TreeElement Root { get; private set; } = new TreeElement { Type = TreeElement.ElementType.Folder, Name = "Root", Protected = true, };

        internal delegate void DrawElementDelegate(Rect rect, TreeElement element);
        internal DrawElementDelegate DrawElementHandler { set; private get; }
        internal delegate bool DrawElementValidateDelegate(TreeElement element);
        internal DrawElementValidateDelegate DrawElementValidateHandler;


        internal delegate void DrawTreeFinishDelegate();
        internal DrawTreeFinishDelegate DrawTreeFinishHandler = null;

        [SerializeField]
        internal byte[] m_source = null;

        internal int ElementHeight = 16;     
        private int m_rowIndex = 0;

        private int m_selectedElementId { get; set; } = -1;
        public int SelectedElementId => m_selectedElementId;
        private Vector2 m_treeScroll = Vector2.zero;
        private Rect m_treeRect;

        private TreeElement m_selectedElement { get; set; }

        public TreeElement SelectedElement
        {
            get
            {
                if (m_selectedElement == null)
                    m_selectedElement = Root.Where(x => x.ID.Equals(m_selectedElementId)).SingleOrDefault();
                return m_selectedElement;
            }
        }

        internal TreeView()
        {
            DrawElementValidateHandler = (e) => { return true; };
        }

        internal void Draw(float offsetY)
        {
            using (GUILayout.ScrollViewScope scope = new GUILayout.ScrollViewScope(m_treeScroll, false, false))
            {
                scope.handleScrollWheel = false;
                m_treeScroll = scope.scrollPosition;
               
                if (Root != null)
                {
                    if (Event.current.type == EventType.Repaint) SetRecurciveIndex(Root, -1);

                    m_rowIndex = 0;
                    if (Root.Elements == null) return;
                    for (int i = 0; i < Root.Length; i++)
                    {
                        if (Root.Elements[i] == null)
                            continue;

                        if (DrawElementHandler != null && DrawElementValidateHandler != null)
                        {
                            if (DrawElementValidateHandler.Invoke(Root.Elements[i]))
                            {

                                GUI.SetNextControlName("TreeViewElement" + (m_rowIndex + 1));
                                GUILayout.Label("", GUIStyle.none, GUILayout.Height(ElementHeight), GUILayout.ExpandWidth(true));
                                m_rowIndex++;
                                Root.Elements[i].Depth = 0;
                                if (Event.current.type == EventType.Repaint)
                                {
                                    Root.Elements[i].Index = m_rowIndex;
                                    Root.Elements[i].Rect = new Rect(0, (ElementHeight * (m_rowIndex - 1)) + offsetY, Screen.width, ElementHeight);
                                }
                                DrawElementHandler(Root.Elements[i].Rect, Root.Elements[i]);
                            }
                        }

                        if (Root.Length <= i || Root.Elements[i] == null)
                            continue;

                        if (Root.Elements[i].Visible)
                        {
                            if (Root.Elements[i].Elements != null)
                            {
                                OnCallRecursiveElement(Root.Elements[i], 1, offsetY);
                            }
                        }
                    }

                }
            }

            if (Event.current.type == EventType.Repaint) m_treeRect = GUILayoutUtility.GetLastRect();

            if (DrawTreeFinishHandler != null)
                DrawTreeFinishHandler.Invoke();

        }

         private void OnCallRecursiveElement(TreeElement element, int depth, float offsetY)
        {
            for (int i = 0; i < element.Length; i++)
            {
                if (element.Elements[i] == null)
                    continue;

                if (DrawElementHandler != null)
                {
                    GUILayout.Label("", GUIStyle.none, GUILayout.Height(ElementHeight), GUILayout.ExpandWidth(true));
                    m_rowIndex++;
                    element.Elements[i].Depth = depth;
                    if (Event.current.type == EventType.Repaint)
                    {
                        element.Elements[i].Index = m_rowIndex;
                        element.Elements[i].Rect = new Rect(0, (ElementHeight * (m_rowIndex - 1)) + offsetY, Screen.width, ElementHeight);
                    }
                    DrawElementHandler(element.Elements[i].Rect, element.Elements[i]);
                }

                if (element.Length <= i || element.Elements[i] == null)
                    continue;

                if (element.Elements[i].Visible)
                {
                    if (element.Elements[i].Elements != null)
                    {
                        element.Elements[i].Depth = depth;
                        OnCallRecursiveElement(element.Elements[i], depth + 1, offsetY);
                    }
                }
            }
        }

        private void SetRecurciveIndex(TreeElement element, int index)
        {
            element.Index = index;
            for (int i = 0; i < element.Elements.Count; i++)
                SetRecurciveIndex(element.Elements[i], index);
        }

        internal void SelectElement(TreeElement element)
        {
            if (element == null)
            {
                m_selectedElement = null;
                m_selectedElementId = -1;
            }
            else
            {
                m_selectedElementId = element.ID;
                m_selectedElement = element;

                if ((m_treeScroll.y + m_treeRect.y) - m_selectedElement.Rect.y > 0)
                {
                    m_treeScroll.y = m_selectedElement.Rect.y;
                }  
                else
                {
                    if ((m_treeScroll.y +  m_treeRect.height) < m_selectedElement.Rect.y + ElementHeight)
                    {
                        m_treeScroll.y += ElementHeight;
                    }
                }
            }
        }

        internal TreeElement GetParent(TreeElement element)
        {
            if (element == Root)
                return null;

            TreeElement e = Root.Where(x => x.Elements.Contains(element)).Take(1).SingleOrDefault();
            
            if (e == null)
                e = Root;
            return e;
        }        

        internal TreeElement[] GetElementPath(TreeElement element)
        {
            List<TreeElement> result = new List<TreeElement>();
            TreeElement parent = element;
            while(parent != null)
            {
                result.Add(parent);
                parent = GetParent(parent);
            }
            result.Reverse();
            return result.ToArray();
        }

        internal string GetDirectoryName(TreeElement element, string separator = "/")
        {
            List<string> sections = new List<string>();
            string path = "";
            while(element != null)
            {
                sections.Add(element.Name);
                element = GetParent(element);
            }

            sections.Reverse();

            path = string.Join("\\", sections.ToArray());

            if(separator.Length > 0)
                path = path.TrimEnd(separator[0]);
            return path;
        }       

        internal byte[] Serialize()
        {
            
            using (MemoryStream mem = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(mem))
                {
                    writer.Write(m_selectedElementId);
                    Root.Serialize(writer);
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
                    m_selectedElementId = reader.ReadInt32();
                    Root.Deserialize(reader);
                }
            }

            if (m_selectedElementId != -1)
            {
                TreeElement element = Root.Where(x => x.ID == m_selectedElementId).SingleOrDefault();
                SelectElement(element);
            }
                    
        }

        public void OnBeforeSerialize()
        {
            m_source = Serialize();
        }

        public void OnAfterDeserialize()
        {
            if (Root == null) Root = new TreeElement { Type = TreeElement.ElementType.Folder, Name = "Root", Protected = true, };
            if (m_source != null)  Deserialize(m_source);
        }

        internal TreeElement GetNext(TreeElement element)
        {
            if (element == null) return null;
            if (element.Index + 1 > m_rowIndex) return element;
            TreeElement el = Root.Where(x => x.Index == element.Index + 1).Take(1).SingleOrDefault();
            return el;
        }

        internal TreeElement GetBack(TreeElement element)
        {
            if (element == null) return null;
            if (element.Index - 1 <= 0) return element;            
            TreeElement el = Root.Where(x => x.Index == element.Index - 1).Take(1).SingleOrDefault();
            return el;
        }
    }
}
