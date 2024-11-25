using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FightStand : MonoBehaviour
{
    public GameObject Katana;
    public GameObject KatatnaHide;

    bool withDraw;

    public Quaternion targetRotation;
    private Quaternion saveRotation;
    public float rotationSpeed = 1f;

    public Transform handPosition;
    private Vector3 rightPosition;

    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
        rightPosition = handPosition.position - Katana.transform.position;
        saveRotation = Katana.transform.rotation;
    }

    void WithdrawSword()
    {
        Debug.Log("Yes");

        if (Katana.transform.parent == KatatnaHide.transform)
        {
            Katana.transform.parent = null;
            Katana.transform.position = handPosition.position;
            Katana.transform.SetParent(handPosition, true);
            Katana.transform.rotation = targetRotation;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            animator.CrossFade("Sword", 0.25f);
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (Katana.transform.parent != KatatnaHide.transform)
            {
                animator.CrossFade("Movement", 1f);
                Katana.transform.parent = null;
                Katana.transform.position = KatatnaHide.transform.position;
                Katana.transform.SetParent(KatatnaHide.transform, true);

                Katana.transform.rotation = saveRotation;
            }   
        }
    }
}
