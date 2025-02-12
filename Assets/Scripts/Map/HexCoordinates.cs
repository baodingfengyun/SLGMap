﻿using UnityEngine;

[System.Serializable]
public struct HexCoordinates {

    [SerializeField]
	private int x, z;

	public int X {
    	get {
        	return x;
        }
    }

	public int Z {
    	get {
        	return z;
        }
    }

	public int Y {
    	get {
        	return -X - Z;
        }
    }

	public HexCoordinates (int x, int z) {
    	this.x = x;
    	this.z = z;
    }

	public static HexCoordinates FromOffsetCoordinates (int x, int z) {
    	return new HexCoordinates(x - z / 2, z);
    }

	public static HexCoordinates FromPosition (Vector3 position) {
    	float x = position.x / (HexMetrics.instance.innerRadius * 2f);
    	float y = -x;

    	float offset = position.z / (HexMetrics.instance.outerRadius * 3f);
    	x -= offset;
    	y -= offset;

    	int iX = Mathf.RoundToInt(x);
    	int iY = Mathf.RoundToInt(y);
    	int iZ = Mathf.RoundToInt(-x -y);

    	if (iX + iY + iZ != 0) {
        	float dX = Mathf.Abs(x - iX);
        	float dY = Mathf.Abs(y - iY);
        	float dZ = Mathf.Abs(-x -y - iZ);

        	if (dX > dY && dX > dZ) {
            	iX = -iY - iZ;
            }
        	else if (dZ > dY) {
            	iZ = -iX - iY;
            }
        }

    	return new HexCoordinates(iX, iZ);
    }

    public static HexDirection GetDirection(HexCell cell, HexCell otherCell)
    {
        float cellX = cell.transform.position.x;
        float cellZ = cell.transform.position.z;
        float otherCellX = otherCell.transform.position.x;
        float otherCellZ = otherCell.transform.position.z;

        if (cellZ == otherCellZ)
        {
            if(cellX>otherCellX)
            {
                return HexDirection.W;
            }
            else
            {
                return HexDirection.E;
            }
        }
        else if(cellZ > otherCellZ)
        {
            if (cellX > otherCellX)
            {
                return HexDirection.SW;
            }
            else
            {
                return HexDirection.SE;
            }
        }
        else
        {
            if (cellX > otherCellX)
            {
                return HexDirection.NW;
            }
            else
            {
                return HexDirection.NE;
            }
        }
    }

    public int DistanceToOther(HexCoordinates other)
    {
        return ((X < other.X ? other.X - X : X - other.X) +

                (Y < other.Y ? other.Y - Y : Y - other.Y) +

                (Z < other.Z ? other.Z - Z : Z - other.Z)) / 2;
    }

}