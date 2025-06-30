using UnityEngine; // GameObject, Component, MonoBehaviour 등 Unity 기본 클래스 사용
using UnityEditor; // EditorWindow, MenuItem, EditorUtility, AssetDatabase, Handles 등 에디터 관련 클래스 사용
using System.Collections.Generic; // List, Dictionary 등 컬렉션 사용
using System.Linq; // LINQ 확장 메서드 (예: Select) 사용
using System.IO; // 파일 입출력 사용
using System.Reflection; // 리플렉션을 사용하여 스크립트 필드 검사

public class SceneDiagramExporter : EditorWindow
{
    // JSON 출력을 위한 데이터 구조 정의
    // System.Serializable 어트리뷰트를 사용하여 JsonUtility가 이 클래스들을 직렬화할 수 있도록 합니다.
    [System.Serializable]
    public class DiagramData
    {
        public List<GameObjectData> gameObjects = new List<GameObjectData>(); // 게임 오브젝트 목록
        public List<ReferenceData> references = new List<ReferenceData>();   // 참조 관계 목록
    }

    [System.Serializable]
    public class GameObjectData
    {
        public string id;                   // 다이어그램에서 사용할 고유 ID (예: "player-12345")
        public string name;                 // 게임 오브젝트의 이름
        public List<string> components = new List<string>(); // 포함된 컴포넌트 이름 목록

        // GUI 그리기용 필드 (JSON 직렬화에서는 제외)
        [System.NonSerialized] public Rect guiRect; // 게임 오브젝트 전체 박스의 GUI 위치 및 크기
        [System.NonSerialized] public Dictionary<string, Rect> componentRects = new Dictionary<string, Rect>(); // 각 컴포넌트의 GUI 위치 및 크기 (상대적 아님)
    }

    [System.Serializable] // ReferenceData도 JSON으로 내보내야 하므로 Serializable을 유지합니다.
    public class ReferenceData
    {
        public string fromObjId;         // 참조를 시작하는 게임 오브젝트의 ID
        public string fromComponentName; // 참조를 시작하는 컴포넌트의 이름 (예: "Transform" 또는 "Player (Script)")
        public string toObjId;           // 참조 대상 게임 오브젝트의 ID
        public string label;             // 참조 관계 설명 (예: "Child Of (Transform)" 또는 "Script Ref")

        // GUI 그리기용 필드 (JSON 직렬화에서는 제외)
        [System.NonSerialized] public Vector2 startPos; // 참조 선의 시작점
        [System.NonSerialized] public Vector2 endPos;   // 참조 선의 끝점
    }

    private DiagramData _diagramData; // 씬 데이터를 저장할 변수
    private Vector2 _scrollPosition = Vector2.zero; // 스크롤 뷰 위치
    private float _zoomLevel = 1.0f; // 확대/축소 레벨

    // 다이어그램 그리기 상수
    private const float GO_WIDTH = 200;
    private const float GO_PADDING_X = 15;
    private const float GO_PADDING_TOP = 10;
    private const float COMPONENT_HEIGHT = 25;
    private const float COMPONENT_SPACING = 5;
    private const float GO_SPACING_X = 50; // 게임 오브젝트 간의 가로 간격
    private const float GO_SPACING_Y = 50; // 게임 오브젝트 간의 세로 간격


    // 유니티 에디터 메뉴에 "Tools/Scene Diagram Visualizer" 항목을 추가합니다.
    [MenuItem("Tools/Scene Diagram Visualizer")]
    public static void ShowWindow()
    {
        // 윈도우가 이미 열려있으면 해당 윈도우를 가져오고, 없으면 새로 생성하여 띄웁니다.
        GetWindow<SceneDiagramExporter>("Scene Diagram Visualizer");
    }

    // 윈도우가 활성화될 때 호출됩니다. (초기 데이터 로드)
    private void OnEnable()
    {
        CollectAndLayoutSceneData();
    }

