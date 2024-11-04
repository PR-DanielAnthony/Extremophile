using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinOnZ : MonoBehaviour
{
    [SerializeField] private float spinAmt = 360;
    private void FixedUpdate()
    {
        transform.Rotate(Vector3.forward, spinAmt * Time.deltaTime);
    }
}
