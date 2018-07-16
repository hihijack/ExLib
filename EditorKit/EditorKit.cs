using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public class EditorKit : ScriptableObject
{
    [MenuItem("GameObject/GetChildCount")]
    static void GetChildCountKit()
    {
        Debug.Log("ChildCount," + Selection.activeGameObject.name + ":" + Selection.activeGameObject.transform.childCount);
    }

    [MenuItem("GameObject/释放子物体", false, 13)]
    static void DetachChild()
    {
        foreach (Transform tfSelect in Selection.transforms)
        {
            tfSelect.DetachChildren();
        }
        EditorSceneManager.MarkAllScenesDirty();
    }

    [MenuItem("GameObject/包围盒大小", false, 14)]
    static void GetBoundsSize()
    {
        if (Selection.activeGameObject != null)
        {
            Renderer[] renders = Selection.activeGameObject.GetComponentsInChildren<Renderer>();
            if (renders.Length > 0)
            {
                Bounds ori = renders[0].bounds;
                if (renders.Length > 1)
                {
                    for (int i = 1; i < renders.Length; i++)
                    {
                        ori.Encapsulate(renders[i].bounds);
                    }
                }
                Debug.Log("包围盒大小:" + ori.size);
            }
        }
        
    }

    [MenuItem("UniCore/打开场景/Main场景 %M")]
    static void OpenMainScene()
    {
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        EditorSceneManager.OpenScene(Application.dataPath + "/Game/Main.unity", OpenSceneMode.Single);
    }

    class MFNode
    {
        public int vertCount = 0;
        public List<MeshFilter> mfs = new List<MeshFilter>();
    }

    [MenuItem("GameObject/合并网格", false, 12)]
    static void CombineMesh()
    {
        Transform targetTf = Selection.activeGameObject.transform;
        if (!EditorUtility.DisplayDialog("确认", "确定要合并网格吗?", "确定", "取消"))
        {
            return;
        }
        MeshFilter targetMF = targetTf.GetComponent<MeshFilter>();
        //按材质球分组
        Dictionary<Material, List<MFNode>> dicMatMF = new Dictionary<Material, List<MFNode>>();

        MeshFilter[] meshFilters = targetTf.GetComponentsInChildren<MeshFilter>(false);

        for (int i = 0; i < meshFilters.Length; i++)
        {
            
            MeshFilter mfT = meshFilters[i];

            if (mfT.transform == targetTf)
            {
                continue;
            }

            Material matT = meshFilters[i].GetComponent<MeshRenderer>().sharedMaterial;
            if (!dicMatMF.ContainsKey(matT))
            {
                List<MFNode> listMFNode = new List<MFNode>();
                AddAMFToMFNode(mfT, listMFNode);
                dicMatMF.Add(matT, listMFNode);
            }
            else
            {
                AddAMFToMFNode(mfT, dicMatMF[matT]);
            }
        }

        foreach (Material mat in dicMatMF.Keys)
        {
            int nodeIndex = 0;
            foreach (MFNode mfNode in dicMatMF[mat])
            {
                nodeIndex++;

                List<MeshFilter> listMf = mfNode.mfs;

                //开始合并
                CombineInstance[] combine = new CombineInstance[listMf.Count];

                for (int i = 0; i < listMf.Count; i++)
                {
                    combine[i].mesh = listMf[i].sharedMesh;
                    combine[i].transform = listMf[i].transform.localToWorldMatrix;
                    listMf[i].gameObject.SetActive(false);
                }

                string assetName = targetTf.name + "_" + mat.name + nodeIndex;
                GameObject gobjNew = new GameObject(assetName);
                gobjNew.transform.parent = targetTf;
                gobjNew.transform.position = Vector3.zero;
                MeshFilter mf = gobjNew.AddComponent<MeshFilter>();
                MeshRenderer mr = gobjNew.AddComponent<MeshRenderer>();
                mf.mesh = new Mesh();
                mf.sharedMesh.CombineMeshes(combine, true);//为mesh.CombineMeshes添加一个 false 参数，表示并不是合并为一个网格，而是一个子网格列表
                mr.sharedMaterial = mat;

                //创建网格资源
                string path = "/CombineMeshExport";
                if (!Directory.Exists(Application.dataPath + path))
                {
                    Directory.CreateDirectory(Application.dataPath + path);
                }
                AssetDatabase.CreateAsset(mf.sharedMesh, string.Format("{0}/{1}.asset", "Assets" + path, assetName));
            }
        }
    }

    private static void AddAMFToMFNode(MeshFilter mfT, List<MFNode> listMF)
    {
        // 查找一个可以放得下的MFNode，无则创建一个新的Node
        foreach (MFNode node in listMF)
        {
            if (node.vertCount + mfT.sharedMesh.vertexCount < 65000)
            {
                node.mfs.Add(mfT);
                node.vertCount += mfT.sharedMesh.vertexCount;
                return;
            }
        }

        // 创建一个新的Node
        MFNode nodeNew = new MFNode();
        nodeNew.mfs.Add(mfT);
        nodeNew.vertCount += mfT.sharedMesh.vertexCount;
        listMF.Add(nodeNew);
    }

    //[MenuItem("Assets/AddSceneToBuild")]
    //static void AddToBuildScene()
    //{
    //    foreach (object item in Selection.objects)
    //    {

    //    }
    //}

    [MenuItem("Assets/动作/提取动作文件")]
    static void ExportAnimFile()
    {
        foreach (GameObject selectGobj in Selection.gameObjects)
        {
            string selectPath = AssetDatabase.GetAssetPath(selectGobj);
            string[] nameInfos = selectGobj.name.Split('@');
            string rolename = nameInfos[0];
            string animName = nameInfos[1];
            string savePath = Path.GetDirectoryName(selectPath) + "/" + rolename + "_" + animName + ".anim";
            AnimationClip orgClip = (AnimationClip)AssetDatabase.LoadAssetAtPath(selectPath, typeof(AnimationClip));
            AnimationClip placeClip = (AnimationClip)AssetDatabase.LoadAssetAtPath(savePath, typeof(AnimationClip));
            if (placeClip != null)
            {
                EditorUtility.CopySerialized(orgClip, placeClip);
                AssetDatabase.SaveAssets();
            }
            else
            {
                placeClip = new AnimationClip();
                EditorUtility.CopySerialized(orgClip, placeClip);
                AssetDatabase.CreateAsset(placeClip, savePath);
            }

            AssetDatabase.DeleteAsset(selectPath);

            AssetDatabase.Refresh();

            Debug.Log("导出动作文件成功:" + savePath);//###########
        }
    }

    [MenuItem("Assets/动作/提取动作文件", true)]
    static bool ValdExportAnimFile()
    {
        bool r = false;
        foreach (GameObject selectGobj in Selection.gameObjects)
        {
            string selectPath = AssetDatabase.GetAssetPath(selectGobj);
            selectPath = Path.GetFullPath(selectPath);
            FileInfo fi = new FileInfo(selectPath);
            if (fi.Extension == ".FBX" && fi.Name.Contains("@"))
            {
                r = true;
            }
            else
            {
                r = false;
                break;
            }
        }
       
        return r;
    }
}