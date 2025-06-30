using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq; // FindObjectsOfType<T>() 반환 타입을 편리하게 다루기 위해
using UnityEngine.SceneManagement; // SceneManager를 사용하여 현재 씬의 GameObject에 접근

// 이 에디터 확장은 유니티의 기본 API를 사용하여 씬 데이터를 파싱합니다.

// --- JSON 데이터 구조 (내부 데이터 모델로 사용) ---
// GameObject 데이터를 나타내는 클래스
[Serializable]
public class GameObjectData
{
    public string id; // 고유 ID (GetInstanceID() 사용)
    public string name; // 게임 오브젝트 이름
    public List<string> components; // 컴포넌트 목록
}

// 참조(연결선) 데이터를 나타내는 클래스
[Serializable]
public class ReferenceData
{
    public string fromObjId; // 시작 오브젝트의 ID
    public string fromComponentName; // 시작 오브젝트의 컴포넌트 이름 (표시에 활용될 수 있음)
    public string toObjId; // 끝 오브젝트의 ID
    public string label; // 연결선의 라벨 (예: "Child Of", "Script Ref")
}

// 이전에 JSON 파싱을 위해 사용되었던 UnityProjectData와 그 래퍼 클래스는
// 이제 씬 데이터를 직접 로드하므로 필요하지 않지만,
// 이전 코드와의 일관성을 위해 주석으로 남겨둡니다.
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

// --- 에디터 내의 노드 표현 ---
// 그래프 노드를 나타내는 헬퍼 클래스
public class GraphNode
{
    public GameObjectData Data { get; private set; } // 원본 GameObject 데이터
    public Rect Position { get; set; } // 노드 창의 현재 위치와 크기
    public string ID => Data.id; // 노드의 고유 ID (GameObjectData.id와 동일)

    public GraphNode(GameObjectData data, Vector2 initialPosition, Vector2 size)
    {
        Data = data;
        Position = new Rect(initialPosition, size);
    }

    // 노드 창을 그리는 메서드 (OnGUI 루프에서 호출됨)
    public void Draw(int windowID, Vector2 panOffset, float zoomLevel)
    {
        // 팬 및 줌 레벨을 노드의 위치와 크기에 적용하여 그릴 Rect 계산
        Rect zoomedPosition = new Rect(
            (Position.x + panOffset.x) * zoomLevel,
            (Position.y + panOffset.y) * zoomLevel,
            Position.width * zoomLevel,
            Position.height * zoomLevel
        );

        // 폰트 크기 및 색상 조절 (줌 레벨에 따라)
        GUI.skin.window.fontSize = Mathf.RoundToInt(12 * zoomLevel);
        GUI.skin.label.fontSize = Mathf.RoundToInt(10 * zoomLevel);
        GUI.skin.label.normal.textColor = Color.black; // 텍스트 가시성 확보

        // GUI.Window를 사용하여 노드 창을 그리고, 드래그 시 Position을 업데이트합니다.
        // GUI.Window는 드래그 시 rect를 직접 업데이트하므로,
        // 드래그된 줌/팬이 적용된 위치를 다시 내부적인 줌/팬되지 않은 좌표로 변환해야 합니다.
        Rect updatedZoomePos = GUI.Window(windowID, zoomedPosition, DrawNodeWindow, Data.name);

        // GUI.Window에서 반환된 (줌/팬 적용된) 위치를 내부적인 (줌/팬되지 않은) 위치로 변환
        Position = new Rect(
            (updatedZoomePos.x / zoomLevel) - panOffset.x,
            (updatedZoomePos.y / zoomLevel) - panOffset.y,
            updatedZoomePos.width / zoomLevel,
            updatedZoomePos.height / zoomLevel
        );
    }

    // 노드 창의 내용을 그리는 콜백 메서드
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
        GUI.DragWindow(); // 이 호출이 없으면 창이 드래그되지 않습니다.
    }
}

// --- 메인 에디터 윈도우 클래스 ---
public class GoogleDrawingLikeEditor : EditorWindow
{
    // 모든 그래프 노드를 저장하는 Dictionary (ID로 빠르게 접근)
    private Dictionary<string, GraphNode> nodes = new Dictionary<string, GraphNode>();
    // 참조 데이터 리스트
    private List<ReferenceData> references = new List<ReferenceData>();

    // 캔버스 팬(이동) 관련 변수
    private Vector2 panOffset = Vector2.zero; // 현재 캔버스 이동 오프셋
    private Vector2 dragOrigin; // 팬 시작 시 마우스 위치
    private bool isPanning = false; // 팬 모드 여부

