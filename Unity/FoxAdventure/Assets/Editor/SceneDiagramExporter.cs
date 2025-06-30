using UnityEngine; // GameObject, Component, MonoBehaviour �� Unity �⺻ Ŭ���� ���
using UnityEditor; // EditorWindow, MenuItem, EditorUtility, AssetDatabase, Handles �� ������ ���� Ŭ���� ���
using System.Collections.Generic; // List, Dictionary �� �÷��� ���
using System.Linq; // LINQ Ȯ�� �޼��� (��: Select) ���
using System.IO; // ���� ����� ���
using System.Reflection; // ���÷����� ����Ͽ� ��ũ��Ʈ �ʵ� �˻�

public class SceneDiagramExporter : EditorWindow
{
    // JSON ����� ���� ������ ���� ����
    // System.Serializable ��Ʈ����Ʈ�� ����Ͽ� JsonUtility�� �� Ŭ�������� ����ȭ�� �� �ֵ��� �մϴ�.
    [System.Serializable]
    public class DiagramData
    {
        public List<GameObjectData> gameObjects = new List<GameObjectData>(); // ���� ������Ʈ ���
        public List<ReferenceData> references = new List<ReferenceData>();   // ���� ���� ���
    }

    [System.Serializable]
    public class GameObjectData
    {
        public string id;                   // ���̾�׷����� ����� ���� ID (��: "player-12345")
        public string name;                 // ���� ������Ʈ�� �̸�
        public List<string> components = new List<string>(); // ���Ե� ������Ʈ �̸� ���

        // GUI �׸���� �ʵ� (JSON ����ȭ������ ����)
        [System.NonSerialized] public Rect guiRect; // ���� ������Ʈ ��ü �ڽ��� GUI ��ġ �� ũ��
        [System.NonSerialized] public Dictionary<string, Rect> componentRects = new Dictionary<string, Rect>(); // �� ������Ʈ�� GUI ��ġ �� ũ�� (����� �ƴ�)
    }

    [System.Serializable] // ReferenceData�� JSON���� �������� �ϹǷ� Serializable�� �����մϴ�.
    public class ReferenceData
    {
        public string fromObjId;         // ������ �����ϴ� ���� ������Ʈ�� ID
        public string fromComponentName; // ������ �����ϴ� ������Ʈ�� �̸� (��: "Transform" �Ǵ� "Player (Script)")
        public string toObjId;           // ���� ��� ���� ������Ʈ�� ID
        public string label;             // ���� ���� ���� (��: "Child Of (Transform)" �Ǵ� "Script Ref")

        // GUI �׸���� �ʵ� (JSON ����ȭ������ ����)
        [System.NonSerialized] public Vector2 startPos; // ���� ���� ������
        [System.NonSerialized] public Vector2 endPos;   // ���� ���� ����
    }

    private DiagramData _diagramData; // �� �����͸� ������ ����
    private Vector2 _scrollPosition = Vector2.zero; // ��ũ�� �� ��ġ
    private float _zoomLevel = 1.0f; // Ȯ��/��� ����

    // ���̾�׷� �׸��� ���
    private const float GO_WIDTH = 200;
    private const float GO_PADDING_X = 15;
    private const float GO_PADDING_TOP = 10;
    private const float COMPONENT_HEIGHT = 25;
    private const float COMPONENT_SPACING = 5;
    private const float GO_SPACING_X = 50; // ���� ������Ʈ ���� ���� ����
    private const float GO_SPACING_Y = 50; // ���� ������Ʈ ���� ���� ����


    // ����Ƽ ������ �޴��� "Tools/Scene Diagram Visualizer" �׸��� �߰��մϴ�.
    [MenuItem("Tools/Scene Diagram Visualizer")]
    public static void ShowWindow()
    {
        // �����찡 �̹� ���������� �ش� �����츦 ��������, ������ ���� �����Ͽ� ���ϴ�.
        GetWindow<SceneDiagramExporter>("Scene Diagram Visualizer");
    }

    // �����찡 Ȱ��ȭ�� �� ȣ��˴ϴ�. (�ʱ� ������ �ε�)
    private void OnEnable()
    {
        CollectAndLayoutSceneData();
    }

