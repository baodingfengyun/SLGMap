﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundManage : Singleton<RoundManage> {

    public int curPower = 0;
    public int RoundCount = 1;

    public void NewRound(int power)
    {
        if(power == GameUnitManage.instance.myPower)
        {
            RoundCount++;
        }
        curPower = power;
        List<BattleUnit> unit = GameUnitManage.instance.battleUnitPowerDic[power];
        GameUnitManage.instance.UnBlockRoad(power);
        for (int i=0;i<unit.Count;i++)
        {
            unit[i].NewRound();
        }
    }

    public void Init()
    {
        RoundCount = 1;
        curPower = 0;
    }

    void ExitRound()
    {
        GameUnitManage.instance.BlockRoad(curPower);
    }

    public void ChangePower()
    {
        List<BattleUnit> unit = GameUnitManage.instance.battleUnitPowerDic[curPower];
        for(int i=0;i<unit.Count;i++)
        {
            if (unit[i].AttackTarget != null)
            {
                unit[i].AutoAttack();
            }
            else
            {
                unit[i].AutoMove();
            }
        }

        List<BuildUnit> buildUnit = GameUnitManage.instance.buildUnitPowerDic[curPower];
        for(int i=0;i<buildUnit.Count;i++)
        {
            buildUnit[i].AutoAttack();
        }

        List<int> powerList = GameUnitManage.instance.powerList;
        ExitRound();
        if (powerList.IndexOf(curPower) < powerList.Count - 1)
        {
            curPower = powerList[powerList.IndexOf(curPower) + 1];
        }
        else
        {
            curPower = powerList[0];
        }
        NewRound(curPower);
    }

}
