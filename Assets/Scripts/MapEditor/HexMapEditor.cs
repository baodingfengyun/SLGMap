﻿using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class HexMapEditor : MonoBehaviour {

	public Color[] colors;

	public HexGrid hexGrid;

    public HexEdgeMesh hexEdgeMesh;

    int elevation;
    int brushRange;

	Color cellColor;

    public Toggle isStepEditorToggle;
    public Toggle isSlopeEditorToggle;
    public Toggle isStepWholeEditorToggle;



    bool IsEditorStep()
    {
        return isStepEditorToggle.isOn;
    }

    bool IsEditorSlope()
    {
        return isSlopeEditorToggle.isOn;
    }

    bool IsWholeEditor()
    {
        return isStepWholeEditorToggle.isOn;
    }


	public void SelectColor (int index)
    {
		cellColor = colors[index];
	}

	public void SetElevation (float sliderValue)
    {
		elevation = (int)sliderValue;
	}

    public void SetBrushRange(float sliderValue)
    {
        brushRange = (int)sliderValue;
    }

	void Awake ()
    {
		SelectColor(0);
        brushRange = 1;
    }

	void Update () {


        if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            HandleInput();
        }

        RefreshHexEdgeMesh();
    }

    //刷新提示Mesh
    void RefreshHexEdgeMesh()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(inputRay, out hit))
        {
            if (IsEditorStep() || IsEditorSlope())
            {
                EditMeshRefresh(hit.point, hexGrid.GetCell(hit.point));
            }
            else
            {
                hexEdgeMesh.Triangulate(hexGrid.GetCell(hit.point), cellColor);
            }
        }
    }

    //刷新编辑指引的mesh
    void EditMeshRefresh(Vector3 pos, HexCell cell)
    {
        HexDirection clickDir = hexGrid.GetPointDirection(new Vector2(pos.x - cell.transform.position.x, pos.z - cell.transform.position.z));
        if (IsEditorStep())
        {
            if (!IsWholeEditor())
            {
                if (cell.GetEdgeType(cell.isStepDirection[(int)clickDir], clickDir) == HexEdgeType.Slope)
                {
                    hexEdgeMesh.Triangulate(cell, cell.GetNeighbor(clickDir), clickDir, true, new Color(0.18f, 1, 0.18f, 0.5f));
                }
                else
                {
                    hexEdgeMesh.Triangulate(cell, cell.GetNeighbor(clickDir), clickDir, true, new Color(1, 0.18f, 0.18f, 0.5f));
                }
            }
            else
            {
                hexEdgeMesh.Triangulate(cell, new Color(0.18f, 1, 0.18f, 0.5f));
            }
        }
        else if (IsEditorSlope())
        {
            if (!IsWholeEditor())
            {
                if (cell.GetEdgeType(cell.isStepDirection[(int)clickDir], clickDir) == HexEdgeType.Step)
                {
                    hexEdgeMesh.Triangulate(cell, cell.GetNeighbor(clickDir), clickDir, false, new Color(0.18f, 1, 0.18f, 0.5f));
                }
                else
                {
                    hexEdgeMesh.Triangulate(cell, cell.GetNeighbor(clickDir), clickDir, false, new Color(1, 0.18f, 0.18f, 0.5f));
                }
            }
            else
            {
                hexEdgeMesh.Triangulate(cell, new Color(0.18f, 1, 0.18f, 0.5f));
            }
        }

    }

    Dictionary<HexGridChunk, HexCell> refreshChunkDic = new Dictionary<HexGridChunk, HexCell>();
	void HandleInput () {
		Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
        HexCell centerCell = null;
        int centerX = 0;
        int centerZ = 0;

        refreshChunkDic.Clear();
        if (Physics.Raycast(inputRay, out hit)) {
            centerCell = hexGrid.GetCell(hit.point);
            centerX = centerCell.coordinates.X;
            centerZ = centerCell.coordinates.Z;
            for (int l = 0, z = centerZ; z >= centerZ - brushRange + 1; l++, z--)
            {
                for (int x = centerX - brushRange + 1 + l; x <= centerX + brushRange - 1; x++)
                {
                    EditCell(hit.point, hexGrid.GetCell(new HexCoordinates(x, z)));
                }
            }

            for(int l = 1,z = centerZ + 1; z<= centerZ + brushRange - 1;l++,z++)
            {
                for(int x = centerX - brushRange + 1; x<= centerX + brushRange - 1-l;x++)
                {
                    EditCell(hit.point, hexGrid.GetCell(new HexCoordinates(x, z)));
                }
            }
            foreach(HexCell cell in refreshChunkDic.Values)
            {
                cell.Refresh();
            }
		}
	}

    //对整个六边形的边进行编辑
    void EditorWholeCell(HexCell cell)
    {
        if (IsEditorStep())
        {
            for (int i = 0; i < 6; i++)
            {
                {
                    cell.isStepDirection[i] = true;
                    if (cell.GetNeighbor((HexDirection)i) != null)
                        cell.GetNeighbor((HexDirection)i).isStepDirection[(int)((HexDirection)i).Opposite()] = true;
                }
            }
        }
        else if(IsEditorSlope())
        {
            for (int i = 0; i < 6; i++)
            {
                {
                    cell.isStepDirection[i] = false;
                    if (cell.GetNeighbor((HexDirection)i) != null)
                        cell.GetNeighbor((HexDirection)i).isStepDirection[(int)((HexDirection)i).Opposite()] = false;
                }
            }
        }
    }
    //对六边形的某个边进行编辑
    void EditorEdge(HexCell cell,HexDirection clickDir)
    {
        if (IsEditorStep())
        {
            cell.isStepDirection[(int)clickDir] = true;
            if (cell.GetNeighbor(clickDir) != null)
                cell.GetNeighbor(clickDir).isStepDirection[(int)clickDir.Opposite()] = true;
        }
        else if (IsEditorSlope())
        {
            cell.isStepDirection[(int)clickDir] = false;
            if (cell.GetNeighbor(clickDir) != null)
                cell.GetNeighbor(clickDir).isStepDirection[(int)clickDir.Opposite()] = false;
        }
    }

	void EditCell (Vector3 pos,HexCell cell) {

        if (IsEditorStep()|| IsEditorSlope())
        {
            if (!IsWholeEditor())
            {
                HexDirection clickDir = hexGrid.GetPointDirection(new Vector2(pos.x - cell.transform.position.x, pos.z - cell.transform.position.z));
                EditorEdge(cell, clickDir);
            }
            else
            {
                EditorWholeCell(cell);
            }
        }
        else
        {
            if (cell != null)
            {
                cell.color = cellColor;
                cell.Elevation = elevation;
            }
        }
        
        if(cell != null&&!refreshChunkDic.ContainsKey(cell.chunkParent))
        {
            refreshChunkDic.Add(cell.chunkParent, cell);
        }

    }


}