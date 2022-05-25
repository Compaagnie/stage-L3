using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Photon.Pun;
using UnityEngine.UI;
using System.Windows;
using System;

public class Teleporter : MonoBehaviour
{
    public GameObject Menu;

    public GameObject tpsync;
    public GameObject tpNotsync;
    public GameObject tagsync;
    public GameObject tagNotsync;

    public GameObject Cube;
    public GameObject CubePlayer;

    public Transform MurB;
    public Transform MurR;
    public Transform MurL;

    // intersecion raycast and object
    public GameObject m_Pointer;
    private bool m_HasPosition = false;
    RaycastHit hit;
    RaycastHit[] objectHit;
 

    //clic touchpad
    public SteamVR_Action_Boolean m_TeleportAction;
    //trigger
    public SteamVR_Action_Boolean interactWithUI = SteamVR_Input.GetBooleanAction("InteractUI");
    //public SteamVR_Action_Boolean m_up;
    //public SteamVR_Action_Boolean m_down;

    //Pose
    private SteamVR_Behaviour_Pose m_pose = null;

    //Teleportation parameters 
    private bool m_IsTeleportoting = false;
    private readonly float m_FadeTime = 0.5f;
    
    // State machine
    private bool wait = false;
    private bool isMoving = false;
    private bool longclic = false;
    private readonly bool doubleclick = false;
    private Vector3 coordClic;
    private Vector3 coordPrev;
    private Vector3 forwardClic;
    private Vector3 oldControlerRotation;
    private Vector3 oldHitPosition;
    private const float moveSpeed = 0.01f;
    Vector3 plusX = new Vector3(moveSpeed, 0f, 0f);
    Vector3 minusX = new Vector3(-moveSpeed, 0f, 0f);
    Vector3 plusZ = new Vector3(0f, 0f, moveSpeed);
    Vector3 minusZ = new Vector3(0f, 0f, -moveSpeed);

    private float timer = 0;

    public bool syncTeleportation = false;
    private string teleporationMode = "Not syncro";
    readonly float desiredDistance = 1;


    private bool n = false;
    private bool s = false;
    private bool e = false;
    private bool w = false;
    private string moveMode = "drag";
    private bool isOtherSynced = false;
    public bool synctag = true;
   

    int nbClick = 0;

    Vector2 position;


    private PhotonView photonView;
    //player
    private GameObject player;
    public Transform cameraRig;
    public Transform cam;
    public Transform CameraRotator;
    public Transform ControllerRotator;
    public Transform controllerRight;
    public Transform controllerLeft;
    private float initialCamRotationY;
    expe expe;


    // Start is called before the first frame update
    void Awake()
    {
        m_pose = GetComponent<SteamVR_Behaviour_Pose>();
        photonView = GetComponent<PhotonView>();
        Menu.SetActive(false);
       

        
        
    }

