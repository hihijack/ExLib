using UnityEngine;
using UnityEditor;
using System.Collections;

public class EditorWinSetAnimFrame : EditorWindow
{

    class Styles
    {
        public Styles()
        {
        }
    }
    static Styles s_Styles;

    protected GameObject go;
    protected AnimationClip animationClip;
    protected float time = 0.0f;
    protected bool lockSelection = false;
    protected bool animationMode = false;

    [MenuItem("GameObject/指定动画帧", false, 16)]
    public static void DoWindow()
    {
        GetWindow<EditorWinSetAnimFrame>("指定动画帧");
    }

    public void OnEnable()
    {
        go = Selection.activeGameObject;
        Repaint();
    }

    public void OnDisable()
    {
        if (go != null)
        {
            if (AnimationMode.InAnimationMode())
                AnimationMode.StopAnimationMode();
        }
    }

    public void OnSelectionChange()
    {
        if (!lockSelection)
        {
            go = Selection.activeGameObject;
            Repaint();
        }
    }

    public void OnGUI()
    {
        if (s_Styles == null)
            s_Styles = new Styles();

        if (go == null)
        {
            EditorGUILayout.HelpBox("Please select a GO", MessageType.Info);
            return;
        }

        GUILayout.BeginHorizontal(EditorStyles.toolbar);

        EditorGUI.BeginChangeCheck();
        GUILayout.Toggle(AnimationMode.InAnimationMode(), "Animate", EditorStyles.toolbarButton);
        if (EditorGUI.EndChangeCheck())
            ToggleAnimationMode();

        GUILayout.FlexibleSpace();
        lockSelection = GUILayout.Toggle(lockSelection, "Lock", EditorStyles.toolbarButton);
        GUILayout.EndHorizontal();

        EditorGUILayout.BeginVertical();
        animationClip = EditorGUILayout.ObjectField(animationClip, typeof(AnimationClip), false) as AnimationClip;
        if (animationClip != null)
        {
            float startTime = 0.0f;
            float stopTime = animationClip.length;
            time = EditorGUILayout.Slider(time, startTime, stopTime);
            EditorGUILayout.LabelField("Frame:" + Mathf.RoundToInt(time * 30));
        }
        else if (AnimationMode.InAnimationMode())
            AnimationMode.StopAnimationMode();
        if (animationClip == null)
        {
            EditorGUILayout.HelpBox("选择一个动画剪辑", MessageType.Warning);
        }

        EditorGUILayout.EndVertical();
    }

    void Update()
    {
        if (go == null)
            return;

        if (animationClip == null)
            return;

        // there is a bug in AnimationMode.SampleAnimationClip which crash unity if there is no valid controller attached
        Animator animator = go.GetComponent<Animator>();
        if (animator != null && animator.runtimeAnimatorController == null)
            return;

        if (!EditorApplication.isPlaying && AnimationMode.InAnimationMode())
        {
            AnimationMode.BeginSampling();
            AnimationMode.SampleAnimationClip(go, animationClip, time);
            AnimationMode.EndSampling();

            SceneView.RepaintAll();
        }
    }

    void ToggleAnimationMode()
    {
        if (AnimationMode.InAnimationMode())
            AnimationMode.StopAnimationMode();
        else
            AnimationMode.StartAnimationMode();
    }
}
