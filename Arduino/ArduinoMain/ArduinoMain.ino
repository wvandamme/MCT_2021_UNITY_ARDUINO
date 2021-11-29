
#include <IRremote.h>
#include <Stepper.h>

const int IR_RECEIVE_PIN = 7;

const int stepsPerRevolution = 2048;
Stepper motor(stepsPerRevolution, 8, 10, 9, 11);


void setup() {
  Serial.begin(115200);
  IrReceiver.begin(IR_RECEIVE_PIN, ENABLE_LED_FEEDBACK);
  motor.setSpeed(10);  
}

void loop() {
  if (Serial.available())
  {
    char b = Serial.read();
    if (b == 0x12)
    {
      motor.step(1);
    }
    if (b == 0x13)
    {
      motor.step(-1);
    }
  }
  if (IrReceiver.decode()){
        if (IrReceiver.decodedIRData.protocol != UNKNOWN)
        {
          Serial.print("{ \"command\": ");
          Serial.print(IrReceiver.decodedIRData.command);
          Serial.println(" }");
        }
        IrReceiver.resume();
  }
}
