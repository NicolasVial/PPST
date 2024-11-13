// Uduino Default Board
#include<Uduino.h>
Uduino uduino("uduinoBoard");  // Declare and name your object

// Servo
#include <Servo.h>
#define MAXSERVOS 8

// Pin definitions
const int nearLED = 13;           // LED at 5 cm
const int farLED = 12;            // LED at 60 cm
const int fixationCrossLED = 10;  // LED behind fixation cross
const int tactileStimPin = 11;    // Pin for tactile stimulator

unsigned long LEDStartTime = 0;
unsigned long tactileStimStartTime = 0;
bool needToLowerLED = false;
bool needToLowerTactileStim = false;
int LEDToLowerPinNb = 0;

int visuotactileDelay = 0;
unsigned long visuotactileStartTime = 0;
bool isTactileNext = true;
int visuoTactileSelectedLed = 0;
bool isVisuoTactileTrial = false;
int visuoTactileId = 0;
bool resetTriggerPins = false;
unsigned long pinsTriggeredTime = 0;

// Trial parameters
const unsigned long stimDuration = 50;                                 // Duration of stimuli (visual and tactile) in ms
const float delays[] = { -500, -250, -125, -62.5, 0, 62.5, 125, 250, 500 };  // Temporal delays in ms                                  // Maximum ITI in ms

// Trial definitions and counts
enum TrialType { VISUOTACTILE,
                 VISUAL_ONLY,
                 TACTILE_ONLY,
                 ODDBALL };

// Structure to hold trial information
struct Trial {
  TrialType type; // Type of the trial
  bool near;      // true for near, false for far
  float delay;     // delay between LED flash and tactile stimulation
  int id;
};

// Timing variables
unsigned long lastTrialTime = 0;
unsigned long currentMillis = 0;

// Helper functions
void triggerLED(int ledPin) {
  digitalWrite(ledPin, HIGH);
  LEDToLowerPinNb = ledPin;
  needToLowerLED = true;
  LEDStartTime = millis();
}

void lowerLED(int ledPin){
  digitalWrite(ledPin, LOW);
}

void triggerTactileStim() {
  digitalWrite(tactileStimPin, HIGH);
  needToLowerTactileStim = true;
  tactileStimStartTime = millis();
}

void lowerTactileSim(){
  digitalWrite(tactileStimPin, LOW);
}

void allTriggerPinsLow(){
  digitalWrite(0, LOW);
  digitalWrite(1, LOW);
  digitalWrite(2, LOW);
  digitalWrite(3, LOW);
  digitalWrite(4, LOW);
  digitalWrite(5, LOW);
  digitalWrite(6, LOW);
  digitalWrite(7, LOW);
}

void loop() {
  uduino.update();  // Keep Uduino listening for commands
  
  unsigned long currentMillis = millis();
  /*
  if(resetTriggerPins && currentMillis - pinsTriggeredTime >= 25){
    allTriggerPinsLow();
  }
  */
  if(needToLowerLED && currentMillis - LEDStartTime >= stimDuration){
    lowerLED(LEDToLowerPinNb);
  }
  if(needToLowerTactileStim && currentMillis - tactileStimStartTime >= stimDuration){
    lowerTactileSim();
  }

  if(isVisuoTactileTrial && isTactileNext && currentMillis - visuotactileStartTime >= visuotactileDelay){
    triggerTactileStim(); 
    setPinsHigh(visuoTactileId + 100);
    pinsTriggeredTime = millis();
    resetTriggerPins = true;
    isVisuoTactileTrial = false;
  }

  if(isVisuoTactileTrial && !isTactileNext && currentMillis - visuotactileStartTime >= visuotactileDelay){
    triggerLED(visuoTactileSelectedLed);
    setPinsHigh(visuoTactileId);  
    pinsTriggeredTime = millis();
    resetTriggerPins = true;
    isVisuoTactileTrial = false;
  }

}

void setup() {
  Serial.begin(9600);

#if defined(__AVR_ATmega32U4__)  // Leonardo
  while (!Serial) {}
#elif defined(__PIC32MX__)
  delay(1000);
#endif

  uduino.addCommand("s", SetMode);
  uduino.addCommand("d", WritePinDigital);
  uduino.addCommand("a", WritePinAnalog);
  uduino.addCommand("rd", ReadDigitalPin);
  uduino.addCommand("r", ReadAnalogPin);
  uduino.addCommand("br", BundleReadPin);
  uduino.addCommand("b", ReadBundle);
  uduino.addCommand("startTrial", startTrial);
  uduino.addCommand("testSetPinsHigh", testSetPinsHigh);
  uduino.addInitFunction(DisconnectAllServos);
  uduino.addDisconnectedFunction(DisconnectAllServos);

  // Initialize pins
  pinMode(nearLED, OUTPUT);
  pinMode(farLED, OUTPUT);
  pinMode(fixationCrossLED, OUTPUT);
  pinMode(tactileStimPin, OUTPUT);
  pinMode(0, OUTPUT);
  pinMode(1, OUTPUT);
  pinMode(2, OUTPUT);
  pinMode(3, OUTPUT);
  pinMode(4, OUTPUT);
  pinMode(5, OUTPUT);
  pinMode(6, OUTPUT);
  pinMode(7, OUTPUT);
}


