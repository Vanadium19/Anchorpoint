using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace WeaponModule
{
    /// <summary>
    /// Splits a stock part off the mesh of the selected MeshRenderer or SkinnedMeshRenderer, in a scene or a prefab
    /// stage. Pick islands from the list (they are drawn in the Scene view) or a box in mesh space, name the part and
    /// press Create Stock Part. The work is done by <see cref="WeaponPartSplitter"/>.
    /// </summary>
    public sealed class WeaponPartSplitterWindow : EditorWindow
    {
        private const string DefaultFolder = "Assets";
        private const float ToggleWidth = 20f;
        private const float IndexWidth = 36f;
        private const float CountWidth = 56f;
        private const float SizeWidth = 150f;
        private const float BoxFraction = 0.25f;
        private const int MaxDrawnIslands = 256;

        private static readonly Color SelectedColor = Color.yellow;
        private static readonly Color FocusedColor = Color.cyan;
        private static readonly Color IdleColor = new(1f, 1f, 1f, 0.25f);

        private Renderer _renderer;
        private Mesh _mesh;
        private Transform[] _bones = Array.Empty<Transform>();
        private Matrix4x4[] _bindposes = Array.Empty<Matrix4x4>();
        private List<MeshIsland> _islands = new();
        private bool[] _isSelected = Array.Empty<bool>();
        private int _focusedIsland = -1;
        private bool _useBox;
        private Bounds _box;
        private int _boxBone = -1;
        private string _partName = string.Empty;
        private string _folder = DefaultFolder;
        private bool _isFolderChosen;
        private Vector2 _scroll;
        private string _message;
        private MessageType _messageType;

        /// <summary>
        /// Opens the window.
        /// </summary>
        [MenuItem("Tools/Weapons/Part Splitter")]
        public static void Open() => GetWindow<WeaponPartSplitterWindow>("Part Splitter");

        private void OnEnable()
        {
            Selection.selectionChanged += OnSelectionChanged;
            SceneView.duringSceneGui += OnSceneGui;
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
            Refresh(true);
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChanged;
            SceneView.duringSceneGui -= OnSceneGui;
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
        }

        private void OnGUI()
        {
            if (_renderer == null || _mesh == null)
            {
                EditorGUILayout.HelpBox(
                    "Select a MeshRenderer (with a MeshFilter) or a SkinnedMeshRenderer in a scene or prefab stage.",
                    MessageType.Info);
                DrawMessage();

                return;
            }

            DrawSource();

            _useBox = EditorGUILayout.ToggleLeft("Select by box instead of islands", _useBox);

            if (_useBox)
                DrawBox();
            else
                DrawIslands();

            DrawCreate();
            DrawMessage();
        }

        private void OnSelectionChanged()
        {
            Refresh(false);
            Repaint();
        }

        private void OnUndoRedoPerformed()
        {
            Refresh(true);
            Repaint();
        }

        private void OnSceneGui(SceneView sceneView)
        {
            if (_renderer == null || _mesh == null)
                return;

            var previousMatrix = Handles.matrix;
            var previousColor = Handles.color;

            if (_useBox)
                DrawWireBounds(_box, _boxBone, FocusedColor);
            else
                DrawIslandBounds();

            Handles.matrix = previousMatrix;
            Handles.color = previousColor;
        }

        private void DrawIslandBounds()
        {
            var idleDrawn = 0;

            for (var i = 0; i < _islands.Count; i++)
            {
                var isFocused = i == _focusedIsland;
                var isSelected = _isSelected[i];

                if (!isFocused && !isSelected && idleDrawn++ >= MaxDrawnIslands)
                    continue;

                var color = isFocused ? FocusedColor : isSelected ? SelectedColor : IdleColor;

                DrawWireBounds(_islands[i].Bounds, _islands[i].DominantBone, color);
            }
        }

        private void DrawWireBounds(Bounds bounds, int bone, Color color)
        {
            Handles.matrix = GetMeshToWorld(bone);
            Handles.color = color;
            Handles.DrawWireCube(bounds.center, bounds.size);
        }

        private Matrix4x4 GetMeshToWorld(int bone)
        {
            if (bone >= 0 && bone < _bones.Length && bone < _bindposes.Length && _bones[bone] != null)
                return _bones[bone].localToWorldMatrix * _bindposes[bone];

            return _renderer.transform.localToWorldMatrix;
        }

        private void DrawSource()
        {
            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.ObjectField("Renderer", _renderer, typeof(Renderer), true);

            EditorGUILayout.LabelField(
                "Mesh",
                $"{_mesh.name}: {_mesh.vertexCount} vertices, {CountTriangles(_mesh)} triangles, {_islands.Count} islands");
        }

        private void DrawIslands()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Select None"))
                    Array.Clear(_isSelected, 0, _isSelected.Length);

                if (GUILayout.Button("Invert"))
                {
                    for (var i = 0; i < _isSelected.Length; i++)
                        _isSelected[i] = !_isSelected[i];
                }
            }

            using (var scroll = new EditorGUILayout.ScrollViewScope(_scroll, GUILayout.MaxHeight(position.height * 0.5f)))
            {
                _scroll = scroll.scrollPosition;

                for (var i = 0; i < _islands.Count; i++)
                    DrawIslandRow(i);
            }
        }

        private void DrawIslandRow(int index)
        {
            var island = _islands[index];
            var size = island.Bounds.size;

            using (new EditorGUILayout.HorizontalScope())
            {
                _isSelected[index] = EditorGUILayout.Toggle(_isSelected[index], GUILayout.Width(ToggleWidth));

                if (GUILayout.Toggle(index == _focusedIsland, $"#{index}", EditorStyles.miniButton, GUILayout.Width(IndexWidth)) !=
                    (index == _focusedIsland))
                {
                    _focusedIsland = index == _focusedIsland ? -1 : index;
                    SceneView.RepaintAll();
                }

                GUILayout.Label($"{island.VertexCount} v", GUILayout.Width(CountWidth));
                GUILayout.Label($"{island.TriangleCount} t", GUILayout.Width(CountWidth));
                GUILayout.Label($"{size.x:0.###} x {size.y:0.###} x {size.z:0.###} m", GUILayout.Width(SizeWidth));
                GUILayout.Label(DescribeBone(island.DominantBone));
            }
        }

        private void DrawBox()
        {
            EditorGUI.BeginChangeCheck();

            _box = EditorGUILayout.BoundsField("Box (mesh space)", _box);

            if (_renderer is SkinnedMeshRenderer)
                _boxBone = EditorGUILayout.IntField("Bone of the box (-1 none)", _boxBone);

            if (EditorGUI.EndChangeCheck())
                SceneView.RepaintAll();
        }

        private void DrawCreate()
        {
            _partName = EditorGUILayout.TextField("Part name", _partName);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel("Mesh folder");
                EditorGUILayout.SelectableLabel(_folder, GUILayout.Height(EditorGUIUtility.singleLineHeight));

                if (GUILayout.Button("Choose...", GUILayout.Width(80f)))
                    ChooseFolder();
            }

            var selectedCount = _isSelected.Count(static isSelected => isSelected);
            var canCreate = !string.IsNullOrWhiteSpace(_partName) && (_useBox || selectedCount > 0);

            using (new EditorGUI.DisabledScope(!canCreate))
            {
                if (GUILayout.Button(_useBox ? "Create Stock Part" : $"Create Stock Part ({selectedCount} islands)"))
                    CreatePart();
            }
        }

        private void DrawMessage()
        {
            if (!string.IsNullOrEmpty(_message))
                EditorGUILayout.HelpBox(_message, _messageType);
        }

        private void ChooseFolder()
        {
            var absolute = EditorUtility.OpenFolderPanel("Folder for the new meshes", _folder, string.Empty);

            if (string.IsNullOrEmpty(absolute))
                return;

            var dataPath = Application.dataPath;

            if (!absolute.StartsWith(dataPath, StringComparison.OrdinalIgnoreCase))
            {
                SetMessage("The folder must be inside the Assets folder of this project.", MessageType.Warning);

                return;
            }

            _folder = DefaultFolder + absolute.Substring(dataPath.Length);
            _isFolderChosen = true;
            _message = null;
        }

        private void CreatePart()
        {
            try
            {
                var split = _useBox
                    ? WeaponMeshPartExtractor.SplitByBox(_mesh, _box)
                    : WeaponMeshPartExtractor.Split(_mesh, _islands.Where((_, index) => _isSelected[index]));
                var created = WeaponPartSplitter.CreateStockPart(_renderer, split, _partName, _folder);
                var path = AssetDatabase.GetAssetPath(WeaponViewValidator.GetMesh(created));

                Refresh(true);
                SetMessage($"Created '{created.name}' with the mesh {path}. The source renderer now shows the remainder.",
                    MessageType.Info);
                EditorGUIUtility.PingObject(created.gameObject);
            }
            catch (Exception exception) when (exception is ArgumentException or NotSupportedException or InvalidOperationException)
            {
                SetMessage(exception.Message, MessageType.Error);
            }
        }

        private void Refresh(bool isForced)
        {
            var renderer = FindSelectedRenderer();
            var mesh = renderer != null ? WeaponViewValidator.GetMesh(renderer) : null;

            if (!isForced && renderer == _renderer && mesh == _mesh)
                return;

            _renderer = renderer;
            _mesh = mesh;
            _islands = new List<MeshIsland>();
            _isSelected = Array.Empty<bool>();
            _focusedIsland = -1;
            _bones = Array.Empty<Transform>();
            _bindposes = Array.Empty<Matrix4x4>();

            if (_mesh == null)
                return;

            _box = new Bounds(_mesh.bounds.center, _mesh.bounds.size * BoxFraction);
            _bindposes = _mesh.bindposes;

            if (_renderer is SkinnedMeshRenderer skinned)
                _bones = skinned.bones;

            if (!_isFolderChosen)
                _folder = FindDefaultFolder(_mesh);

            try
            {
                _islands = WeaponMeshPartExtractor.FindIslands(_mesh);
                _isSelected = new bool[_islands.Count];
                _message = null;
            }
            catch (NotSupportedException exception)
            {
                SetMessage(exception.Message, MessageType.Error);
            }
        }

        private void SetMessage(string message, MessageType type)
        {
            _message = message;
            _messageType = type;
        }

        private string DescribeBone(int bone)
        {
            if (bone < 0)
                return "no bone";

            return bone < _bones.Length && _bones[bone] != null ? _bones[bone].name : $"bone {bone}";
        }

        private static Renderer FindSelectedRenderer()
        {
            var selected = Selection.activeGameObject;

            if (selected == null)
                return null;

            if (selected.TryGetComponent<SkinnedMeshRenderer>(out var skinned))
                return skinned;

            return selected.TryGetComponent<MeshRenderer>(out var meshRenderer) && selected.GetComponent<MeshFilter>() != null
                ? meshRenderer
                : null;
        }

        private static string FindDefaultFolder(Mesh mesh)
        {
            var path = AssetDatabase.GetAssetPath(mesh);
            var folder = string.IsNullOrEmpty(path) ? DefaultFolder : System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');

            return !string.IsNullOrEmpty(folder) && folder.StartsWith(DefaultFolder, StringComparison.Ordinal) &&
                   AssetDatabase.IsValidFolder(folder)
                ? folder
                : DefaultFolder;
        }

        private static int CountTriangles(Mesh mesh)
        {
            var count = 0L;

            for (var i = 0; i < mesh.subMeshCount; i++)
                count += mesh.GetIndexCount(i);

            return (int)(count / 3);
        }
    }
}
