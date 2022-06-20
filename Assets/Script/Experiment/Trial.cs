using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trial
{
    public GameObject card;
    private Material initialCardMaterial;
    private Network_Player player;
    private Teleporter teleport;
    private Transform cardArea;
    private Expe expe;
    
    // input
    public string group;
    public string participant;
    public string training;

    public string collabEnvironememnt;
    public string moveMode;
    public string cardToTag;

    // measures
    // public float size;
    // public int tct;
    // public float mux, muy, muz;

    //move
    public int nbMove = 0;
    public int nbRotate = 0;

    public float distTotal = 0;
    public float rotateTotal = 0;

    public float trialTime;
    public float moveTime = 0;

    public string pathLog = "";
    public StreamWriter kineWriter;
    private readonly float timer = 0;


    public Trial(Expe e,
        string g_, string p_,
        string colabEnv, string moveM, string cardT
        )
    {
        player = GameObject.Find("Network Player(Clone)").GetComponent<Network_Player>();
        teleport = GameObject.Find("/[CameraRig]/ControllerRotator/Controller (right)").GetComponent<Teleporter>();
        cardArea = GameObject.Find("/Salle").GetComponent<rendering>().cardArea;
        expe = e;
        group = g_;
        participant = p_;
        collabEnvironememnt = colabEnv;
        moveMode = moveM;
        cardToTag = cardT;
        timer = Time.time;
        if (cardT != "")
        {
            card = expe.cardList[int.Parse(cardT)];
        }
        Debug.Log("card found" + card);
    }

    public string StringToLog()
    {
        string str = group + ";" + participant + ";" + collabEnvironememnt + ";" + moveMode;

        return str;
    }



    public void startTrial()
    {
        trialTime = Time.time;
        card.transform.GetChild(0).GetComponent<Renderer>().material = player.none;
        initialCardMaterial = card.transform.GetChild(0).GetComponent<Renderer>().material;
        card.transform.GetChild(1).gameObject.SetActive(true);
        player.palette.gameObject.SetActive(false);
        teleport.moveMode = moveMode;
        Debug.Log("Trial started, card to tag " + cardToTag);
    }

    public void checkConditions()
    {
        float dist = (teleport.cam.position - card.transform.position).magnitude;
        if (dist < 4)
        {
            cardArea.position = new Vector3(card.transform.position.x, 0, card.transform.position.z);
            cardArea.gameObject.SetActive(true);
        }
        if (card.transform.GetChild(0).GetComponent<Renderer>().material != initialCardMaterial && dist <= 2.5)
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
        expe.nextTrial();
    }

    
    public void incNbMove()
    {
        nbMove += 1;
        kineWriter.WriteLine(Time.time - timer + "; Move");
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