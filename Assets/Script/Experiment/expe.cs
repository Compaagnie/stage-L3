using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Threading;

public class Expe
{

    public string participant = "P01";
    public int startTrial = 1;

    private readonly string expeDescriptionFile = "Experiments/expe";
    private string previousCardNum;
    //static string[] letters = {"H", "N", "K", "R"};
    static readonly string[] letters = { "evertnone", "ehornone" };
    private readonly List<Trial> theTrials;
    public List<GameObject> cardList;
    public Trial curentTrial;
    private int trialNb = 0;
    private StreamWriter writer;
    private StreamWriter kineWriter;
    private readonly bool haveEyesCondition = false;

    static class ThreadSafeRandom
    {
        [ThreadStatic] private static System.Random Local;

        public static System.Random ThisThreadsRandom
        {
            get { return Local ?? (Local = new System.Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
        }
    }



    public Expe(string part, List<GameObject> cardL)
    {
        participant = part;
        cardList = cardL;

        Debug.Log("VisExpe :" + expeDescriptionFile + " " + participant);

        TextAsset mytxtData = (TextAsset)Resources.Load(expeDescriptionFile);
        string txt = mytxtData.text;
        List<string> lines = new List<string>(txt.Split('\n'));
        int i = 0;
        

        theTrials = new List<Trial>();

        foreach (string str in lines)
        {
            List<string> values = new List<string>(str.Split(';'));
            if (values[1] == participant)
            {
                theTrials.Add(new Trial(this,
                        values[0], values[1], values[2],
                        values[3], values[4], values[6]
                    ));
                Debug.Log("Goupe: " + curentTrial.group + "Participant: " + curentTrial.participant +
                          "collabEnvironememn: " + curentTrial.collabEnvironememnt + "moveMode: " + curentTrial.moveMode + "cardToTag: " + curentTrial.cardToTag);
            }
        }
        //  Debug.Log("Goupe: " + trial.group + );
        // file name should look like  "class-PXX-2019-MM-DD-HH-MM-SS.csv"
        string mydate = System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        string path = "Assets/Resources/logs/class-" + participant + "-" + mydate + ".csv";
        //string path = "Assets/Resources/logs/test.csv";

        //File.Create(path);
        //Write some text to the test.txt file
        writer = new StreamWriter(path, false);
        writer.WriteLine(
            // "factor"
            "Group;Participant;CardSet;CollabEnvironememnt;MoveMode;Trial"
            // measure
            + ";nbDestroyCard;nbUndoCard;nbDragCard;nbGroupCardTP"
            + ";nbTag;nbChangeTag");
        writer.Flush();
        path = "Assets/Resources/logs/class-" + participant + "-" + mydate + ".txt";
        curentTrial.pathLog = path;
      
        kineWriter = new StreamWriter(path, false);

        curentTrial.kineWriter = kineWriter;
        kineWriter.WriteLine(curentTrial.group + " " + curentTrial.participant + " kine action");
        kineWriter.Flush();

    }

    public void startTrials()
    {
        theTrials[trialNb].startTrial();
    }

    public void nextTrial()
    {
        if(trialNb == theTrials.Count)
        {
            Finished();
        }
        else
        {
            theTrials[trialNb].startTrial();
            if(trialNb-2 >= 0)
            {
                theTrials[trialNb - 2].card.transform.GetChild(1).gameObject.SetActive(false);
            }
        }

    }

    public void Finished()
    {

        writer.WriteLine(
           // "factor"
           curentTrial.group + ";" + curentTrial.participant + ";" + curentTrial.training + ";" + curentTrial.cardSet + ";" + curentTrial.collabEnvironememnt + ";"  
           // measure
           + curentTrial.nbDestroyCard + ";" + curentTrial.nbUndoCard + ";"+ curentTrial.nbDragCard + ";" + curentTrial.nbGroupCardTP + ";" 
           + curentTrial.nbTag + ";" + curentTrial.nbChangeTag
            );
        writer.Flush();
        writer.Close();
        kineWriter.Close();
    }

    public void incTrialNb()
    {
        trialNb += 1;
    }

}
