using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailerScript : MonoBehaviour
{
    // Start is called before the first frame update

    public float width = 1.0f;
    public bool useCurve = true;
    public TrailRenderer tr;

    void Start()
    {
        tr = GetComponent<TrailRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
        AnimationCurve curve = new AnimationCurve();

        curve.AddKey(6.0f, 6.0f);
        curve.AddKey(6.0f, 6.0f);

        tr.widthCurve = curve;
        tr.widthMultiplier = width;
        
    }
}