    // ������ �������� GUI�� �׸��� �Լ��Դϴ�.
    private void OnGUI()
    {
        // ���� �۾��� ���� ǥ���մϴ�.
        GUILayout.Label("Unity Scene Diagram Visualizer", EditorStyles.boldLabel);

        // "JSON Ŭ�����忡 ����" ��ư�� �����մϴ�.
        if (GUILayout.Button("Refresh Diagram (Collect Data & Layout)"))
        {
            CollectAndLayoutSceneData(); // �� �����͸� �����ϰ� ���̾ƿ��� �ٽ� ����մϴ�.
        }

        // "JSON Ŭ�����忡 ����" ��ư
        if (GUILayout.Button("Copy JSON to Clipboard"))
        {
            if (_diagramData != null)
            {
                string jsonOutput = JsonUtility.ToJson(_diagramData, true);
                EditorGUIUtility.systemCopyBuffer = jsonOutput;
                Debug.Log("Scene diagram JSON copied to clipboard!");
            }
            else
            {
                Debug.LogWarning("No diagram data to copy. Please refresh first.");
            }
        }

        // "JSON ���Ϸ� ����" ��ư
        if (GUILayout.Button("Save JSON to File"))
        {
            if (_diagramData != null)
            {
                string path = EditorUtility.SaveFilePanel("Save Scene Diagram JSON", "", "scene_diagram.json", "json");
                if (!string.IsNullOrEmpty(path))
                {
                    string jsonOutput = JsonUtility.ToJson(_diagramData, true);
                    File.WriteAllText(path, jsonOutput);
                    Debug.Log($"Scene diagram JSON saved to: {path}");
                }
            }
            else
            {
                Debug.LogWarning("No diagram data to save. Please refresh first.");
            }
        }

        EditorGUILayout.Space(10); // ���� �߰�

        // ���̾�׷� �׸��� ���� ����
        // GUILayout.ExpandWidth(true)�� GUILayout.ExpandHeight(true)�� ����Ͽ� ���� ������ ��� ä��ϴ�.
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

        if (_diagramData == null || _diagramData.gameObjects.Count == 0)
        {
            GUILayout.Label("No scene data available. Please click 'Refresh Diagram'.", EditorStyles.centeredGreyMiniLabel);
        }
        else
        {
            // Ȯ��/��� �� �̵� ó�� (Event.current�� �� �����ӿ� �ѹ����� ó���ؾ� �մϴ�.)
            HandleZoomAndPan();

            // GUI.matrix�� ����Ͽ� Ȯ��/��� ����
            Matrix4x4 oldMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(new Vector3(-_scrollPosition.x, -_scrollPosition.y, 0), Quaternion.identity, Vector3.one * _zoomLevel);

            // ���� ���̾�׷� �׸���
            DrawDiagramElements();

            GUI.matrix = oldMatrix; // ���� GUI ��Ʈ������ �ǵ�����
        }

        EditorGUILayout.EndScrollView();

        // �� ���� ���� ������ �����Ͽ� GUI�� �ٽ� �׸����� ��û (�ɼ�, ���� ���)
        if (GUI.changed)
        {
            Repaint();
        }
    }

    // �� �����͸� �����ϰ� ���̾ƿ��� ����ϴ� �Լ�
    private void CollectAndLayoutSceneData()
    {
        _diagramData = CollectSceneData(); // �� ������ ����
        CalculateLayout(_diagramData);    // ���̾ƿ� ���
        Debug.Log("Scene diagram data collected and layout calculated.");
        Repaint(); // �����츦 �ٽ� �׸����� ��û
    }