    // Update is called once per frame
    void Update()
    {
        /*
        Debug.Log("Cam rotation" + cam.eulerAngles.y);
        Debug.Log("Frame num " + Time.frameCount);*/

        if (Time.frameCount == 3 && (cam.rotation.eulerAngles.y < 315 || cam.rotation.eulerAngles.y > 45))
        {
            initialCamRotationY = cam.rotation.eulerAngles.y;
            Debug.Log("Camera initial rotation");
            cameraRig.RotateAround(cameraRig.position, Vector3.up, initialCamRotationY);
            CameraRotator.RotateAround(cameraRig.position, Vector3.up, -initialCamRotationY);
            ControllerRotator.RotateAround(cameraRig.position, Vector3.up, -initialCamRotationY);
            Debug.Log("Camera:"+CameraRotator.rotation.eulerAngles.y+" Ctrl:"+ControllerRotator.rotation.eulerAngles.y);
        }

        if (expe == null)
        {
            expe = GameObject.Find("/Salle").GetComponent<rendering>().expe;
        }
        //Pointer
        m_HasPosition = UpdatePointer();

        if (interactWithUI.GetStateDown(m_pose.inputSource) && m_HasPosition)
        {
            if (hit.transform.tag == "MoveControlTP" && moveMode != "TP")
            {
                if (moveMode == "sync")
                {
                    photonView.RPC("toggleOtherSync", Photon.Pun.RpcTarget.Others);
                }
                moveMode = "TP";
            }
            else if (hit.transform.tag == "MoveControlJoy" && moveMode != "joy")
            {
                if (moveMode == "sync")
                {
                    photonView.RPC("toggleOtherSync", Photon.Pun.RpcTarget.Others);
                }
                moveMode = "joy";
            }
            else if (hit.transform.tag == "MoveControlDrag" && moveMode != "drag")
            {
                if(moveMode == "sync")
                {
                    photonView.RPC("toggleOtherSync", Photon.Pun.RpcTarget.Others);
                }
                moveMode = "drag";
            }
            else if (hit.transform.tag == "MoveControlSync" && !isOtherSynced && moveMode != "sync")
            {
                moveMode = "sync";
                photonView.RPC("toggleOtherSync", Photon.Pun.RpcTarget.Others);
            }
        }

        if (syncTeleportation)
        {
            tpsync.SetActive(false);
            tpNotsync.SetActive(true);
           // Cube.SetActive(true);
        }
        if (!syncTeleportation)
        {
            tpNotsync.SetActive(false);
            tpsync.SetActive(true);
            Cube.SetActive(false);

        }
        if (synctag)
        {
            tagsync.SetActive(false);
            tagNotsync.SetActive(true);
        }
        if (!synctag)
        {
            tagNotsync.SetActive(false);
            tagsync.SetActive(true);
        }

        //Teleport
        position = SteamVR_Actions.default_Pos.GetAxis(SteamVR_Input_Sources.Any);
        if (m_TeleportAction.GetStateDown(m_pose.inputSource))
        {
            oldControlerRotation = controllerRight.transform.rotation.eulerAngles;
            oldHitPosition = m_Pointer.transform.position;
            //Debug.Log("update old"+ oldControlerRotation
            // head position + camera rig
        }
        if (moveMode != "sync")
        {
            if (moveMode == "TP")
            {
                if (m_TeleportAction.GetStateDown(m_pose.inputSource))
                {
                    if (position.x < -0.5)
                    {
                        Debug.Log("W");
                        w = true;
                        tryTeleport();
                    }
                    /*
                    else if(position.y > 0.5)
                    {
                        Debug.Log("N");
                        n = true;
                        tryTeleport();
                    }
                    else if (position.y < -0.5)
                    {
                        Debug.Log("S");
                        s = true;
                        tryTeleport();
                    }
                    */
                    else if (position.x > 0.5)
                    {
                        Debug.Log("E");
                        e = true;
                        tryTeleport();
                    }

                    else
                    {
                        nbClick++;
                        //Debug.Log("C");
                        coordClic = coordPrev = m_Pointer.transform.position; //hit.transform.position;
                        forwardClic = transform.forward;
                        wait = true;
                        timer = Time.time;
                    }
                }


                if (wait)
                {
                    if (Time.time - timer > 0.7) //  after 0.7s it is long clic
                    {
                        longclic = true;
                        wait = false;
                        // Debug.Log("long clic");
                    }
                }
                if (longclic)
                {
                    syncTeleportation = true;
                    tryTeleport();
                    if (expe != null)
                    {
                        expe.curentTrial.incNbSyncTp();
                    }
                    //syncTeleportation = false;
                    longclic = false;
                }

                if (m_TeleportAction.GetStateUp(m_pose.inputSource))
                {

                    //Debug.Log("reset");

                    isMoving = false;
                    longclic = false;
                    n = false;
                    s = false;
                    e = false;
                    w = false;

                    if (wait)
                    {
                        tryTeleport();
                    }
                    wait = false;
                    longclic = false;
                }

                if (isMoving)
                {
                    //dragTeleport(coordPrev, m_Pointer.transform.position);
                    coordPrev = m_Pointer.transform.position;
                }


                // MENU //
                if (UpdatePointer() == true && hit.transform.name == "syncro")
                {
                    // Debug.Log("Syncro");
                    Menu.SetActive(false);
                    photonView.RPC("teleportationMode", Photon.Pun.RpcTarget.All, syncTeleportation);
                }

                if (UpdatePointer() == true && hit.transform.name == "not syncro")
                {
                    // Debug.Log("Not Syncro");
                    Menu.SetActive(false);
                    photonView.RPC("teleportationMode", Photon.Pun.RpcTarget.All, syncTeleportation);
                }

                if (UpdatePointer() == true && hit.transform.name == "syncro tag")
                {
                    // Debug.Log("Syncro");
                    Menu.SetActive(false);
                    player = GameObject.Find("Network Player(Clone)");
                    synctag = true;
                    photonView.RPC("tagMode", Photon.Pun.RpcTarget.All, synctag);
                }

                if (UpdatePointer() == true && hit.transform.name == "not syncro tag")
                {
                    // Debug.Log("Not Syncro");
                    Menu.SetActive(false);
                    player = GameObject.Find("Network Player(Clone)");
                    synctag = false;
                    photonView.RPC("tagMode", Photon.Pun.RpcTarget.All, synctag);
                }

                if (UpdatePointer() == true && hit.transform.name == "cancel")
                {
                    // Debug.Log("Cancel");
                    Menu.SetActive(false);
                }
            }
            else if (moveMode == "joy")
            {
                if (m_TeleportAction.GetState(m_pose.inputSource))
                {
                    Quaternion rotation = Quaternion.Euler(controllerRight.rotation.eulerAngles);
                    Matrix4x4 m = Matrix4x4.Rotate(rotation);
                    Vector3 translateVect = new Vector3(0, 0, 0);
                    if (position.x < -0.5)
                    {
                        //translateVect = m.MultiplyPoint3x4(minusX);
                        cameraRig.RotateAround(cam.transform.position, Vector3.up, -0.35f);
                        if (isOtherSynced)
                        {
                            photonView.RPC("MoveRigFromTransform", Photon.Pun.RpcTarget.Others, translateVect, -0.35f);
                        }
                    }
                    if (position.y > 0.5)
                    {
                        translateVect = m.MultiplyPoint3x4(plusZ);
                        /* Rotation de la camera dans la direction du pointeur, à décommenter avec le if getStateUp
                        
                        Vector3 camAngle = new Vector3(cam.transform.forward.x, 0, cam.transform.forward.z);
                        Vector3 ctrlAngle = new Vector3(controllerRight.transform.forward.x, 0, controllerRight.transform.forward.z);

                        double crossProduct = Vector3.Cross(camAngle, ctrlAngle).y;
                        Debug.Log(crossProduct);
                        
                        if (crossProduct > 0)
                        {
                            CameraRotator.RotateAround(cam.transform.position, Vector3.up, 0.25f);

                        }
                        else if (crossProduct < 0)
                        {
                            CameraRotator.RotateAround(cam.transform.position, Vector3.up, -0.25f);
                        }*/
                    }
                    if (position.y < -0.5)
                    {
                        translateVect = m.MultiplyPoint3x4(minusZ);
                    }
                    if (position.x > 0.5)
                    {
                        //translateVect = m.MultiplyPoint3x4(plusX);
                        cameraRig.RotateAround(cam.transform.position, Vector3.up, 0.35f);
                        if (isOtherSynced)
                        {
                            photonView.RPC("MoveRigFromTransform", Photon.Pun.RpcTarget.Others, translateVect, 0.35f);
                        }
                    }
                    translateVect.y = 0;
                    if (cam.position.x + translateVect.x < -3.5) { translateVect.x = -3.5f - cam.position.x; }
                    if (cam.position.x + translateVect.x > 3.5) { translateVect.x = 3.5f - cam.position.x; }
                    if (cam.position.z + translateVect.z < -3.5) { translateVect.z = -3.5f - cam.position.z; }
                    if (cam.position.z + translateVect.z > 3.5) { translateVect.z = 3.5f - cam.position.z; }
                    cameraRig.position += translateVect;
                    if (isOtherSynced)
                    {
                        photonView.RPC("MoveRigFromTransform", Photon.Pun.RpcTarget.Others, translateVect, 0f);
                    }
                }
                /*
                // if getStateUp,  "Annulation" de la rotation de la caméra pour repositionner les controllers au bon endroit
                if (m_TeleportAction.GetStateUp(m_pose.inputSource) && position.y > 0.5)
                {
                    float tmp = CameraRotator.localEulerAngles.y + initialCamRotationY;
                    Debug.Log("Avant annulation :\n"+tmp);
                    CameraRotator.RotateAround(cam.transform.position, Vector3.up, -tmp);

                    cameraRig.RotateAround(cam.transform.position, Vector3.up, tmp);
                    Debug.Log("Après annulation :\n" + CameraRotator.localEulerAngles.y);

                }
                */

            }
            else if (moveMode == "drag")
            {
                if (m_TeleportAction.GetState(m_pose.inputSource))
                {
                    Vector3 translateVect = new Vector3(0, 0, 0);
                    //Debug.Log(m_HasPosition + hit.transform.tag);
                    if (m_HasPosition && hit.transform.tag == "TpLimit")
                    {
                        float b = Mathf.Tan((90 - controllerRight.rotation.eulerAngles.x) * Mathf.PI / 180) * controllerRight.transform.position.y;
                        //Debug.Log("b: " + b);
                        Vector3 camToHit = oldHitPosition - cam.position;
                        Vector3 ctrlToHit = oldHitPosition - controllerRight.position;
                        //Debug.Log(camToHit.z);
                        camToHit.y = 0;
                        ctrlToHit.y = 0;
                        float c = camToHit.magnitude * (ctrlToHit.magnitude - b) / ctrlToHit.magnitude;
                        translateVect = camToHit.normalized * c;
                        //Debug.Log(c);
                        if (cam.position.x + translateVect.x < -3.5) { translateVect.x = -3.5f - cam.position.x; }
                        if (cam.position.x + translateVect.x > 3.5) { translateVect.x = 3.5f - cam.position.x; }
                        if (cam.position.z + translateVect.z < -3.5) { translateVect.z = -3.5f - cam.position.z; }
                        if (cam.position.z + translateVect.z > 3.5) { translateVect.z = 3.5f - cam.position.z; }
                        cameraRig.position += translateVect;
                        if (isOtherSynced)
                        {
                            photonView.RPC("MoveRigFromTransform", Photon.Pun.RpcTarget.Others, translateVect, 0f);
                        }

                        //cameraRig.position += a - a.normalized*b;
                    }
                    else if (m_HasPosition && hit.transform.tag == "Wall")
                    {
                        //Debug.Log(oldControlerRotation.y + "  " + controlerRight.transform.rotation.eulerAngles.y);
                        /*

                        */
                        Transform mur;
                        float camToHitOnWall, ctrlToHit, b, distMur;

                        if (hit.transform.name == "MUR B" || hit.transform.parent.name == "MUR B")
                        {
                            mur = MurB;
                            distMur = Mathf.Abs(mur.position.z - controllerRight.position.z);
                            if (Mathf.Round(controllerRight.rotation.eulerAngles.y - mur.rotation.eulerAngles.y) != 0)
                            {
                                b = Mathf.Tan((controllerRight.rotation.eulerAngles.y - mur.rotation.eulerAngles.y) * Mathf.PI / 180) * distMur;
                            }
                            else
                            {
                                b = 0;
                            }
                            camToHitOnWall = oldHitPosition.x - cam.position.x;
                            ctrlToHit = oldHitPosition.x - controllerRight.position.x;
                            translateVect.x = 1.0f;

                        }
                        else if (hit.transform.name == "MUR R" || hit.transform.parent.name == "MUR R")
                        {
                            mur = MurR;
                            distMur = Mathf.Abs(mur.position.x - controllerRight.position.x);
                            b = -Mathf.Tan((controllerRight.rotation.eulerAngles.y + mur.rotation.eulerAngles.y) * Mathf.PI / 180) * distMur;
                            camToHitOnWall = oldHitPosition.z - cam.position.z;
                            ctrlToHit = oldHitPosition.z - controllerRight.position.z;
                            translateVect.z = 1.0f;

                        }
                        else
                        {
                            mur = MurL;
                            distMur = Mathf.Abs(mur.position.x - controllerRight.position.x);
                            b = Mathf.Tan((controllerRight.rotation.eulerAngles.y - mur.rotation.eulerAngles.y) * Mathf.PI / 180) * distMur;
                            camToHitOnWall = oldHitPosition.z - cam.position.z;
                            ctrlToHit = oldHitPosition.z - controllerRight.position.z;
                            translateVect.z = 1.0f;

                        }
                        Debug.Log("b: " + b);
                        Debug.Log("Tan : " + Mathf.Tan((controllerRight.rotation.eulerAngles.y - mur.rotation.eulerAngles.y) * Mathf.PI / 180));
                        //Debug.Log(camToHitOnWall);

                        float c = camToHitOnWall * (ctrlToHit - b) / ctrlToHit;
                        translateVect *= c;
                        //Debug.Log(translateVect);
                        if (cam.position.x + translateVect.x < -3.5) { translateVect.x = -3.5f - cam.position.x; }
                        if (cam.position.x + translateVect.x > 3.5) { translateVect.x = 3.5f - cam.position.x; }
                        if (cam.position.z + translateVect.z < -3.5) { translateVect.z = -3.5f - cam.position.z; }
                        if (cam.position.z + translateVect.z > 3.5) { translateVect.z = 3.5f - cam.position.z; }
                        cameraRig.position += translateVect;
                        if (isOtherSynced)
                        {
                            photonView.RPC("MoveRigFromTransform", Photon.Pun.RpcTarget.Others, translateVect, 0f);
                        }
                    }
                    else
                    {
                        float angle = oldControlerRotation.y - controllerRight.transform.rotation.eulerAngles.y;
                        cameraRig.RotateAround(cam.transform.position, Vector3.up, angle);
                        CubePlayer.transform.RotateAround(CubePlayer.transform.position, Vector3.up, angle);
                        if (isOtherSynced)
                        {
                            photonView.RPC("MoveRigFromTransform", Photon.Pun.RpcTarget.Others, translateVect, angle);
                        }
                        oldControlerRotation = controllerRight.transform.rotation.eulerAngles;
                        oldHitPosition = m_Pointer.transform.position;
                    }
                }

            }
        }
    }

