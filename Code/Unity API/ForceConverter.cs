using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceConverter : MonoBehaviour {
    static float PWMtoNewton_m = 0.0009f; 
    static float PWMtoNewton_k = -0.3019f; 

    static public float Newton(int PWM) {
        return (PWM == 0) ? 0f : (float)PWM * PWMtoNewton_m + PWMtoNewton_k;
    } 

    static public int PWM(float Newton) {
        return (Newton == 0) ? 0 : (int)((Newton - PWMtoNewton_k) / PWMtoNewton_m);
    }
}
