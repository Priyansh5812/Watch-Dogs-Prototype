using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;


[CustomEditor(typeof(WarpAsset))]
public class WarpAssetEditor : Editor
{
    SerializedProperty clip;
    SerializedProperty start;
    SerializedProperty end;
    SerializedProperty preview;
    SerializedProperty previewPrefab;

    const float TimelineHeight = 40f;
    const float MarkerWidth = 10f;

    PreviewRenderUtility renderUtil;
    GameObject previewObject;
    Transform cameraAnchor;
    Vector3 anchorPosition = new Vector3(2.2f, 2f, 3.6f);
    float startDistance = 2.0f;
    Vector3 finalPosition;
    Quaternion finalRotation;
    float zoomSpeed = 0.15f;
    float panSpeed = 0.01f;
    float rotateSpeed = 0.25f;
    void OnEnable()
    {
        clip = serializedObject.FindProperty("TargetClip");
        start = serializedObject.FindProperty("Start");
        end = serializedObject.FindProperty("End");
        preview = serializedObject.FindProperty("PreviewTime");
        previewPrefab = serializedObject.FindProperty("PreviewPrefab");

        renderUtil = new();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        GUILayout.Space(10);
        EditorGUILayout.PropertyField(clip);
        EditorGUILayout.PropertyField(previewPrefab);
        GUILayout.BeginVertical();
        GUILayout.Space(20);
        GUILayout.EndVertical();
        if (clip.objectReferenceValue != null)
        {
            DrawTimeline();
        }


        EnsurePreviewEditor();
        UpdatePreviewedTargetAnimation();
        serializedObject.ApplyModifiedProperties();
    }


    // ------------------------------------------------------------------------
    // Preview Scene Initialization
    // ------------------------------------------------------------------------

    void EnsurePreviewEditor()
    {   
        if(previewObject != null)
            return;
        GameObject targetObject = previewPrefab.objectReferenceValue as GameObject;
        previewObject = renderUtil.InstantiatePrefabInScene(targetObject);
        previewObject.hideFlags = HideFlags.HideAndDontSave;
        foreach (var r in previewObject.GetComponentsInChildren<Renderer>())
        {
            Debug.Log(r.sharedMaterial.shader.name);
        }
        
        var ground = new GameObject("Ground");
        var filter = ground.AddComponent<MeshFilter>();
        filter.mesh = Resources.GetBuiltinResource<Mesh>("Quad.fbx");
        var rend = ground.AddComponent<MeshRenderer>();
        rend.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        ground.transform.forward = Vector3.down;
        ground.transform.localScale = Vector3.one * 15f;
        SceneManager.MoveGameObjectToScene(ground,renderUtil.camera.scene);
        cameraAnchor = new GameObject("Camera Anchor").transform;
        SceneManager.MoveGameObjectToScene(cameraAnchor.gameObject,renderUtil.camera.scene);
        cameraAnchor.position = new Vector3(2.89f, 1.34f, 3.42f);
        cameraAnchor.rotation = Quaternion.Euler(13.90f, 218.68f, 0.00f);
        renderUtil.camera.transform.forward = cameraAnchor.forward;
        renderUtil.camera.transform.position = finalPosition = cameraAnchor.position - cameraAnchor.forward * startDistance;
    }

    public override bool HasPreviewGUI()
    {
        return previewObject != null && renderUtil != null;
    }


    public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
    {   
        r.width = Mathf.Min(r.width , 1920); // Clamping the size makes i
        r.height = Mathf.Min(r.height , 1080);
        renderUtil.BeginPreview(r, background);
        renderUtil.camera.farClipPlane = 100f;
        UpdateInteraction();
        renderUtil.camera.transform.position = finalPosition;
        renderUtil.camera.renderingPath = RenderingPath.UsePlayerSettings;
        renderUtil.camera.allowHDR = true;
        renderUtil.camera.allowMSAA = true;
        renderUtil.lights[0].intensity = 1.2f;
        renderUtil.lights[1].intensity = 1.2f;
        renderUtil.Render(
        allowScriptableRenderPipeline: true,
        updatefov: false);
        GUI.DrawTexture(r, renderUtil.EndPreview(), ScaleMode.StretchToFill, false);
        DebugMe();
    }

