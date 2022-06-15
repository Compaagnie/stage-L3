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

    //tag 
    public int nbTag = 0;
    public int nbChangeTag = 0;

    //move
    public int nbMove = 0;
    public int nbRotate = 0;

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
        expe = e;
        group = g_;
        participant = p_;
        collabEnvironememnt = colabEnv;
        moveMode = moveM;
        cardToTag = cardT;
        timer = Time.time;

        card = expe.cardList[int.Parse(cardT)];
        initialCardMaterial = card.transform.GetChild(0).GetComponent<Renderer>().material;
        Debug.Log("card found" + card);
    }

    public string StringToLog()
    {
        string str = group + ";" + participant + ";" + collabEnvironememnt + ";" + moveMode;

        return str;
    }



    public void startTrial()
    {
        card.transform.GetChild(1).gameObject.SetActive(true);
        player.palette.gameObject.SetActive(false);
        teleport.moveMode = moveMode;
        Debug.Log("Trial started, card to tag " + cardToTag);
    }

    public void checkConditions()
    {
        float dist = (teleport.cam.position - card.transform.position).magnitude;
        if (dist < 3)
        {

        }
        if (card.transform.GetChild(0).GetComponent<Renderer>().material != initialCardMaterial )
        {
            Debug.Log("Card tagged with new color " + card);
            endTrial();
        }
    }

    public void endTrial()
    {
        card.transform.GetChild(1).GetComponent<Renderer>().material = player.green;
        expe.nextTrial();
    }






    // Tag 
    public void incNbTag(string nameR)
    {
        nbTag += 1;
        kineWriter.WriteLine(Time.time - timer + ";" + " Tag" + " ; color : " + nameR);
        kineWriter.Flush();
    }

    public void incNbChangeTag(string nameR)
    {
        nbChangeTag = nbChangeTag + 1;
        kineWriter.WriteLine(Time.time - timer + ";" + " Change Tag" + " ; color : " + nameR);
        kineWriter.Flush();
    }
}