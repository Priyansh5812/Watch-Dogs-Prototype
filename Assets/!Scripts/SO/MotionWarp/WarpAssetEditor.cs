#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;


[CustomEditor(typeof(WarpAsset))]
public class WarpAssetEditor : Editor
{
    SerializedProperty clip;
    SerializedProperty preview;
    SerializedProperty previewPrefab;
    SerializedProperty windows;
    SerializedProperty sampleRate;

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
        preview = serializedObject.FindProperty("PreviewTime");
        previewPrefab = serializedObject.FindProperty("PreviewPrefab");
        sampleRate = serializedObject.FindProperty("SampleRate");
        windows = serializedObject.FindProperty("windows");
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
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(windows, true);
            if (EditorGUI.EndChangeCheck())
                Repaint();
            EditorGUILayout.PropertyField(sampleRate);
            GUILayout.Space(20);
            GUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Space(20);
            EditorGUILayout.EndHorizontal();
        }

        if(GUILayout.Button("Refresh Authored Values"))
        {
            RefreshAuthoredValues();
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
    }

    /// <summary>
    /// Prints the current camera anchor transform for debugging.
    /// Remove or wrap in conditional compilation when no longer needed.
    /// </summary>

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
        DrawWindowSelections(rect);
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

    void DrawSelection(Rect rect, float startTime, float endTime, Color color)
    {
        float sx =
            Mathf.Lerp(rect.x, rect.xMax, startTime);

        float ex =
            Mathf.Lerp(rect.x, rect.xMax, endTime);

        Rect selection =
            new Rect(
                sx,
                rect.center.y-3,
                ex-sx,
                12);

        EditorGUI.DrawRect(
            selection,
            new Color(color.r, color.g, color.b, 0.25f));
    }

    void DrawWindowSelections(Rect rect)
    {
        for (int i = 0; i < windows.arraySize; i++)
        {
            if (!TryGetWindowProperties(i, out SerializedProperty windowStart, out SerializedProperty windowEnd, out SerializedProperty windowColor))
                continue;

            DrawSelection(
                rect,
                windowStart.floatValue,
                windowEnd.floatValue,
                windowColor.colorValue);
        }
    }

    void DrawMarkers(Rect rect)
    {
        for (int i = 0; i < windows.arraySize; i++)
        {
            if (!TryGetWindowProperties(i, out SerializedProperty windowStart, out SerializedProperty windowEnd, out SerializedProperty windowColor))
                continue;

            Color markerColor = GetOpaqueColor(windowColor.colorValue);

            DrawMarker(rect, windowStart.floatValue, markerColor);
            DrawMarker(rect, windowEnd.floatValue, markerColor);
        }

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

    enum TimelineHandleType
    {
        None = -1,
        WindowStart = 0,
        WindowEnd = 1,
        Preview = 2
    }

    TimelineHandleType activeHandle = TimelineHandleType.None;
    int activeWindowIndex = -1;

    void HandleEvents(Rect rect)
    {
        Event e = Event.current;

        int id = GUIUtility.GetControlID(FocusType.Passive);

        switch (e.GetTypeForControl(id))
        {
            case EventType.MouseDown:

                if (e.button != 0)
                    break;

                if (!TryGetHandleAtPosition(rect, e.mousePosition, out activeHandle, out activeWindowIndex))
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

                    case TimelineHandleType.Preview:
                        preview.floatValue = t;
                        break;

                    case TimelineHandleType.WindowStart:
                        if (TryGetWindowProperties(activeWindowIndex, out SerializedProperty windowStart, out SerializedProperty windowEnd, out _))
                            windowStart.floatValue = Mathf.Min(t, windowEnd.floatValue);
                        break;

                    case TimelineHandleType.WindowEnd:
                        if (TryGetWindowProperties(activeWindowIndex, out SerializedProperty draggedWindowStart, out SerializedProperty draggedWindowEnd, out _))
                            draggedWindowEnd.floatValue = Mathf.Max(t, draggedWindowStart.floatValue);
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
                activeHandle = TimelineHandleType.None;
                activeWindowIndex = -1;

                e.Use();
                break;
        }

    }

    bool TryGetHandleAtPosition(Rect rect, Vector2 mousePosition, out TimelineHandleType handleType, out int windowIndex)
    {
        handleType = TimelineHandleType.None;
        windowIndex = -1;

        float bestDistance = float.MaxValue;

        bool res = TryRegisterHandleHit(
            mousePosition,
            rect,
            preview.floatValue,
            TimelineHandleType.Preview,
            -1,
            ref handleType,
            ref windowIndex,
            ref bestDistance);

        if (res)
            return handleType != TimelineHandleType.None;

        for (int i = 0; i < windows.arraySize; i++)
        {
            if (!TryGetWindowProperties(i, out SerializedProperty windowStart, out SerializedProperty windowEnd, out _))
                continue;

            TryRegisterHandleHit(
                mousePosition,
                rect,
                windowStart.floatValue,
                TimelineHandleType.WindowStart,
                i,
                ref handleType,
                ref windowIndex,
                ref bestDistance);

            TryRegisterHandleHit(
                mousePosition,
                rect,
                windowEnd.floatValue,
                TimelineHandleType.WindowEnd,
                i,
                ref handleType,
                ref windowIndex,
                ref bestDistance);
        }

        return handleType != TimelineHandleType.None;
    }

    void RefreshAuthoredValues()
    {
        for(int i = 0 ; i < windows.arraySize; i++)
        {
            if (!TryGetWindowProperties(i, out SerializedProperty windowStart, out SerializedProperty windowEnd, out _))
                continue;

            float startTime = windowStart.floatValue; // Norm
            float endTime = windowEnd.floatValue; // Norm
            AnimationClip clip = this.clip.objectReferenceValue as AnimationClip;
            float clipTime = clip.length;

            startTime *= clipTime;
            endTime *= clipTime;

            float requiredTime = endTime - startTime;
            
            if(requiredTime < 0.05)
                continue;

            int count = Mathf.CeilToInt(requiredTime * sampleRate.intValue);
            Vector3 authoredValues = Vector3.zero;
            Vector3 originalPos = Vector3.zero;
            for(int k = 0 ; k <= count; k++)
            {
                float t = startTime + (k /(float)count) * requiredTime;
                clip.SampleAnimation(previewObject , t);
                if(k == 0)
                {
                    originalPos = previewObject.transform.position;
                }

                authoredValues.x = Mathf.Max(authoredValues.x , Mathf.Abs(originalPos.x - previewObject.transform.position.x));      

                authoredValues.y = Mathf.Max(authoredValues.y , Mathf.Abs(originalPos.y - previewObject.transform.position.y)); 

                authoredValues.z = Mathf.Max(authoredValues.z , Mathf.Abs(originalPos.z - previewObject.transform.position.z));
            }

            SerializedProperty window = windows.GetArrayElementAtIndex(i);
            var prop_av = window.FindPropertyRelative("AuthoredValues");
            prop_av.vector3Value = authoredValues;
        }   
    }

    bool TryRegisterHandleHit(
        Vector2 mousePosition,
        Rect rect,
        float markerTime,
        TimelineHandleType candidateType,
        int candidateWindowIndex,
        ref TimelineHandleType currentType,
        ref int currentWindowIndex,
        ref float bestDistance)
    {
        float markerX = Mathf.Lerp(rect.x, rect.xMax, markerTime);
        Rect markerRect = new Rect(markerX - 6f, rect.y, 12f, rect.height);

        if (!markerRect.Contains(mousePosition))
            return false;

        float distance = Mathf.Abs(mousePosition.x - markerX);

        if (distance > bestDistance)
            return false;

        bestDistance = distance;
        currentType = candidateType;
        currentWindowIndex = candidateWindowIndex;
        return true;
    }

    bool TryGetWindowProperties(int index, out SerializedProperty windowStart, out SerializedProperty windowEnd, out SerializedProperty windowColor)
    {
        windowStart = null;
        windowEnd = null;
        windowColor = null;

        if (index < 0 || index >= windows.arraySize)
            return false;

        SerializedProperty window = windows.GetArrayElementAtIndex(index);
        windowStart = window.FindPropertyRelative("start");
        windowEnd = window.FindPropertyRelative("end");
        windowColor = window.FindPropertyRelative("windowBoundsColor");

        return windowStart != null && windowEnd != null && windowColor != null;
    }

    Color GetOpaqueColor(Color color)
    {
        return new Color(color.r, color.g, color.b, 1f);
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
#endif