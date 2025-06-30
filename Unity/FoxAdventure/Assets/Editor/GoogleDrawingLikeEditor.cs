using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq; // FindObjectsOfType<T>() ��ȯ Ÿ���� ���ϰ� �ٷ�� ����
using UnityEngine.SceneManagement; // SceneManager�� ����Ͽ� ���� ���� GameObject�� ����

// �� ������ Ȯ���� ����Ƽ�� �⺻ API�� ����Ͽ� �� �����͸� �Ľ��մϴ�.

// --- JSON ������ ���� (���� ������ �𵨷� ���) ---
// GameObject �����͸� ��Ÿ���� Ŭ����
[Serializable]
public class GameObjectData
{
    public string id; // ���� ID (GetInstanceID() ���)
    public string name; // ���� ������Ʈ �̸�
    public List<string> components; // ������Ʈ ���
}

// ����(���ἱ) �����͸� ��Ÿ���� Ŭ����
[Serializable]
public class ReferenceData
{
    public string fromObjId; // ���� ������Ʈ�� ID
    public string fromComponentName; // ���� ������Ʈ�� ������Ʈ �̸� (ǥ�ÿ� Ȱ��� �� ����)
    public string toObjId; // �� ������Ʈ�� ID
    public string label; // ���ἱ�� �� (��: "Child Of", "Script Ref")
}

// ������ JSON �Ľ��� ���� ���Ǿ��� UnityProjectData�� �� ���� Ŭ������
// ���� �� �����͸� ���� �ε��ϹǷ� �ʿ����� ������,
// ���� �ڵ���� �ϰ����� ���� �ּ����� ���ܵӴϴ�.
/*
[Serializable]
public class UnityProjectData
{
    public List<GameObjectData> gameObjects;
    public List<ReferenceData> references;

    [Serializable]
    public class GameObjectListWrapper
    {
        public List<GameObjectData> gameObjects;
    }

    [Serializable]
    public class ReferenceListWrapper
    {
        public List<ReferenceData> references;
    }
}
*/

// --- ������ ���� ��� ǥ�� ---
// �׷��� ��带 ��Ÿ���� ���� Ŭ����
public class GraphNode
{
    public GameObjectData Data { get; private set; } // ���� GameObject ������
    public Rect Position { get; set; } // ��� â�� ���� ��ġ�� ũ��
    public string ID => Data.id; // ����� ���� ID (GameObjectData.id�� ����)

    public GraphNode(GameObjectData data, Vector2 initialPosition, Vector2 size)
    {
        Data = data;
        Position = new Rect(initialPosition, size);
    }

    // ��� â�� �׸��� �޼��� (OnGUI �������� ȣ���)
    public void Draw(int windowID, Vector2 panOffset, float zoomLevel)
    {
        // �� �� �� ������ ����� ��ġ�� ũ�⿡ �����Ͽ� �׸� Rect ���
        Rect zoomedPosition = new Rect(
            (Position.x + panOffset.x) * zoomLevel,
            (Position.y + panOffset.y) * zoomLevel,
            Position.width * zoomLevel,
            Position.height * zoomLevel
        );

        // ��Ʈ ũ�� �� ���� ���� (�� ������ ����)
        GUI.skin.window.fontSize = Mathf.RoundToInt(12 * zoomLevel);
        GUI.skin.label.fontSize = Mathf.RoundToInt(10 * zoomLevel);
        GUI.skin.label.normal.textColor = Color.black; // �ؽ�Ʈ ���ü� Ȯ��

        // GUI.Window�� ����Ͽ� ��� â�� �׸���, �巡�� �� Position�� ������Ʈ�մϴ�.
        // GUI.Window�� �巡�� �� rect�� ���� ������Ʈ�ϹǷ�,
        // �巡�׵� ��/���� ����� ��ġ�� �ٽ� �������� ��/�ҵ��� ���� ��ǥ�� ��ȯ�ؾ� �մϴ�.
        Rect updatedZoomePos = GUI.Window(windowID, zoomedPosition, DrawNodeWindow, Data.name);

        // GUI.Window���� ��ȯ�� (��/�� �����) ��ġ�� �������� (��/�ҵ��� ����) ��ġ�� ��ȯ
        Position = new Rect(
            (updatedZoomePos.x / zoomLevel) - panOffset.x,
            (updatedZoomePos.y / zoomLevel) - panOffset.y,
            updatedZoomePos.width / zoomLevel,
            updatedZoomePos.height / zoomLevel
        );
    }

