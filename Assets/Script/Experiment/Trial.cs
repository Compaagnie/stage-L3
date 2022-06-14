using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trial
{
    public GameObject card;
    private Network_Player player;
    private Teleporter teleport;
    private Expe expe;
    // input
    public string group;
    public string participant;
    public string training;

    public string cardSet;
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

    //card
    public int nbDragCard = 0;
    public int nbGroupCardTP = 0;

    public int nbDestroyCard = 0;
    public int nbUndoCard = 0;

    public string pathLog = "";
    public StreamWriter kineWriter;
    private readonly float timer = 0;


    public Trial(Expe e,
        string g_, string p_,
        string cardS, string colabEnv, string moveM, string cardT
        )
    {
        player = GameObject.Find("Network Player(Clone)").GetComponent<Network_Player>();
        teleport = GameObject.Find("/[CameraRig]/ControllerRotator/Controller (right)").GetComponent<Teleporter>();
        expe = e;
        group = g_;
        participant = p_;
        cardSet = cardS;
        collabEnvironememnt = colabEnv;
        moveMode = moveM;
        cardToTag = cardT;
        timer = Time.time;

        card = expe.cardList[int.Parse(cardT)];
        Debug.Log("card found" + card);
    }
    public string StringToLog()
    {
        string str = group + ";" + participant + ";" + training + ";" + cardSet + ";" + collabEnvironememnt + ";" + moveMode;

        return str;
    }



    public void startTrial()
    {
        card.transform.GetChild(1).gameObject.SetActive(true);
    }

    public void checkConditions()
    {
        float dist = (teleport.cam.position - card.transform.position).magnitude;
        if (dist < 3)
        {

        }
        if (card.transform.GetChild(0).GetComponent<Renderer>().material != player.none)
        {
            endTrial();
        }
    }

    public void endTrial()
    {
        card.transform.GetChild(1).GetComponent<Renderer>().material = player.green;
        expe.incTrialNb();
        expe.nextTrial();
    }






    // Tag 
    public void incNbTag(string nameR)
    {
        nbTag = nbTag + 1;
        kineWriter.WriteLine(Time.time - timer + ";" + " Tag" + " ; color : " + nameR);
        kineWriter.Flush();
    }

    public void incNbChangeTag(string nameR)
    {
        nbChangeTag = nbChangeTag + 1;
        kineWriter.WriteLine(Time.time - timer + ";" + " Change Tag" + " ; color : " + nameR);
        kineWriter.Flush();
    }

    //card
    public void incNbDragCard()
    {
        nbDragCard = nbDragCard + 1;
        kineWriter.WriteLine(Time.time - timer + ";" + " Drag card ");
        kineWriter.Flush();
    }
    public void incNbGroupCardTP(string namewall)
    {
        nbGroupCardTP = nbGroupCardTP + 1;
        kineWriter.WriteLine(Time.time - timer + ";" + " GroupCardTP" + " ; wall : " + namewall);
        kineWriter.Flush();
    }

    public void incNbDestroyCard()
    {
        nbDestroyCard = nbDestroyCard + 1;
        kineWriter.WriteLine(Time.time - timer + ";" + " Destroy card ");
        kineWriter.Flush();
    }
    public void incNbUndoCard()
    {
        nbUndoCard = nbUndoCard + 1;
        kineWriter.WriteLine(Time.time - timer + ";" + " Undo destroy ");
        kineWriter.Flush();
    }
}