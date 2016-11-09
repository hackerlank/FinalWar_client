﻿using UnityEngine;
using xy3d.tstd.lib.superTween;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using FinalWar;

public class HeroBattle : HeroBase
{
    [SerializeField]
    public Transform moveTrans;

    [SerializeField]
    public Transform shockTrans;

    [SerializeField]
    private CanvasGroup canvasGroup;

    [SerializeField]
    private Text hp;

    [SerializeField]
    private Text shield;

    [SerializeField]
    private Text attack;

    [SerializeField]
    private Text shoot;

    private Hero hero;

    public bool isMine
    {
        get
        {
            return hero.isMine;
        }
    }

    public int pos
    {
        get
        {
            return hero.pos;
        }
    }

    public void Init(int _id)
    {
        HeroSDS heroSDS = StaticData.GetData<HeroSDS>(_id);

        InitCard(heroSDS);

        hp.gameObject.SetActive(false);

        shield.gameObject.SetActive(false);

        attack.gameObject.SetActive(false);

        shoot.gameObject.SetActive(false);
    }

    public void Init(Hero _hero)
    {
        hero = _hero;

        InitCard(hero.sds as HeroSDS);

        attack.text = sds.GetAttack().ToString();

        shoot.text = sds.GetShoot().ToString();

        RefreshHp();

        RefreshShield();
    }

    public void RefreshHp()
    {
        hp.text = hero.nowHp.ToString();
    }

    public void RefreshShield()
    {
        if (sds.GetShield() > 0)
        {
            if (!shield.gameObject.activeSelf)
            {
                shield.gameObject.SetActive(true);
            }

            shield.text = hero.nowShield.ToString();
        }
        else
        {
            if (shield.gameObject.activeSelf)
            {
                shield.gameObject.SetActive(false);
            }
        }
    }

    public void Shock(List<Vector3> _targets, AnimationCurve _curve, float _shockDis, int _shieldDamage, int _hpDamage)
    {
        Vector3 shockVector = Vector3.zero;

        for (int i = 0; i < _targets.Count; i++)
        {
            shockVector += (transform.position - _targets[i]).normalized;
        }

        if (shockVector == Vector3.zero)
        {
            Vector3 v2 = transform.position - _targets[0];

            float angle = Mathf.Atan2(v2.y, v2.x);

            angle += Mathf.PI * 0.5f;

            shockVector = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * _shockDis;
        }
        else
        {
            shockVector = shockVector.normalized * _shockDis;
        }

        Action<float> shockToDel = delegate (float obj)
        {
            float value = _curve.Evaluate(obj);

            shockTrans.position = moveTrans.position - shockVector * value;
        };

        SuperTween.Instance.To(0, 1, 1, shockToDel, null);

        string str = string.Empty;

        if (_shieldDamage < 0)
        {
            str += "<color=\"#FFFF00\">" + _shieldDamage + "</color>";

            if (_hpDamage < 0)
            {
                str += "   ";
            }
        }

        if (_hpDamage < 0)
        {
            str += "<color=\"#FF0000\">" + _hpDamage + "</color>";
        }

        ShowHud(str, Color.red, null);

        RefreshShield();

        RefreshHp();
    }

    public void ShowHud(string _str, Color _color, Action _callBack)
    {
        GameObject go = GameObject.Instantiate<GameObject>(BattleControl.Instance.damageNumResources);

        go.transform.SetParent(transform.parent, false);

        go.transform.position = transform.position;

        DamageNum damageNum = go.GetComponent<DamageNum>();

        damageNum.Init(_str, _color, _callBack);
    }

    public void Die(Action _del)
    {
        Action dieOver = delegate ()
        {
            GameObject.Destroy(gameObject);

            if (_del != null)
            {
                _del();
            }
        };

        SuperTween.Instance.To(1, 0, BattleControl.Instance.dieTime, DieTo, dieOver);
    }

    private void DieTo(float _v)
    {
        canvasGroup.alpha = _v;
    }
}