    private void tryTeleport()
    {

        //if no hit stop the fonction
        if ( m_IsTeleportoting) //!m_HasPosition ||
            return;

        // head position + camera rig
        Vector3 headPosition = SteamVR_Render.Top().head.position;
        //Transform cameraRig = SteamVR_Render.Top().origin;
      
        //player possition
        Vector3 groundPosition = new Vector3(headPosition.x, cameraRig.position.y, headPosition.z);
        if (e)
        {
            if (!syncTeleportation)
            {
                cameraRig.RotateAround(cam.transform.position, Vector3.up, 90);
                CubePlayer.transform.RotateAround(CubePlayer.transform.position, Vector3.up, 90);
            }
            else
            {
                photonView.RPC("RotationRigRPC", Photon.Pun.RpcTarget.All, "e");
            }
            return;
        }
        else if (w)
        {
            if (!syncTeleportation)
            {
                cameraRig.RotateAround(cam.transform.position, Vector3.up, -90);
                CubePlayer.transform.RotateAround(CubePlayer.transform.position, Vector3.up, -90);
            }
            else
            {
                photonView.RPC("RotationRigRPC", Photon.Pun.RpcTarget.All, "w");
            }
            return;
        }
        Vector3 translateVector;
        /*
        if (n)
        {
            //translateVector =  character.transform.forward * desiredDistance; // y not fix
            translateVector = new Vector3(character.transform.forward.x * desiredDistance, cameraRig.position.y, character.transform.forward.z * desiredDistance);  //  y fix
                                                                                                                                                                    // StartCoroutine(MoveRig(cameraRig, translateVector));
            if (!syncTeleportation)
            {
                StartCoroutine(MoveRig(cameraRig, translateVector));
            }
            else
            {
                photonView.RPC("MoveRigRPC", Photon.Pun.RpcTarget.All, cameraRig.gameObject.GetComponent<PhotonView>().ViewID, translateVector);
            }
        }
        else if (s)
        {
            //translateVector =  - character.transform.forward * desiredDistance;
            translateVector = new Vector3(-character.transform.forward.x * desiredDistance, cameraRig.position.y, -character.transform.forward.z * desiredDistance);  //  y fix
                                                                                                                                                                      //StartCoroutine(MoveRig(cameraRig, translateVector));
            if (!syncTeleportation)
            {
                StartCoroutine(MoveRig(cameraRig, translateVector));
            }
            else
            {
                photonView.RPC("MoveRigRPC", Photon.Pun.RpcTarget.All, cameraRig.gameObject.GetComponent<PhotonView>().ViewID, translateVector);
            }
        }
        */
        if (!m_HasPosition) // ||
            return;


        
        
        else if (hit.transform.tag == "Tp" || hit.transform.tag == "TpLimit" )
        {
            Vector3 posPointer = m_Pointer.transform.position;
            if (posPointer.x < -3.5) { posPointer.x = -3.5f; }
            if (posPointer.x >  3.5) { posPointer.x =  3.5f; }
            if (posPointer.z < -3.5) { posPointer.z = -3.5f; }
            if (posPointer.z >  3.5) { posPointer.z =  3.5f; }
            translateVector = posPointer - groundPosition;

            if (!syncTeleportation)
            {
                if (expe != null)
                {
                    expe.curentTrial.incNbAsyncTPGround(translateVector);
                }
            }
            else
            {
                if (expe != null)
                {
                    expe.curentTrial.incNbSyncTpGround(translateVector);
                }
            }
            StartCoroutine(MoveRig(cameraRig, translateVector));
        }
        else if (hit.transform.tag == "Wall" || hit.transform.tag == "Card")
        {
            translateVector = new Vector3(0, 0, 0);
            Vector3 camLookDirection = new Vector3(cam.transform.forward.x, 0, cam.transform.forward.z);
            objectHit = Physics.RaycastAll(cameraRig.transform.position, cameraRig.transform.forward, 100.0F);
            float x = -cameraRig.transform.position.x;
            float z = -cameraRig.transform.position.z;
            //check the wall
            if (hit.transform.name == "MUR B" || hit.transform.parent.name == "MUR B")
            {
                for (int i = 0; i < objectHit.Length; i++)
                {
                    //Debug.Log("objHit : " + objectHit[i].transform.name);
                    if (objectHit[i].transform.name == "MUR B" || objectHit[i].transform.parent.name == "MUR B")
                    {
                        translateVector = new Vector3(m_Pointer.transform.position.x - groundPosition.x, 0, 0);
                    }
                    else if (objectHit[i].transform.name == "MUR L" || objectHit[i].transform.parent.name == "MUR L")
                    {
                        //Debug.Log("need to rotate w 1 time");
                        cameraRig.RotateAround(cam.transform.position, Vector3.up, 90);
                        translateVector = new Vector3(m_Pointer.transform.position.x - groundPosition.x, 0, z + Mathf.Abs(x));
                    }
                    else if (objectHit[i].transform.name == "MUR R" || objectHit[i].transform.parent.name == "MUR R")
                    {
                        // Debug.Log("need to rotate w 1 times");
                        cameraRig.RotateAround(cam.transform.position, Vector3.up, -90);
                        translateVector = new Vector3(m_Pointer.transform.position.x - groundPosition.x, 0, z + Mathf.Abs(x));
                    }
                    else
                    {
                        cameraRig.RotateAround(cam.transform.position, Vector3.up, 180);
                    }
                }
                //objectHit = Physics.RaycastAll(cameraRig.transform.position, -cameraRig.transform.forward, 100.0F);
                /*
                for (int i = 0; i < objectHit.Length; i++)
                {
                    if (objectHit[i].transform.name == "MUR B" || objectHit[i].transform.parent.name == "MUR B")
                    {
                        cameraRig.RotateAround(cam.transform.position, Vector3.up, -180);
                    }
                }
                */
            }
            else if (hit.transform.name == "MUR R" || hit.transform.parent.name == "MUR R")
            {
                for (int i = 0; i < objectHit.Length; i++)
                { 
                    //Debug.Log("objHit : " + objectHit[i].transform.name);
                   
                    if (objectHit[i].transform.name == "MUR R" || objectHit[i].transform.parent.name == "MUR R")
                    {
                        translateVector = new Vector3(0, 0, m_Pointer.transform.position.z- groundPosition.z);
                    }
                    else if (objectHit[i].transform.name == "MUR B" || objectHit[i].transform.parent.name == "MUR B")
                    {
                        //Debug.Log("need to rotate e 1 time");
                        cameraRig.RotateAround(cam.transform.position, Vector3.up, 90);
                        translateVector = new Vector3(x + Mathf.Abs(z), 0, m_Pointer.transform.position.z - groundPosition.z);
                    }
                    else if (objectHit[i].transform.name == "MUR L" || objectHit[i].transform.parent.name == "MUR L")
                    {
                        //Debug.Log("need to rotate w 2 times");
                        cameraRig.RotateAround(cam.transform.position, Vector3.up, 180);
                        translateVector = new Vector3(-2 * groundPosition.x, 0, m_Pointer.transform.position.z - groundPosition.z);
                    }
                    else
                    {
                        cameraRig.RotateAround(cam.transform.position, Vector3.up, -90);
                    }
                }
                //objectHit = Physics.RaycastAll(cameraRig.transform.position, -cameraRig.transform.forward, 100.0F);
                /*
                for (int i = 0; i < objectHit.Length; i++)
                {
                    if (objectHit[i].transform.name == "MUR B" || objectHit[i].transform.parent.name == "MUR B")
                    {
                        cameraRig.RotateAround(cam.transform.position, Vector3.up, -90);
                    }
                }
                */
            }
            else //(hit.transform.name == "MUR L" || hit.transform.parent.name == "MUR L")
            {
                for (int i = 0; i < objectHit.Length; i++)
                {
                    //cameraRig.rotation = new Quaternion(0.0f, -0.7f, 0.0f, 0.7f);
                    //Debug.Log("objHit : " + objectHit[i].transform.name);

                    if (objectHit[i].transform.name == "MUR L" || objectHit[i].transform.parent.name == "MUR L")
                    {
                        translateVector = new Vector3(0, 0, m_Pointer.transform.position.z - groundPosition.z);
                    }
                    else if (objectHit[i].transform.name == "MUR B" || objectHit[i].transform.parent.name == "MUR B")
                    {
                        //Debug.Log("need to rotate w 1 time");
                            
                        cameraRig.RotateAround(cam.transform.position, Vector3.up, -90);
                        translateVector = new Vector3(x - Mathf.Abs(z), 0, m_Pointer.transform.position.z - groundPosition.z);
                    }
                    else if (objectHit[i].transform.name == "MUR R" || objectHit[i].transform.parent.name == "MUR R")
                    {
                        //Debug.Log("need to rotate w 2 times");
                        cameraRig.RotateAround(cam.transform.position, Vector3.up, 180);
                        translateVector = new Vector3(-2 * groundPosition.x, 0, m_Pointer.transform.position.z - groundPosition.z);
                    }
                    else
                    {
                        cameraRig.RotateAround(cam.transform.position, Vector3.up, 90);
                    }
                }
                //objectHit = Physics.RaycastAll(cameraRig.transform.position, -cameraRig.transform.forward, 100.0F);
                /*
                for (int i = 0; i < objectHit.Length; i++)
                {
                    if (objectHit[i].transform.name == "MUR B" || objectHit[i].transform.parent.name == "MUR B")
                    {
                        cameraRig.RotateAround(cam.transform.position, Vector3.up, 90);
                    }
                }
                */

            }

            //then teleport
            if (expe != null)
            {
                if (!syncTeleportation)
                {
                    expe.curentTrial.incNbAsyncTPWall(translateVector);
                }
                else
                {
                    expe.curentTrial.incNbSyncTpWall(translateVector);
                }
            }
            StartCoroutine(MoveRig(cameraRig, translateVector));

        }
        else if (hit.transform.tag == "Player")
        {
            Debug.Log(hit.collider.transform.parent.parent);
            Vector3 otherPlayerPos = hit.collider.transform.parent.transform.position;
            Vector3 otherPlayerRotation = hit.collider.transform.parent.parent.Find("Head").rotation.eulerAngles;
            Debug.Log(otherPlayerRotation);
            if (Math.Round(otherPlayerPos.x,3) != Math.Round(cam.localPosition.x+cameraRig.position.x,3) && Math.Round(otherPlayerPos.z,3) != Math.Round(cam.localPosition.z+cameraRig.position.z,3))
            {
                if (otherPlayerRotation.y >= 225 && otherPlayerRotation.y <= 315)
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        otherPlayerPos.z += 1;
                    }
                    else
                    {
                        otherPlayerPos.z -= 1;
                    }
                }
                //B
                else if (otherPlayerRotation.y >= 135 && otherPlayerRotation.y <= 225)
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        otherPlayerPos.x -= 1;
                    }
                    else
                    {
                        otherPlayerPos.x += 1;
                    }
                }
                //L
                else if (otherPlayerRotation.y <= 135 && otherPlayerRotation.y >= 45)
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        otherPlayerPos.z -= 1;
                    }
                    else
                    {
                        otherPlayerPos.z += 1;
                    }
                }
                //no wall
                else
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        otherPlayerPos.x += 1;
                    }
                    else
                    {
                        otherPlayerPos.x -= 1;
                    }
                }
                StartCoroutine(MoveRigForSyncTP(cameraRig, otherPlayerPos, otherPlayerRotation));
            }

        }
    }

    [PunRPC]
    void MoveRigRPC(int cameraRig, Vector3 pos, Vector3 rotat)
    {
        // StartCoroutine(MoveRig(PhotonView.Find(cameraRig).transform, translation));

        StartCoroutine(MoveRigForSyncTP(PhotonView.Find(cameraRig).transform, pos , rotat));
        
    }

    [PunRPC]
    void MoveRigFromTransform(Vector3 translation, float rotation)
    {
        cameraRig.position += translation;
        cameraRig.RotateAround(cam.position, Vector3.up, rotation);
    }

    [PunRPC]
    void toggleOtherSync()
    {
        isOtherSynced = !isOtherSynced;
    }

    [PunRPC]
    void RotationRigRPC(string s)
    {
        Transform cameraRig2 = SteamVR_Render.Top().origin;

        Debug.Log("test ");
        if (s == "e")
        {
            Cube.transform.RotateAround(Cube.transform.position, Vector3.up, 90);
            cameraRig2.RotateAround(Cube.transform.position, Vector3.up, 90);

        }
        else if (s == "w")
        {
            Cube.transform.RotateAround(Cube.transform.position, Vector3.up, -90);
            cameraRig2.RotateAround(Cube.transform.position, Vector3.up, -90);
        }
        
    }

    [PunRPC]
    void teleportationMode(bool tp)
    {
        Debug.Log("Change teleportation mode");
        if (tp)
        {
            syncTeleportation = false;
            teleporationMode = "Not syncro";
        }
        else
        {
            syncTeleportation = true;
            teleporationMode = "Syncro";
        }
        
    }

    [PunRPC]
    void tagMode(bool tag)
    {
       // Debug.Log("Change tag mode");
        synctag = tag;
    }

    private IEnumerator MoveRig(Transform cameraRig , Vector3 translation)
    {
        m_IsTeleportoting = true;

        SteamVR_Fade.Start(Color.black, m_FadeTime, true); // black screen

        yield return new WaitForSeconds( m_FadeTime); // fade time

        if (cam.position.x + translation.x < -3.5) { translation.x = -3.5f - cam.position.x; }
        if (cam.position.x + translation.x > 3.5) { translation.x = 3.5f - cam.position.x; }
        if (cam.position.z + translation.z < -3.5) { translation.z = -3.5f - cam.position.z; }
        if (cam.position.z + translation.z > 3.5) { translation.z = 3.5f - cam.position.z; }
        cameraRig.position += translation;

        Debug.Log("camera rig pos tp :" +cameraRig.position);
        if (syncTeleportation || isOtherSynced)
        {
            Cube.transform.position += translation; // teleportation

            Vector3 rotat = SteamVR_Render.Top().origin.rotation.eulerAngles;
            Debug.Log("rotation" +rotat);
            Vector3 headPosition = SteamVR_Render.Top().head.position;
            Vector3 playerPos = new Vector3(headPosition.x, cameraRig.position.y, headPosition.z);
            Debug.Log(playerPos);
            //R
            if (rotat.y >= 225 && rotat.y <= 315)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    playerPos.z += 1;
                }
                else
                {
                    playerPos.z -= 1;
                }
            }
            //B
            else if (rotat.y >= 135 && rotat.y <= 225)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    playerPos.x += 1;
                }
                else
                {
                    playerPos.x -= 1;
                }
            }
            //L
            else if (rotat.y <= 135 && rotat.y >= 45)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    playerPos.z -= 1;
                }
                else
                {
                    playerPos.z += 1;
                }
            }
            //no wall
            else //if (Physics.RaycastAll(player.transform.position, player.transform.forward, 100.0F)[0].transform.name == "MUR R")
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    playerPos.x -= 1;
                }
                else
                {
                    playerPos.x += 1;
                }
            }
            Debug.Log(playerPos);
            photonView.RPC("MoveRigRPC", Photon.Pun.RpcTarget.Others, cameraRig.gameObject.GetComponent<PhotonView>().ViewID, cameraRig.position, rotat);
            syncTeleportation = false;
        }

        SteamVR_Fade.Start(Color.clear, m_FadeTime, true); // normal screen

        m_IsTeleportoting = false;

    }

    private IEnumerator MoveRigForSyncTP(Transform cameraRig, Vector3 pos, Vector3 rotat)
    {
        m_IsTeleportoting = true;

        SteamVR_Fade.Start(Color.black, m_FadeTime, true); // black screen
        yield return new WaitForSeconds(m_FadeTime); // fade time
        // Rotation
        
        cameraRig.RotateAround(cam.position, Vector3.up, rotat.y - cameraRig.rotation.eulerAngles.y);
        cameraRig.position = pos; // teleportation
        if (syncTeleportation || isOtherSynced)
        {
            Cube.transform.position = pos; // teleportation
        }

        SteamVR_Fade.Start(Color.clear, m_FadeTime, true); // normal screen
        m_IsTeleportoting = false;

    }

    private bool UpdatePointer()
    {
        Ray ray = new Ray(transform.position, transform.forward);
       
        //check if there is a hit
        if(Physics.Raycast(ray , out hit) )
        {
            if (hit.transform.tag == "Player" || hit.transform.tag == "MoveControlJoy" || hit.transform.tag == "MoveControlSync" || hit.transform.tag == "MoveControlDrag" || hit.transform.tag == "MoveControlTP" || hit.transform.tag == "Tp" || hit.transform.tag == "TpLimit" || hit.transform.tag == "Card" || hit.transform.tag == "Wall" || hit.transform.tag == "tag")
            {
                m_Pointer.transform.position = hit.point;
                return true;
                
            }
        }
        return false;
    }

    private Vector3 checkAndCorrectColisionBox()
    {
        Vector3 translateVector = new Vector3(0, 0, 0);
        //to do, to avoid colision beetween players
        return translateVector;
    }
}
