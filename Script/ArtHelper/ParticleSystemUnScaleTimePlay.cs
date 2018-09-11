                                                                                                                                                                                                                                                                                                                                                                                                      using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemUnScaleTimePlay : MonoBehaviour {

    ParticleSystem[] particleSystems = null;

    void Awake()
    {
        particleSystems = this.GetComponentsInChildren<ParticleSystem>();
    }
	void Start () 
    {

	}

    public float SpeedUpRate = 1.0f;
	void Update () 
    {
        for (int i = 0; i < particleSystems.Length; ++i)
        {
            particleSystems[i].Simulate(Time.unscaledDeltaTime * SpeedUpRate, true, false, true);
        }
	}
}
