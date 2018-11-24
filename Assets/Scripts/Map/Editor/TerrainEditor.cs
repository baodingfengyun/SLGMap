﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public enum EditorType
{
    HeightEditor,
    WaterEditor,
    EdgeEditor,
    MaterialEditor,
    SceneObjEditor,
}

public enum EditorTagType
{
    TerrainEditor,
    MaterialEditor,
    SceneObjEditor,
}


[CustomEditor(typeof(HexTerrain))]
public class TerrainEditor : Editor{

    public EditorType editorType = EditorType.HeightEditor;
    EditorTagType editorTagType = EditorTagType.TerrainEditor;

    string[] EDGE_BRUSEH_NAMES = { "斜边", "台阶" };
    int[] EDGE_BRUSH_VALUES = { 0, 1 };

    string[] MATERIAL_TEXTURE_NAMES = { "绿地", "泥地", "雪地", "沙地", "石地" };
    int[] MATERIAL_TEXTURE_VALUES = { 0, 1, 2, 3, 4, 5 };

    string[] SCENEOBJ_OPERATION_NAMES = { "增加", "删除"};
    int[] SCENEOBJ_OPERATION_VALUES = { 0, 1, 2 };

    public HexEdgeMesh hexEdgeMesh;

    public HeightBrush heightBrush;
    public MaterialBrush materialBrush;
    public SceneObjBrush sceneObjBrush;
    public EdgeBrush edgeBrush;
    public WaterBrush waterBrush;

    public static Stack<UndoRedoOperation> undoStack = new Stack<UndoRedoOperation>();
    public static Stack<UndoRedoOperation> redoStack = new Stack<UndoRedoOperation>();

    TerrainBrush currentBrush;

    void Init()
    {
        undoStack.Clear();
        redoStack.Clear();
    }

    private void Awake()
    {
        heightBrush = new HeightBrush(HexGrid.instance.HexEditMesh);
        materialBrush = new MaterialBrush(HexGrid.instance.HexEditMesh);
        sceneObjBrush = new SceneObjBrush(HexGrid.instance.HexEditMesh);
        edgeBrush = new EdgeBrush( HexGrid.instance.HexEditMesh);
        waterBrush = new WaterBrush(HexGrid.instance.HexEditMesh);
        Init();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        string[] captions = { "地形编辑","材质编辑","场景物体编辑"};
        editorTagType =  (EditorTagType)GUILayout.Toolbar((int)editorTagType, captions);
        switch(editorTagType)
        {
            case EditorTagType.TerrainEditor:
                DrawTerrainEditorGUI(captions[0]);
                break;
            case EditorTagType.MaterialEditor:
                editorType = EditorType.MaterialEditor;
                DrawMaterialEditorGUI(captions[1]);
                break;
            case EditorTagType.SceneObjEditor:
                editorType = EditorType.SceneObjEditor;
                DrawSceneObjEditorGUI(captions[2]);
                break;
        }

        //DrawTerrainMgrUI();
        DrawOperationMgrUI();



    }

    private void OnSceneGUI()
    {
        HandleUtility.AddDefaultControl(0);
        switch(editorType)
        {
            case EditorType.HeightEditor:
                currentBrush = heightBrush;
                MeshModifier.instance.m_brush = currentBrush;
                MeshModifier.instance.DoEvent();
                break;
            case EditorType.WaterEditor:
                currentBrush = waterBrush;
                MeshModifier.instance.m_brush = currentBrush;
                MeshModifier.instance.DoEvent();
                break;
            case EditorType.EdgeEditor:
                currentBrush = edgeBrush;
                MeshModifier.instance.m_brush = currentBrush;
                MeshModifier.instance.DoEvent();
                break;
            case EditorType.MaterialEditor:
                currentBrush = materialBrush;
                MaterialModifier.instance.m_brush = currentBrush;
                MaterialModifier.instance.DoEvent();
                break;
            case EditorType.SceneObjEditor:
                currentBrush = sceneObjBrush;
                SceneObjModifier.instance.m_brush = sceneObjBrush;
                SceneObjModifier.instance.DoEvent();
                break;

        }
    }

