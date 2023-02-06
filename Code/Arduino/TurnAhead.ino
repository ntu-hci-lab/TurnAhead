const int PWM_Pins[] = {32, 33, 25, 26, 27, 13, 02, 05};
const int RegulatorCount = sizeof(PWM_Pins) / sizeof(int);
const int ValvePins[] = {23, 22, 21, 19, 18, 17, 16, 04};
const int ValveCount = sizeof(ValvePins) / sizeof(int);
const int PWM_Freq = 10000;
const int PWM_Step = 12;  // 12-bit, [0-4095]

#define PINA 23
#define PINB 22
#define PINC 21
#define PIND 19
#define PINE 18
#define PINF 17
#define PWM1 0
#define PWM2 1
#define PWM3 2

void setup() {
  // Set Serial Baud Rate
  Serial.begin(115200);

  // Enable PWM pins and initialize all Regulators
  for (int i = 0; i < RegulatorCount; i++) {
    ledcSetup(i, PWM_Freq, PWM_Step);
    ledcAttachPin(PWM_Pins[i], i);
    ledcWrite(i, 0);
  }

  // Set All Valve Pins as Output
  for (int i = 0; i < ValveCount; i++) {
    pinMode(ValvePins[i], OUTPUT);
  }
}

void loop() {
  if (Serial.available()) {
    // Read command char
    char command;
    command = Serial.read();

    if (command == 'q') {
      // Initialize all Regulators and Valve Pins
      ledcWrite(PWM1, 0);
      ledcWrite(PWM2, 0);
      ledcWrite(PWM3, 0);

      digitalWrite(PINA, 0);
      digitalWrite(PINB, 0);
      digitalWrite(PINC, 0);
      digitalWrite(PIND, 0);
      digitalWrite(PINE, 0);
      digitalWrite(PINF, 0);

    } else if (command >= 'a' && command <= 'f') {
      // Set the air pressure (force) of the specified nozzle between "a" to "f" (a, b, c, d, e, f)
      // The opposite nozzles (ab, cd, and ef) share the same air source and cannot be turned on at the same time

      // read a integer as force
      int force = Serial.parseInt();

      if (command == 'a') {
        if (force == 0) {
          // Close nozzle "a"
          ledcWrite(PWM1, 0);
          digitalWrite(PINA, 0);
        } else {
          // Open nozzle "a" and close nozzle "b"
          ledcWrite(PWM1, force);
          digitalWrite(PINA, 1);
          digitalWrite(PINB, 0);
        }
      } else if (command == 'b') {
        if (force == 0) {
          // Close nozzle "b"
          ledcWrite(PWM1, 0);
          digitalWrite(PINB, 0);
        } else {
          // Open nozzle "b" and close nozzle "a"
          digitalWrite(PINB, 1);
          digitalWrite(PINA, 0);
          ledcWrite(PWM1, force);
        }
      } else if (command == 'c') {
        if (force == 0) {
          // Close nozzle "c"
          ledcWrite(PWM2, 0);
          digitalWrite(PINC, 0);
        } else {
          // Open nozzle "c" and close nozzle "d"
          digitalWrite(PINC, 1);
          digitalWrite(PIND, 0);
          ledcWrite(PWM2, force);
        }
      } else if (command == 'd') {
        if (force == 0) {
          // Close nozzle "d"
          ledcWrite(PWM2, 0);
          digitalWrite(PIND, 0);
        } else {
          // Open nozzle "d" and close nozzle "c"
          digitalWrite(PIND, 1);
          digitalWrite(PINC, 0);
          ledcWrite(PWM2, force);
        }
      } else if (command == 'e') {
        if (force == 0) {
          // Close nozzle "e"
          ledcWrite(PWM3, 0);
          digitalWrite(PINE, 0);
        } else {
          // Open nozzle "e" and close nozzle "f"
          digitalWrite(PINE, 1);
          digitalWrite(PINF, 0);
          ledcWrite(PWM3, force);
        }
      }
      else if (command == 'f') {
        if (force == 0) {
          // Close nozzle "f"
          ledcWrite(PWM3, 0);
          digitalWrite(PINF, 0);
        } else {
          // Open nozzle "f" and close nozzle "e"
          digitalWrite(PINF, 1);
          digitalWrite(PINE, 0);
          ledcWrite(PWM3, force);
        }
      }
    }
  }
  
  Serial.flush();
}
