using SpriteAnimatorRuntime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SpriteAnimatorEditor
{
    public class SpriteAnimatorWindow : EditorWindow
    {
        internal static bool DEV_MODE = false;
        internal static SpriteAnimatorWindow Instance { get; private set; }

        [MenuItem("SpriteAnimator/Manager")]
        private static void Init()
        {
            Instance = GetWindow<SpriteAnimatorWindow>();
            Instance.Initialize();
            Instance.Show();
        }

        [MenuItem("SpriteAnimator/RepareTree")]
        private static void RecreateTree()
        {
            Init();
            UpdateTree();
            Instance.Save();
        }       

        private SpriteAnimatorData m_data { get; set; }
        private TreeList m_treeView = new TreeList();
        private TreeElement m_treeViewElementRename { get; set; }
        private Rect m_rectElementRename { get; set; }
        private SpriteAnimatorControl m_control { get; set; }
        private Vector2 m_treeScroll { get; set; }
        private int m_lastSelectId { get; set; }
        private TreeElement m_currentElement { get; set; }
        private SpriteAnimatorLibrary m_library { get; set; }
        private string m_tempRenameValue = "";
        private Rect m_treeRectRootFolder { get; set; }
        private Rect m_rectTree { get; set; }
        private bool InRename { get { return m_treeViewElementRename != null; } }

        internal SpriteAnimatorData Data { get { return m_data; } }
        internal bool IsActiveWindows { get { return focusedWindow == this; } }
        internal static SpriteAnimatorResources EditorResources { get; private set; }
        internal SpriteAnimatorContextMenu MenuContext { get; private set; }


        private TreeElement m_elementDrag { get; set; }
        private DragState m_stateTreeDrag { get; set; }

        private enum DragState
        {
            Start,
            Update,
            End,
            Clear,
            Use
        }

        private enum DragResult
        {
            EqualContext,
            ElementIsParentOfTarget,
            Success,
            TargetIsNotFolder,
        }

        private void Initialize()
        {
            Instance = this;
            Load();
            InitializeResources();
            UpdateTree();
            m_control = new SpriteAnimatorControl(this);
            m_library = new SpriteAnimatorLibrary(this);


            if (EditorPrefs.HasKey("lastId"))
            {
                m_lastSelectId = EditorPrefs.GetInt("lastId");
                TreeElement element = m_treeView.Root.Where(x => x.Value == m_lastSelectId.ToString()).Take(1).SingleOrDefault();
                if (element != null)
                    SelectElement(element);
            }

            if (EditorResources != null)
                titleContent = new GUIContent("Animator", EditorResources.GetIcon("AppIcon"));

            Undo.undoRedoPerformed -= AffterRedo;
            Undo.undoRedoPerformed += AffterRedo;

            MenuContext = new SpriteAnimatorContextMenu();

        }

        private void Load()
        {
            if (m_data == null)
                m_data = AssetDatabase.LoadAssetAtPath<SpriteAnimatorData>("Assets/Plugins/SpriteAnimator/Runtime/default.asset");

            if (m_data == null)
            {
                m_data = (SpriteAnimatorData)CreateInstance(typeof(SpriteAnimatorData));
                AssetDatabase.CreateAsset(m_data, "Assets/Plugins/SpriteAnimator/Runtime/default.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private void InitializeResources()
        {
            if (EditorResources == null)
                EditorResources = AssetDatabase.LoadAssetAtPath<SpriteAnimatorResources>("Assets/Plugins/SpriteAnimator/Editor/style.asset");

            if (EditorResources == null)
            {
                EditorResources = (SpriteAnimatorResources)CreateInstance(typeof(SpriteAnimatorResources));
                EditorUtility.DisplayProgressBar("Setup UI", "Creating resources for UI experience", 100);
                EditorUtility.ClearProgressBar();
                if (!Application.isPlaying)
                {
                    AssetDatabase.CreateAsset(EditorResources, "Assets/Plugins/SpriteAnimator/Editor/style.asset");
                    AssetDatabase.SaveAssets();
                }
                AssetDatabase.Refresh();
            }

            EditorResources.Load();
        }

        private void OnDisable()
        {
            m_data.TreeViewData = m_treeView.Serialize();
            EditorPrefs.SetInt("lastId", m_lastSelectId);
        }

        private void OnGUI()
        {
            if (!IsInitialize)
            {
                Initialize();
                return;
            }
            /*
            if (MenuContext != null)

            if (Event.current.keyCode == KeyCode.F11 &&
                Event.current.type == EventType.KeyDown &&
                IsActiveWindows)
            {
                EditorResources.Load();
                return;
            }

            if (EditorResources == null)
            {
                EditorGUILayout.HelpBox("Visual Resources no load!", MessageType.Error);
                if (GUILayout.Button("Initialize Resources"))
                    InitializeResources();
                return;
            }*/

            MenuContext.GUIEvent();
            SpriteAnimatorEditorExtencion.Layout.Control(SpriteAnimatorEditorExtencion.Layout.Direction.Vertical, -1, -1, EditorResources.Background5, () =>
            {
                SpriteAnimatorEditorExtencion.Layout.Control(SpriteAnimatorEditorExtencion.Layout.Direction.Vertical, -1, -1, null, () =>
                {
                    SpriteAnimatorEditorExtencion.Layout.Control(SpriteAnimatorEditorExtencion.Layout.Direction.Horizontal, -1, -1, EditorResources.Background5, () =>
                    {
                        SpriteAnimatorEditorExtencion.Layout.Control(SpriteAnimatorEditorExtencion.Layout.Direction.Vertical, 200, -1, EditorResources.Background1, () =>
                        {
                            SpriteAnimatorEditorExtencion.Elements.Header("Animations", 35);
                            DrawTreeView();
                        });
                        GUILayout.Space(2);
                        SpriteAnimatorEditorExtencion.Layout.Control(SpriteAnimatorEditorExtencion.Layout.Direction.Vertical, -1, -1, EditorResources.Background5, DrawControl);
                    });
                });
            });


            if (m_elementDrag != null)
            {
                Rect rect = new Rect(Event.current.mousePosition, new Vector2(200, 20));
                GUI.Box(rect, m_elementDrag.Name, EditorResources.Background3);
                Repaint();
            }

            if (Event.current.type == EventType.MouseUp && IsActiveWindows)
            {
                m_stateTreeDrag = DragState.End;
                DrawTreeFinish();
            }


            OverrideGUI();
            Repaint();
        }

        private void OverrideGUI()
        {
            Rect labelDate = new Rect(Screen.width - 200, 0, 200, 20);
            GUI.Label(labelDate, "last save " + m_data.LastSave, EditorResources.LabelDate);

            if(m_currentElement != null)
            {
                labelDate.x -= 300;
                string path = m_currentElement.Name;
                TreeElement parent = m_treeView.GetParent(m_currentElement);
                while(parent != null)
                {
                    path = parent.Name + "/" + path;
                    parent = m_treeView.GetParent(parent);
                }

                GUI.Label(labelDate, path);
            }

            MenuContext.Draw();
        }

        internal void SelectElementForID(int id)
        {
            TreeElement element = m_treeView.Root.Where(x => x.Value == id.ToString()).Take(1).SingleOrDefault();
            if (element != null)
                SelectElement(element);
        }

        internal void Save()
        {
            if (m_treeView.Root == null)
                return;

            m_data.TreeViewData = m_treeView.Serialize();
            m_data.Save();
            EditorUtility.SetDirty(m_data);
            AssetDatabase.SaveAssets();
        }

        private void DrawControl()
        {
            try
            {
                if (m_currentElement == null || m_currentElement.Type == TreeElement.ElementType.Animation)
                {
                    m_control.Draw();
                }
                else if (m_currentElement.Type == TreeElement.ElementType.Folder)
                {
                    m_library.Draw();
                }
            }
            catch (Exception ex)
            {
                if (Event.current.type != EventType.Repaint && DEV_MODE)
                    Debug.LogError(ex);

                return;
            }
        }

        private void DrawToolbar()
        {
            if (GUILayout.Button("File", EditorStyles.toolbarButton, GUILayout.Width(30)))
            {
                GenericMenu menu = new GenericMenu();

                menu.AddItem(new GUIContent("Save"), false, Save);
                menu.AddSeparator("");

                menu.AddItem(new GUIContent("Create Folder"), false, () =>
                m_treeView.Root.AddElement(new TreeElement
                {
                    Name = "new folder",
                    Type = TreeElement.ElementType.Folder
                }));

                menu.AddItem(new GUIContent("Reindex"), false, ReindexData);

                Rect rect = GUILayoutUtility.GetLastRect();
                rect.y += 16;
                menu.DropDown(rect);
            }
        }

        #region Tools

        private static void UpdateTree()
        {
            Instance.m_treeView = new TreeList();
            TreeElement element = Instance.m_treeView.Root;

            foreach (SpriteAnimation anim in Instance.Data.Animations)
            {
                string path = anim.Path.Replace("Root/", "");
                element.AddElement(new TreeElement
                {
                    Name = anim.Name,
                    Type = TreeElement.ElementType.Animation,
                    Value = anim.Id.ToString(),
                    Visible = true,
                }, path);
            }

            Instance.Save();
            Instance.Load();

            if (Instance.m_data.TreeViewData.Length > 0)
                Instance.m_treeView.Deserialize(Instance.m_data.TreeViewData);

            Instance.m_treeView.DrawElementValidateHandler = (e) => { return true; };
            Instance.m_treeView.DrawElementHandler = Instance.DrawTreeElement;
            Instance.m_treeView.DrawTreeFinishHandler = Instance.DrawTreeFinish;
            Instance.m_treeView.ElementHeight = 35;
        }

        private bool IsInitialize
        {
            get
            {
                if (m_data == null) return false;
                if (m_treeView == null) return false;
                if (m_control == null) return false;
                if (m_library == null) return false;
                if (EditorResources == null) return false;
                if (MenuContext == null) return false;
                return true;
            }
        }

        private void ReindexData()
        {
            int i = 0;
            for (i = 0; i < m_data.Animations.Length; i++)
            {
                int id = m_data.Animations[i].Id;
                TreeElement element = m_treeView.Root.Where(x => x.Value == id.ToString()).SingleOrDefault();
                if (element != null)
                {
                    element.Value = (i + 1).ToString();
                    SetInstanceField(typeof(SpriteAnimation), m_data.Animations[i], "m_id", i + 1);
                }
            }

            SetInstanceField(typeof(SpriteAnimatorData), m_data, "m_lastId", i);

            Repaint();
        }

        [MenuItem("SpriteAnimator/Tools/RefreshStyle")]
        private static void RefreshStyle()
        {
            SpriteAnimatorResources resources = null;

            if (resources == null)
                resources = AssetDatabase.LoadAssetAtPath<SpriteAnimatorResources>("Assets/Plugins/SpriteAnimator/Editor/style.asset");

            if (resources == null)
            {
                resources = (SpriteAnimatorResources)CreateInstance(typeof(SpriteAnimatorResources));
                EditorUtility.DisplayProgressBar("Setup UI", "Creating resources for UI experience", 100);
                EditorUtility.ClearProgressBar();
                AssetDatabase.CreateAsset(resources, "Assets/Plugins/SpriteAnimator/Editor/style.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            resources.Load();
        }

        private void AffterRedo()
        {
            if (m_data.TreeViewData.Length > 0)
                m_treeView.Deserialize(m_data.TreeViewData);

            m_treeView.DrawElementValidateHandler = (e) => { return true; };
            m_treeView.DrawElementHandler = DrawTreeElement;
            m_treeView.DrawTreeFinishHandler = DrawTreeFinish;
            m_treeView.ElementHeight = 18;

            SelectElementForID(m_lastSelectId);
        }

        private static void SetInstanceField(Type type, object instance, string fieldName, object value)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                 | BindingFlags.Static;
            FieldInfo field = type.GetField(fieldName, bindFlags);
            field.SetValue(instance, value);
        }

        internal void Snapshot(string name)
        {
            Undo.RecordObject(m_data, name);
            EditorUtility.SetDirty(m_data);
        }

        #endregion

        #region Tree

        private void DrawTreeView()
        {
            m_treeScroll = GUILayout.BeginScrollView(m_treeScroll, false, false);
            GUILayout.Space(2);
            if (SpriteAnimatorEditorExtencion.Button.Normal("Create Folder"))
            {
                CreateFolder(false);
            }

            GUILayout.Label("Root Directory " + m_treeView.Root.Where(x => x != null).Count(), EditorResources.TreeViewFolder, GUILayout.ExpandWidth(true), GUILayout.Height(20));
            m_treeRectRootFolder = GUILayoutUtility.GetLastRect();


            GUI.DrawTexture(new Rect(m_treeRectRootFolder.x + 2, m_treeRectRootFolder.y + 2, 16, 16), EditorResources.GetIcon("Star"));

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 &&
                m_treeRectRootFolder.Contains(Event.current.mousePosition) && IsActiveWindows && !MenuContext.IsShow)
            {
                SelectElement(m_treeView.Root);
                Event.current.Use();
            }

            m_treeView.Draw();

            if (m_treeViewElementRename != null)
            {
                if ((Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.Escape)) ||
                    (Event.current.type == EventType.MouseDown && !m_rectElementRename.Contains(Event.current.mousePosition)) && IsActiveWindows)
                {
                    ValidateElementName(m_treeViewElementRename, m_tempRenameValue);
                    if (m_treeViewElementRename.Type == TreeElement.ElementType.Animation)
                    {
                        int id;
                        if (int.TryParse(m_treeViewElementRename.Value, out id))
                        {
                            Snapshot("Change name");
                            m_data.GetAnimation(id).Name = m_treeViewElementRename.Name;
                        }
                    }
                    Event.current.Use();


                    m_treeViewElementRename = null;
                    m_rectElementRename = default(Rect);
                    UpdateTreeElementPath();
                }
                else
                {
                    GUI.SetNextControlName("RenameTextField");
                    Rect rect = m_rectElementRename;
                    EditorGUI.DrawRect(rect, Color.black);
                    m_tempRenameValue = EditorGUI.TextField(rect, m_tempRenameValue);
                    EditorGUI.FocusTextInControl("RenameTextField");

                }
            }

            GUILayout.EndScrollView();


            if (Event.current.type == EventType.Repaint)
                m_rectTree = GUILayoutUtility.GetLastRect();
        }

        private void DrawTreeFinish()
        {
            if (m_stateTreeDrag == DragState.End)
            {
                m_elementDrag = null;
                m_stateTreeDrag = DragState.Start;
                Repaint();
            }
        }

        private bool ValidateElementName(TreeElement element, string value)
        {
            TreeElement parent = m_treeView.GetParent(element);
            if (parent == null)
                parent = m_treeView.Root;

            if (parent == m_treeView.Root && value.ToLower().Equals("root"))
            {
                EditorUtility.DisplayDialog("¡Error!", "Root is reserved", "Ok");
                return false;
            }

            TreeElement[] childrens = parent.Elements.Where(x => x.Type == element.Type).ToArray();
            string originalValue = value;
            int repeat = childrens.Where(x => x != element).Count(x => x.Name.ToLower() == value.ToLower());
            int count = 0;
            while (repeat != 0)
            {
                value = originalValue + "(" + (++count) + ")";
                repeat = childrens.Where(x => x != element).Count(x => x.Name.ToLower() == value.ToLower());
            }

            element.Name = value;
            return true;
        }

        private void UpdateTreeElementPath()
        {
            List<TreeElement> elements = m_treeView.Root.Where(x => x.Type == TreeElement.ElementType.Animation).ToList();

            foreach (TreeElement e in elements)
            {
                int id;
                if (int.TryParse(e.Value, out id))
                {
                    SpriteAnimation data = m_data.GetAnimation(id);
                    if (data != null)
                    {
                        string fullPath = "";
                        foreach (TreeElement p in m_treeView.GetElementPath(e))
                        {
                            fullPath += p.Name + "/";
                        }

                        fullPath = fullPath.TrimEnd('/');
                        data.SetPath(fullPath);
                    }
                }
            }

            Save();
        }

        private void DrawTreeElement(Rect rect, TreeElement element)
        {
            rect.x = 0;
            rect.width = 200;
            DragAndDropElement(rect, element);
            int childSpace = 10;
                       
            Rect rectLabel = new Rect(rect.x + childSpace * element.Depth, rect.y, rect.width - childSpace * element.Depth, rect.height);
            Rect rectIcon = new Rect(5 + rect.x + childSpace * element.Depth, rect.y + 12, 12, 12);

            if (element.Type == TreeElement.ElementType.Folder)
            {
                GUI.Label(rectLabel, $"{element.Name} [{element.Elements.Count}]",element.Selected ? EditorResources.TreeViewFolderSelected : EditorResources.TreeViewFolder);
                GUI.DrawTexture(rectIcon, EditorResources.GetIcon("Folder" + (element.Visible ? "Open" : "Close")));
                EditorGUI.DrawRect(new Rect(rectLabel.x, rectLabel.y + rect.height, rectLabel.width, 2), EditorResources.Colors[4]);
            }

            if (element.Type == TreeElement.ElementType.Animation)
            {
                rectLabel = new Rect(rect.x + (childSpace * element.Depth), rect.y, rect.width - (childSpace * element.Depth), rect.height);
                GUI.Label(rectLabel, " " + element.Name, element.Selected ? EditorResources.TreeViewAnimationSelected : EditorResources.TreeViewAnimation);
                EditorGUI.DrawRect(new Rect(rectLabel.x, rectLabel.y, 2, m_treeView.ElementHeight), EditorResources.Colors[4]);
            }

            
            if (!IsActiveWindows) return;

            switch (Event.current.type)
            {
                case EventType.MouseDown:
                    
                    if (Event.current.button == 0)
                    {
                        if (Event.current.clickCount == 1)
                        {                            
                            if (rectIcon.Contains(Event.current.mousePosition) && element.Type == TreeElement.ElementType.Folder)
                            {
                                element.Visible = !element.Visible;
                                Event.current.Use();
                            }
                            else if(rectLabel.Contains(Event.current.mousePosition))
                            {
                                SelectElement(element);
                                Event.current.Use();
                            }
                        }
                        else if (Event.current.clickCount == 2 && rectLabel.Contains(Event.current.mousePosition))
                        {
                            RenameElement(element, rectLabel);
                            Event.current.Use();
                        }
                    }
                    else if (Event.current.button == 1 && rectLabel.Contains(Event.current.mousePosition))
                    {
                        ShowElementMenuContext(element, rectLabel);
                        Event.current.Use();
                    }
                    break;

                case EventType.KeyDown:
                    if (Event.current.keyCode == KeyCode.F2 && element.Selected)
                    {
                        RenameElement(element, rectLabel);
                        Event.current.Use();
                    }
                    /*
                    else if (Event.current.keyCode == KeyCode.Delete && m_rectTree.Contains(Event.current.mousePosition))
                    {
                        if (element.Type == TreeElement.ElementType.Animation)
                        {
                            DeleteAnimation(element);
                            Event.current.Use();
                        }
                        if (element.Type == TreeElement.ElementType.Folder)
                        {
                            DeleteFolder(element);
                            Event.current.Use();
                        }
                    }
                    */
                    break;
            }
        }

        private void ShowElementMenuContext(TreeElement element, Rect rect)
        {
            if (element.Type == TreeElement.ElementType.Folder)
            {
                SelectElement(element);
                MenuContext.Prepare();
                MenuContext.AddItem("New Folder", () => CreateFolder(true));
                MenuContext.AddItem("New Animation", CreateAnimation);
                MenuContext.AddSeparator();
                MenuContext.AddItem("Eliminar", () => DeleteFolder(element));
                MenuContext.ShowAsContext();                
            }
            else if (element.Type == TreeElement.ElementType.Animation)
            {
                SelectElement(element);
                MenuContext.Prepare();
                MenuContext.AddItem("Delete", () => DeleteAnimation(element));
                MenuContext.ShowAsContext();
            }
        }

        private void RenameElement(TreeElement element, Rect rect)
        {
            m_tempRenameValue = element.Name;
            m_treeViewElementRename = element;
            if (element.Type == TreeElement.ElementType.Folder)
            {
                rect.x += EditorResources.TreeViewFolder.padding.left;
                rect.width -= EditorResources.TreeViewFolder.padding.left;
            }
            else
            {
                rect.x += EditorResources.TreeViewAnimation.padding.left;
                rect.width -= EditorResources.TreeViewAnimation.padding.left;
            }
            m_rectElementRename = rect;
            Event.current.Use();
            Repaint();
        }

        private void DragAndDropElement(Rect rect, TreeElement element)
        {
            if (m_treeViewElementRename != null || !IsActiveWindows)
                return;
            
            switch(Event.current.type)
            {
                case EventType.MouseUp:

                    if(m_stateTreeDrag == DragState.Update || m_stateTreeDrag == DragState.Use)
                    {
                        m_stateTreeDrag = DragState.Use;

                        if (rect.Contains(Event.current.mousePosition))
                        {
                            if (element.Type == TreeElement.ElementType.Animation && m_treeView.GetParent(m_elementDrag).Value == m_treeView.GetParent(element).Value)
                            {
                                ChangeIndex(m_elementDrag, element);
                            }
                            else
                            {
                                ChangeElementFolder(m_elementDrag, element);
                            }
                        }
                        else if (m_treeRectRootFolder.Contains(Event.current.mousePosition))
                        {
                            ChangeElementFolder(m_elementDrag, m_treeView.Root);
                        }
                    }
                    break;

                case EventType.MouseDrag:
                    if (m_stateTreeDrag == DragState.Start && Event.current.button == 0)
                    {
                        if (rect.Contains(Event.current.mousePosition))
                        {
                            m_elementDrag = element;
                            m_stateTreeDrag = DragState.Update;
                            Event.current.Use();
                        }                        
                    }
                    break;

                case EventType.Repaint:
                    if (m_stateTreeDrag == DragState.Use)
                    {
                        m_stateTreeDrag = DragState.End;
                    }
                    break;
            }
        }

        private void ChangeIndex(TreeElement element, TreeElement reference)
        {
            if(m_treeView.GetParent(element) != m_treeView.GetParent(reference))
            {
                return;
            }

            int index = m_treeView.GetParent(reference).Elements.IndexOf(reference);

            if (index < 0)
                return;

            Snapshot("Order Element");

            TreeElement parent = m_treeView.GetParent(element);
            if (parent == null)
                parent = m_treeView.Root;

            index = Mathf.Clamp(index, 0, parent.Length -1);

            parent.SetIndex(element, index);

            Save();
        }

        private DragResult ChangeElementFolder(TreeElement element, TreeElement folder)
        {
            if (folder.Type != TreeElement.ElementType.Folder)
                return DragResult.TargetIsNotFolder;

            if(element.Where(x => x == folder).Count() > 0)
                return DragResult.ElementIsParentOfTarget;

            TreeElement elementParent = m_treeView.GetParent(element);
            if (elementParent == null)
                elementParent = m_treeView.Root;

            if (element == folder)
                return DragResult.EqualContext;

            Snapshot("Move Element");

            if (elementParent != null)
                elementParent.Remove(x => x == element);

            folder.AddElement(element);
            ValidateElementName(element, element.Name);

            UpdateTreeElementPath();
            return DragResult.Success;
        }

        private void SelectElement(TreeElement element)
        {
            if (m_currentElement == element)
                return;

            m_treeViewElementRename = null;


            m_currentElement = element;
            m_treeView.Root.Where(x => x.Selected).ToList().ForEach(x => x.Selected = false);
            element.Selected = true;
            if(element.Type == TreeElement.ElementType.Animation)
            {
                int id;
                if (int.TryParse(element.Value, out id))
                {
                    m_lastSelectId = id;
                    m_control.SetAnimation(m_data.GetAnimation(id));
                }
            }
            else if (element.Type == TreeElement.ElementType.Folder)
            {
                TreeElement[] animationElements = element.Where(x => x.Type == TreeElement.ElementType.Animation).Take(100).ToArray();
                List<SpriteAnimation> animations = new List<SpriteAnimation>();
                for(int i = 0; i < animationElements.Length; i++)
                {
                    int id;
                    if (int.TryParse(animationElements[i].Value, out id))
                    {
                        SpriteAnimation anim = m_data.GetAnimation(id);
                        if (anim != null)
                            animations.Add(anim);
                    }
                }
                m_library.SetAnimation(animations.ToArray());
            }
            Repaint();
        }

        private void DeleteFolder(TreeElement element, bool showDialog = true)
        {
            if (showDialog && !EditorUtility.DisplayDialog("¡Atention!",
                      "Seguro quiere eliminar esta carpeta y todo su contenido",
                      "yes",
                      "no"))
                return;

            if (showDialog)
                Snapshot("Delete Folder");

            if (element.Type == TreeElement.ElementType.Folder)
            {
                element.Where(x => x.Type == TreeElement.ElementType.Animation)
                    .ToList()
                    .ForEach(x => DeleteAnimation(x, false));

                m_treeView.Root.Remove(x => x == element);
            }
            else if (element.Type == TreeElement.ElementType.Animation)
            {
                DeleteAnimation(element, false);
            }
            Save();
        }

        private void DeleteAnimation(TreeElement element, bool showDialog = true)
        {
            if (showDialog && !EditorUtility.DisplayDialog("¡Atention!",
                        "Seguro quiere eliminar esta animación",
                        "yes",
                        "no"))
                return;


            if (showDialog)
                Snapshot("Delete Animation");

            int id;
            if (int.TryParse(element.Value, out id))
                m_data.RemoveAnimation(id);

            m_treeView.Root.Remove(x => x == element);
            m_control.SetAnimation(null);
            Save();
        }

        private void CreateFolder(bool relative)
        {
            Snapshot("Create Folder");

            TreeElement element = null;
            if (relative)
                element = m_treeView.Root.Where(x => x.Selected).Take(1).SingleOrDefault();

            if (element == null)
                element = m_treeView.Root;

            TreeElement e = element.AddElement(new TreeElement { Type = TreeElement.ElementType.Folder });
            ValidateElementName(e, "New Folder");
            Save();
        }

        private void CreateAnimation()
        {
            Snapshot("Create Animation");
            TreeElement element = m_treeView.Root.Where(x => x.Selected).Take(1).SingleOrDefault();
            if (element == null)
                element = m_treeView.Root;

            int id = m_data.CreateAnimation().Id;
            element = element.AddElement(new TreeElement { Value = id.ToString(), Type = TreeElement.ElementType.Animation });
            ValidateElementName(element, "New Animation");
            SelectElement(element);
            UpdateTreeElementPath();
        }


        #endregion
    }
}
