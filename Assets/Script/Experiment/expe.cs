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
    private List<Trial> theTrials;
    public List<GameObject> cardList;
    public Trial curentTrial;
    public int trialNb = 0;
    private StreamWriter writer;
    private StreamWriter kineWriter;
    private readonly bool haveEyesCondition = false;
    public bool expeRunning = false;

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
        expeRunning = true;
        participant = part;
        cardList = cardL;

        string mydate = System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        //  Debug.Log("Goupe: " + trial.group + );
        // file name should look like  "class-PXX-2019-MM-DD-HH-MM-SS.csv"
        string path = "Assets/Resources/logs/class-" + participant + "-" + mydate + ".csv";
        //string path = "Assets/Resources/logs/test.csv";

        //File.Create(path);
        //Write some text to the test.txt file
        writer = new StreamWriter(path, false);

        writer.WriteLine(
            // "factor"
            "Group;Participant;CollabEnvironememnt;MoveMode;CardToTag"
            // measure
            + ";nbTag;nbChangeTag");
        writer.Flush();
        path = "Assets/Resources/logs/class-" + participant + "-" + mydate + ".txt";
        kineWriter = new StreamWriter(path, false);
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
                        values[0], values[1],
                        values[2], values[3], values[4]
                    ));
                Debug.Log("Goupe: " + theTrials[i].group + "Participant: " + theTrials[i].participant +
                          "collabEnvironememn: " + theTrials[i].collabEnvironememnt + "moveMode: " + theTrials[i].moveMode + "cardToTag: " + theTrials[i].cardToTag);

                theTrials[i].pathLog = path;

                theTrials[i].kineWriter = kineWriter;

                i++;
            }
        }
        curentTrial = theTrials[trialNb];

        kineWriter.WriteLine(curentTrial.group + " " + curentTrial.participant + " kine action");
        kineWriter.Flush();
    }

    public void startTrials()
    {
        theTrials[trialNb].startTrial();
        curentTrial = theTrials[trialNb];
    }

    public void nextTrial()
    {
        Debug.Log("Trial count" + theTrials.Count + " curent nb " + trialNb);
        if(trialNb == theTrials.Count-1)
        {
            writer.WriteLine(
            // "factor"
            theTrials[trialNb].group + ";" + theTrials[trialNb].participant + ";" + theTrials[trialNb].collabEnvironememnt + ";" + theTrials[trialNb].moveMode + ";" + theTrials[trialNb].cardToTag + ";"
            // measure
            + theTrials[trialNb].nbTag + ";" + theTrials[trialNb].nbChangeTag
            );
            writer.Flush();
            Finished();
        }
        else
        {
            writer.WriteLine(
            // "factor"
            theTrials[trialNb].group + ";" + theTrials[trialNb].participant + ";" + theTrials[trialNb].collabEnvironememnt + ";" + theTrials[trialNb].moveMode + ";" + theTrials[trialNb].cardToTag + ";"
            // measure
            + theTrials[trialNb].nbTag + ";" + theTrials[trialNb].nbChangeTag
            );
            writer.Flush();

            incTrialNb();
            theTrials[trialNb].startTrial();
            curentTrial = theTrials[trialNb];
            if(trialNb-2 >= 0)
            {
                theTrials[trialNb - 2].card.transform.GetChild(1).gameObject.SetActive(false);
            }
        }

    }

    public void Finished()
    {
        expeRunning = false;
        writer.Close();
        kineWriter.Close();
    }

    public void incTrialNb()
    {
        trialNb += 1;
    }

}