    // 캔버스 줌(확대/축소) 관련 변수
    private float zoomLevel = 1.0f; // 현재 줌 레벨
    private const float MinZoom = 0.2f; // 최소 줌 레벨
    private const float MaxZoom = 2.0f; // 최대 줌 레벨
    private const float ZoomSpeed = 0.05f; // 줌 속도

    // 노드 기본 크기
    private Vector2 defaultNodeSize = new Vector2(200, 100);

    // 유니티 메뉴에 에디터 윈도우 추가
    [MenuItem("Window/Google Drawing-like Editor")]
    public static void ShowWindow()
    {
        GetWindow<GoogleDrawingLikeEditor>("Google Drawing Editor"); // 윈도우 생성 및 타이틀 설정
    }

    // 윈도우가 활성화될 때 호출됨 (초기화)
    private void OnEnable()
    {
        // 씬 변경 감지 이벤트 등록: 계층 구조 변경 시 (오브젝트 생성/삭제/부모 변경 등)
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
        // 씬 열기/닫기/저장 등 씬 관련 변경 감지 이벤트 등록 (선택적)
        // EditorSceneManager.sceneOpened += OnSceneOpened; // 필요한 경우 추가
        LoadSceneData(); // 씬 데이터 로드
    }

    // 윈도우가 비활성화될 때 호출됨
    private void OnDisable()
    {
        // 씬 변경 감지 이벤트 해제 (메모리 누수 방지)
        EditorApplication.hierarchyChanged -= OnHierarchyChanged;
        // EditorSceneManager.sceneOpened -= OnSceneOpened; // 필요한 경우 해제
    }

    // 계층 구조 변경 시 호출되는 콜백
    private void OnHierarchyChanged()
    {
        Debug.Log("Hierarchy changed. Reloading scene data.");
        LoadSceneData(); // 씬 데이터 다시 로드
        Repaint(); // 윈도우 다시 그리기 요청
    }

    // 윈도우의 GUI를 그리는 핵심 메서드
    private void OnGUI()
    {
        // 입력 이벤트 처리 (팬 및 줌)
        HandleInputEvents();

        // GUI 그리기 매트릭스를 저장하여 줌 및 팬 적용 전 상태로 복원할 수 있도록 함
        Matrix4x4 oldGuiMatrix = GUI.matrix;

        // 줌 적용: 원점을 기준으로 스케일 조정 (0,0은 에디터 윈도우의 좌상단)
        // 이 매트릭스 변환은 모든 GUI.Window 및 Handles.DrawXX 호출에 영향을 줍니다.
        GUIUtility.ScaleAroundPivot(Vector2.one * zoomLevel, Vector2.zero);
        GUI.matrix = oldGuiMatrix * Matrix4x4.Scale(new Vector3(zoomLevel, zoomLevel, 1f));


        // 모든 노드 그리기
        foreach (var nodeEntry in nodes)
        {
            GraphNode node = nodeEntry.Value;
            // 각 노드의 Draw 메서드에 팬 오프셋과 줌 레벨을 전달하여 노드별 위치 계산에 사용
            node.Draw(node.ID.GetHashCode(), panOffset, zoomLevel);
        }

        // 연결선(참조) 그리기
        DrawConnections();

        // GUI 매트릭스를 원래 상태로 복원
        GUI.matrix = oldGuiMatrix;
    }

