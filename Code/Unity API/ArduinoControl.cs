using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArduinoControl : MonoBehaviour {
    
    public enum RotateDirection {
        UP = 0, DOWN = 1, LEFT = 2, RIGHT = 3, CLOCKWISE = 4, COUNTERCLOCKWISE = 5, NONE = 6
    }
    public enum AdvancedMode {
        EasyIn, LowestFixed, HalfFixed
    }

    public ArduinoBasic arduino;
    public float ForwardTorqueArm = 23f;
    public float LowerTorqueArm = 23.2f;
    public float UpperTorqueArm = 24.2f;
    public float HorizontalTorqueArm = 12f;

    public float PitchTorque;
    public float YawTorque;
    public float RollTorque;

    public bool useLinearMappingByRotationSpeed;
    public float lowerboundOfPitchRotationSpeed;
    public float upperboundOfPitchRotationSpeed;
    public float lowerboundOfYawRotationSpeed;
    public float upperboundOfYawRotationSpeed;
    public float lowerboundOfRollRotationSpeed;
    public float upperboundOfRollRotationSpeed;
    public float lowerboundOfPitchTorque;
    public float upperboundOfPitchTorque;
    public float lowerboundOfYawTorque;
    public float upperboundOfYawTorque;
    public float lowerboundOfRollTorque;
    public float upperboundOfRollTorque;

    public bool useAdjustmentCurveByTime;
    public float lowerboundOfTorque = 2f;
    public AdvancedMode advancedMode;
    public AnimationCurve adjustmentCurve;
    public AnimationCurve eazyInCurve;

    private float aForce = 0f;
    private float bForce = 0f;
    private float cForce = 0f;
    private float dForce = 0f;
    private float eForce = 0f;
    private float fForce = 0f;

    private float advanceTime = 0;
    private int rotationSpeed = 0;
    private bool[] isRotating = new bool[3];


    private RotateDirection pitchDirection = RotateDirection.NONE;
    private RotateDirection yawDirection = RotateDirection.NONE;
    private RotateDirection rollDirection = RotateDirection.NONE;

    enum RotateType {
        PITCH = 0, YAW = 1, ROLL = 2
    }
    struct RotateEvent {
        public bool rotating;
        public float lastRecord;
        public float duration;
    }
    private RotateEvent[] events = new RotateEvent[3];

    float getAdjust(float x) {
        if (x < 0f || x > 1f) return 1f;
        else return adjustmentCurve.Evaluate(x);
    }

    float getEasyInAdjust(float x) {
        if (x < 0f || x > 1f) return 1f;
        else return eazyInCurve.Evaluate(x);
    }

    void ChangeForce() {
        // Calculate the force required for each nozzle
        float pitchAdjust = 1f, yawAdjust = 1f, rollAdjust = 1f;
        float yawTorque = YawTorque, pitchTorque = PitchTorque, rollTorque = RollTorque;
        if (pitchDirection != RotateDirection.NONE) lowerboundOfTorque = lowerboundOfPitchTorque;
        else if (yawDirection != RotateDirection.NONE) lowerboundOfTorque = lowerboundOfYawTorque;
        else lowerboundOfTorque = lowerboundOfRollTorque;

        // wheteher the view is actually rotating
        if (Time.time - events[(int)RotateType.PITCH].lastRecord > advanceTime) isRotating[(int)RotateType.PITCH] = true;
        if (Time.time - events[(int)RotateType.YAW].lastRecord > advanceTime) isRotating[(int)RotateType.YAW] = true;
        if (Time.time - events[(int)RotateType.ROLL].lastRecord > advanceTime) isRotating[(int)RotateType.ROLL] = true;
        if (Time.time - events[(int)RotateType.PITCH].lastRecord <= advanceTime) isRotating[(int)RotateType.PITCH] = false;
        if (Time.time - events[(int)RotateType.YAW].lastRecord <= advanceTime) isRotating[(int)RotateType.YAW] = false;
        if (Time.time - events[(int)RotateType.ROLL].lastRecord <= advanceTime) isRotating[(int)RotateType.ROLL] = false;


        if (useAdjustmentCurveByTime) {
            if (events[(int)RotateType.PITCH].rotating) {
                if (isRotating[(int)RotateType.PITCH]) {
                    pitchAdjust = getAdjust((Time.time - events[(int)RotateType.PITCH].lastRecord - advanceTime) / (events[(int)RotateType.PITCH].duration - advanceTime)); // (current time point / the time rotation takes)           
                    lowerboundOfTorque = 2f;
                }
            }
            if (events[(int)RotateType.YAW].rotating) {
                if (isRotating[(int)RotateType.YAW]) {
                    yawAdjust = getAdjust((Time.time - events[(int)RotateType.YAW].lastRecord - advanceTime) / (events[(int)RotateType.YAW].duration - advanceTime));
                    lowerboundOfTorque = 2f;
                }
            }
            if (events[(int)RotateType.ROLL].rotating) {
                if (isRotating[(int)RotateType.ROLL]) {
                    rollAdjust = getAdjust((Time.time - events[(int)RotateType.ROLL].lastRecord - advanceTime) / (events[(int)RotateType.ROLL].duration - advanceTime));
                    lowerboundOfTorque = 2f;
                }
            }
        }



        float upperRate = UpperTorqueArm / (UpperTorqueArm + LowerTorqueArm);
        float lowerRate = LowerTorqueArm / (UpperTorqueArm + LowerTorqueArm);

        if (useLinearMappingByRotationSpeed) {
            float RateOfPitchRotationSpeed = (rotationSpeed - lowerboundOfPitchRotationSpeed) / (upperboundOfPitchRotationSpeed - lowerboundOfPitchRotationSpeed);
            float RateOfYawRotationSpeed = (rotationSpeed - lowerboundOfYawRotationSpeed) / (upperboundOfYawRotationSpeed - lowerboundOfYawRotationSpeed);
            float RateOfRollRotationSpeed = (rotationSpeed - lowerboundOfRollRotationSpeed) / (upperboundOfRollRotationSpeed - lowerboundOfRollRotationSpeed);
            if (pitchDirection != RotateDirection.NONE)
                pitchTorque = (rotationSpeed > upperboundOfPitchRotationSpeed) ? upperboundOfPitchTorque : lowerboundOfPitchTorque + RateOfPitchRotationSpeed * (upperboundOfPitchTorque - lowerboundOfPitchTorque);
            if (yawDirection != RotateDirection.NONE)
                yawTorque = (rotationSpeed > upperboundOfYawRotationSpeed) ? upperboundOfYawTorque : lowerboundOfYawTorque + RateOfYawRotationSpeed * (upperboundOfYawTorque - lowerboundOfYawTorque);
            if (rollDirection != RotateDirection.NONE)
                rollTorque = (rotationSpeed > upperboundOfRollRotationSpeed) ? upperboundOfRollTorque : lowerboundOfRollTorque + RateOfRollRotationSpeed * (upperboundOfRollTorque - lowerboundOfRollTorque);
        }

        // increasing curve 
        if (advancedMode == AdvancedMode.EasyIn) {
            if (events[(int)RotateType.PITCH].rotating) {
                if (!isRotating[(int)RotateType.PITCH]) {
                    pitchAdjust = getEasyInAdjust((Time.time - events[(int)RotateType.PITCH].lastRecord) / advanceTime);
                    pitchTorque = lowerboundOfTorque + (pitchTorque - lowerboundOfTorque) * pitchAdjust;
                }
            }
            if (events[(int)RotateType.YAW].rotating) {
                if (!isRotating[(int)RotateType.YAW]) {
                    yawAdjust = getEasyInAdjust((Time.time - events[(int)RotateType.YAW].lastRecord) / advanceTime);
                    yawTorque = lowerboundOfTorque + (yawTorque - lowerboundOfTorque) * yawAdjust;
                }
            }
            if (events[(int)RotateType.ROLL].rotating) {
                if (!isRotating[(int)RotateType.ROLL]) {
                    rollAdjust = getEasyInAdjust((Time.time - events[(int)RotateType.ROLL].lastRecord) / advanceTime);
                    rollTorque = lowerboundOfTorque + (rollTorque - lowerboundOfTorque) * rollAdjust;
                }
            }
        }

        if (advancedMode == AdvancedMode.LowestFixed) {
            if (events[(int)RotateType.PITCH].rotating) {
                if (!isRotating[(int)RotateType.PITCH]) {
                    pitchTorque = lowerboundOfPitchTorque;
                }
            }
            if (events[(int)RotateType.YAW].rotating) {
                if (!isRotating[(int)RotateType.YAW]) {
                    yawTorque = lowerboundOfYawTorque;
                }
            }
            if (events[(int)RotateType.ROLL].rotating) {
                if (!isRotating[(int)RotateType.ROLL]) {
                    rollTorque = lowerboundOfRollTorque;
                }
            }
        }

        if (advancedMode == AdvancedMode.HalfFixed) {
            if (events[(int)RotateType.PITCH].rotating) {
                if (!isRotating[(int)RotateType.PITCH]) {
                    pitchTorque = pitchTorque / 2;
                }
            }
            if (events[(int)RotateType.YAW].rotating) {
                if (!isRotating[(int)RotateType.YAW]) {
                    yawTorque = yawTorque / 2;
                }
            }
            if (events[(int)RotateType.ROLL].rotating) {
                if (!isRotating[(int)RotateType.ROLL]) {
                    rollTorque = rollTorque / 2;
                }
            }
        }

        if (isRotating[(int)RotateType.PITCH]) pitchTorque = lowerboundOfTorque + (pitchTorque - lowerboundOfTorque) * pitchAdjust;
        if (isRotating[(int)RotateType.YAW]) yawTorque = lowerboundOfTorque + (yawTorque - lowerboundOfTorque) * yawAdjust;
        if (isRotating[(int)RotateType.ROLL]) rollTorque = lowerboundOfTorque + (rollTorque - lowerboundOfTorque) * rollAdjust;
        float newAForce = (yawDirection == RotateDirection.RIGHT) ? yawTorque / ForwardTorqueArm : 0f;
        float newBForce = (yawDirection == RotateDirection.LEFT) ? yawTorque / ForwardTorqueArm : 0f;
        float newCForce = ((pitchDirection == RotateDirection.DOWN) ? (pitchTorque / 2) / UpperTorqueArm : 0f) +
                          ((rollDirection == RotateDirection.COUNTERCLOCKWISE) ? (rollTorque / HorizontalTorqueArm) * lowerRate : 0f);
        float newDForce = ((pitchDirection == RotateDirection.UP) ? (pitchTorque / 2) / LowerTorqueArm : 0f) +
                          ((rollDirection == RotateDirection.CLOCKWISE) ? (rollTorque / HorizontalTorqueArm) * upperRate : 0f);
        float newEForce = ((pitchDirection == RotateDirection.DOWN) ? (pitchTorque / 2) / UpperTorqueArm : 0f) +
                          ((rollDirection == RotateDirection.CLOCKWISE) ? (rollTorque / HorizontalTorqueArm) * lowerRate : 0f);
        float newFForce = ((pitchDirection == RotateDirection.UP) ? (pitchTorque / 2) / LowerTorqueArm : 0f) +
                          ((rollDirection == RotateDirection.COUNTERCLOCKWISE) ? (rollTorque / HorizontalTorqueArm) * upperRate : 0f);

        // The forces of the opposing nozzles cancel each other out
        if (newCForce > newDForce && newDForce != 0) {
            newCForce -= newDForce;
            newDForce = 0f;
        } else if (newDForce > newCForce && newCForce != 0) {
            newDForce -= newCForce;
            newCForce = 0f;
        }
        if (newEForce > newFForce && newFForce != 0) {
            newEForce -= newFForce;
            newFForce = 0f;
        } else if (newFForce > newEForce && newEForce != 0) {
            newFForce -= newEForce;
            newEForce = 0f;
        }

        // Send commands to the Arduino with force updated nozzles
        if (newAForce != aForce) {
            aForce = newAForce;
            arduino.ArduinoWrite(string.Format("a {0:d}\n", ForceConverter.PWM(aForce)));
        }
        if (newBForce != bForce) {
            bForce = newBForce;
            arduino.ArduinoWrite(string.Format("b {0:d}\n", ForceConverter.PWM(bForce)));
        }
        if (newCForce != cForce) {
            cForce = newCForce;
            arduino.ArduinoWrite(string.Format("c {0:d}\n", ForceConverter.PWM(cForce)));
        }
        if (newDForce != dForce) {
            dForce = newDForce;
            arduino.ArduinoWrite(string.Format("d {0:d}\n", ForceConverter.PWM(dForce)));
        }
        if (newEForce != eForce) {
            eForce = newEForce;
            arduino.ArduinoWrite(string.Format("e {0:d}\n", ForceConverter.PWM(eForce)));
        }
        if (newFForce != fForce) {
            fForce = newFForce;
            arduino.ArduinoWrite(string.Format("f {0:d}\n", ForceConverter.PWM(fForce)));
        }
    }

    void Update() {
        for (int i = 0; i < 3; i++) {
            if (events[i].rotating) {
                if (Time.time > events[i].lastRecord + events[i].duration) {
                    switch ((RotateType)i) {
                        case RotateType.PITCH:
                            pitchDirection = RotateDirection.NONE;
                            break;
                        case RotateType.YAW:
                            yawDirection = RotateDirection.NONE;
                            break;
                        case RotateType.ROLL:
                            rollDirection = RotateDirection.NONE;
                            break;
                    }
                    events[i].rotating = false;
                }
            }
        }
        ChangeForce();
    }

    public void StartRotate(RotateDirection rd, float duration, float advanceTime = 0f, int rotationSpeed = 0) {
        RotateType type = RotateType.PITCH;
        switch (rd) {
            case RotateDirection.UP:
                type = RotateType.PITCH;
                pitchDirection = RotateDirection.UP;
                break;
            case RotateDirection.DOWN:
                type = RotateType.PITCH;
                pitchDirection = RotateDirection.DOWN;
                break;
            case RotateDirection.LEFT:
                type = RotateType.YAW;
                yawDirection = RotateDirection.LEFT;
                break;
            case RotateDirection.RIGHT:
                type = RotateType.YAW;
                yawDirection = RotateDirection.RIGHT;
                break;
            case RotateDirection.CLOCKWISE:
                type = RotateType.ROLL;
                rollDirection = RotateDirection.CLOCKWISE;
                break;
            case RotateDirection.COUNTERCLOCKWISE:
                type = RotateType.ROLL;
                rollDirection = RotateDirection.COUNTERCLOCKWISE;
                break;
        }
        events[(int)type].rotating = true;
        events[(int)type].lastRecord = Time.time;
        events[(int)type].duration = duration;
        this.advanceTime = advanceTime;
        this.rotationSpeed = rotationSpeed;
        ChangeForce();
    }

    public void ShutDown() {
        pitchDirection = RotateDirection.NONE;
        yawDirection = RotateDirection.NONE;
        rollDirection = RotateDirection.NONE;
        ChangeForce();
    }
}