    private void DrawTerrainMgrUI()
    {
        if(EditorUtils.DrawHeader("地形管理","TerrainEditor"))
        {
            if(GUILayout.Button("新建地形",GUILayout.MaxWidth(100)))
            {
                Debug.Log(1);
            }

            if(GUILayout.Button("保存地形", GUILayout.MaxWidth(100)))
            {
                Debug.Log(2);
            }

            if(GUILayout.Button("加载地形", GUILayout.MaxWidth(100)))
            {
                Debug.Log(3);
            }
        }
    }
    public static bool isUndoTopPop = false;

    private void DrawOperationMgrUI()
    {
        if(EditorUtils.DrawHeader("操作管理","TerrainEditor"))
        {
            if(GUILayout.Button("Undo" +"( "+ (undoStack.Count > 0 ? undoStack.Peek().name : "无") +" )"))
            {
                if (undoStack.Count > 0)
                {
                    isUndoTopPop = true;
                    UndoRedoOperation undoRedoOperation = undoStack.Pop();
                    redoStack.Push(undoRedoOperation);
                    undoRedoOperation = undoStack.Pop();
                    undoRedoOperation.DoIt();
                    redoStack.Push(undoRedoOperation);
                }
            }
            if(GUILayout.Button("Redo" + "( " + (redoStack.Count > 0?redoStack.Peek().name:"无") + " )"))
            {
                if (redoStack.Count > 0)
                {
                    UndoRedoOperation undoRedoOperation = redoStack.Pop();
                    undoStack.Push(undoRedoOperation);
                    undoRedoOperation = redoStack.Pop();
                    undoRedoOperation.DoIt();
                    undoStack.Push(undoRedoOperation);
                }
            }
        }
    }


    public static void UndoAdd(SceneObjBrush.OperationType brushType,List<SceneObjectClass> sceneObjectClasses)
    {
        SceneObjOperation.OperationType operationType = SceneObjOperation.OperationType.AddSceneObj;
        string name = "";
        switch(brushType)
        {
            case SceneObjBrush.OperationType.Add:
                operationType = SceneObjOperation.OperationType.AddSceneObj;
                name = "删除场景物体";
                break;
            case SceneObjBrush.OperationType.Delete:
                operationType = SceneObjOperation.OperationType.DeleteSceneObj;
                name = "添加场景物体";
                break;
        }
        List<UndoRedoOperation.UndoRedoInfo> undoRedoInfoList = new List<UndoRedoOperation.UndoRedoInfo>();
        for (int i=0;i<sceneObjectClasses.Count;i++)
        {
            UndoRedoOperation.UndoRedoInfo undoRedoInfo = new UndoRedoOperation.UndoRedoInfo();
            undoRedoInfo.hexCell = null;
            undoRedoInfo.parma = new object[]{ sceneObjectClasses[i] };
            undoRedoInfoList.Add(undoRedoInfo);
        }

        SceneObjOperation sceneObjOperation = new SceneObjOperation(operationType, name, undoRedoInfoList);
        undoStack.Push(sceneObjOperation);

    }