void executeTrial(Trial trial) {
  int selectedLED = trial.near ? nearLED : farLED;

  switch (trial.type) {
    case VISUOTACTILE:
      visuotactileStartTime = millis();
      visuoTactileSelectedLed = selectedLED; 
      if (trial.delay > 0) {
        triggerLED(selectedLED);
        setPinsHigh(trial.id);
        isVisuoTactileTrial = true;
        pinsTriggeredTime = millis();
        resetTriggerPins = true;
        visuoTactileId = trial.id;
        isTactileNext = true;    
        visuotactileDelay = trial.delay;         
      } else if (trial.delay < 0) {
        triggerTactileStim();
        setPinsHigh(trial.id + 100);
        isVisuoTactileTrial = true;
        pinsTriggeredTime = millis();
        resetTriggerPins = true;
        visuoTactileId = trial.id;
        isTactileNext = false; 
        visuotactileDelay = -trial.delay;                              
      }else if(trial.delay == 0){
        triggerLED(selectedLED);
        triggerTactileStim();
        setPinsHigh(trial.id);
        pinsTriggeredTime = millis();
        resetTriggerPins = true;
        visuoTactileId = trial.id;  
        visuotactileDelay = trial.delay;  
      }
      break;
      
    case VISUAL_ONLY:
      triggerLED(selectedLED);   
      setPinsHigh(trial.id);              // LED only
      break;
      
    case TACTILE_ONLY:
      triggerTactileStim();  
      setPinsHigh(trial.id);                  // Tactile only
      break;
      
    case ODDBALL:
      triggerLED(fixationCrossLED);  
      setPinsHigh(trial.id);          // LED behind fixation cross
      break;
  }
}

// Function to handle the startTrial command from Unity and pass parameters to executeTrial
void startTrial() {
  Trial trial;
  
  char *arg;
  arg = uduino.next();

  trial.type = static_cast<TrialType>(uduino.charToInt(arg));
  arg = uduino.next();
  trial.near = uduino.charToInt(arg) == 1; 
  arg = uduino.next();
  int delay = uduino.charToInt(arg);
  trial.delay = delay / 10;
  arg = uduino.next();
  trial.id = uduino.charToInt(arg);
  // Call executeTrial with the constructed Trial
  executeTrial(trial);
}

void setPinsHigh(int decimal) {
  allTriggerPinsLow();
  for (int i = 0; i <= 7; i++) {
    if (decimal & (1 << i)) {  // Check if the i-th bit is set
      digitalWrite(i, HIGH);    // Set the pin to HIGH
    } else {
      digitalWrite(i, LOW);     // Set the pin to LOW if the bit is not set
    }
  }
}

void testSetPinsHigh(){
  char *arg;
  arg = uduino.next();

  int decimal = uduino.charToInt(arg);
  allTriggerPinsLow();
  for (int i = 0; i <= 7; i++) {
    if (decimal & (1 << i)) {  // Check if the i-th bit is set
      digitalWrite(i, HIGH);    // Set the pin to HIGH
    } else {
      digitalWrite(i, LOW);     // Set the pin to LOW if the bit is not set
    }
  }
}

int pinsToDecimal(int pins[])
{
    int result = 0;
    for (int i = 0; i < sizeof(pins); i++)
    {
        int pin = pins[i];
        if (pin >= 0 && pin <= 13)
        { // Ensure the pin is within the 0-13 range
            result |= (1 << pin);      // Set the bit corresponding to the pin
        }
    }
    return result;
}



void WritePinDigital() {
  int pinToMap = -1;
  char *arg = NULL;
  arg = uduino.next();
  if (arg != NULL)
    pinToMap = atoi(arg);

  int writeValue;
  arg = uduino.next();
  if (arg != NULL && pinToMap != -1) {
    writeValue = atoi(arg);
    //if(pinToMap == startPin && writeValue == HIGH){
    //  triggerLED(fixationCrossLED);
    //}
  }
}