    // ��� â�� ������ �׸��� �ݹ� �޼���
    private void DrawNodeWindow(int id)
    {
        GUILayout.Label("<b>Components:</b>");
        if (Data.components != null)
        {
            foreach (string component in Data.components)
            {
                GUILayout.Label("- " + component);
            }
        }
        GUI.DragWindow(); // �� ȣ���� ������ â�� �巡�׵��� �ʽ��ϴ�.
    }
}

// --- ���� ������ ������ Ŭ���� ---
public class GoogleDrawingLikeEditor : EditorWindow
{
    // ��� �׷��� ��带 �����ϴ� Dictionary (ID�� ������ ����)
    private Dictionary<string, GraphNode> nodes = new Dictionary<string, GraphNode>();
    // ���� ������ ����Ʈ
    private List<ReferenceData> references = new List<ReferenceData>();

    // ĵ���� ��(�̵�) ���� ����
    private Vector2 panOffset = Vector2.zero; // ���� ĵ���� �̵� ������
    private Vector2 dragOrigin; // �� ���� �� ���콺 ��ġ
    private bool isPanning = false; // �� ��� ����

    // ĵ���� ��(Ȯ��/���) ���� ����
    private float zoomLevel = 1.0f; // ���� �� ����
    private const float MinZoom = 0.2f; // �ּ� �� ����
    private const float MaxZoom = 2.0f; // �ִ� �� ����
    private const float ZoomSpeed = 0.05f; // �� �ӵ�

    // ��� �⺻ ũ��
    private Vector2 defaultNodeSize = new Vector2(200, 100);

    // ����Ƽ �޴��� ������ ������ �߰�
    [MenuItem("Window/Google Drawing-like Editor")]
    public static void ShowWindow()
    {
        GetWindow<GoogleDrawingLikeEditor>("Google Drawing Editor"); // ������ ���� �� Ÿ��Ʋ ����
    }

    // �����찡 Ȱ��ȭ�� �� ȣ��� (�ʱ�ȭ)
    private void OnEnable()
    {
        // �� ���� ���� �̺�Ʈ ���: ���� ���� ���� �� (������Ʈ ����/����/�θ� ���� ��)
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
        // �� ����/�ݱ�/���� �� �� ���� ���� ���� �̺�Ʈ ��� (������)
        // EditorSceneManager.sceneOpened += OnSceneOpened; // �ʿ��� ��� �߰�
        LoadSceneData(); // �� ������ �ε�
    }

    // �����찡 ��Ȱ��ȭ�� �� ȣ���
    private void OnDisable()
    {
        // �� ���� ���� �̺�Ʈ ���� (�޸� ���� ����)
        EditorApplication.hierarchyChanged -= OnHierarchyChanged;
        // EditorSceneManager.sceneOpened -= OnSceneOpened; // �ʿ��� ��� ����
    }

    // ���� ���� ���� �� ȣ��Ǵ� �ݹ�
    private void OnHierarchyChanged()
    {
        Debug.Log("Hierarchy changed. Reloading scene data.");
        LoadSceneData(); // �� ������ �ٽ� �ε�
        Repaint(); // ������ �ٽ� �׸��� ��û
    }

    // �������� GUI�� �׸��� �ٽ� �޼���
    private void OnGUI()
    {
        // �Է� �̺�Ʈ ó�� (�� �� ��)
        HandleInputEvents();

        // GUI �׸��� ��Ʈ������ �����Ͽ� �� �� �� ���� �� ���·� ������ �� �ֵ��� ��
        Matrix4x4 oldGuiMatrix = GUI.matrix;

        // �� ����: ������ �������� ������ ���� (0,0�� ������ �������� �»��)
        // �� ��Ʈ���� ��ȯ�� ��� GUI.Window �� Handles.DrawXX ȣ�⿡ ������ �ݴϴ�.
        GUIUtility.ScaleAroundPivot(Vector2.one * zoomLevel, Vector2.zero);
        GUI.matrix = oldGuiMatrix * Matrix4x4.Scale(new Vector3(zoomLevel, zoomLevel, 1f));


        // ��� ��� �׸���
        foreach (var nodeEntry in nodes)
        {
            GraphNode node = nodeEntry.Value;
            // �� ����� Draw �޼��忡 �� �����°� �� ������ �����Ͽ� ��庰 ��ġ ��꿡 ���
            node.Draw(node.ID.GetHashCode(), panOffset, zoomLevel);
        }

        // ���ἱ(����) �׸���
        DrawConnections();

        // GUI ��Ʈ������ ���� ���·� ����
        GUI.matrix = oldGuiMatrix;
    }

