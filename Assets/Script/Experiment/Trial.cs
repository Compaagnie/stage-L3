﻿using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEditor;

public class Trial
{
    public GameObject card;
    private Material initialCardMaterial;
    private Network_Player player;
    private Teleporter teleport;
    private rendering render;
    private Transform cardArea;
    private Expe expe;
    
    // input
    public string group;
    public string participant;
    public string trialNb;
    public string training;

    public string collabEnvironememnt;
    public string moveMode;
    public string task;
    public string wall;
    public string cardToTag;

    public bool trialEnded = false;
    public bool canTagCard = false;

    // measures
    // public float size;
    // public int tct;
    // public float mux, muy, muz;

    //move
    public int nbMove = 0;
    public int nbMoveWall = 0;
    public int nbRotate = 0;

    public float distTotal = 0;
    public float rotateTotal = 0;

    public float trialTime;
    public float moveTime = 0;

    public string pathLog = "";
    public StreamWriter kineWriter;
    private float timer = 0;


    public Trial(Expe e,
        string g_, string p_,
        string colabEnv, string trial, string train, string moveM, string t, string w, string cardT
        )
    {
        player = GameObject.Find("Network Player(Clone)").GetComponent<Network_Player>();
        teleport = GameObject.Find("/[CameraRig]/ControllerRotator/Controller (right)").GetComponent<Teleporter>();
        render = GameObject.Find("/Salle").GetComponent<rendering>();
        cardArea = GameObject.Find("/Salle").GetComponent<rendering>().cardArea;
        expe = e;
        group = g_;
        participant = p_;
        collabEnvironememnt = colabEnv;
        trialNb = trial;
        training = train;
        moveMode = moveM;
        task = t;
        wall = w;
        cardToTag = cardT;
        timer = Time.time;
        if (cardT != "")
        {
            card = expe.cardList[int.Parse(cardT)];
        }
        //Debug.Log("card found" + card);
    }

    public string StringToLog()
    {
        string str = group + ";" + participant + ";" + collabEnvironememnt + ";" + moveMode;

        return str;
    }



    public void startTrial()
    {
        card.transform.GetChild(0).GetComponent<Renderer>().material = player.none;
        initialCardMaterial = card.transform.GetChild(0).GetComponent<Renderer>().material;
        if (task == "search")
        {
            teleport.isOtherSynced = false;
            teleport.moveMode = "sync";
        }
        else
        {
            teleport.isOtherSynced = true;
            teleport.moveMode = moveMode;
        }
        //player.palette.gameObject.SetActive(false);
        Debug.Log("Trial started, card to tag " + cardToTag);
    }

    public void startTrialTimer()
    {
        Debug.Log("                                                             Trial Timer started");
        trialTime = Time.time;
        expe.trialRunning = true;
        if (task == "search")
        {
            card.transform.GetChild(1).gameObject.SetActive(true);
        }
    }

    public void checkConditions()
    {
        float dist = (teleport.centerBetweenPlayers - card.transform.position).magnitude;
        if (dist < 4)
        {
            cardArea.position = new Vector3(card.transform.position.x, 0, card.transform.position.z);
            cardArea.rotation = card.transform.rotation;
            cardArea.gameObject.SetActive(true);
        }
        if (!trialEnded && (card.transform.rotation.eulerAngles.y == 0 && Math.Abs(teleport.centerBetweenPlayers.x - card.transform.position.x) < 1 && Math.Abs(teleport.centerBetweenPlayers.z - card.transform.position.z) < 2.5f) || (card.transform.rotation.eulerAngles.y != 0 && Math.Abs(teleport.centerBetweenPlayers.x - card.transform.position.x) < 2.5f && Math.Abs(teleport.centerBetweenPlayers.z - card.transform.position.z) < 1))
        {
            canTagCard = true;
            cardArea.GetComponent<Renderer>().material = player.white;
        }
        else
        {
            canTagCard = false;
            cardArea.GetComponent<Renderer>().material = player.none;
        }
        if (canTagCard && card.transform.GetChild(0).GetComponent<Renderer>().material != initialCardMaterial)
        {
            Debug.Log("Card tagged with new color " + card);
            endTrial();
        }
    }

    public void endTrial()
    {
        trialTime = Time.time - trialTime;
        cardArea.gameObject.SetActive(false);
        card.transform.GetChild(1).GetComponent<Renderer>().material = player.green;
        trialEnded = true;
        render.nextTrial();
    }


    public void incNbMove()
    {
        nbMove += 1;
        kineWriter.WriteLine(Time.time - timer + "; Move");
        kineWriter.Flush();
    }
    public void incNbMoveWall()
    {
        nbMoveWall += 1;
        kineWriter.WriteLine(Time.time - timer + "; MoveWall");
        kineWriter.Flush();
    }
    public void incNbRotate()
    {
        nbRotate += 1;
        kineWriter.WriteLine(Time.time - timer + "; Rotate");
        kineWriter.Flush();
    }
    public void incDistTotal(float dist)
    {
        distTotal += 1;
        kineWriter.WriteLine(Time.time - timer + "; Move " + dist);
        kineWriter.Flush();
    }
    public void incRotateTotal(float angle)
    {
        rotateTotal += 1;
        kineWriter.WriteLine(Time.time - timer + "; Rotate " + angle);
        kineWriter.Flush();
    }

    public void incMoveTime(float t)
    {
        moveTime += t;
    }
}