void ReadBundle() {
  char *arg = NULL;
  char *number = NULL;
  number = uduino.next();
  int len = 0;
  if (number != NULL)
    len = atoi(number);
  for (int i = 0; i < len; i++) {
    uduino.launchCommand(arg);
  }
}

void SetMode() {
  int pinToMap = 100;  //100 is never reached
  char *arg = NULL;
  arg = uduino.next();
  if (arg != NULL) {
    pinToMap = atoi(arg);
  }
  int type;
  arg = uduino.next();
  if (arg != NULL) {
    type = atoi(arg);
    PinSetMode(pinToMap, type);
  }
}

void PinSetMode(int pin, int type) {
  /*
  switch (type) {
    case 0: // Output
      pinMode(pin, OUTPUT);
      break;
    case 1: // PWM
      pinMode(pin, OUTPUT);
      break;
    case 2: // Analog
      pinMode(pin, OUTPUT);
      break;
    case 3: // Input_Pullup
      pinMode(pin, OUTPUT);
      break;
    case 4: // Input_Pullup
      pinMode(pin, OUTPUT);
      break;
    case 5: // Input_Pullup
      pinMode(pin, OUTPUT);
      break;
    case 6: // Input_Pullup
      pinMode(pin, OUTPUT);
      break;
    case 7: // Input_Pullup
      pinMode(pin, OUTPUT);
      break;
  }
  */
}

void WritePinAnalog() {
  int pinToMap = 100;
  char *arg = NULL;
  arg = uduino.next();
  if (arg != NULL) {
    pinToMap = atoi(arg);
  }

  int valueToWrite;
  arg = uduino.next();
  if (arg != NULL) {
    valueToWrite = atoi(arg);

    if (ServoConnectedPin(pinToMap)) {
      UpdateServo(pinToMap, valueToWrite);
    } else {
      analogWrite(pinToMap, valueToWrite);
    }
  }
}

void ReadAnalogPin() {
  int pinToRead = -1;
  char *arg = NULL;
  arg = uduino.next();
  if (arg != NULL) {
    pinToRead = atoi(arg);
    if (pinToRead != -1)
      printValue(pinToRead, analogRead(pinToRead));
  }
}

void ReadDigitalPin() {
  int pinToRead = -1;
  char *arg = NULL;
  arg = uduino.next();
  if (arg != NULL) {
    pinToRead = atoi(arg);
  }

  if (pinToRead != -1)
    printValue(pinToRead, digitalRead(pinToRead));
}

void BundleReadPin() {
  int pinToRead = -1;
  char *arg = NULL;
  arg = uduino.next();
  if (arg != NULL) {
    pinToRead = atoi(arg);
    if (pinToRead != -1) {
      if (pinToRead < 13)
        printValue(pinToRead, digitalRead(pinToRead));
      else
        printValue(pinToRead, analogRead(pinToRead));
    }
  }
}

Servo myservo;



void printValue(int pin, int targetValue) {
  uduino.print(pin);
  uduino.print(" ");  //<- Todo : Change that with Uduino delimiter
  uduino.println(targetValue);
  // TODO : Here we could put bundle read multiple pins if(Bundle) { ... add delimiter // } ...
}




/* SERVO CODE */
Servo servos[MAXSERVOS];
int servoPinMap[MAXSERVOS];
/*
  void InitializeServos() {
  for (int i = 0; i < MAXSERVOS - 1; i++ ) {
    servoPinMap[i] = -1;
    servos[i].detach();
  }
  }
*/
void SetupServo(int pin) {
  if (ServoConnectedPin(pin))
    return;

  int nextIndex = GetAvailableIndexByPin(-1);
  if (nextIndex == -1)
    nextIndex = 0;
  servos[nextIndex].attach(pin);
  servoPinMap[nextIndex] = pin;
}


void DisconnectServo(int pin) {
  servos[GetAvailableIndexByPin(pin)].detach();
  servoPinMap[GetAvailableIndexByPin(pin)] = 0;
}

bool ServoConnectedPin(int pin) {
  if (GetAvailableIndexByPin(pin) == -1) return false;
  else return true;
}

int GetAvailableIndexByPin(int pin) {
  for (int i = 0; i < MAXSERVOS - 1; i++) {
    if (servoPinMap[i] == pin) {
      return i;
    } else if (pin == -1 && servoPinMap[i] < 0) {
      return i;  // return the first available index
    }
  }
  return -1;
}

void UpdateServo(int pin, int targetValue) {
  int index = GetAvailableIndexByPin(pin);
  servos[index].write(targetValue);
  delay(10);
}

void DisconnectAllServos() {
  for (int i = 0; i < MAXSERVOS; i++) {
    servos[i].detach();
    digitalWrite(servoPinMap[i], LOW);
    servoPinMap[i] = -1;
  }
}
