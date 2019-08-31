using SpriteAnimatorRuntime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
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
            RecreateTree();
            Instance.Show();
        }

        [MenuItem("SpriteAnimator/Tools/Repare DDBB")]
        private static void RecreateTree()
        {
            if (Instance == null) Init();
            
            Instance.m_treeView.Root.Clear();

            foreach (SpriteAnimation anim in Instance.Data.Animations)
            {
                Instance.m_treeView.Root.AddElement(new TreeElement
                {
                    Name = anim.Name,
                    Type = TreeElement.ElementType.Animation,
                    Value = anim.Id.ToString(),
                    Visible = true,
                }, anim.Path);
            }
        }

        [MenuItem("SpriteAnimator/Tools/RefreshStyle")]
        private static void RefreshStyle()
        {
            GUIResources resources = null;
            resources = (GUIResources)CreateInstance(typeof(GUIResources));
            AssetDatabase.CreateAsset(resources, "Assets/Plugins/SpriteAnimator/Editor/style.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            resources.Load();

            EditorResources = resources;
        }

        private static void TestTree()
        {
            Init();
            SpriteAnimation data = Instance.Data.Animations[0];
            string name = DateTime.Now.TimeOfDay.TotalSeconds.ToString();
            for(int i = 0; i < 500; i++)
            {
                SpriteAnimation anim = Instance.Data.CreateAnimation();
                anim.Name = name;
                anim.SetPath("Test/" + name + i);
                anim.SetFrames(data.Frames);
                anim.Direction = data.Direction;
            }
        }

        [SerializeField]
        private SpriteAnimatorData m_data = null;

        //Tree Cache Vars
        [SerializeField]
        private TreeView m_treeView = new TreeView();
        private Vector2 m_treeScroll { get; set; }
        private TreeElement m_elementDrag { get; set; }
        private bool m_treeDragElement { get; set; }
        private int m_treeDragPosition { get; set; }
        private TreeElement m_treeViewElementRename { get; set; }
        private Rect m_rectElementRename { get; set; }
        private string m_tempRenameValue = "";
        private bool InRename { get { return m_treeViewElementRename != null; } }

        private Rect m_rectTreeContext { get; set; }
        private Rect m_rectControlContext { get; set; }

        public enum Context
        {
            Tree,
            Control,
        }


        public static bool AvailableSave => !EditorApplication.isPlayingOrWillChangePlaymode;

        //SpriteAnimator
        [SerializeField]
        private SpriteAnimatorControl m_control = null;
        [SerializeField]
        private SpriteAnimatorLibrary m_library = null;

        internal SpriteAnimatorData Data { get { return m_data; } }
        internal bool IsActiveWindows { get { return focusedWindow == this; } }
        internal ContextMenu MenuContext { get; private set; }
        internal static GUIResources EditorResources { get; private set; }
        public static Context ActiveContext { get; private set; }
        
        private static int m_lastMilliseconds { set; get; }

        public static int FrameRate { get; private set; }

        private void OnEnable()
        {
            Instance = this;
            LoadData();
            Initialize();


            EditorApplication.playModeStateChanged += (state) =>
            {
                if (m_treeView.SelectedElement != null && m_treeView.SelectedElement.Type == TreeElement.ElementType.Folder)
                {
                    TreeViewSelectElement(m_treeView.SelectedElement);
                }
            };
        }

        private void OnDisable()
        {
            if (AvailableSave)
            {
                SaveData();
                m_data.TreeViewData = m_treeView.Serialize();
                m_data.EditorData = m_control.Serialize();
                AssetDatabase.SaveAssets();
            }
        }

        private void Initialize()
        {
            Instance = this;
            InitializeResources();
            UpdateTree();
            m_control = new SpriteAnimatorControl(this);
            m_library = new SpriteAnimatorLibrary(this);

            
            if (EditorResources != null)
                titleContent = new GUIContent("Animator", EditorResources.GetIcon("AppIcon"));

            Undo.undoRedoPerformed = AffterRedo;
            
            MenuContext = new ContextMenu();
        }

        internal static void Open()
        {
            if (Instance == null) Init();
        }

        private void InitializeResources()
        {
            if (EditorResources == null)
                EditorResources = AssetDatabase.LoadAssetAtPath<GUIResources>("Assets/Plugins/SpriteAnimator/Editor/style.asset");

            if (EditorResources == null && AvailableSave)
            {
                EditorResources = (GUIResources)CreateInstance(typeof(GUIResources));
                AssetDatabase.CreateAsset(EditorResources, "Assets/Plugins/SpriteAnimator/Editor/style.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            EditorResources.Load();
        }

        private void OnGUI()
        {
            MenuContext.UpdateMousePosition();
            if (Event.current.type == EventType.Repaint) Update();            
            GUIPro.Layout.Control(GUIPro.Layout.Direction.Vertical, -1, -1, EditorResources.Background5, AppGUI);            
            OverrideGUI();
            Repaint();
        }

        private void AppGUI()
        {
            GUIPro.Layout.Control(GUIPro.Layout.Direction.Horizontal, -1, -1, EditorResources.Background5, () =>
            {
                if (m_data.ShowAnimationTree)
                {
                    GUI.SetNextControlName("TreeView");
                    GUIPro.Layout.Control(GUIPro.Layout.Direction.Vertical, 200, -1, EditorResources.Background1, () =>
                    {

                        GUIPro.Elements.Header("Animations", 35);
                        Rect animationHeaderRect = GUILayoutUtility.GetLastRect();
                        if (Event.current.type == EventType.MouseDown && animationHeaderRect.Contains(Event.current.mousePosition))
                        {
                            TreeViewSelectElement(null);
                            Event.current.Use();
                        }
                        
                        DrawTreeView();
                    });

                    if (Event.current.type == EventType.Repaint)
                        m_rectTreeContext = GUILayoutUtility.GetLastRect();
                    
                    GUILayout.Space(2);
                }
                
                
                GUIPro.Layout.Control(GUIPro.Layout.Direction.Vertical, -1, -1, EditorResources.Background5, DrawControl);
                if (Event.current.type == EventType.Repaint)
                    m_rectControlContext = GUILayoutUtility.GetLastRect();

                Rect hiddenTreeButton = m_rectControlContext;
                hiddenTreeButton.width = 20;
                hiddenTreeButton.height = 20;
                hiddenTreeButton.y += 5;
                hiddenTreeButton.x += 5;
                GUI.DrawTexture(hiddenTreeButton, (m_data.ShowAnimationTree ? EditorResources.GetIcon("Forward") : EditorResources.GetIcon("Backward")));

                if (Event.current.type == EventType.MouseDown && hiddenTreeButton.Contains(Event.current.mousePosition))
                {
                    Snapshot("Show TreeAnimations");
                    m_data.ShowAnimationTree = !m_data.ShowAnimationTree;
                    Event.current.Use();
                }
            });

            if (m_rectTreeContext.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown) ActiveContext = Context.Tree;
            if (m_rectControlContext.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown) ActiveContext = Context.Control;

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Tab)
            {
                if (ActiveContext == Context.Tree)
                {
                    GUI.FocusControl("Control");
                    ActiveContext = Context.Control;
                }
                else
                {
                    GUI.FocusControl("TreeView");
                    ActiveContext = Context.Tree;
                }
                Event.current.Use();
            }            
        }

        private void Update()
        {
            if (m_treeView.Root.GetTotalLength(TreeElement.ElementType.Animation) != m_data.Animations.Length) RecreateTree();       
        }

        private void OverrideGUI()
        {
            Rect labelDate = new Rect(Screen.width - 200, 0, 200, 20);
            GUI.Label(labelDate, "last save " + m_data.LastSave, EditorResources.LabelDate);
            FrameRate = Mathf.CeilToInt((m_lastMilliseconds / (float)DateTime.Now.TimeOfDay.Milliseconds) * 60);           

            MenuContext.Draw();

            if (Event.current.type == EventType.Repaint) m_lastMilliseconds = DateTime.Now.TimeOfDay.Milliseconds;    
            
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.F1 && IsActiveWindows)
            {
                Snapshot("Show TreeAnimations");
                m_data.ShowAnimationTree = !m_data.ShowAnimationTree;
                Event.current.Use();
            }
        }

        internal void SelectElementForID(int id)
        {
            TreeElement element = m_treeView.Root.Where(x => x.Value == id.ToString()).Take(1).SingleOrDefault();
            if (element != null)
                TreeViewSelectElement(element);
        }

        private void DrawControl()
        {
            if (m_treeView.SelectedElement == null)
            {
                DrawStartPage();
                return;
            }
            if (m_treeView.SelectedElement.Type == TreeElement.ElementType.Animation)
            {
                if (!m_control.AvailableAnimation)
                {
                    m_control.SetAnimation(m_data.GetAnimation(int.Parse(m_treeView.SelectedElement.Value)));
                }
                m_control.Draw();
            }
            else if (m_treeView.SelectedElement.Type == TreeElement.ElementType.Folder)
            {
                m_library.Draw();
            }
        }

        private void DrawStartPage()
        {
            GUIPro.Layout.ControlCenter(() =>
            {
                GUIStyle style = new GUIStyle();
                style.fontStyle = FontStyle.Bold;
                style.normal.textColor = Color.grey;
                style.fontSize = ((Screen.width + Screen.height) / 2) / 18;
                style.padding = new RectOffset(10, 10, 10, 10);
                GUILayout.Label("Sprite Animator".ToUpper(), style);
                GUILayout.Label("v1.0");
            });
        }

        public void SaveData()
        {
            if (!AvailableSave) return;
            m_data.TreeViewData = m_treeView.Serialize();
            m_data.EditorData = m_control.Serialize();
            EditorUtility.SetDirty(m_data);
        }

        public void LoadData()
        {
            if (m_data == null)
                m_data = AssetDatabase.LoadAssetAtPath<SpriteAnimatorData>("Assets/Plugins/SpriteAnimator/Runtime/default.asset");

            if (m_data == null && AvailableSave)
            {
                m_data = (SpriteAnimatorData)CreateInstance(typeof(SpriteAnimatorData));
                AssetDatabase.CreateAsset(m_data, "Assets/Plugins/SpriteAnimator/Runtime/default.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            
            if (m_data == null)
            {
                throw new Exception("Database error");
            }

            UpdateTree();
        }

        #region Tools

        private static void UpdateTree()
        {
            if (Instance == null) throw new Exception("Instance is null");
            if (Instance.m_data == null) throw new Exception("Data is null");
            if (Instance.m_treeView == null) throw new Exception("Tree is null");

            Instance.m_treeView.DrawElementValidateHandler = (e) => { return true; };
            Instance.m_treeView.DrawElementHandler = Instance.DrawTreeElement;
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
        }        

        private void AffterRedo()
        {
            m_treeView.Deserialize(m_data.TreeViewData);
            if (m_treeView.SelectedElement != null) TreeViewSelectElement(m_treeView.SelectedElement);
            m_control.Deserialize(m_data.EditorData);
            UpdateTree();
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
            SaveData();
            name = "Animator:" + name;
            m_data.TreeViewData = m_treeView.Serialize();
            if (m_control != null)
            {
                m_data.EditorData = m_control.Serialize();
            }
            Undo.RegisterCompleteObjectUndo(m_data, name);
        }

        #endregion

        #region Tree

        private void DrawTreeView()
        {
            {
                m_treeView.Draw(0);
                if (Event.current.type == EventType.MouseDown)
                {
                    if (Event.current.button == 1 && m_rectTreeContext.Contains(Event.current.mousePosition))
                    {
                        m_treeView.SelectElement(m_treeView.Root);
                        MenuContext.Prepare();

                        MenuContext.AddItem("Create Folder", () => CreateFolder());
                        MenuContext.AddItem("Create Animation", () => CreateAnimation());

                        MenuContext.ShowAsContext();
                        Event.current.Use();
                    }

                }

                if (Event.current.type == EventType.KeyDown && ActiveContext == Context.Tree && IsActiveWindows)
                {
                    if (m_treeView.SelectedElement != null)
                    {
                        if (Event.current.keyCode == KeyCode.DownArrow)
                        {
                            TreeElement element = m_treeView.GetNext(m_treeView.SelectedElement);
                            if (element != null) TreeViewSelectElement(element);
                            Event.current.Use();
                        }

                        if (Event.current.keyCode == KeyCode.UpArrow)
                        {
                            TreeElement element = m_treeView.GetBack(m_treeView.SelectedElement);
                            if (element != null) TreeViewSelectElement(element);
                            Event.current.Use();
                        }

                        if (Event.current.keyCode == KeyCode.RightArrow)
                        {
                            if (m_treeView.SelectedElement.Type == TreeElement.ElementType.Folder)
                            {
                                m_treeView.SelectedElement.Visible =  true;
                            }
                                Event.current.Use();
                        }

                        if (Event.current.keyCode == KeyCode.LeftArrow)
                        {
                            if (m_treeView.SelectedElement.Type == TreeElement.ElementType.Folder)
                            {
                                m_treeView.SelectedElement.Visible = false;
                            }
                            Event.current.Use();
                        }
                    }
                }


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
            }
        }

        private void DrawTreeElement(Rect rect, TreeElement element)
        {
            rect.x = 0;
            rect.width = 200;
            int childSpace = 10;

            bool selected = element.ID == m_treeView.SelectedElementId;


            Rect rectLabel = new Rect(rect.x + childSpace * element.Depth, rect.y, rect.width - childSpace * element.Depth, rect.height);
            Rect rectIcon = new Rect(5 + rect.x + childSpace * element.Depth, rect.y + 12, 12, 12);
            
            if (element.Type == TreeElement.ElementType.Folder)
            {
                GUI.Label(rectLabel, $"{element.Name} [{element.Elements.Count}]", selected ? EditorResources.TreeViewFolderSelected : EditorResources.TreeViewFolder);
                GUI.DrawTexture(rectIcon, EditorResources.GetIcon("Folder" + (element.Visible ? "Open" : "Close")));
                EditorGUI.DrawRect(new Rect(rectLabel.x, rectLabel.y + rect.height, rectLabel.width, 1), EditorResources.Colors[4]);
                EditorGUI.DrawRect(new Rect(rectLabel.x, rectLabel.y, 1, m_treeView.ElementHeight), EditorResources.Colors[4]);
            }

            if (element.Type == TreeElement.ElementType.Animation)
            {
                rectLabel = new Rect(rect.x + (childSpace * element.Depth), rect.y, rect.width - (childSpace * element.Depth), rect.height);
                GUI.Label(rectLabel, element.Name, selected ? EditorResources.TreeViewAnimationSelected : EditorResources.TreeViewAnimation);
                EditorGUI.DrawRect(new Rect(rectLabel.x, rectLabel.y, 1, m_treeView.ElementHeight), EditorResources.Colors[4]);
            }

            Rect topEdgeRect = rectLabel;
            topEdgeRect.height = 10;

            Rect bottomEdgeRect = rectLabel;
            bottomEdgeRect.height = 10;
            bottomEdgeRect.y += rectLabel.height - 10;

            if (m_treeDragElement)
            {
                if (selected || rectLabel.Contains(Event.current.mousePosition))
                {
                    Color selectColor = Color.white;
                    selectColor.a = .5f;
                    EditorGUI.DrawRect(rectLabel, selectColor);
                }

                if (selected)
                {
                    if (topEdgeRect.Contains(Event.current.mousePosition))
                    {
                        EditorGUI.DrawRect(new Rect(topEdgeRect.x, rectLabel.y, topEdgeRect.width, 2), EditorResources.Colors[4]);
                    }

                    if (bottomEdgeRect.Contains(Event.current.mousePosition))
                    {
                        EditorGUI.DrawRect(new Rect(topEdgeRect.x, rectLabel.y + rectLabel.height - 2, topEdgeRect.width, 2), EditorResources.Colors[4]);
                    }
                }
            }
            else
            {
                if (rectLabel.Contains(Event.current.mousePosition))
                {
                    Color selectColor = Color.white;
                    selectColor.a = .1f;
                    EditorGUI.DrawRect(rectLabel, selectColor);
                }
            }

            if (!IsActiveWindows || ActiveContext != Context.Tree) return;

            switch (Event.current.type)
            {
                case EventType.MouseDown:
                    
                    if (Event.current.button == 0)
                    {
                        if (Event.current.clickCount == 1)
                        {                            
                            if (rectIcon.Contains(Event.current.mousePosition) && element.Type == TreeElement.ElementType.Folder)
                            {
                                Event.current.Use();
                                element.Visible = !element.Visible;
                            }
                            else if(rectLabel.Contains(Event.current.mousePosition))
                            {
                                TreeViewSelectElement(element);
                                Event.current.Use();
                            }
                        }
                        else if (Event.current.clickCount == 2 && rectLabel.Contains(Event.current.mousePosition))
                        {
                            Event.current.Use();
                            RenameElement(element, rectLabel);
                        }
                    }
                    else if (Event.current.button == 1 && rectLabel.Contains(Event.current.mousePosition))
                    {
                        Event.current.Use();
                        ShowElementMenuContext(element, rectLabel);
                    }
                    break;

                case EventType.MouseDrag:
                    m_elementDrag = null;
                    if (rectLabel.Contains(Event.current.mousePosition) && Event.current.button == 0)
                    {
                        if (m_treeView.SelectedElement.ID != element.ID)
                        {
                            m_treeDragPosition = 0;
                            if (topEdgeRect.Contains(Event.current.mousePosition)) m_treeDragPosition = 1;
                            else if (bottomEdgeRect.Contains(Event.current.mousePosition))
                            {
                                m_treeDragPosition = -1;
                                if (element.Type == TreeElement.ElementType.Folder && element.Visible == true) m_treeDragPosition = 0;
                            }
                        }
                        m_elementDrag = element;
                        m_treeDragElement = true;
                        DrawTreetDragElement(element);
                        Event.current.Use();
                    }
                    break;

                case EventType.MouseUp:
                    if (Event.current.button == 0 && m_treeDragElement)
                    {
                        if (m_elementDrag != null)
                        {
                            DrawTreeDropElement(m_treeView.SelectedElement, m_elementDrag, m_treeDragPosition);
                        }
                        else if (m_rectTreeContext.Contains(Event.current.mousePosition))
                        {
                            DrawTreeDropElement(m_treeView.SelectedElement, m_treeView.Root, 0);
                        }

                        m_treeDragElement = false;
                        m_elementDrag = null;
                        Event.current.Use();
                    }
                    break;

                case EventType.KeyDown:
                    if (selected)
                    {
                        if (Event.current.keyCode == KeyCode.F2)
                        {
                            Event.current.Use();
                            RenameElement(element, rectLabel);
                        }
                        else if (Event.current.keyCode == KeyCode.Delete)
                        {
                            TreeElement newSelection = m_treeView.GetNext(element);
                            m_treeView.SelectElement(newSelection == null ? m_treeView.GetBack(element) : newSelection);

                            if (element.Type == TreeElement.ElementType.Animation)
                            {
                                Event.current.Use();
                                DeleteAnimation(element);
                            }
                            if (element.Type == TreeElement.ElementType.Folder)
                            {
                                Event.current.Use();
                                DeleteFolder(element);
                            }

                            Focus();
                        }
                    }
                    break;
            }
        }

        private void DrawTreetDragElement(TreeElement element)
        {

        }

        private void DrawTreeDropElement(TreeElement dragElement, TreeElement dropElement, int position)
        {
            if (dragElement.ID == dropElement.ID) return;


            TreeElement dragParent = m_treeView.GetParent(dragElement);
            TreeElement dropParent = m_treeView.GetParent(dropElement);
            if (dropParent == null) dropParent = m_treeView.Root;

            bool sharedContext = dragParent.ID == dropParent.ID;                       
            bool isParent = dragElement.Where(x => x.ID == dropElement.ID || x.ID == dropParent.ID).Count() > 0;

            if (isParent) return;

            if (dropElement.Type == TreeElement.ElementType.Folder && position == 0)
            {
                dragParent.Remove(x => x.ID == dragElement.ID);
                dragElement = dropElement.AddElement(dragElement);
                dropElement.MoveToStart(dragElement);
            }
            else
            {
                if (!sharedContext)
                {
                    dragParent.Remove(x => x.ID == dragElement.ID);
                    dragElement = dropParent.AddElement(dragElement, dropElement, position );
                }
                else
                {
                    dropParent.MoveTo(dragElement, dropElement, position);
                }
            }

            //Debug.Log($"Mode Element [{dragElement.Name}] to [{dropElement.Name}] position({position}) context [{sharedContext}]");
        }
       
        private bool ValidateElementName(TreeElement element, string value)
        {
            TreeElement parent = m_treeView.GetParent(element);
            if (parent == null)
                parent = m_treeView.Root;

            value = Regex.Replace(value, "[^a-zA-Z0-9\x20]", string.Empty);

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
        }

        private void ShowElementMenuContext(TreeElement element, Rect rect)
        {
            if (element.Type == TreeElement.ElementType.Folder)
            {
                TreeViewSelectElement(element);
                MenuContext.Prepare();
                MenuContext.AddItem("New Folder", () => CreateFolder());
                MenuContext.AddItem("New Animation", () => CreateAnimation());
                MenuContext.AddSeparator();
                MenuContext.AddItem("Eliminar", () => DeleteFolder(element));
                MenuContext.ShowAsContext();                
            }
            else if (element.Type == TreeElement.ElementType.Animation)
            {
                TreeViewSelectElement(element);
                MenuContext.Prepare();
                MenuContext.AddItem("Delete", () => DeleteAnimation(element));
                MenuContext.AddItem("Duplicate", () => { DuplicateAnimation(element); });
                MenuContext.ShowAsContext();
            }
        }

        private void DuplicateAnimation(TreeElement element)
        {
            m_treeView.SelectElement(m_treeView.GetParent(element));
            if (int.TryParse(element.Value, out int id))
            {
                SpriteAnimation anim = m_data.GetAnimation(id);
                m_data.CopyAnimation(id, CreateAnimation());
                ValidateElementName(m_treeView.SelectedElement, anim.Name);
            }
        }

        private void RenameElement(TreeElement element, Rect rect)
        {
            rect.y += m_treeView.ElementHeight;
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
        }
                      
        private void TreeViewSelectElement(TreeElement element)
        {
            if (element == null)
            {
                m_treeView.SelectElement(null);
                return;
            }

            if (element.ID != m_treeView.SelectedElementId)
            {
                Snapshot("SelectionChange");
                GUI.FocusControl("TreeViewElement");
            }

            m_treeView.SelectElement(element);
            
            m_treeViewElementRename = null;
            
            if(element.Type == TreeElement.ElementType.Animation)
            {
                int id;
                if (int.TryParse(element.Value, out id))
                {
                    m_control.SetAnimation(m_data.GetAnimation(id));
                }
            }
            else if (element.Type == TreeElement.ElementType.Folder)
            {
                TreeElement[] animationElements = element.Where(x => x.Type == TreeElement.ElementType.Animation).Take(100).ToArray();
                List<SpriteAnimation> animations = new List<SpriteAnimation>();
                for (int i = 0; i < animationElements.Length; i++)
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
        }

        private void DeleteFolder(TreeElement element, bool showDialog = true)
        {
            if (showDialog && !EditorUtility.DisplayDialog("Delete Folder",
                      $"Sure delete folder [{element.Value}]",
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
        }

        private void DeleteAnimation(TreeElement element, bool showDialog = true)
        {
            int id = int.Parse(element.Value);
            SpriteAnimation animation = m_data.GetAnimation(id);

            if (showDialog && !EditorUtility.DisplayDialog("Delete Animation",
                        $"Sure delete [{animation.Name}] in [{animation.Path}]",
                        "yes",
                        "no"))
                return;


            if (showDialog)
                Snapshot("Delete Animation");

            m_data.RemoveAnimation(animation.Id);
            m_treeView.Root.Remove(x => x == element);
            m_control.SetAnimation(null);
        }

        private void CreateFolder()
        {
            Snapshot("Create Folder");

            TreeElement element = m_treeView.SelectedElement;

            if (element == null)
                element = m_treeView.Root;

            if (element.Type == TreeElement.ElementType.Animation)
                element = m_treeView.GetParent(element);

            TreeElement e = element.AddElement(new TreeElement { Type = TreeElement.ElementType.Folder });
            ValidateElementName(e, "New Folder");
        }

        private int CreateAnimation()
        {
            Snapshot("Create Animation");
            TreeElement element = m_treeView.SelectedElement;

            if (element == null)
                element = m_treeView.Root;

            if (element.Type == TreeElement.ElementType.Animation)
                element = m_treeView.GetParent(element);

            int id = m_data.CreateAnimation().Id;
            element = element.AddElement(new TreeElement { Value = id.ToString(), Type = TreeElement.ElementType.Animation });
            ValidateElementName(element, "New Animation");
            TreeViewSelectElement(element);
            UpdateTreeElementPath();

            return id;
        }

        #endregion
    }
}