    // 현재 유니티 씬의 데이터를 로드하고 노드 및 참조 데이터를 초기화
    private void LoadSceneData()
    {
        nodes.Clear(); // 기존 노드 데이터 비우기
        references.Clear(); // 기존 참조 데이터 비우기

        // 현재 활성화된 씬 가져오기
        Scene activeScene = SceneManager.GetActiveScene();
        if (!activeScene.isLoaded)
        {
            Debug.LogWarning("현재 로드된 씬이 없습니다. 씬을 로드하거나 새로 생성해주세요.");
            return;
        }

        // 씬 내의 모든 GameObject 찾기
        // FindObjectsOfType<GameObject>()는 활성화된 오브젝트만 찾습니다.
        // 비활성화된 오브젝트까지 포함하려면 SceneManager.GetActiveScene().GetRootGameObjects()를 시작으로
        // 계층 구조를 재귀적으로 탐색해야 합니다. 여기서는 편의상 FindObjectsOfType을 사용합니다.
        GameObject[] allGameObjects = FindObjectsOfType<GameObject>();

        // 노드 초기화 및 초기 배치
        Vector2 currentPosition = new Vector2(20, 20); // 초기 노드 배치 시작 위치
        int nodesPerRow = 5; // 한 줄에 표시할 노드 수
        float nodeSpacingX = defaultNodeSize.x + 30; // 노드 간 가로 간격
        float nodeSpacingY = defaultNodeSize.y + 30; // 노드 간 세로 간격
        int nodeCount = 0;

        foreach (GameObject obj in allGameObjects)
        {
            if (obj == null) continue; // 유효하지 않은 오브젝트 건너뛰기 (씬 변경 등으로 인해 발생 가능)

            // GameObjectData 생성 (시각화 모델)
            GameObjectData goData = new GameObjectData
            {
                id = obj.GetInstanceID().ToString(), // 유니티의 고유 인스턴스 ID를 노드 ID로 사용
                name = obj.name,
                components = new List<string>()
            };

            // 모든 컴포넌트 이름 가져오기
            // Missing (스크립트가 없는) 컴포넌트는 null일 수 있으므로 null 체크
            foreach (Component comp in obj.GetComponents<Component>())
            {
                if (comp != null)
                {
                    goData.components.Add(comp.GetType().Name);
                }
                else
                {
                    goData.components.Add("Missing Script (컴포넌트 누락)");
                }
            }

            // GraphNode 생성 및 딕셔너리에 추가
            GraphNode newNode = new GraphNode(goData, currentPosition, defaultNodeSize);
            nodes[goData.id] = newNode;

            // 그리드 레이아웃 업데이트 (초기 배치용)
            currentPosition.x += nodeSpacingX;
            nodeCount++;
            if (nodeCount % nodesPerRow == 0)
            {
                currentPosition.x = 20;
                currentPosition.y += nodeSpacingY;
            }

            // --- 참조 (references) 추가 ---

            // 1. 부모-자식 관계 (Transform 계층)
            if (obj.transform.parent != null)
            {
                references.Add(new ReferenceData
                {
                    fromObjId = obj.GetInstanceID().ToString(), // 자식 오브젝트
                    fromComponentName = "Transform",
                    toObjId = obj.transform.parent.gameObject.GetInstanceID().ToString(), // 부모 오브젝트
                    label = "Child Of (Transform)"
                });
            }

            // 2. 스크립트 참조 (Script Ref):
            // 유니티 에디터에서 스크립트 간의 참조를 일반적이고 견고하게 감지하는 것은 복잡합니다.
            // 여기서는 `unityJson.json` 예시에서 나타난 몇 가지 특정 컴포넌트 간의 참조를
            // "이름 기반"으로 찾아서 추가하는 매우 제한적인 예시를 보여줍니다.
            // 실제 사용 시에는 이 로직을 프로젝트의 필요에 맞게 확장하거나,
            // 리플렉션, 커스텀 애트리뷰트, 또는 SerializedProperty를 통한 필드 탐색 등으로
            // 더욱 정교하게 구현해야 합니다.

            // GameManager가 ResponPlayer, ResponEagle, CameraTracker를 참조하는 경우 (JSON 예시 기반)
            if (obj.GetComponent<GameManager>() != null)
            {
                // GameObject.Find는 씬 전체를 탐색하므로 성능에 영향을 줄 수 있으며,
                // 이름이 중복될 경우 예상치 못한 결과를 초래할 수 있습니다.
                // 실제 스크립트의 public 필드를 직접 Inspector에서 설정하는 방식을 에디터 확장으로 재현하려면
                // SerializedObject와 SerializedProperty를 탐색하는 것이 더 적합합니다.

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

            // Gun이 Mozzle을 참조하는 경우 (JSON 예시 기반)
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
            // Eagle이 ResponEagle을 참조하는 경우 (JSON 예시 기반)
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
        Debug.Log($"로드된 씬 게임 오브젝트 수: {nodes.Count}, 생성된 참조 수: {references.Count}");
    }


    // 사용자 입력 이벤트 (팬, 줌) 처리
    private void HandleInputEvents()
    {
        Event currentEvent = Event.current; // 현재 이벤트 가져오기

        // --- 팬 (이동) 기능 ---
        // Ctrl/Cmd 키를 누른 상태에서 왼쪽 마우스 버튼을 누르면 팬 시작
        // 유니티 에디터에서는 Ctrl/Cmd 키를 사용하여 캔버스 팬을 하는 것이 일반적입니다.
        if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && currentEvent.modifiers == EventModifiers.Control)
        {
            isPanning = true;
            dragOrigin = currentEvent.mousePosition; // 현재 마우스 위치를 드래그 시작점으로 설정
            currentEvent.Use(); // 이벤트 소비 (다른 GUI 요소가 이 이벤트를 처리하지 않도록)
        }
        // 왼쪽 마우스 버튼을 뗄 때 팬 종료
        else if (currentEvent.type == EventType.MouseUp && currentEvent.button == 0 && isPanning)
        {
            isPanning = false;
            currentEvent.Use(); // 이벤트 소비
        }
        // 드래그 중일 때 팬 처리
        else if (currentEvent.type == EventType.MouseDrag && isPanning)
        {
            Vector2 mouseDelta = currentEvent.mousePosition - dragOrigin; // 마우스 이동량
            panOffset += mouseDelta / zoomLevel; // 줌 레벨에 맞춰 오프셋 조정 (줌 레벨이 높을수록 이동량이 작아짐)
            dragOrigin = currentEvent.mousePosition; // 드래그 시작점 업데이트 (연속적인 팬을 위해)
            Repaint(); // 윈도우 다시 그리기 요청
            currentEvent.Use(); // 이벤트 소비
        }

        // --- 줌 (확대/축소) 기능 ---
        if (currentEvent.type == EventType.ScrollWheel)
        {
            // 마우스 위치를 기준으로 줌
            // 줌 전 마우스 위치가 월드 좌표(팬/줌 미적용) 상 어디에 해당하는지 계산
            Vector2 mousePosInWorld = (currentEvent.mousePosition / zoomLevel) - panOffset;

            float oldZoom = zoomLevel; // 이전 줌 레벨 저장

            // 스크롤 휠 방향에 따라 줌 레벨 조정
            // currentEvent.delta.y는 스크롤 방향에 따라 양수 또는 음수 (Mac에서는 반대일 수 있음)
            zoomLevel += currentEvent.delta.y * -ZoomSpeed;
            zoomLevel = Mathf.Clamp(zoomLevel, MinZoom, MaxZoom); // 최소/최대 줌 레벨 제한

            // 줌 후, 마우스가 화면의 동일한 "월드 위치"를 계속 가리키도록 panOffset 조정
            // (즉, 줌의 중심을 마우스 커서로 만듦)
            panOffset = mousePosInWorld - (mousePosInWorld * (oldZoom / zoomLevel)) + panOffset;

            Repaint(); // 윈도우 다시 그리기 요청
            currentEvent.Use(); // 이벤트 소비
        }
    }

    // 노드들 간의 연결선(참조) 그리기
    private void DrawConnections()
    {
        Handles.BeginGUI(); // GUI 모드에서 Handles 그리기 시작
        foreach (var reference in references)
        {
            // 시작 및 끝 노드 찾기
            if (nodes.TryGetValue(reference.fromObjId, out GraphNode fromNode) &&
                nodes.TryGetValue(reference.toObjId, out GraphNode toNode))
            {
                // 시작 노드의 중심 위치 계산 (팬 및 줌 적용)
                Vector2 startPos = new Vector2(
                    (fromNode.Position.center.x + panOffset.x) * zoomLevel,
                    (fromNode.Position.center.y + panOffset.y) * zoomLevel
                );
                // 끝 노드의 중심 위치 계산 (팬 및 줌 적용)
                Vector2 endPos = new Vector2(
                    (toNode.Position.center.x + panOffset.x) * zoomLevel, // Corrected typo here
                    (toNode.Position.center.y + panOffset.y) * zoomLevel
                );

                Handles.color = Color.grey; // 선 색상 설정 (회색)

                // 베지어 곡선으로 연결선 그리기
                // 제어점(control points)을 사용하여 곡선 형태 조절
                Handles.DrawBezier(
                    startPos,
                    endPos,
                    startPos + Vector2.right * 50 * zoomLevel, // 시작점에서 오른쪽으로 튀어나오는 제어점
                    endPos - Vector2.right * 50 * zoomLevel,   // 끝점에서 왼쪽으로 튀어나오는 제어점
                    Color.grey, // 선 색상
                    null,       // 텍스처 (없음)
                    2f * zoomLevel // 선 두께 (줌 레벨에 따라 조절)
                );

                // 참조 라벨(label) 그리기
                Vector2 labelPos = Vector2.Lerp(startPos, endPos, 0.5f); // 선의 중간 지점
                GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel); // 작은 라벨 스타일 사용
                labelStyle.normal.textColor = Color.black; // 라벨 텍스트 색상
                labelStyle.fontSize = Mathf.RoundToInt(9 * zoomLevel); // 라벨 폰트 크기 (줌 레벨에 따라 조절)
                labelStyle.alignment = TextAnchor.MiddleCenter; // 텍스트 중앙 정렬

                // 라벨 위치를 선 위로 약간 이동하여 겹치지 않도록 조정
                labelPos.y -= 10 * zoomLevel;

                // 라벨 그리기 (텍스트와 위치, 크기)
                GUI.Box(new Rect(labelPos.x - 50 * zoomLevel, labelPos.y, 100 * zoomLevel, 20 * zoomLevel), reference.label, labelStyle);
            }
        }
        Handles.EndGUI(); // Handles 그리기 종료
    }
}
