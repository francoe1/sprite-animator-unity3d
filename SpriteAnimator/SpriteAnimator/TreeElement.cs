using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace SpriteAnimatorEditor
{
    internal class TreeElement
    {
        internal string Name = "";
        internal string Value = "";
        internal int ID { get; private set; }
        internal bool Visible { get; set; } = true;
        internal bool Protected { get; set; }
        internal int Depth { get; set; }
        internal ElementType Type = ElementType.Any;
        internal object[] NonPersistentData { get; set; }
        internal List<TreeElement> Elements { get; private set; }
        internal int Length { get { return Elements.Count; } }
               

        internal int GetTotalLength(ElementType type)
        {
            int count = 0;
            foreach (TreeElement element in Elements)
            {
                if (element.Type == type || type == ElementType.Any) count++;
                count += element.GetTotalLength(type);                
            }
            return count;
        }

        internal TreeElement()
        {
            ID = GetHashCode();
            Elements = new List<TreeElement>();
        }

        internal bool HasElement
        {
            get
            {
                return Elements != null && Elements.Count > 0;
            }
        }

        public Rect Rect { get; internal set; }
        public int Index { get; internal set; }

        internal enum ElementType : int
        {
            Folder = 0,
            Animation = 2,
            Any = 1,
        }

        internal TreeElement AddElement(TreeElement element)
        {
            if (Elements == null)
                Elements = new List<TreeElement>();
            Elements.Add(element);
            return element;
        }

        internal TreeElement AddElement(TreeElement element, TreeElement refenrence, int pos = 0)
        {
            int index = Elements.IndexOf(refenrence);
            if (Elements == null)
                Elements = new List<TreeElement>();
            if (pos < 0 && Elements.Count > 0) index -= pos;
            Elements.Insert(index, element);
            return element;
        }

        internal TreeElement AddElement(TreeElement element, string path)
        {
            TreeElement originalElement = element;
            element = this;
            string[] paths = path.Split('/');

            for (int i = 0; i < paths.Length - 1; i++)
            {
                if (i == 0 && paths[i].ToLower().Equals("root")) continue;

                TreeElement newElement = element.Where(x => x.Name == paths[i], false).Take(1).SingleOrDefault();
                if (newElement == null)
                {
                    newElement = element.AddElement(new TreeElement
                    {
                        Name = paths[i],
                        Type = ElementType.Folder,
                        Visible = true,
                    });
                }
                element = newElement;
            }
            return element.AddElement(originalElement);
        }
        
        internal IEnumerable<TreeElement> Where(Func<TreeElement, bool> r, bool deph = true)
        {
            List<TreeElement> result = new List<TreeElement>();
            result.AddRange(Elements.Where(r));
            if (deph)
            {
                for (int i = 0; i < Elements.Count; i++)
                    result.AddRange(Elements[i].Where(r));
            }
            return result;
        }

        internal void Remove(Predicate<TreeElement> r, bool deph = true)
        {
            Elements.RemoveAll(r);
            if (deph)
            {
                for (int i = 0; i < Elements.Count; i++)
                    Elements[i].Remove(r);
            }
        }

        internal virtual void Serialize(BinaryWriter writer)
        {
            try
            {
                writer.Write(ID);
                writer.Write(Name);
                writer.Write(Value);
                writer.Write((int)Type);
                writer.Write(Protected);
                writer.Write(Visible);
                writer.Write(Elements == null ? 0 : Elements.Count);

                if (Elements != null)
                {
                    for (int i = 0; i < Elements.Count; i++)
                        Elements[i].Serialize(writer);
                }
            }
            catch
            {
                throw new Exception("Error Serialize TreeElement");
            }
        }

        internal virtual void Deserialize(BinaryReader reader)
        {
            try
            {
                ID = reader.ReadInt32();
                Name = reader.ReadString();
                Value = reader.ReadString();
                Type = (ElementType)reader.ReadInt32();
                Protected = reader.ReadBoolean();
                Visible = reader.ReadBoolean();

                int count = reader.ReadInt32();
                if (count > 0)
                {
                    Elements = new List<TreeElement>();
                    for (int i = 0; i < count; i++)
                    {
                        Elements.Add(new TreeElement());
                        Elements[i].Deserialize(reader);
                    }
                }                
            }
            catch
            {
                throw new Exception("Error Deserialize TreeElement");
            }
        }
        
        internal void Clear()
        {
            Elements.Clear();
        }

        internal void Order<T>(Func<TreeElement, T> predicate)
        {
            Elements = Elements.OrderBy(predicate).ToList();
        }

        internal void MoveTo(TreeElement from, TreeElement target, int pos = 0)
        {
            int indexFrom = Elements.IndexOf(from);
            Elements.Remove(from);
            int indexTarget = Elements.IndexOf(target);            

            if (pos < 0)
            {
                indexTarget -= pos;
                Elements.Insert(indexTarget, from);
            }
            else
            {
                Elements.Insert(indexFrom, from);
                TreeElement tmp = Elements[indexTarget];
                Elements[indexTarget] = Elements[indexFrom];
                Elements[indexFrom] = tmp;
            }
        }

        internal void MoveToStart(TreeElement element)
        {
            Elements.Remove(element);
            Elements.Insert(0, element);
        }

        internal void MoveToLast(TreeElement element)
        {
            Elements.Remove(element);
            Elements.Add(element);
        }

        internal TreeElement GetNext(TreeElement element)
        {
            int index = Elements.IndexOf(element);
            index++;
            if (index >= Elements.Count) return null;
            return Elements[index];
        }

        internal TreeElement GetBack(TreeElement element)
        {
            int index = Elements.IndexOf(element);
            index--;
            if (index < 0) return null;
            return Elements[index];
        }

        public override string ToString()
        {
            return $"(TreeElement {Index}:[{ID}]{Name})";
        }

        public override bool Equals(object element)
        {
            if (element == null) return false;
            return ((TreeElement)element).ID == ID;
        }

        public override int GetHashCode()
        {
            return new object().GetHashCode();
        }

    }
}