    // �� �����͸� �����ϴ� �Լ� (���� �ڵ�� ����)
    private DiagramData CollectSceneData()
    {
        DiagramData diagramData = new DiagramData();
        Dictionary<GameObject, string> gameObjectIds = new Dictionary<GameObject, string>();
        Dictionary<string, GameObject> idToGameObjectMap = new Dictionary<string, GameObject>();


        GameObject[] allGameObjects = FindObjectsOfType<GameObject>(true);

        // ù ��° ��ȸ: ��� ���� ������Ʈ�� ������Ʈ ������ ����
        foreach (GameObject go in allGameObjects)
        {
            string go_id = go.name.Replace(" ", "-").ToLower() + "-" + go.GetInstanceID();
            gameObjectIds[go] = go_id;
            idToGameObjectMap[go_id] = go; // ID -> GameObject ���� ����

            GameObjectData goData = new GameObjectData
            {
                id = go_id,
                name = go.name
            };

            foreach (Component comp in go.GetComponents<Component>())
            {
                if (comp == null) continue;

                string compName = comp.GetType().Name;

                if (comp is MonoBehaviour monoBehaviour)
                {
                    MonoScript script = MonoScript.FromMonoBehaviour(monoBehaviour);
                    if (script != null)
                    {
                        compName = script.GetClass().Name;
                    }
                    else
                    {
                        compName = $"{compName} (Missing Script)";
                    }
                }
                goData.components.Add(compName);
            }
            diagramData.gameObjects.Add(goData);
        }

        // �� ��° ��ȸ: �θ�-�ڽ� ���� ������ ��ũ��Ʈ ������ �߰�
        foreach (GameObject go in allGameObjects)
        {
            string child_id = gameObjectIds[go];

            // �θ�-�ڽ� ����
            if (go.transform.parent != null && gameObjectIds.ContainsKey(go.transform.parent.gameObject))
            {
                string parent_id = gameObjectIds[go.transform.parent.gameObject];
                diagramData.references.Add(new ReferenceData
                {
                    fromObjId = parent_id,
                    fromComponentName = "Transform",
                    toObjId = child_id,
                    label = "Child Of (Transform)"
                });
            }

            // ��ũ��Ʈ (MonoBehaviour) ���� �ۺ� ���� ����
            foreach (MonoBehaviour monoBehaviour in go.GetComponents<MonoBehaviour>())
            {
                if (monoBehaviour == null) continue; // ������ ��ũ��Ʈ ó��

                SerializedObject serializedObject = new SerializedObject(monoBehaviour);
                SerializedProperty property = serializedObject.GetIterator();

                while (property.NextVisible(true)) // ��� ���̴� �ʵ带 ��ȸ
                {
                    // ObjectReference Ÿ���� �ʵ� (GameObject, Component, ScriptableObject �� ����)
                    if (property.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        if (property.objectReferenceValue != null)
                        {
                            GameObject referencedGo = null;
                            string referencedCompName = null;
                            string referencedId = null;

                            if (property.objectReferenceValue is GameObject refGo)
                            {
                                referencedGo = refGo;
                                if (gameObjectIds.ContainsKey(referencedGo))
                                {
                                    referencedId = gameObjectIds[referencedGo];
                                }
                            }
                            else if (property.objectReferenceValue is Component refComp)
                            {
                                referencedGo = refComp.gameObject;
                                referencedCompName = refComp.GetType().Name;
                                if (gameObjectIds.ContainsKey(referencedGo))
                                {
                                    referencedId = gameObjectIds[referencedGo];
                                }
                            }

                            if (referencedId != null)
                            {
                                string fromCompName = monoBehaviour.GetType().Name; // ��ũ��Ʈ ������Ʈ �̸�
                                diagramData.references.Add(new ReferenceData
                                {
                                    fromObjId = child_id,
                                    fromComponentName = fromCompName,
                                    toObjId = referencedId,
                                    label = $"Script Ref: {property.name}" // �ʵ� �̸����� ���� ��
                                });
                            }
                        }
                    }
                }
            }
        }

        return diagramData;
    }