    public static void UndoAdd(HexCell centerCell, TerrainBrush brush)
    {
        List<HexCell> cellList = new List<HexCell>();
        int centerX = centerCell.coordinates.X;
        int centerZ = centerCell.coordinates.Z;

        for (int l = 0, z = centerZ; z >= centerZ - brush.brushRange + 1; l++, z--)
        {
            for (int x = centerX - brush.brushRange + 1 + l; x <= centerX + brush.brushRange - 1; x++)
            {
                if (HexGrid.instance.GetCell(new HexCoordinates(x, z)) != null)
                    cellList.Add(HexGrid.instance.GetCell(new HexCoordinates(x, z)));
            }
        }

        for (int l = 1, z = centerZ + 1; z <= centerZ + brush.brushRange - 1; l++, z++)
        {
            for (int x = centerX - brush.brushRange + 1; x <= centerX + brush.brushRange - 1 - l; x++)
            {
                if (HexGrid.instance.GetCell(new HexCoordinates(x, z)) != null)
                    cellList.Add(HexGrid.instance.GetCell(new HexCoordinates(x, z)));
            }
        }


        List<UndoRedoOperation.UndoRedoInfo> undoRedoInfoList = new List<UndoRedoOperation.UndoRedoInfo>();
        for (int i = 0; i < cellList.Count; i++)
        {
            UndoRedoOperation.UndoRedoInfo undoRedoInfo = new UndoRedoOperation.UndoRedoInfo();
            undoRedoInfo.hexCell = cellList[i];
            switch (brush.m_editorType)
            {
                case EditorType.HeightEditor:
                    undoRedoInfo.parma = new object[] { cellList[i].Elevation };
                    break;
                case EditorType.WaterEditor:
                    undoRedoInfo.parma = new object[] { cellList[i].WaterLevel };
                    break;
                case EditorType.EdgeEditor:
                    undoRedoInfo.parma = new object[] { cellList[i].isStepDirection[0], cellList[i].isStepDirection[1], cellList[i].isStepDirection[2],
         cellList[i].isStepDirection[3], cellList[i].isStepDirection[4], cellList[i].isStepDirection[5]};
                    break;
                case EditorType.MaterialEditor:
                    if (HexMetrics.instance.isEditorTexture)
                    {
                        undoRedoInfo.parma = new object[] {cellList[i].TerrainTypeIndex};
                    }
                    else
                    {
                        undoRedoInfo.parma = new object[] { cellList[i].color };
                    }
                    break;
            }
            undoRedoInfoList.Add(undoRedoInfo);
        }

        string name = "";
        if (brush.m_editorType == EditorType.HeightEditor ||
           brush.m_editorType == EditorType.WaterEditor ||
           brush.m_editorType == EditorType.EdgeEditor)
        {
            MeshOperation.OperationType operationType = MeshOperation.OperationType.HeightEdit;
            switch (brush.m_editorType)
            {
                case EditorType.HeightEditor:
                    name = "高度编辑";
                    operationType = MeshOperation.OperationType.HeightEdit;
                    break;
                case EditorType.WaterEditor:
                    name = "水平线编辑";
                    operationType = MeshOperation.OperationType.WaterLevelEdit;
                    break;
                case EditorType.EdgeEditor:
                    name = "边界编辑";
                    operationType = MeshOperation.OperationType.EdgeEdit;
                    break;
            }

            MeshOperation meshOperation = new MeshOperation(operationType, name, undoRedoInfoList);
            undoStack.Push(meshOperation);
        }
        else if(brush.m_editorType == EditorType.MaterialEditor)
        {
            MaterialOperation.OperationType operationType = MaterialOperation.OperationType.WholeCellEdit;
            name = "材质编辑";
            MaterialOperation materialOperation = new MaterialOperation(operationType, name, undoRedoInfoList);
            undoStack.Push(materialOperation);
        }
    }



