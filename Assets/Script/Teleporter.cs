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
    public GameObject menu;

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
    private GameObject oldHit;
    private float oldFingerX = 0;
    private float oldFingerY = 0;
    private const float moveSpeed = 0.02f;
    Vector3 plusZ = new Vector3(0f, 0f, moveSpeed);
    Vector3 minusZ = new Vector3(0f, 0f, -moveSpeed);
    private const float joystickRotation = 0.5f;
    private float moveTimer;

    private float timer = 0;

    public bool syncTeleportation = false;
    private string teleporationMode = "Not syncro";
    readonly float desiredDistance = 1;

    private bool e = false;
    private bool w = false;
    public string moveMode = "drag";
    public bool isOtherSynced = false;
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
    private float initialCamRotationY = 0;

    private Vector3 otherPlayerPosition = new Vector3(0,0,0);
    private Vector3 otherPlayerRotation = new Vector3(0, 0, 0);
    private Vector3 otherPlayerCameraRigPos = new Vector3(0, 0, 0);
    private Vector3 centerBetweenPlayers = new Vector3(0, 0, 0);

    Expe expe;


    // Start is called before the first frame update
    void Awake()
    {
        m_pose = GetComponent<SteamVR_Behaviour_Pose>();
        photonView = GetComponent<PhotonView>();
        menu.SetActive(false);
        menu.transform.Find("moveModeText").GetComponent<TextMesh>().text = moveMode;
        oldHit = m_Pointer;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.frameCount == 3 && (cam.rotation.eulerAngles.y < 315 || cam.rotation.eulerAngles.y > 45))
        {
            initialCamRotationY = cam.rotation.eulerAngles.y;
            Debug.Log("Camera initial rotation");
            cameraRig.RotateAround(cam.position, Vector3.up, initialCamRotationY);
            CameraRotator.RotateAround(cam.position, Vector3.up, -initialCamRotationY);
            ControllerRotator.RotateAround(cam.position, Vector3.up, -initialCamRotationY);
            Debug.Log("Camera:"+CameraRotator.rotation.eulerAngles.y+" Ctrl:"+ControllerRotator.rotation.eulerAngles.y);
            cameraRig.RotateAround(cam.position, Vector3.up, -initialCamRotationY);
        }

        if (expe == null)
        {
            expe = GameObject.Find("/Salle").GetComponent<rendering>().expe;
        }
        //Pointer
        m_HasPosition = UpdatePointer();
        Debug.Log(hit);
        photonView.RPC("receiveOtherPosition", Photon.Pun.RpcTarget.Others, cam.position, cameraRig.rotation.eulerAngles, cameraRig.position);

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
                tpToOther();
            }
            menu.transform.Find("moveModeText").GetComponent<TextMesh>().text = moveMode;
        }

        //Teleport
        position = SteamVR_Actions.default_Pos.GetAxis(SteamVR_Input_Sources.Any);
        if (m_TeleportAction.GetStateDown(m_pose.inputSource))
        {
            oldControlerRotation = controllerRight.transform.rotation.eulerAngles;
            oldHit = m_Pointer;
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
                        //expe.curentTrial.incNbSyncTp();
                    }
                    //syncTeleportation = false;
                    longclic = false;
                }

                if (m_TeleportAction.GetStateUp(m_pose.inputSource))
                {

                    //Debug.Log("reset");

                    isMoving = false;
                    longclic = false;
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
                    menu.SetActive(false);
                    photonView.RPC("teleportationMode", Photon.Pun.RpcTarget.All, syncTeleportation);
                }

                if (UpdatePointer() == true && hit.transform.name == "not syncro")
                {
                    // Debug.Log("Not Syncro");
                    menu.SetActive(false);
                    photonView.RPC("teleportationMode", Photon.Pun.RpcTarget.All, syncTeleportation);
                }

                if (UpdatePointer() == true && hit.transform.name == "syncro tag")
                {
                    // Debug.Log("Syncro");
                    menu.SetActive(false);
                    player = GameObject.Find("Network Player(Clone)");
                    synctag = true;
                    photonView.RPC("tagMode", Photon.Pun.RpcTarget.All, synctag);
                }

                if (UpdatePointer() == true && hit.transform.name == "not syncro tag")
                {
                    // Debug.Log("Not Syncro");
                    menu.SetActive(false);
                    player = GameObject.Find("Network Player(Clone)");
                    synctag = false;
                    photonView.RPC("tagMode", Photon.Pun.RpcTarget.All, synctag);
                }

                if (UpdatePointer() == true && hit.transform.name == "cancel")
                {
                    // Debug.Log("Cancel");
                    menu.SetActive(false);
                }
            }
            else if (moveMode == "joy")
            {

                if ((oldFingerX > 0.5 && (position.x < 0.5 || m_TeleportAction.GetStateUp(m_pose.inputSource))) || (oldFingerX < -0.5 && (position.x > -0.5 || m_TeleportAction.GetStateUp(m_pose.inputSource))) && expe != null)
                {
                    expe.curentTrial.incNbRotate();
                }
                if ((oldFingerY > 0.5 && (position.y < 0.5 || m_TeleportAction.GetStateUp(m_pose.inputSource))) || (oldFingerY < -0.5 && (position.y > -0.5 || m_TeleportAction.GetStateUp(m_pose.inputSource))) && expe != null)
                {
                    expe.curentTrial.incNbMove();
                    expe.curentTrial.incMoveTime(Time.time - moveTimer);
                }

                if ((oldFingerY < 0.5 && position.y > 0.5) || (oldFingerY > -0.5 && position.y < -0.5 ))
                {
                    moveTimer = Time.time;
                }

                if (m_TeleportAction.GetState(m_pose.inputSource))
                {
                    Quaternion rotation = Quaternion.Euler(controllerRight.rotation.eulerAngles);
                    Matrix4x4 m = Matrix4x4.Rotate(rotation);
                    Vector3 translateVect = new Vector3(0, 0, 0);
                    if (position.x < -0.5)
                    {
                        //translateVect = m.MultiplyPoint3x4(minusX);
                        if (isOtherSynced)
                        {
                            photonView.RPC("MoveRigFromTransform", Photon.Pun.RpcTarget.Others, translateVect, -joystickRotation);
                            cameraRig.RotateAround(centerBetweenPlayers, Vector3.up, -joystickRotation);

                        }
                        else
                        {
                            cameraRig.RotateAround(cam.position, Vector3.up, -joystickRotation);
                        }
                        if (expe != null)
                        {
                            expe.curentTrial.incRotateTotal(joystickRotation);
                        }
                    }
                    if (position.y > 0.5)
                    {
                        translateVect = m.MultiplyPoint3x4(plusZ);
                        /* Rotation de la camera dans la direction du pointeur, � d�commenter avec le if getStateUp
                        
                        Vector3 camAngle = new Vector3(cam.transform.forward.x, 0, cam.transform.forward.z);
                        Vector3 ctrlAngle = new Vector3(controllerRight.transform.forward.x, 0, controllerRight.transform.forward.z);

                        double crossProduct = Vector3.Cross(camAngle, ctrlAngle).y;
                        Debug.Log(crossProduct);
                        
                        if (crossProduct > 0)
                        {
                            CameraRotator.RotateAround(cam.position, Vector3.up, 0.25f);

                        }
                        else if (crossProduct < 0)
                        {
                            CameraRotator.RotateAround(cam.position, Vector3.up, -0.25f);
                        }*/
                    }
                    if (position.y < -0.5)
                    {
                        translateVect = m.MultiplyPoint3x4(minusZ);
                    }
                    if (position.x > 0.5)
                    {
                        //translateVect = m.MultiplyPoint3x4(plusX);
                        if (isOtherSynced)
                        {
                            photonView.RPC("MoveRigFromTransform", Photon.Pun.RpcTarget.Others, translateVect, joystickRotation);
                            cameraRig.RotateAround(centerBetweenPlayers, Vector3.up, joystickRotation);
                        }
                        else
                        {
                            cameraRig.RotateAround(cam.position, Vector3.up, joystickRotation);
                        }
                        if (expe != null)
                        {
                            expe.curentTrial.incRotateTotal(joystickRotation);
                        }
                    }
                    translateVect.y = 0;
                    if (cam.position.x + translateVect.x < -3.5) { translateVect.x = -3.5f - cam.position.x; }
                    if (cam.position.x + translateVect.x > 3.5) { translateVect.x = 3.5f - cam.position.x; }
                    if (cam.position.z + translateVect.z < -3.5) { translateVect.z = -3.5f - cam.position.z; }
                    if (cam.position.z + translateVect.z > 3.5) { translateVect.z = 3.5f - cam.position.z; }
                    cameraRig.position += translateVect;
                    if (expe != null)
                    {
                        expe.curentTrial.incDistTotal(translateVect.magnitude);
                    }
                    if (isOtherSynced)
                    {
                        photonView.RPC("MoveRigFromTransform", Photon.Pun.RpcTarget.Others, translateVect, 0f);
                        if (position.y > 0.5)
                        {
                            //expe.curentTrial.incNbSyncJoyForward(translateVect);
                        }
                        else if (position.y < -0.5)
                        {
                            //expe.curentTrial.incNbSyncJoyBackward(translateVect);
                        }
                    }
                    else
                    {
                        if (position.y > 0.5)
                        {
                            //expe.curentTrial.incNbSyncJoyForward(translateVect);
                        }
                        else if (position.y < -0.5)
                        {
                            //expe.curentTrial.incNbSyncJoyBackward(translateVect);
                        }
                    }
                }
                oldFingerX = position.x;
                oldFingerY = position.y;
            }
            else if (moveMode == "drag")
            {
                if (m_TeleportAction.GetStateUp(m_pose.inputSource) && expe != null)
                {
                    if(oldHit != null)
                    {
                        expe.curentTrial.incNbMove();
                        expe.curentTrial.incMoveTime(Time.time - moveTimer);
                    }
                    else
                    {
                        expe.curentTrial.incNbRotate();
                    }
                }
                if (oldHit != null)
                {
                    if ((oldHit.transform.tag == "TpLimit" || oldHit.transform.tag == "Tp") && (hit.transform.tag == "Wall" || hit.transform.parent.tag == "Wall" || !m_HasPosition) && expe != null)
                    {
                        expe.curentTrial.incNbMove();
                        expe.curentTrial.incMoveTime(Time.time - moveTimer);
                    }
                    if ((oldHit.transform.tag == "Wall" || oldHit.transform.parent.tag == "Wall") && (hit.transform.tag == "Tp" || hit.transform.tag == "TpLimit" || !m_HasPosition) && expe != null)
                    {
                        expe.curentTrial.incNbMove();
                        expe.curentTrial.incMoveTime(Time.time - moveTimer);
                    }
                }
                else
                {
                    if (m_HasPosition && expe != null)
                    {
                        expe.curentTrial.incNbRotate();
                    }
                }
                if (m_TeleportAction.GetState(m_pose.inputSource))
                {
                    Vector3 translateVect = new Vector3(0, 0, 0);
                    //Debug.Log(m_HasPosition + hit.transform.tag);
                    if (m_HasPosition && (hit.transform.tag == "TpLimit" || hit.transform.tag == "Tp"))
                    {
                        float a = Mathf.Tan((90 - oldControlerRotation.x) * Mathf.PI / 180) * controllerRight.transform.position.y;
                        float b = Mathf.Tan((90 - controllerRight.rotation.eulerAngles.x) * Mathf.PI / 180) * controllerRight.transform.position.y;
                        //Debug.Log("b: " + b);
                        Vector3 camToHit = oldHit.transform.position - cam.position;
                        Vector3 ctrlToHit = oldHit.transform.position - controllerRight.position;
                        //Debug.Log(camToHit.z);
                        camToHit.y = 0;
                        ctrlToHit.y = 0;
                        float c = camToHit.magnitude * (ctrlToHit.magnitude - b) / ctrlToHit.magnitude;
                        translateVect = camToHit.normalized * (a-b);
                        //Debug.Log(c);
                        if (cam.position.x + translateVect.x < -3.5) { translateVect.x = -3.5f - cam.position.x; }
                        if (cam.position.x + translateVect.x > 3.5) { translateVect.x = 3.5f - cam.position.x; }
                        if (cam.position.z + translateVect.z < -3.5) { translateVect.z = -3.5f - cam.position.z; }
                        if (cam.position.z + translateVect.z > 3.5) { translateVect.z = 3.5f - cam.position.z; }
                        cameraRig.position += translateVect;
                        if (expe != null)
                        {
                            expe.curentTrial.incDistTotal(translateVect.magnitude);
                        }
                        if (isOtherSynced)
                        {
                            photonView.RPC("MoveRigFromTransform", Photon.Pun.RpcTarget.Others, translateVect, 0f);
                            //expe.curentTrial.incNbSyncDragGround(translateVect);
                        }
                        else
                        {
                            //expe.curentTrial.incNbAsyncDragGround(translateVect);
                        }

                        //cameraRig.position += a - a.normalized*b;
                    }
                    else if (m_HasPosition && (hit.transform.tag == "Wall" || hit.transform.parent.tag == "Wall"))
                    {
                        Transform mur;
                        float a, b, distMur;

                        if (hit.transform.name == "MUR B" || hit.transform.parent.name == "MUR B")
                        {
                            mur = MurB;
                            distMur = Mathf.Abs(mur.position.z - controllerRight.position.z);
                            a = Mathf.Tan((oldControlerRotation.y - mur.rotation.eulerAngles.y) * Mathf.PI / 180);
                            b = Mathf.Tan((controllerRight.rotation.eulerAngles.y - mur.rotation.eulerAngles.y) * Mathf.PI / 180);
                            translateVect.x = 1.0f;

                        }
                        else if (hit.transform.name == "MUR R" || hit.transform.parent.name == "MUR R")
                        {
                            mur = MurR;
                            distMur = Mathf.Abs(mur.position.x - controllerRight.position.x);
                            a = -Mathf.Tan((oldControlerRotation.y - mur.rotation.eulerAngles.y) * Mathf.PI / 180);
                            b = -Mathf.Tan((controllerRight.rotation.eulerAngles.y + mur.rotation.eulerAngles.y) * Mathf.PI / 180);
                            translateVect.z = 1.0f;

                        }
                        else
                        {
                            mur = MurL;
                            distMur = Mathf.Abs(mur.position.x - controllerRight.position.x);
                            a = Mathf.Tan((oldControlerRotation.y - mur.rotation.eulerAngles.y) * Mathf.PI / 180);
                            b = Mathf.Tan((controllerRight.rotation.eulerAngles.y - mur.rotation.eulerAngles.y) * Mathf.PI / 180);
                            translateVect.z = 1.0f;

                        }

                        translateVect *= (a-b)*distMur;
                        //Debug.Log(translateVect);
                        if (cam.position.x + translateVect.x < -3.5) { translateVect.x = -3.5f - cam.position.x; }
                        if (cam.position.x + translateVect.x > 3.5) { translateVect.x = 3.5f - cam.position.x; }
                        if (cam.position.z + translateVect.z < -3.5) { translateVect.z = -3.5f - cam.position.z; }
                        if (cam.position.z + translateVect.z > 3.5) { translateVect.z = 3.5f - cam.position.z; }
                        cameraRig.position += translateVect;
                        if (expe != null)
                        {
                            expe.curentTrial.incDistTotal(translateVect.magnitude);
                        }
                        if (isOtherSynced)
                        {
                            photonView.RPC("MoveRigFromTransform", Photon.Pun.RpcTarget.Others, translateVect, 0f);
                            //expe.curentTrial.incNbSyncDragWall(translateVect);
                        }
                        else
                        {
                           // expe.curentTrial.incNbAsyncDragWall(translateVect);
                        }
                    }
                    else
                    {
                        float angle = oldControlerRotation.y - controllerRight.transform.rotation.eulerAngles.y;
                        if (isOtherSynced)
                        {
                            photonView.RPC("MoveRigFromTransform", Photon.Pun.RpcTarget.Others, translateVect, angle);
                            cameraRig.RotateAround(centerBetweenPlayers, Vector3.up, angle);
                        }
                        else
                        {
                            cameraRig.RotateAround(cam.position, Vector3.up, angle);
                        }
                        if (expe != null)
                        {
                            expe.curentTrial.incRotateTotal(angle);
                        }
                    }
                    oldControlerRotation = controllerRight.transform.rotation.eulerAngles;
                    oldHit = m_Pointer;
                }

            }
        }
    }

    private void tryTeleport()
    {

        //if no hit stop the fonction
        if ( m_IsTeleportoting) //!m_HasPosition ||
            return;
        
      
        //player possition
        Vector3 posPointer = m_Pointer.transform.position;
        if (e)
        {
            if (syncTeleportation || isOtherSynced)
            {
                photonView.RPC("RotationRigRPC", Photon.Pun.RpcTarget.All, "e");
            }
            else
            {
                cameraRig.RotateAround(cam.position, Vector3.up, 90);
                expe.curentTrial.incRotateTotal(90);
            }
            return;
        }
        else if (w)
        {
            if (syncTeleportation || isOtherSynced)
            {
                photonView.RPC("RotationRigRPC", Photon.Pun.RpcTarget.All, "w");
            }
            else
            {
                cameraRig.RotateAround(cam.position, Vector3.up, -90);
                expe.curentTrial.incRotateTotal(90);
            }
            return;
        }
        Vector3 translateVector;

        if (!m_HasPosition) // ||
            return;


        
        
        else if (hit.transform.tag == "Tp" || hit.transform.tag == "TpLimit" )
        {
            if (posPointer.x < -3.5) { posPointer.x = -3.5f; }
            if (posPointer.x >  3.5) { posPointer.x =  3.5f; }
            if (posPointer.z < -3.5) { posPointer.z = -3.5f; }
            if (posPointer.z >  3.5) { posPointer.z =  3.5f; }
            translateVector = posPointer - cam.position;
            translateVector.y = 0;

            if (expe != null)
            {
                if (syncTeleportation)
                {
                    //expe.curentTrial.incNbSyncTpGround(translateVector);
                }
                else
                {
                    //expe.curentTrial.incNbAsyncTpGround(translateVector);
                }
            }
            StartCoroutine(MoveRig(translateVector, null));
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
                translateVector = new Vector3(m_Pointer.transform.position.x - cam.position.x, 0, 3 - cam.position.z);
                StartCoroutine(MoveRig(translateVector, MurB));
            }
            else if (hit.transform.name == "MUR R" || hit.transform.parent.name == "MUR R")
            {
                translateVector = new Vector3(3 - cam.position.x, 0, m_Pointer.transform.position.z- cam.position.z);
                StartCoroutine(MoveRig(translateVector, MurR));
            }
            else //(hit.transform.name == "MUR L" || hit.transform.parent.name == "MUR L")
            {
                translateVector = new Vector3(-3 - cam.position.x, 0, m_Pointer.transform.position.z - cam.position.z); 
                StartCoroutine(MoveRig(translateVector, MurL));
            }

            //then teleport
            if (expe != null)
            {
                if (syncTeleportation)
                {
                    //expe.curentTrial.incNbSyncTpWall(translateVector);
                }
                else
                {
                    //expe.curentTrial.incNbAsyncTpWall(translateVector);
                }
            }

        }
        else if (hit.transform.tag == "Player")
        {
            tpToOther();
        }
    }

    [PunRPC]
    void MoveRigFromTransform(Vector3 translation, float rotation)
    {
        cameraRig.position += translation;
        cameraRig.RotateAround(centerBetweenPlayers, Vector3.up, rotation);
        expe.curentTrial.incRotateTotal(joystickRotation);
        expe.curentTrial.incDistTotal(translation.magnitude);
    }

    [PunRPC]
    void toggleOtherSync()
    {
        isOtherSynced = !isOtherSynced;
    }

    [PunRPC]
    void receiveOtherPosition(Vector3 position, Vector3 rotation, Vector3 cameraRigPosition)
    {
        otherPlayerCameraRigPos = cameraRigPosition;
        otherPlayerPosition = position;
        otherPlayerPosition.y = 0;
        otherPlayerRotation = rotation;
        updateCenter();
        Cube.transform.position = centerBetweenPlayers;
    }

    void updateCenter()
    {
        centerBetweenPlayers = (otherPlayerPosition + cam.position) / 2f;
        centerBetweenPlayers.y = 0;

    }

    [PunRPC]
    void RotationRigRPC(string s)
    {
        Transform cameraRig2 = SteamVR_Render.Top().origin.parent;

        Debug.Log("test ");
        if (s == "e")
        {
            Cube.transform.RotateAround(Cube.transform.position, Vector3.up, 90);
            cameraRig2.RotateAround(Cube.transform.position, Vector3.up, 90);
            expe.curentTrial.incRotateTotal(90);
        }
        else if (s == "w")
        {
            Cube.transform.RotateAround(Cube.transform.position, Vector3.up, -90);
            cameraRig2.RotateAround(Cube.transform.position, Vector3.up, -90);
            expe.curentTrial.incRotateTotal(90);
        }

    }

    [PunRPC]
    void tpToOther()
    {
        /*
        Vector3 posToMove = otherPlayerPosition;
        posToMove.x -= cam.localPosition.x;
        posToMove.z -= cam.localPosition.z;
        //Debug.Log(otherPlayerRotation);
        Vector3 specificPos;
        if (PhotonNetwork.IsMasterClient)
        {
            specificPos = Vector3.right;
        }
        else
        {
            specificPos = Vector3.left;
        }
        Debug.Log(specificPos);
        specificPos = Quaternion.AngleAxis(otherPlayerRotation.y, Vector3.up) * specificPos;
        
        Debug.Log(specificPos);
        Debug.Log(specificPos.magnitude);

        posToMove += specificPos;
        */
        StartCoroutine(MoveRigForSyncTP(otherPlayerCameraRigPos, otherPlayerRotation));
        
    }

    [PunRPC]
    void tagMode(bool tag)
    {
       // Debug.Log("Change tag mode");
        synctag = tag;
    }

    private IEnumerator MoveRig(Vector3 translation, Transform wall)
    {
        moveTimer = Time.time;
        m_IsTeleportoting = true;

        SteamVR_Fade.Start(Color.black, m_FadeTime, true); // black screen

        yield return new WaitForSeconds( m_FadeTime); // fade time
        
        if (wall != null)
        {
            cameraRig.rotation = wall.rotation;
            if (expe != null)
            {
                expe.curentTrial.incRotateTotal(wall.rotation.eulerAngles.y - cameraRig.rotation.eulerAngles.y);
            }
        }

        if (cam.position.x + translation.x < -3.5) { translation.x = -3.5f - cam.position.x; }
        if (cam.position.x + translation.x > 3.5) { translation.x = 3.5f - cam.position.x; }
        if (cam.position.z + translation.z < -3.5) { translation.z = -3.5f - cam.position.z; }
        if (cam.position.z + translation.z > 3.5) { translation.z = 3.5f - cam.position.z; }
        cameraRig.position += translation;
        if (expe != null)
        {
            expe.curentTrial.incDistTotal(translation.magnitude);
        }
        if (syncTeleportation || isOtherSynced)
        {
            photonView.RPC("tpToOther", Photon.Pun.RpcTarget.Others);
            syncTeleportation = false;
        }
        Debug.Log("camera rig pos tp :" +cameraRig.position);
        

        SteamVR_Fade.Start(Color.clear, m_FadeTime, true); // normal screen

        m_IsTeleportoting = false;
        if (expe != null)
        {
            expe.curentTrial.incMoveTime(Time.time - moveTimer);
            expe.curentTrial.incNbMove();
        }
    }

    private IEnumerator MoveRigForSyncTP(Vector3 pos, Vector3 rotat)
    {
        moveTimer = Time.time;
        m_IsTeleportoting = true;

        SteamVR_Fade.Start(Color.black, m_FadeTime, true); // black screen
        yield return new WaitForSeconds(m_FadeTime); // fade time
                                                     // Rotation

        cameraRig.RotateAround(cam.position, Vector3.up, rotat.y - cameraRig.rotation.eulerAngles.y);
        if (expe != null)
        {
            expe.curentTrial.incRotateTotal(rotat.y - cameraRig.rotation.eulerAngles.y);
            expe.curentTrial.incDistTotal((pos - cameraRig.position).magnitude);
        }
        cameraRig.position = pos; // teleportation

        SteamVR_Fade.Start(Color.clear, m_FadeTime, true); // normal screen
        m_IsTeleportoting = false;
        if (expe != null)
        {
            expe.curentTrial.incMoveTime(Time.time - moveTimer);
        }
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
}
