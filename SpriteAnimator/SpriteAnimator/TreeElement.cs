using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpriteAnimatorEditor
{
    internal class TreeElement
    {
        public int RowIndex { get; internal set; }
        public string Name = "";
        public string Value = "undefined";
        public bool Visible = true;
        public bool Selected { get; set; }
        public bool Protected { get; set; }        
        public int Depth { get; set; }
        public ElementType Type = ElementType.Scene;
        public object[] NonPersistentData { get; set; }

        //public TreeElement[] Elements { get { return m_elements.ToArray(); } }
        public List<TreeElement> Elements { get; set; }   
        public int Length { get { return Elements.Count; } }

        public TreeElement()
        {
            Elements = new List<TreeElement>();
        }

        public bool HasElement
        {
            get
            {
                return Elements != null && Elements.Count > 0;
            }
        }
        public enum ElementType : int
        {
            Folder,
            Scene,
            Animation,
        }   

        public TreeElement AddElement(TreeElement element)
        {
            if (Elements == null)
                Elements = new List<TreeElement>();
            Elements.Add(element);
            return element;
        }

        public TreeElement AddElement(TreeElement element, string path)
        {
            TreeElement originalElement = element;
            element = this;
            string[] paths = path.Split('/');

            for (int i = 0; i < paths.Length - 1; i++)
            {
                TreeElement newElement = element.Where(x => x.Name == paths[i], false).Take(1).SingleOrDefault();
                if (newElement == null)
                {
                    newElement = element.AddElement(new TreeElement
                    {
                        Name = paths[i],
                        Type = TreeElement.ElementType.Folder,
                        Visible = true,
                    });
                }
                element = newElement;
            }
            return element.AddElement(originalElement);
        }

        //LINQ function

        public IEnumerable<TreeElement> Where(Func<TreeElement, bool> r, bool deph = true)
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

        public void Remove(Predicate<TreeElement> r, bool deph = true)
        {
            Elements.RemoveAll(r);
            if (deph)
            {
                for (int i = 0; i < Elements.Count; i++)
                    Elements[i].Remove(r);
            }
        }

        public virtual void Serialize(BinaryWriter writer)
        {
            try
            {
                writer.Write(Name);
                writer.Write(Value);
                writer.Write(Visible);
                writer.Write((int)Type);
                writer.Write(Protected);
                writer.Write(Elements == null ? 0 : Elements.Count);

                if (Elements != null)
                {
                    for (int i = 0; i < Elements.Count; i++)
                        Elements[i].Serialize(writer);
                }
            }
            catch
            {
                throw new Exception("Error deserialize TreeElement");
            }
        }

        internal void Clear()
        {
            Elements.Clear();
        }

        public virtual void Deserialize(BinaryReader reader)
        {
            try
            {
                Name = reader.ReadString();
                Value = reader.ReadString();
                Visible = reader.ReadBoolean();
                Type = (ElementType)reader.ReadInt32();
                Protected = reader.ReadBoolean();

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
                throw new Exception("Error serialize TreeElement");
            }
        }

        internal void Order<T>(Func<TreeElement, T> predicate)
        {
            Elements = Elements.OrderBy(predicate).ToList();
        }

        internal void SetIndex(TreeElement element, int index)
        {
            if(Elements.Remove(element))
            {
                Elements.Insert(index, element);
            }
        }
    }
}
