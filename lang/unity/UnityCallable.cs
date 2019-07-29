using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UnityCallable : MonoBehaviour
{
    public string name;
    public bool enabled;
    public Callable callable;

    public void Start()
    {
        this.setup();
        if(enabled)
        {
            UnityLoader.load(this);
        }
    }

    public abstract void setup();
}
