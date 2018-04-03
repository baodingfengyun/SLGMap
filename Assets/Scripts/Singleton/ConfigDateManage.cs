﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BaseInfo
{

    public int id;

    public virtual void ChangeValues(string[] table){}

    public virtual BaseInfo GetNew() { return new BaseInfo(); }
}

public class TerrainTextureInfo : BaseInfo
{
    public TerrainTypes terrainType;
    public string iconName;
    public string iconPath;


    public override void ChangeValues(string[] table)
    {
        id = int.Parse(table[0]);
        iconName = table[1];
        iconPath = table[2];
        terrainType = (TerrainTypes)int.Parse(table[3]);
    }

    public override BaseInfo GetNew()
    {
        return new TerrainTextureInfo();
    }
}

public class TerrainColorInfo : BaseInfo
{
    public string iconName;
    public Color itemColor;
    public override void ChangeValues(string[] table)
    {
        id = int.Parse(table[0]);
        iconName = table[1];
        itemColor = ToolClass.instance.ConvertColor(table[2]);
    }

    public override BaseInfo GetNew()
    {
        return new TerrainColorInfo();
    }
}
public class ConfigDateManage : Singleton<ConfigDateManage> {

	public void InitData()
    {
        FileManage.instance.LoadCSV("terrainTexture", new TerrainTextureInfo());
        FileManage.instance.LoadCSV("terrainColor", new TerrainColorInfo());
    }

}