    // ���̾�׷� ��ҵ��� GUI ��ġ�� ����ϴ� �Լ�
    private void CalculateLayout(DiagramData data)
    {
        if (data == null || data.gameObjects.Count == 0) return;

        // ���� ������Ʈ ID�� ������� GameObjectData�� ������ ã�� ���� ��
        Dictionary<string, GameObjectData> goMap = data.gameObjects.ToDictionary(go => go.id);

        // ���̾ƿ� ����� ���� �ӽ� ����
        float currentX = GO_SPACING_X;
        float currentY = GO_SPACING_Y;
        float maxYInRow = 0;

        // GameObject�� Component�� Rect ��ġ�� ���
        foreach (GameObjectData goData in data.gameObjects)
        {
            float goHeight = GO_PADDING_TOP * 2 + goData.components.Count * (COMPONENT_HEIGHT + COMPONENT_SPACING);
            if (goData.components.Count > 0) goHeight -= COMPONENT_SPACING; // ������ ������Ʈ �� ���� ����

            goData.guiRect = new Rect(currentX, currentY, GO_WIDTH, goHeight);

            float compY = currentY + GO_PADDING_TOP + 20; // ������Ʈ ���� Y ��ġ

            goData.componentRects.Clear(); // ���� ������ �ʱ�ȭ
            foreach (string compName in goData.components)
            {
                // ������Ʈ�� ���� ��ġ�� ����
                // ���⼭ ������Ʈ�� rect�� ���� ������Ʈ�� rect ���ο� ��������� �׷�������,
                // ������ ���� startPos/endPos ����� ���� ���� ��ǥ�� ����
                goData.componentRects[compName] = new Rect(currentX + GO_PADDING_X, compY, GO_WIDTH - (GO_PADDING_X * 2), COMPONENT_HEIGHT);
                compY += COMPONENT_HEIGHT + COMPONENT_SPACING;
            }

            maxYInRow = Mathf.Max(maxYInRow, goHeight);

            currentX += GO_WIDTH + GO_SPACING_X;
            // ������ �׸��� ���̾ƿ� (����: �� �ٿ� 3���� ��ġ)
            if (currentX > 3 * (GO_WIDTH + GO_SPACING_X))
            {
                currentX = GO_SPACING_X;
                currentY += maxYInRow + GO_SPACING_Y;
                maxYInRow = 0;
            }
        }

        // ���� ���� �������� ���� ���
        foreach (ReferenceData refData in data.references)
        {
            Rect fromRect;
            Rect toRect;

            // fromObjId�� �ش��ϴ� GameObjectData ã��
            GameObjectData fromGoData;
            if (goMap.TryGetValue(refData.fromObjId, out fromGoData))
            {
                // fromComponentName�� �����Ǹ� �ش� ������Ʈ�� ��ġ�� ���
                if (!string.IsNullOrEmpty(refData.fromComponentName) && fromGoData.componentRects.ContainsKey(refData.fromComponentName))
                {
                    fromRect = fromGoData.componentRects[refData.fromComponentName];
                }
                else
                {
                    // �ƴϸ� ���� ������Ʈ �ڽ��� ��ġ�� ���
                    fromRect = fromGoData.guiRect;
                }
            }
            else
            {
                Debug.LogWarning($"Cannot find 'from' GameObjectData for ID: {refData.fromObjId}");
                continue;
            }

            // toObjId�� �ش��ϴ� GameObjectData ã��
            GameObjectData toGoData;
            if (goMap.TryGetValue(refData.toObjId, out toGoData))
            {
                toRect = toGoData.guiRect;
            }
            else
            {
                Debug.LogWarning($"Cannot find 'to' GameObjectData for ID: {refData.toObjId}");
                continue;
            }

            // �������� ���� ��� (�����ϰ� �ڽ� �߾ӿ��� �߾����� ����)
            // �� ������ ������ ����� (��: ���� ����� �𼭸�) ���⵵�� ���� �� �ֽ��ϴ�.
            refData.startPos = fromRect.center;
            refData.endPos = toRect.center;
        }
    }