    /// <summary>
    /// Prints the current camera anchor transform for debugging.
    /// Remove or wrap in conditional compilation when no longer needed.
    /// </summary>
    public void DebugMe()
    {
        Debug.Log(cameraAnchor.transform.position);
        Debug.Log(cameraAnchor.transform.eulerAngles);
    }

    // ------------------------------------------------------------------------
    // Preview Camera Controls
    // ------------------------------------------------------------------------

void UpdateInteraction()
{
    Event e = Event.current;

    switch (e.type)
    {
        case EventType.ScrollWheel:
        {
            startDistance += e.delta.y * zoomSpeed;
            startDistance = Mathf.Clamp(startDistance, 2f, 35f);

            ComputePosition(0f, 0f);

            e.Use();
            Repaint();
            break;
        }

        case EventType.MouseDrag:
        {
            if (e.button == 1)         // Right Mouse -> Orbit
            {
                ComputeRotation(e.delta.x, e.delta.y);
                ComputePosition(0f, 0f);
            }
            else if (e.button == 0)    // Left Mouse -> Pan
            {
                ComputePosition(e.delta.x, e.delta.y);
            }

            e.Use();
            Repaint();
            break;
        }
    }
}

void ComputePosition(float deltaX, float deltaY)
{
    cameraAnchor.position +=
        (-cameraAnchor.right * deltaX +
          cameraAnchor.up    * deltaY) * panSpeed;

    finalPosition =
        cameraAnchor.position -
        cameraAnchor.forward * startDistance;
}

void ComputeRotation(float deltaX, float deltaY)
{
    cameraAnchor.Rotate(Vector3.up, deltaX * rotateSpeed, Space.World);
    cameraAnchor.Rotate(Vector3.right, deltaY * rotateSpeed, Space.Self);
    renderUtil.camera.transform.forward = cameraAnchor.forward;
}

    void UpdatePreviewedTargetAnimation()
    {   
        if(previewObject == null)
            return;

        AnimationClip clip = this.clip.objectReferenceValue as AnimationClip;
        float targetPreviewTime = preview.floatValue * clip.length;

        clip.SampleAnimation(previewObject , targetPreviewTime);
    }

    // ------------------------------------------------------------------------
    // Timeline UI
    // ------------------------------------------------------------------------

    void DrawTimeline()
    {
        AnimationClip animation = clip.objectReferenceValue as AnimationClip;

        Rect rect = GUILayoutUtility.GetRect(
        0f,
        TimelineHeight,
        GUILayout.ExpandWidth(true));
        DrawBackground(rect);
        DrawTicks(rect, animation.length);
        DrawSelection(rect);
        DrawMarkers(rect);
        HandleEvents(rect);
        EditorUtility.SetDirty(target);
    }

    void DrawBackground(Rect rect)
    {
        EditorGUI.DrawRect(rect, new Color(.18f,.18f,.18f));

        Rect line =
            new Rect(rect.x,
                     rect.center.y,
                     rect.width,
                     2);

        EditorGUI.DrawRect(line, Color.gray);
    }

    void DrawTicks(Rect rect, float length)
    {
        Handles.color = Color.gray;

        const int MajorTicks = 10;

        for(int i=0;i<=MajorTicks;i++)
        {
            float t = i / (float)MajorTicks;

            float x = Mathf.Lerp(rect.x, rect.xMax, t);

            Handles.DrawLine(
                new Vector2(x, rect.y + 18),
                new Vector2(x, rect.y + 28));

            string label =
                (length * t).ToString("0.00");

            GUI.Label(
                new Rect(x-18, rect.y, 40,18),
                label,
                EditorStyles.miniLabel);

            if(i == MajorTicks)
                continue;

            for(int j=1;j<5;j++)
            {
                float minor =
                    (i + j/5f) / MajorTicks;

                float mx =
                    Mathf.Lerp(rect.x, rect.xMax, minor);

                Handles.DrawLine(
                    new Vector2(mx, rect.y+22),
                    new Vector2(mx, rect.y+28));
            }
        }
    }

