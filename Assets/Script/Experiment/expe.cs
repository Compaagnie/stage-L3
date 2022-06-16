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
    public bool trialRunning = false;

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

        theTrials = new List<Trial>();

        foreach (string str in lines)
        {
            List<string> values = new List<string>(str.Split(';'));
            if (values[0] == "#pause")
            {
                theTrials.Add(new Trial(this, values[0], "", "", "", ""));
                Debug.Log("Pause added to trials");
            }
            else if (values[1] == participant)
            {
                theTrials.Add(new Trial(this,
                        values[0], values[1],
                        values[2], values[3], values[4]
                    ));
                Debug.Log("Goupe: " + theTrials[theTrials.Count - 1].group + "Participant: " + theTrials[theTrials.Count - 1].participant +
                          "collabEnvironememn: " + theTrials[theTrials.Count - 1].collabEnvironememnt + "moveMode: " + theTrials[theTrials.Count - 1].moveMode + "cardToTag: " + theTrials[theTrials.Count - 1].cardToTag);

                theTrials[theTrials.Count - 1].pathLog = path;

                theTrials[theTrials.Count - 1].kineWriter = kineWriter;
            }
        }
        curentTrial = theTrials[trialNb];

        kineWriter.WriteLine(curentTrial.group + " " + curentTrial.participant + " kine action");
        kineWriter.Flush();
    }

    public void nextTrial()
    {

        Debug.Log("Trial count" + theTrials.Count + " curent nb " + trialNb);
        if (!trialRunning)
        {
            theTrials[trialNb].startTrial();
            curentTrial = theTrials[trialNb];
            trialRunning = true;
        }
        else if (trialNb == theTrials.Count - 1)
        {
            writer.WriteLine(
            // "factor"
            theTrials[trialNb].group + ";" + theTrials[trialNb].participant + ";" + theTrials[trialNb].collabEnvironememnt + ";" + theTrials[trialNb].moveMode + ";" + theTrials[trialNb].cardToTag + ";"
            // measure
            + theTrials[trialNb].nbTag + ";" + theTrials[trialNb].nbChangeTag
            );
            writer.Flush();
            theTrials[trialNb - 1].card.transform.GetChild(1).gameObject.SetActive(false);
            theTrials[trialNb].card.transform.GetChild(1).gameObject.SetActive(false);
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


            if (theTrials[trialNb].group == "#pause")
            {
                trialRunning = false;
                writer.WriteLine("#pause;");
                writer.Flush();
                incTrialNb();
            }
            else
            {
                theTrials[trialNb].startTrial();
                curentTrial = theTrials[trialNb];
            }
        }
    }

    public void Finished()
    {
        trialRunning = false;
        expeRunning = false;
        writer.Close();
        kineWriter.Close();
    }

    public void incTrialNb()
    {
        trialNb += 1;
        if (trialNb - 2 >= 0 && theTrials[trialNb - 2].group != "pause")
        {
            theTrials[trialNb - 2].card.transform.GetChild(1).gameObject.SetActive(false);
        }
    }

}