    // 에디터 윈도우의 GUI를 그리는 함수입니다.
    private void OnGUI()
    {
        // 굵은 글씨의 라벨을 표시합니다.
        GUILayout.Label("Unity Scene Diagram Visualizer", EditorStyles.boldLabel);

        // "JSON 클립보드에 복사" 버튼을 생성합니다.
        if (GUILayout.Button("Refresh Diagram (Collect Data & Layout)"))
        {
            CollectAndLayoutSceneData(); // 씬 데이터를 추출하고 레이아웃을 다시 계산합니다.
        }

        // "JSON 클립보드에 복사" 버튼
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

        // "JSON 파일로 저장" 버튼
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

        EditorGUILayout.Space(10); // 간격 추가

        // 다이어그램 그리기 영역 시작
        // GUILayout.ExpandWidth(true)와 GUILayout.ExpandHeight(true)를 사용하여 남은 공간을 모두 채웁니다.
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

        if (_diagramData == null || _diagramData.gameObjects.Count == 0)
        {
            GUILayout.Label("No scene data available. Please click 'Refresh Diagram'.", EditorStyles.centeredGreyMiniLabel);
        }
        else
        {
            // 확대/축소 및 이동 처리 (Event.current는 한 프레임에 한번씩만 처리해야 합니다.)
            HandleZoomAndPan();

            // GUI.matrix를 사용하여 확대/축소 적용
            Matrix4x4 oldMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(new Vector3(-_scrollPosition.x, -_scrollPosition.y, 0), Quaternion.identity, Vector3.one * _zoomLevel);

            // 실제 다이어그램 그리기
            DrawDiagramElements();

            GUI.matrix = oldMatrix; // 원래 GUI 매트릭스로 되돌리기
        }

        EditorGUILayout.EndScrollView();

        // 씬 뷰의 변경 사항을 감지하여 GUI를 다시 그리도록 요청 (옵션, 성능 고려)
        if (GUI.changed)
        {
            Repaint();
        }
    }

    // 씬 데이터를 수집하고 레이아웃을 계산하는 함수
    private void CollectAndLayoutSceneData()
    {
        _diagramData = CollectSceneData(); // 씬 데이터 수집
        CalculateLayout(_diagramData);    // 레이아웃 계산
        Debug.Log("Scene diagram data collected and layout calculated.");
        Repaint(); // 윈도우를 다시 그리도록 요청
    }