    void DrawSelection(Rect rect)
    {
        float sx =
            Mathf.Lerp(rect.x, rect.xMax, start.floatValue);

        float ex =
            Mathf.Lerp(rect.x, rect.xMax, end.floatValue);

        Rect selection =
            new Rect(
                sx,
                rect.center.y-3,
                ex-sx,
                12);

        EditorGUI.DrawRect(
            selection,
            new Color(.2f,.6f,1f,.3f));
    }

    void DrawMarkers(Rect rect)
    {
        DrawMarker(rect,
            start.floatValue,
            Color.green);

        DrawMarker(rect,
            end.floatValue,
            Color.red);

        DrawMarker(rect,
            preview.floatValue,
            Color.white, true);
    }

    void DrawMarker(Rect rect,float t,Color color, bool isPreview = false)
    {
        float x =
            Mathf.Lerp(rect.x,rect.xMax,t);

        Handles.color = color;

        Handles.DrawLine(
            new Vector2(x,!isPreview ? rect.y+16 : rect.y),
            new Vector2(x,rect.yMax));

        Handles.DrawSolidDisc(new Vector2(x,!isPreview ? rect.y+12 : rect.y+6),Vector3.forward,MarkerWidth*0.75f);
    }

    int activeHandle = -1;

    const int None = -1;
    const int StartHandle = 0;
    const int EndHandle = 1;    
    const int PreviewHandle = 2;

    void HandleEvents(Rect rect)
    {
        Event e = Event.current;

        float sx = Mathf.Lerp(rect.x, rect.xMax, start.floatValue);
        float ex = Mathf.Lerp(rect.x, rect.xMax, end.floatValue);
        float px = Mathf.Lerp(rect.x, rect.xMax, preview.floatValue);

        Rect startRect = new Rect(sx - 6, rect.y, 12, rect.height);
        Rect endRect = new Rect(ex - 6, rect.y, 12, rect.height);
        Rect previewRect = new Rect(px - 6, rect.y, 12, rect.height);

        int id = GUIUtility.GetControlID(FocusType.Passive);

        switch (e.GetTypeForControl(id))
        {
            case EventType.MouseDown:

                if (e.button != 0)
                    break;

                if (previewRect.Contains(e.mousePosition))
                    activeHandle = PreviewHandle;
                else if (endRect.Contains(e.mousePosition))
                    activeHandle = EndHandle;
                else if (startRect.Contains(e.mousePosition))
                    activeHandle = StartHandle;
                else
                    break;

                GUIUtility.hotControl = id;
                e.Use();
                break;

            case EventType.MouseDrag:

                if (GUIUtility.hotControl != id)
                    break;

                float t = Mathf.Clamp01(
                    Mathf.InverseLerp(rect.x, rect.xMax, e.mousePosition.x));

                switch (activeHandle)
                {
                    case StartHandle:
                        start.floatValue = Mathf.Min(t, end.floatValue);
                        break;

                    case EndHandle:
                        end.floatValue = Mathf.Max(t, start.floatValue);
                        break;

                    case PreviewHandle:
                        preview.floatValue = t;
                        break;
                }

                serializedObject.ApplyModifiedProperties();
                Repaint();

                e.Use();
                break;

            case EventType.MouseUp:

                if (GUIUtility.hotControl != id)
                    break;

                GUIUtility.hotControl = 0;
                activeHandle = None;

                e.Use();
                break;
        }

    }

    // ------------------------------------------------------------------------
    // Cleanup
    // ------------------------------------------------------------------------

    void OnDisable()
    {   
        if(renderUtil != null)
        {
            renderUtil.Cleanup();
            renderUtil = null;
        }

        if(previewObject != null)
        {    
            DestroyImmediate(previewObject);
            previewObject = null;
        }
        
        if(cameraAnchor != null)
        {    
            DestroyImmediate(cameraAnchor);
            cameraAnchor = null;
        }
    }
}