    // ���� ����Ƽ ���� �����͸� �ε��ϰ� ��� �� ���� �����͸� �ʱ�ȭ
    private void LoadSceneData()
    {
        nodes.Clear(); // ���� ��� ������ ����
        references.Clear(); // ���� ���� ������ ����

        // ���� Ȱ��ȭ�� �� ��������
        Scene activeScene = SceneManager.GetActiveScene();
        if (!activeScene.isLoaded)
        {
            Debug.LogWarning("���� �ε�� ���� �����ϴ�. ���� �ε��ϰų� ���� �������ּ���.");
            return;
        }

        // �� ���� ��� GameObject ã��
        // FindObjectsOfType<GameObject>()�� Ȱ��ȭ�� ������Ʈ�� ã���ϴ�.
        // ��Ȱ��ȭ�� ������Ʈ���� �����Ϸ��� SceneManager.GetActiveScene().GetRootGameObjects()�� ��������
        // ���� ������ ��������� Ž���ؾ� �մϴ�. ���⼭�� ���ǻ� FindObjectsOfType�� ����մϴ�.
        GameObject[] allGameObjects = FindObjectsOfType<GameObject>();

        // ��� �ʱ�ȭ �� �ʱ� ��ġ
        Vector2 currentPosition = new Vector2(20, 20); // �ʱ� ��� ��ġ ���� ��ġ
        int nodesPerRow = 5; // �� �ٿ� ǥ���� ��� ��
        float nodeSpacingX = defaultNodeSize.x + 30; // ��� �� ���� ����
        float nodeSpacingY = defaultNodeSize.y + 30; // ��� �� ���� ����
        int nodeCount = 0;

        foreach (GameObject obj in allGameObjects)
        {
            if (obj == null) continue; // ��ȿ���� ���� ������Ʈ �ǳʶٱ� (�� ���� ������ ���� �߻� ����)

            // GameObjectData ���� (�ð�ȭ ��)
            GameObjectData goData = new GameObjectData
            {
                id = obj.GetInstanceID().ToString(), // ����Ƽ�� ���� �ν��Ͻ� ID�� ��� ID�� ���
                name = obj.name,
                components = new List<string>()
            };

            // ��� ������Ʈ �̸� ��������
            // Missing (��ũ��Ʈ�� ����) ������Ʈ�� null�� �� �����Ƿ� null üũ
            foreach (Component comp in obj.GetComponents<Component>())
            {
                if (comp != null)
                {
                    goData.components.Add(comp.GetType().Name);
                }
                else
                {
                    goData.components.Add("Missing Script (������Ʈ ����)");
                }
            }

            // GraphNode ���� �� ��ųʸ��� �߰�
            GraphNode newNode = new GraphNode(goData, currentPosition, defaultNodeSize);
            nodes[goData.id] = newNode;

            // �׸��� ���̾ƿ� ������Ʈ (�ʱ� ��ġ��)
            currentPosition.x += nodeSpacingX;
            nodeCount++;
            if (nodeCount % nodesPerRow == 0)
            {
                currentPosition.x = 20;
                currentPosition.y += nodeSpacingY;
            }

            // --- ���� (references) �߰� ---

            // 1. �θ�-�ڽ� ���� (Transform ����)
            if (obj.transform.parent != null)
            {
                references.Add(new ReferenceData
                {
                    fromObjId = obj.GetInstanceID().ToString(), // �ڽ� ������Ʈ
                    fromComponentName = "Transform",
                    toObjId = obj.transform.parent.gameObject.GetInstanceID().ToString(), // �θ� ������Ʈ
                    label = "Child Of (Transform)"
                });
            }

            // 2. ��ũ��Ʈ ���� (Script Ref):
            // ����Ƽ �����Ϳ��� ��ũ��Ʈ ���� ������ �Ϲ����̰� �߰��ϰ� �����ϴ� ���� �����մϴ�.
            // ���⼭�� `unityJson.json` ���ÿ��� ��Ÿ�� �� ���� Ư�� ������Ʈ ���� ������
            // "�̸� ���"���� ã�Ƽ� �߰��ϴ� �ſ� �������� ���ø� �����ݴϴ�.
            // ���� ��� �ÿ��� �� ������ ������Ʈ�� �ʿ信 �°� Ȯ���ϰų�,
            // ���÷���, Ŀ���� ��Ʈ����Ʈ, �Ǵ� SerializedProperty�� ���� �ʵ� Ž�� ������
            // ���� �����ϰ� �����ؾ� �մϴ�.

            // GameManager�� ResponPlayer, ResponEagle, CameraTracker�� �����ϴ� ��� (JSON ���� ���)
            if (obj.GetComponent<GameManager>() != null)
            {
                // GameObject.Find�� �� ��ü�� Ž���ϹǷ� ���ɿ� ������ �� �� ������,
                // �̸��� �ߺ��� ��� ����ġ ���� ����� �ʷ��� �� �ֽ��ϴ�.
                // ���� ��ũ��Ʈ�� public �ʵ带 ���� Inspector���� �����ϴ� ����� ������ Ȯ������ �����Ϸ���
                // SerializedObject�� SerializedProperty�� Ž���ϴ� ���� �� �����մϴ�.

                GameObject responPlayerObj = GameObject.Find("ResponPlayer");
                if (responPlayerObj != null)
                {
                    references.Add(new ReferenceData
                    {
                        fromObjId = obj.GetInstanceID().ToString(),
                        fromComponentName = "GameManager",
                        toObjId = responPlayerObj.GetInstanceID().ToString(),
                        label = "Script Ref responnerPlayer"
                    });
                }

                GameObject responEagleObj = GameObject.Find("ResponEagle");
                if (responEagleObj != null)
                {
                    references.Add(new ReferenceData
                    {
                        fromObjId = obj.GetInstanceID().ToString(),
                        fromComponentName = "GameManager",
                        toObjId = responEagleObj.GetInstanceID().ToString(),
                        label = "Script Ref responnerEagle"
                    });
                }

                GameObject mainCameraObj = GameObject.Find("Main Camera");
                if (mainCameraObj != null)
                {
                    references.Add(new ReferenceData
                    {
                        fromObjId = obj.GetInstanceID().ToString(),
                        fromComponentName = "GameManager",
                        toObjId = mainCameraObj.GetInstanceID().ToString(),
                        label = "Script Ref tracker"
                    });
                }
            }

            // Gun�� Mozzle�� �����ϴ� ��� (JSON ���� ���)
            if (obj.GetComponent<Gun>() != null)
            {
                GameObject mozzleObj = GameObject.Find("Mozzle");
                if (mozzleObj != null)
                {
                    references.Add(new ReferenceData
                    {
                        fromObjId = obj.GetInstanceID().ToString(),
                        fromComponentName = "Gun",
                        toObjId = mozzleObj.GetInstanceID().ToString(),
                        label = "Script Ref trMozzle"
                    });
                }
            }
            // Eagle�� ResponEagle�� �����ϴ� ��� (JSON ���� ���)
            if (obj.GetComponent<Eagle>() != null)
            {
                GameObject responEagleObj = GameObject.Find("ResponEagle");
                if (responEagleObj != null)
                {
                    references.Add(new ReferenceData
                    {
                        fromObjId = obj.GetInstanceID().ToString(),
                        fromComponentName = "Eagle",
                        toObjId = responEagleObj.GetInstanceID().ToString(),
                        label = "Script Ref trResponPoint"
                    });
                }
            }
        }
        Debug.Log($"�ε�� �� ���� ������Ʈ ��: {nodes.Count}, ������ ���� ��: {references.Count}");
    }


