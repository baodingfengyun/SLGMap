﻿using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;


public class HexCell : MonoBehaviour {


    public BattleUnit unit = null;
    public BuildUnit buildUnit = null;

    //距离相关
    public Text label;
    int distanceWithOthers = 0;

    public int DistanceWithOthers
    {

        get
        {
            return distanceWithOthers;
        }

        set
        {
            distanceWithOthers = value;
            label.text = distanceWithOthers.ToString();
        }
    }


    //纹理相关
    [SerializeField]
    TerrainTypes terrainTypeIndex = TerrainTypes.Grass;

    public TerrainTypes TerrainTypeIndex
    {
        get
        {
            return terrainTypeIndex;
        }
        set
        {
            terrainTypeIndex = value;
        }
    }

    //道路相关
    public bool isRoad = false;
    bool[] isThroughRoad = new bool[6];

    public bool HasRoadThroughInDir(HexDirection direction)
    {
        return isThroughRoad[(int)direction];
    }

    public bool IsDrawPreviousRoadMesh(HexDirection direction)
    {
        if (isThroughRoad[(int)direction] == true && isThroughRoad[(int)direction.Previous()] == false)
        {
            return true;
        }

        return false;
    }

    public bool IsDrawNextRoadMesh(HexDirection direction)
    {
        if (isThroughRoad[(int)direction] == true && isThroughRoad[(int)direction.Next()] == false)
        {
            return true;
        }

        return false;
    }

    //水相关
    [SerializeField]
    int waterLevel = 0;
    public int WaterLevel
    {
        get
        {
            return waterLevel;
        }
        set
        {
            if(waterLevel != value)
            {
                waterLevel = value;
            }
        }
    }

    public bool isUnderWaterLevel
    {
        get
        {
            return waterLevel > elevation;
        }
    }

    public float waterY
    {
        get
        {
            return (waterLevel + HexMetrics.instance.waterOffset) * HexMetrics.instance.elevationStep;
        }
    }


    //坐标
	public HexCoordinates coordinates;
    //颜色
	public Color color;
    //UI位置
	public RectTransform uiRect;

    //高度
	public int Elevation {
    	get
        {
        	return elevation;
        }
    	set
        {
        	elevation = value;
            RefreshPosition(value);
            //label.text = coordinates.X.ToString() + "\n" + coordinates.Y.ToString() + "\n" + coordinates.Z.ToString();
            //Vector3 uiPosition = uiRect.localPosition;
            //uiPosition.z = -position.y;
            //uiRect.localPosition = uiPosition;
        }
    }


    void RefreshPosition(int value)
    {
        Vector3 position = transform.position;
        position.y = value * HexMetrics.instance.elevationStep;
        position.y += (HexMetrics.instance.SampleNoise(position).y * 2f - 1f) * HexMetrics.elevationPerturbStrength;//对高度进行微扰;
        transform.position = position;

        if (uiRect != null)
        {
            Vector3 uiPosition = uiRect.localPosition;
            uiPosition.z = -position.y;
            uiRect.localPosition = uiPosition;
        }
    }

    public Vector3 LocalPosition
    {
        get
        {
            return transform.localPosition;
        }
    }

    public Vector3 Position
    {
        get
        {
            return transform.position;
        }
    }

    [SerializeField]
    int elevation = 0;

    [SerializeField]
    public bool[] isStepDirection;

    [SerializeField]
	HexCell[] neighbors;


    
    public void Save(BinaryWriter writer)
    {
        writer.Write((byte)elevation);
        writer.Write((byte)waterLevel);
        if (!HexMetrics.instance.isEditorTexture)
        {
            writer.Write((byte)(color.r * 255));
            writer.Write((byte)(color.g * 255));
            writer.Write((byte)(color.b * 255));
            writer.Write((byte)(color.a * 255));
        }
        else
        {
            writer.Write((byte)terrainTypeIndex);
        }

        for (int i = 0; i < isStepDirection.Length; i++)
        {
            writer.Write(isStepDirection[i]);
        }
    }

    public void Load(BinaryReader reader)
    {
        elevation = reader.ReadByte();
        waterLevel = reader.ReadByte();
        if (!HexMetrics.instance.isEditorTexture)
        {
            color = new Color(reader.ReadByte() / 255.0f, reader.ReadByte() / 255.0f, reader.ReadByte() / 255.0f, reader.ReadByte() / 255.0f);
        }
        else
        {
            terrainTypeIndex = (TerrainTypes)reader.ReadByte();
        }
        RefreshPosition(elevation);
        for (int i = 0; i < isStepDirection.Length; i++)
        {
            isStepDirection[i] =  reader.ReadBoolean();
        }
    }

	public HexCell GetNeighbor (HexDirection direction) {
    	return neighbors[(int)direction];
    }

	public void SetNeighbor (HexDirection direction, HexCell cell) {
    	neighbors[(int)direction] = cell;
    	cell.neighbors[(int)direction.Opposite()] = this;
    }

	public HexEdgeType GetEdgeType (bool isStep,HexDirection direction) {
        if (neighbors[(int)direction] == null)
            return 0;
    	return HexMetrics.instance.GetEdgeType(
        	elevation, neighbors[(int)direction].elevation, isStep
        );
    }

    public HexEdgeType GetEdgeType (bool isStep, HexCell otherCell) {
    	return HexMetrics.instance.GetEdgeType(
        	elevation, otherCell.elevation, isStep
        );
    }

    public HexGridChunk chunkParent;
    //反向获取所在块
    public void SetParentChunk(HexGridChunk chunk)
    {
        chunkParent = chunk;
    }

    //以块为单位进行刷新
    public void Refresh()
    {
        if (chunkParent)
        {
            chunkParent.Refresh();
            for (int i = 0; i < neighbors.Length; i++)
            {
                HexCell neighbor = neighbors[i];
                if (neighbor != null && neighbor.chunkParent != chunkParent)
                {
                    neighbor.chunkParent.Refresh();
                }
            }
        }
    }

    //刷新单个mesh类型
    public void Refresh(MeshClass meshClass)
    {
        if (chunkParent == null)
            return;
        chunkParent.Refresh(meshClass);
        for (int i = 0; i < neighbors.Length; i++)
        {
            HexCell neighbor = neighbors[i];
            if (neighbor != null && neighbor.chunkParent != chunkParent)
            {
                neighbor.chunkParent.Refresh(meshClass);
            }
        }
    }

    public List<HexGridChunk> NeighorChunk()
    {
        List<HexGridChunk> returnList = new List<HexGridChunk>();
        for (int i = 0; i < neighbors.Length; i++)
        {
            HexCell neighbor = neighbors[i];
            if (neighbor != null && neighbor.chunkParent != chunkParent)
            {
                returnList.Add(neighbor.chunkParent);
            }
        }
        return returnList;
    }

}