    private void DrawTerrainEditorGUI(string caption)
    {
        if (EditorUtils.DrawHeader("地形编辑","TerrainEditor"))
        {
            string[] captions = { "地形高度", "水体高度", "地形边界" };

            if (editorType > EditorType.EdgeEditor) editorType = EditorType.HeightEditor;
            editorType = (EditorType)GUILayout.Toolbar((int)editorType, captions);

            switch(editorType)
            {
                case EditorType.HeightEditor:
                    EditorUtils.BeginContents();
                    heightBrush.brushRange = EditorGUILayout.IntSlider("笔刷范围", heightBrush.brushRange, 1, 5);
                    heightBrush.Elevation = EditorGUILayout.IntSlider("地形高度", heightBrush.Elevation, 0, 5);
                    EditorUtils.EndContents();
                    break;
                case EditorType.WaterEditor:
                    EditorUtils.BeginContents();
                    waterBrush.brushRange = EditorGUILayout.IntSlider("笔刷范围", waterBrush.brushRange, 1, 5);
                    waterBrush.WaterLevel = EditorGUILayout.IntSlider("水体高度", waterBrush.WaterLevel, 0, 5);
                    EditorUtils.EndContents();
                    break;
                case EditorType.EdgeEditor:
                    EditorUtils.BeginContents();
                    edgeBrush.EditEdgeType = (EdgeBrush.EditorEdgeType)EditorGUILayout.IntPopup("边界类型", (int)edgeBrush.EditEdgeType, EDGE_BRUSEH_NAMES, EDGE_BRUSH_VALUES);
                    edgeBrush.IsWholeEditor = EditorGUILayout.Toggle("整个六边形编辑", edgeBrush.IsWholeEditor);
                    edgeBrush.brushRange = EditorGUILayout.IntSlider("笔刷范围", waterBrush.brushRange, 1, 5);
                    EditorUtils.EndContents();
                    break;

            }
        }
    }

    private void DrawMaterialEditorGUI(string caption)
    {
        if(EditorUtils.DrawHeader("材质笔刷", "TerrainEditor"))
        {
            EditorUtils.BeginContents();
            materialBrush.brushRange = EditorGUILayout.IntSlider("笔刷范围", materialBrush.brushRange, 1, 5);
            EditorUtils.EndContents();
        }
        if (EditorUtils.DrawHeader("材质编辑", "TerrainEditor"))
        {
            EditorUtils.BeginContents();
            if(HexMetrics.instance.isEditorTexture)
            {
                materialBrush.TerrainType = (TerrainTypes)EditorGUILayout.IntPopup("材质类型", (int)materialBrush.TerrainType, MATERIAL_TEXTURE_NAMES, MATERIAL_TEXTURE_VALUES);
                materialBrush.EditColor = EditorGUILayout.ColorField(materialBrush.EditColor);
            }
            else
            {
                materialBrush.EditColor = EditorGUILayout.ColorField(materialBrush.EditColor);
            }

            EditorUtils.EndContents();
        }
    }

    private void DrawSceneObjEditorGUI(string caption)
    {
        if (EditorUtils.DrawHeader("笔刷编辑", "TerrainEditor"))
        {
            EditorUtils.BeginContents();
            sceneObjBrush.IsBrushSceneObject = !EditorGUILayout.Toggle("是否单个编辑", !sceneObjBrush.IsBrushSceneObject);
            sceneObjBrush.brushRange = EditorGUILayout.IntSlider("笔刷范围", sceneObjBrush.brushRange, 1, 5);
            sceneObjBrush.SceneObjectDensity = EditorGUILayout.IntSlider("物体密度", sceneObjBrush.SceneObjectDensity, 1, 20);
            sceneObjBrush.SceneObjOperationType = (SceneObjBrush.OperationType)EditorGUILayout.IntPopup("操作", (int)sceneObjBrush.SceneObjOperationType,
                SCENEOBJ_OPERATION_NAMES, SCENEOBJ_OPERATION_VALUES);

            EditorUtils.EndContents();
        }

        if(EditorUtils.DrawHeader("场景物体编辑","TerrainEditor"))
        {
            //EditorGUILayout.LabelField("场景物体选择");
            sceneObjBrush.SceneObj = EditorGUILayout.ObjectField("场景物体选择",sceneObjBrush.SceneObj, typeof(GameObject), false) as GameObject;
            sceneObjBrush.ObjSize = EditorGUILayout.Slider("物体大小", sceneObjBrush.ObjSize, 0.0f, 10.0f);
        }


    }



}
