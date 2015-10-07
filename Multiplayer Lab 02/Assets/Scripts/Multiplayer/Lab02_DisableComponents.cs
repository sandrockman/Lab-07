using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Lab02_DisableComponents : NetworkBehaviour
{

    [Header("Player")]
    [Header("\tValues for Client Management")]
    public Rigidbody myRigidbody;
    public CapsuleCollider myCollider;
    [Header("Camera")]
    public GameObject myCameraObject;
    public Camera myCamera;
    public AudioListener myListener;

    // Use this for initialization
    void Start()
    {

        if (!isLocalPlayer)
        {
            Destroy(myRigidbody);
            Destroy(myCollider);
            Destroy(myCameraObject);
        }
    }
}