    // GUI ��Ʈ������ ��ũ���� ����Ͽ� ���̾�׷� ��Ҹ� �׸��� �Լ�
    private void DrawDiagramElements()
    {
        if (_diagramData == null) return;

        // Handles.color�� GUI.color�ʹ� �����Դϴ�.
        // Handles�� ����Ͽ� ���� �׸� ���̹Ƿ� Handles.color�� �����մϴ�.
        Handles.color = new Color(0.443f, 0.502f, 0.588f, 1.0f); // Gray-ish blue for lines

        // ���� �� �׸��� (���� ������Ʈ �ڽ� ���� �׷������� ���� �׸��ϴ�.)
        foreach (ReferenceData refData in _diagramData.references)
        {
            // �� ��ġ�� �̹� Ȯ��/��� �� ��ũ���� ����� GUI.matrix ���ؽ�Ʈ ���� ���� ��ǥ�Դϴ�.
            Vector3 start = refData.startPos;
            Vector3 end = refData.endPos;

            // �� �׸���
            Handles.DrawLine(start, end);

            // ȭ��ǥ �Ӹ� �׸��� (������ �ﰢ��)
            Vector2 direction = (end - start).normalized;
            Vector2 perpendicular = new Vector2(-direction.y, direction.x);
            float arrowSize = 10f;

            Vector3[] arrowHeadPoints = new Vector3[3];
            arrowHeadPoints[0] = end;
            arrowHeadPoints[1] = end - (Vector3)direction * arrowSize + (Vector3)perpendicular * (arrowSize / 2);
            arrowHeadPoints[2] = end - (Vector3)direction * arrowSize - (Vector3)perpendicular * (arrowSize / 2);
            Handles.DrawAAConvexPolygon(arrowHeadPoints);

            // ���� �� �׸��� (�� �߰��� ��ġ)
            if (!string.IsNullOrEmpty(refData.label))
            {
                Vector2 labelPos = (refData.startPos + refData.endPos) / 2;
                // �� ��ġ�� ���� ���⿡ ���� ��¦ ������
                labelPos += perpendicular * 10;

                // GUI.Label�� ���� GUI.matrix�� ������ �޽��ϴ�.
                GUI.Label(new Rect(labelPos.x - 50, labelPos.y - 10, 100, 20), refData.label, EditorStyles.miniLabel);
            }
        }

        // ���� ������Ʈ�� ������Ʈ �׸��� (���� �� ���� �׷������� ���߿� �׸��ϴ�.)
        foreach (GameObjectData goData in _diagramData.gameObjects)
        {
            // ���� ������Ʈ �ڽ�
            // GUI.Box�� ���� GUI.matrix�� ������ �޽��ϴ�.
            GUI.Box(goData.guiRect, goData.name, GUI.skin.box);

            // ������Ʈ �׸���
            float currentCompY = goData.guiRect.y + GO_PADDING_TOP + 20;
            foreach (string compName in goData.components)
            {
                Rect compRect = new Rect(goData.guiRect.x + GO_PADDING_X, currentCompY, GO_WIDTH - (GO_PADDING_X * 2), COMPONENT_HEIGHT);
                GUI.Box(compRect, compName, GUI.skin.textArea); // ������Ʈ�� textArea ��Ÿ�Ϸ� �ٸ��� ǥ��

                currentCompY += COMPONENT_HEIGHT + COMPONENT_SPACING;
            }

            // Ŭ�� ���� �� ������Ʈ ����
            Event currentEvent = Event.current;
            if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && goData.guiRect.Contains(currentEvent.mousePosition))
            {
                // ���̾�׷� ID���� GameObject �ν��Ͻ� ID�� �����մϴ�.
                // �� ����� go_id ���� ��Ŀ� ���� �޶��� �� �ֽ��ϴ�.
                // ���⼭�� go_id�� "�̸�-�ν��Ͻ�ID" �����̹Ƿ� �ν��Ͻ� ID�� �����մϴ�.
                string instanceIdStr = goData.id.Split('-').LastOrDefault();
                if (int.TryParse(instanceIdStr, out int instanceId))
                {
                    GameObject selectedGo = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
                    if (selectedGo != null)
                    {
                        Selection.activeGameObject = selectedGo; // Hierarchy���� �ش� ������Ʈ�� ����
                        EditorGUIUtility.PingObject(selectedGo); // ������Ʈ �信�� �ش� ������Ʈ�� �����̰� ��
                        currentEvent.Use(); // �̺�Ʈ ó�� �Ϸ�
                    }
                }
            }
        }
    }

    // Ȯ��/��� �� ȭ�� �̵� ó��
    private void HandleZoomAndPan()
    {
        Event currentEvent = Event.current;

        // ���콺 �ٷ� Ȯ��/���
        if (currentEvent.type == EventType.ScrollWheel)
        {
            float zoomDelta = -currentEvent.delta.y * 0.05f; // �� ���⿡ ���� Ȯ��/���
            _zoomLevel += zoomDelta;
            _zoomLevel = Mathf.Clamp(_zoomLevel, 0.5f, 2.0f); // Ȯ��/��� ���� ����

            // Ȯ��/��� �� ���콺 ��ġ �������� ��ũ�� ���� (���� ����, �� �ڿ������� Ȯ��/��Ҹ� ����)
            Vector2 mousePos = currentEvent.mousePosition;
            Vector2 adjustedMousePos = mousePos / _zoomLevel; // ���� Ȯ�� ������ ����� ���콺 ��ġ
            _scrollPosition = adjustedMousePos - (mousePos - _scrollPosition);


            currentEvent.Use(); // �̺�Ʈ ó�� �Ϸ�
        }
        // ���콺 �巡�׷� ȭ�� �̵� (��� ��ư �Ǵ� Alt+��Ŭ��)
        else if (currentEvent.type == EventType.MouseDrag && (currentEvent.button == 2 || (currentEvent.button == 0 && currentEvent.alt)))
        {
            _scrollPosition -= currentEvent.delta / _zoomLevel; // Ȯ�� ������ ���� �̵� �ӵ� ����
            currentEvent.Use(); // �̺�Ʈ ó�� �Ϸ�
        }
    }
}
