using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Photon.Pun;

public class Network_Player : MonoBehaviourPun
{
    // empty
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;
    public Transform torse;
    public Transform ray;
    public Transform pos;

    //avatar
    public Transform headSphere;
    public Transform leftHandSphere;
    public Transform rightHandSphere;
    public Transform palette;
    public Transform rayCast;
    public Transform circle;

    //Tag color
    public Material blue;
    public Material green;
    public Material white;
    public Material red;
    public Material none;

    //camera tracker
    private GameObject headset;
    private GameObject right;
    private GameObject left;

    //room + wall
    private GameObject salle;

    //private PhotonView photonView;

    private RaycastHit hit;
    public string nameR="";
    private string nameT="";
    private SteamVR_Behaviour_Pose m_pose = null;
    public SteamVR_Action_Boolean interactWithUI = SteamVR_Input.GetBooleanAction("InteractUI");

    private bool synctag = true;
    private bool modeMove = true;



    expe expe;

    void Start()
    {
        //photonView = GetComponent<PhotonView>();

        //room + wall + camera
        headset = GameObject.Find("Camera (eye)");
        right = GameObject.Find("/[CameraRig]/Controller (right)");
        left = GameObject.Find("/[CameraRig]/Controller (left)");

        salle = GameObject.Find("Salle");

        m_pose = right.GetComponent<SteamVR_Behaviour_Pose>();

        leftHand.gameObject.SetActive(false);

        if (photonView.IsMine)
        {
            //don't show my avatar
            leftHand.gameObject.SetActive(false);
            rightHand.gameObject.SetActive(false);
            head.gameObject.SetActive(false);
            torse.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (expe == null)
        {
            expe = GameObject.Find("/Salle").GetComponent<rendering>().expe;
        }

        synctag = GameObject.Find("/[CameraRig]/Controller (right)").GetComponent<Teleporter>().synctag;
        Ray ray = new Ray(right.transform.position, right.transform.forward);
        if (photonView.IsMine)
        {
            // end the position and rotation over the network
            MapPosition();
        }

        if (Physics.Raycast(ray, out hit))
        {
            //change tag color of the ray cast
            if (interactWithUI.GetStateDown(m_pose.inputSource))
            {

                if (hit.transform.tag == "Color tag")
                {
                    if (synctag)
                    {
                        nameR = hit.transform.GetComponent<Renderer>().material.name;
                        photonView.RPC("ChangeRayColour", Photon.Pun.RpcTarget.All, nameR);
                        right.GetComponent<PhotonView>().RPC("RayColour", Photon.Pun.RpcTarget.All, nameR);
                        // Debug.Log("tag sync");
                    }
                    else if (photonView.IsMine)
                    {
                        nameR = hit.transform.GetComponent<Renderer>().material.name;
                        photonView.RPC("ChangeRayColour", Photon.Pun.RpcTarget.All, nameR);
                        right.GetComponent<PhotonView>().RPC("RayColour", Photon.Pun.RpcTarget.All, nameR);
                        if (expe != null)
                        {
                            expe.curentTrial.incNbChangeTag(nameR);
                        }
                        // Debug.Log("tag not sync: photonView.IsMine");
                    }
                } else if (hit.transform.tag == "MoveControl")
                {
                    if (palette.Find("CubeMoveMode").GetComponent<Renderer>().material.name == "green (Instance)")
                    {
                        palette.Find("CubeMoveMode").GetComponent<Renderer>().material = red;
                        modeMove = false;
                    } else if (palette.Find("CubeMoveMode").GetComponent<Renderer>().material.name == "red (Instance)")
                    {
                        palette.Find("CubeMoveMode").GetComponent<Renderer>().material = green;
                        modeMove = true;
                    }
                }
            }

        }

    }

    void MapPosition()
    {
        // left hand 
        palette.position = left.transform.position;
        palette.rotation = left.transform.rotation;


        // right hand
        rightHand.position = right.transform.position;
        rightHand.rotation = right.transform.rotation;

        ray.position = right.transform.position;
        ray.rotation = right.transform.rotation;

        // head
        head.position = headset.transform.position;
        head.rotation = headset.transform.rotation;

        // circle
        pos.position = new Vector3(headset.transform.position.x, 0 , headset.transform.position.z);

        // body
        torse.position = headset.transform.position;
    }

    [PunRPC]
    void ChangeRayColour(string nameR)
    {
       // change the ray color 
        if (nameR == "blue (Instance)")
        {
            rayCast.GetComponent<Renderer>().material = blue;
        }
        else if(nameR == "Green (Instance)")
        {
            rayCast.GetComponent<Renderer>().material = green;
        }
        else if(nameR == "Red (Instance)")
        {
            rayCast.GetComponent<Renderer>().material = red;
        }
        else if (nameR == "white (Instance)")
        {
            rayCast.GetComponent<Renderer>().material = white;
        }
        else 
        {
            rayCast.GetComponent<Renderer>().material = none;
        }
    }

    [PunRPC]
    void ChangeTag( int OB)
    {
        {
            // change the tag color of a picture
            if (PhotonView.Find(OB).gameObject.tag != "Card"){ return; }
           
            nameT = rayCast.GetComponent<Renderer>().material.name;
            
              if (nameT == "blue (Instance)")
              {
                PhotonView.Find(OB).gameObject.transform.GetChild(0).GetComponent<Renderer>().material = blue;
              }
              else if (nameT == "Green (Instance)")
              {
                PhotonView.Find(OB).gameObject.transform.GetChild(0).GetComponent<Renderer>().material = green;
              }
              else if (nameT == "Red (Instance)")
              {
                PhotonView.Find(OB).gameObject.transform.GetChild(0).GetComponent<Renderer>().material = red;
              }
              else if (nameT == "white (Instance)")
              {
                PhotonView.Find(OB).gameObject.transform.GetChild(0).GetComponent<Renderer>().material = white;
              }
             else
             {
                PhotonView.Find(OB).gameObject.transform.GetChild(0).GetComponent<Renderer>().material = none;
             }
        }

    }
    [PunRPC]
    void removeTag(int OB)
    {
        if (PhotonView.Find(OB).gameObject.tag != "Card") { return; }

        PhotonView.Find(OB).gameObject.transform.GetChild(0).GetComponent<Renderer>().material = none;
     
    }

    [PunRPC]
    void tagMode(string tag)
    {
        Debug.Log("Change tag mode");
        if (tag == "syncro tag")
        {
            synctag = true;
        }
        else
        {
            synctag = false;
        }
        Debug.Log("tag mode"+synctag);
    }

}
