/*
  IIDXTicker
*/

#include <Arduino.h>
#include <MD_Parola.h>

/* Display settings */
uint8_t MD_SIZE = 8;                        //Number of displays
uint8_t MD_DATA = 14;                       //DATA pin
uint8_t MD_CLK = 9;                         //CLK PIN
uint8_t MD_CS = 10;                         //CS PIN
uint8_t MD_Intensity = 0;                   //Display instensity (Min: 0 Max: 15)

/* Serial */
#define BAUD 115200                         //Serial baud

/* IIDXTicker buffer */
byte _receivedBytes[10];

/*  */
MD_Parola MAX = MD_Parola(MD_DATA, MD_CLK, MD_CS, MD_SIZE);

void setup() {
  // Serial baud
  Serial.begin(BAUD);

  // Initialize MD_Parola
  MAX.begin();
  
  // Set display intensity
  MAX.setIntensity(MD_Intensity);
}

void loop() {
  // Run service
  if (MAX.displayAnimate()) {
    if (Serial.available() > 0) {
      // IIDXTicker is running...
      Serial.readBytes(_receivedBytes, 9);
      MAX.displayText((char*)_receivedBytes, PA_CENTER, 0, 0, PA_PRINT, PA_NO_EFFECT);
    } else {
      // Clear buffer
      memset(_receivedBytes, 0, sizeof(_receivedBytes));
    }
  }
}
