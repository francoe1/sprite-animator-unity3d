using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace SpriteAnimatorEditor
{
    internal class TreeList
    {
        public TreeElement Root { get; } = new TreeElement { Type = TreeElement.ElementType.Folder, Name = "Root", Protected = true, };

        public delegate void DrawElementDelegate(Rect rect, TreeElement element);
        public DrawElementDelegate DrawElementHandler { set; private get; }
        public delegate bool DrawElementValidateDelegate(TreeElement element);
        public DrawElementValidateDelegate DrawElementValidateHandler;

        public delegate void DrawTreeFinishDelegate();
        public DrawTreeFinishDelegate DrawTreeFinishHandler;

        public int ElementHeight = 16;
        
        private int m_rowIndex = 0;

        public TreeList()
        {
            DrawElementValidateHandler = (e) => { return true; };
        }

        public void Draw()
        {
            if (Root != null)
            {
                m_rowIndex = 0;
                if (Root.Elements == null)
                    return;
                for (int i = 0; i < Root.Length; i++)
                {
                    if (Root.Elements[i] == null)
                        continue;

                    if (DrawElementHandler != null && DrawElementValidateHandler != null)
                    {
                        if (DrawElementValidateHandler.Invoke(Root.Elements[i]))
                        {
                            try { GUILayout.Label("", GUIStyle.none, GUILayout.Height(ElementHeight), GUILayout.ExpandWidth(true)); }
                            catch { continue; }
                            m_rowIndex++;
                            Root.Elements[i].RowIndex = m_rowIndex;
                            Root.Elements[i].Depth = 0;
                            DrawElementHandler(GUILayoutUtility.GetLastRect(), Root.Elements[i]);
                        }
                    }

                    if (Root.Length <= i || Root.Elements[i] == null)
                        continue;

                    if (Root.Elements[i].Visible)
                    {
                        if (Root.Elements[i].Elements != null)
                        {
                            OnCallRecursiveElement(Root.Elements[i], 1);
                        }
                    }
                }
            }

            if (DrawTreeFinishHandler != null)
                DrawTreeFinishHandler.Invoke();
        }

        private void OnCallRecursiveElement(TreeElement element, int depth)
        {
            for (int i = 0; i < element.Length; i++)
            {
                if (element.Elements[i] == null)
                    continue;

                if (DrawElementHandler != null)
                {
                    try { GUILayout.Label("", GUIStyle.none, GUILayout.Height(ElementHeight), GUILayout.ExpandWidth(true)); }
                    catch { continue; }
                    m_rowIndex++;
                    element.Elements[i].RowIndex = m_rowIndex;
                    element.Elements[i].Depth = depth;
                    DrawElementHandler(GUILayoutUtility.GetLastRect(), element.Elements[i]);
                }

                if (element.Length <= i || element.Elements[i] == null)
                    continue;
                
                if (element.Elements[i].Visible)
                {
                    if (element.Elements[i].Elements != null)
                    {
                        element.Elements[i].Depth = depth;
                        OnCallRecursiveElement(element.Elements[i], depth + 1);
                    }
                }
            }
        }

        public byte[] Serialize()
        {
            using (MemoryStream mem = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(mem))
                {
                    Root.Serialize(writer);
                }

                return mem.ToArray();
            }
        }

        public void Deserialize(byte[] bytes)
        {
            using (MemoryStream mem = new MemoryStream(bytes))
            {
                using (BinaryReader reader = new BinaryReader(mem))
                {
                    Root.Deserialize(reader);
                }
            }
        }

        public TreeElement GetParent(TreeElement element)
        {
            if (element == Root)
                return null;

            TreeElement e = Root.Where(x => x.Elements.Contains(element)).Take(1).SingleOrDefault();
            
            if (e == null)
                e = Root;
            return e;
        }

        public TreeElement[] GetElementPath(TreeElement element)
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

        public string GetDirectoryName(TreeElement element, string separator = "/")
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
    }
}