    // ����� �Է� �̺�Ʈ (��, ��) ó��
    private void HandleInputEvents()
    {
        Event currentEvent = Event.current; // ���� �̺�Ʈ ��������

        // --- �� (�̵�) ��� ---
        // Ctrl/Cmd Ű�� ���� ���¿��� ���� ���콺 ��ư�� ������ �� ����
        // ����Ƽ �����Ϳ����� Ctrl/Cmd Ű�� ����Ͽ� ĵ���� ���� �ϴ� ���� �Ϲ����Դϴ�.
        if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && currentEvent.modifiers == EventModifiers.Control)
        {
            isPanning = true;
            dragOrigin = currentEvent.mousePosition; // ���� ���콺 ��ġ�� �巡�� ���������� ����
            currentEvent.Use(); // �̺�Ʈ �Һ� (�ٸ� GUI ��Ұ� �� �̺�Ʈ�� ó������ �ʵ���)
        }
        // ���� ���콺 ��ư�� �� �� �� ����
        else if (currentEvent.type == EventType.MouseUp && currentEvent.button == 0 && isPanning)
        {
            isPanning = false;
            currentEvent.Use(); // �̺�Ʈ �Һ�
        }
        // �巡�� ���� �� �� ó��
        else if (currentEvent.type == EventType.MouseDrag && isPanning)
        {
            Vector2 mouseDelta = currentEvent.mousePosition - dragOrigin; // ���콺 �̵���
            panOffset += mouseDelta / zoomLevel; // �� ������ ���� ������ ���� (�� ������ �������� �̵����� �۾���)
            dragOrigin = currentEvent.mousePosition; // �巡�� ������ ������Ʈ (�������� ���� ����)
            Repaint(); // ������ �ٽ� �׸��� ��û
            currentEvent.Use(); // �̺�Ʈ �Һ�
        }