    // 씬 데이터를 추출하는 함수 (이전 코드와 유사)
    private DiagramData CollectSceneData()
    {
        DiagramData diagramData = new DiagramData();
        Dictionary<GameObject, string> gameObjectIds = new Dictionary<GameObject, string>();
        Dictionary<string, GameObject> idToGameObjectMap = new Dictionary<string, GameObject>();


        GameObject[] allGameObjects = FindObjectsOfType<GameObject>(true);

        // 첫 번째 순회: 모든 게임 오브젝트와 컴포넌트 정보를 수집
        foreach (GameObject go in allGameObjects)
        {
            string go_id = go.name.Replace(" ", "-").ToLower() + "-" + go.GetInstanceID();
            gameObjectIds[go] = go_id;
            idToGameObjectMap[go_id] = go; // ID -> GameObject 매핑 저장

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

        // 두 번째 순회: 부모-자식 계층 구조와 스크립트 참조를 추가
        foreach (GameObject go in allGameObjects)
        {
            string child_id = gameObjectIds[go];

            // 부모-자식 관계
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

            // 스크립트 (MonoBehaviour) 내의 퍼블릭 변수 참조
            foreach (MonoBehaviour monoBehaviour in go.GetComponents<MonoBehaviour>())
            {
                if (monoBehaviour == null) continue; // 누락된 스크립트 처리

                SerializedObject serializedObject = new SerializedObject(monoBehaviour);
                SerializedProperty property = serializedObject.GetIterator();

                while (property.NextVisible(true)) // 모든 보이는 필드를 순회
                {
                    // ObjectReference 타입의 필드 (GameObject, Component, ScriptableObject 등 참조)
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
                                string fromCompName = monoBehaviour.GetType().Name; // 스크립트 컴포넌트 이름
                                diagramData.references.Add(new ReferenceData
                                {
                                    fromObjId = child_id,
                                    fromComponentName = fromCompName,
                                    toObjId = referencedId,
                                    label = $"Script Ref: {property.name}" // 필드 이름으로 참조 라벨
                                });
                            }
                        }
                    }
                }
            }
        }

        return diagramData;
    }

    // 다이어그램 요소들의 GUI 위치를 계산하는 함수
    private void CalculateLayout(DiagramData data)
    {
        if (data == null || data.gameObjects.Count == 0) return;

        // 게임 오브젝트 ID를 기반으로 GameObjectData를 빠르게 찾기 위한 맵
        Dictionary<string, GameObjectData> goMap = data.gameObjects.ToDictionary(go => go.id);

        // 레이아웃 계산을 위한 임시 변수
        float currentX = GO_SPACING_X;
        float currentY = GO_SPACING_Y;
        float maxYInRow = 0;

        // GameObject와 Component의 Rect 위치를 계산
        foreach (GameObjectData goData in data.gameObjects)
        {
            float goHeight = GO_PADDING_TOP * 2 + goData.components.Count * (COMPONENT_HEIGHT + COMPONENT_SPACING);
            if (goData.components.Count > 0) goHeight -= COMPONENT_SPACING; // 마지막 컴포넌트 후 간격 제외

            goData.guiRect = new Rect(currentX, currentY, GO_WIDTH, goHeight);

            float compY = currentY + GO_PADDING_TOP + 20; // 컴포넌트 시작 Y 위치

            goData.componentRects.Clear(); // 이전 데이터 초기화
            foreach (string compName in goData.components)
            {
                // 컴포넌트의 전역 위치를 저장
                // 여기서 컴포넌트의 rect는 게임 오브젝트의 rect 내부에 상대적으로 그려지지만,
                // 참조를 위한 startPos/endPos 계산을 위해 전역 좌표로 저장
                goData.componentRects[compName] = new Rect(currentX + GO_PADDING_X, compY, GO_WIDTH - (GO_PADDING_X * 2), COMPONENT_HEIGHT);
                compY += COMPONENT_HEIGHT + COMPONENT_SPACING;
            }

            maxYInRow = Mathf.Max(maxYInRow, goHeight);

            currentX += GO_WIDTH + GO_SPACING_X;
            // 간단한 그리드 레이아웃 (예시: 한 줄에 3개씩 배치)
            if (currentX > 3 * (GO_WIDTH + GO_SPACING_X))
            {
                currentX = GO_SPACING_X;
                currentY += maxYInRow + GO_SPACING_Y;
                maxYInRow = 0;
            }
        }

        // 참조 선의 시작점과 끝점 계산
        foreach (ReferenceData refData in data.references)
        {
            Rect fromRect;
            Rect toRect;

            // fromObjId에 해당하는 GameObjectData 찾기
            GameObjectData fromGoData;
            if (goMap.TryGetValue(refData.fromObjId, out fromGoData))
            {
                // fromComponentName이 지정되면 해당 컴포넌트의 위치를 사용
                if (!string.IsNullOrEmpty(refData.fromComponentName) && fromGoData.componentRects.ContainsKey(refData.fromComponentName))
                {
                    fromRect = fromGoData.componentRects[refData.fromComponentName];
                }
                else
                {
                    // 아니면 게임 오브젝트 박스의 위치를 사용
                    fromRect = fromGoData.guiRect;
                }
            }
            else
            {
                Debug.LogWarning($"Cannot find 'from' GameObjectData for ID: {refData.fromObjId}");
                continue;
            }

            // toObjId에 해당하는 GameObjectData 찾기
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

            // 시작점과 끝점 계산 (간단하게 박스 중앙에서 중앙으로 연결)
            // 더 정교한 연결점 계산은 (예: 가장 가까운 모서리) 복잡도를 높일 수 있습니다.
            refData.startPos = fromRect.center;
            refData.endPos = toRect.center;
        }
    }

    // GUI 매트릭스와 스크롤을 고려하여 다이어그램 요소를 그리는 함수
    private void DrawDiagramElements()
    {
        if (_diagramData == null) return;

        // Handles.color는 GUI.color와는 별개입니다.
        // Handles를 사용하여 선을 그릴 것이므로 Handles.color를 설정합니다.
        Handles.color = new Color(0.443f, 0.502f, 0.588f, 1.0f); // Gray-ish blue for lines

        // 참조 선 그리기 (게임 오브젝트 박스 위에 그려지도록 먼저 그립니다.)
        foreach (ReferenceData refData in _diagramData.references)
        {
            // 이 위치는 이미 확대/축소 및 스크롤이 적용된 GUI.matrix 컨텍스트 내의 전역 좌표입니다.
            Vector3 start = refData.startPos;
            Vector3 end = refData.endPos;

            // 선 그리기
            Handles.DrawLine(start, end);

            // 화살표 머리 그리기 (간단한 삼각형)
            Vector2 direction = (end - start).normalized;
            Vector2 perpendicular = new Vector2(-direction.y, direction.x);
            float arrowSize = 10f;

            Vector3[] arrowHeadPoints = new Vector3[3];
            arrowHeadPoints[0] = end;
            arrowHeadPoints[1] = end - (Vector3)direction * arrowSize + (Vector3)perpendicular * (arrowSize / 2);
            arrowHeadPoints[2] = end - (Vector3)direction * arrowSize - (Vector3)perpendicular * (arrowSize / 2);
            Handles.DrawAAConvexPolygon(arrowHeadPoints);

            // 참조 라벨 그리기 (선 중간에 위치)
            if (!string.IsNullOrEmpty(refData.label))
            {
                Vector2 labelPos = (refData.startPos + refData.endPos) / 2;
                // 라벨 위치를 선의 방향에 따라 살짝 오프셋
                labelPos += perpendicular * 10;

                // GUI.Label은 현재 GUI.matrix의 영향을 받습니다.
                GUI.Label(new Rect(labelPos.x - 50, labelPos.y - 10, 100, 20), refData.label, EditorStyles.miniLabel);
            }
        }

        // 게임 오브젝트와 컴포넌트 그리기 (참조 선 위에 그려지도록 나중에 그립니다.)
        foreach (GameObjectData goData in _diagramData.gameObjects)
        {
            // 게임 오브젝트 박스
            // GUI.Box는 현재 GUI.matrix의 영향을 받습니다.
            GUI.Box(goData.guiRect, goData.name, GUI.skin.box);

            // 컴포넌트 그리기
            float currentCompY = goData.guiRect.y + GO_PADDING_TOP + 20;
            foreach (string compName in goData.components)
            {
                Rect compRect = new Rect(goData.guiRect.x + GO_PADDING_X, currentCompY, GO_WIDTH - (GO_PADDING_X * 2), COMPONENT_HEIGHT);
                GUI.Box(compRect, compName, GUI.skin.textArea); // 컴포넌트는 textArea 스타일로 다르게 표시

                currentCompY += COMPONENT_HEIGHT + COMPONENT_SPACING;
            }

            // 클릭 감지 및 오브젝트 선택
            Event currentEvent = Event.current;
            if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && goData.guiRect.Contains(currentEvent.mousePosition))
            {
                // 다이어그램 ID에서 GameObject 인스턴스 ID를 추출합니다.
                // 이 방법은 go_id 생성 방식에 따라 달라질 수 있습니다.
                // 여기서는 go_id가 "이름-인스턴스ID" 형태이므로 인스턴스 ID만 추출합니다.
                string instanceIdStr = goData.id.Split('-').LastOrDefault();
                if (int.TryParse(instanceIdStr, out int instanceId))
                {
                    GameObject selectedGo = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
                    if (selectedGo != null)
                    {
                        Selection.activeGameObject = selectedGo; // Hierarchy에서 해당 오브젝트를 선택
                        EditorGUIUtility.PingObject(selectedGo); // 프로젝트 뷰에서 해당 오브젝트를 깜빡이게 함
                        currentEvent.Use(); // 이벤트 처리 완료
                    }
                }
            }
        }
    }

    // 확대/축소 및 화면 이동 처리
    private void HandleZoomAndPan()
    {
        Event currentEvent = Event.current;

        // 마우스 휠로 확대/축소
        if (currentEvent.type == EventType.ScrollWheel)
        {
            float zoomDelta = -currentEvent.delta.y * 0.05f; // 휠 방향에 따라 확대/축소
            _zoomLevel += zoomDelta;
            _zoomLevel = Mathf.Clamp(_zoomLevel, 0.5f, 2.0f); // 확대/축소 범위 제한

            // 확대/축소 시 마우스 위치 기준으로 스크롤 조정 (선택 사항, 더 자연스러운 확대/축소를 위함)
            Vector2 mousePos = currentEvent.mousePosition;
            Vector2 adjustedMousePos = mousePos / _zoomLevel; // 현재 확대 레벨을 고려한 마우스 위치
            _scrollPosition = adjustedMousePos - (mousePos - _scrollPosition);


            currentEvent.Use(); // 이벤트 처리 완료
        }
        // 마우스 드래그로 화면 이동 (가운데 버튼 또는 Alt+좌클릭)
        else if (currentEvent.type == EventType.MouseDrag && (currentEvent.button == 2 || (currentEvent.button == 0 && currentEvent.alt)))
        {
            _scrollPosition -= currentEvent.delta / _zoomLevel; // 확대 레벨에 따라 이동 속도 조정
            currentEvent.Use(); // 이벤트 처리 완료
        }
    }
}