        // --- �� (Ȯ��/���) ��� ---
        if (currentEvent.type == EventType.ScrollWheel)
        {
            // ���콺 ��ġ�� �������� ��
            // �� �� ���콺 ��ġ�� ���� ��ǥ(��/�� ������) �� ��� �ش��ϴ��� ���
            Vector2 mousePosInWorld = (currentEvent.mousePosition / zoomLevel) - panOffset;

            float oldZoom = zoomLevel; // ���� �� ���� ����

            // ��ũ�� �� ���⿡ ���� �� ���� ����
            // currentEvent.delta.y�� ��ũ�� ���⿡ ���� ��� �Ǵ� ���� (Mac������ �ݴ��� �� ����)
            zoomLevel += currentEvent.delta.y * -ZoomSpeed;
            zoomLevel = Mathf.Clamp(zoomLevel, MinZoom, MaxZoom); // �ּ�/�ִ� �� ���� ����

            // �� ��, ���콺�� ȭ���� ������ "���� ��ġ"�� ��� ����Ű���� panOffset ����
            // (��, ���� �߽��� ���콺 Ŀ���� ����)
            panOffset = mousePosInWorld - (mousePosInWorld * (oldZoom / zoomLevel)) + panOffset;

            Repaint(); // ������ �ٽ� �׸��� ��û
            currentEvent.Use(); // �̺�Ʈ �Һ�
        }
    }

    // ���� ���� ���ἱ(����) �׸���
    private void DrawConnections()
    {
        Handles.BeginGUI(); // GUI ��忡�� Handles �׸��� ����
        foreach (var reference in references)
        {
            // ���� �� �� ��� ã��
            if (nodes.TryGetValue(reference.fromObjId, out GraphNode fromNode) &&
                nodes.TryGetValue(reference.toObjId, out GraphNode toNode))
            {
                // ���� ����� �߽� ��ġ ��� (�� �� �� ����)
                Vector2 startPos = new Vector2(
                    (fromNode.Position.center.x + panOffset.x) * zoomLevel,
                    (fromNode.Position.center.y + panOffset.y) * zoomLevel
                );
                // �� ����� �߽� ��ġ ��� (�� �� �� ����)
                Vector2 endPos = new Vector2(
                    (toNode.Position.center.x + panOffset.x) * zoomLevel, // Corrected typo here
                    (toNode.Position.center.y + panOffset.y) * zoomLevel
                );

                Handles.color = Color.grey; // �� ���� ���� (ȸ��)

                // ������ ����� ���ἱ �׸���
                // ������(control points)�� ����Ͽ� � ���� ����
                Handles.DrawBezier(
                    startPos,
                    endPos,
                    startPos + Vector2.right * 50 * zoomLevel, // ���������� ���������� Ƣ����� ������
                    endPos - Vector2.right * 50 * zoomLevel,   // �������� �������� Ƣ����� ������
                    Color.grey, // �� ����
                    null,       // �ؽ�ó (����)
                    2f * zoomLevel // �� �β� (�� ������ ���� ����)
                );

                // ���� ��(label) �׸���
                Vector2 labelPos = Vector2.Lerp(startPos, endPos, 0.5f); // ���� �߰� ����
                GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel); // ���� �� ��Ÿ�� ���
                labelStyle.normal.textColor = Color.black; // �� �ؽ�Ʈ ����
                labelStyle.fontSize = Mathf.RoundToInt(9 * zoomLevel); // �� ��Ʈ ũ�� (�� ������ ���� ����)
                labelStyle.alignment = TextAnchor.MiddleCenter; // �ؽ�Ʈ �߾� ����

                // �� ��ġ�� �� ���� �ణ �̵��Ͽ� ��ġ�� �ʵ��� ����
                labelPos.y -= 10 * zoomLevel;

                // �� �׸��� (�ؽ�Ʈ�� ��ġ, ũ��)
                GUI.Box(new Rect(labelPos.x - 50 * zoomLevel, labelPos.y, 100 * zoomLevel, 20 * zoomLevel), reference.label, labelStyle);
            }
        }
        Handles.EndGUI(); // Handles �׸��� ����
    }
}
