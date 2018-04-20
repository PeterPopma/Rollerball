// Pin definitions:
// The 74HC595 uses a type of serial connection called SPI
// (Serial Peripheral Interface) that requires three pins:

const int buttonPin = 5;
const int stroboPin = 6;     
const int ball1Pin = A0;
const int ball2Pin = A1;
const int ball3Pin = A2;

const int no_ball_limit =  180; 
bool ball_present[] = {0,0,0};
bool reset_pressed = false;
int strobo_left = 0;

void setup() {
  pinMode(buttonPin, INPUT);
//  pinMode(stroboPin, OUTPUT);
  Serial.begin(9600);
  reset_game();
}

void loop() {
  int tcrt;
  
  for(int k=0; k<3; k++) {
    tcrt = analogRead(A0+k);  // assuming pins A0..A2 are at increasing port numbers
//    Serial.print(tcrt);
//    Serial.print(" ");
    if(!ball_present[k])
    {
      if(tcrt>no_ball_limit)
      {
        ball_present[k] = true;
//        digitalWrite(stroboPin, HIGH);
//        strobo_left = 300;   // approx. 3 seconds strobo light
//        Serial.println(tcrt);  
        Serial.println(char(65+k));  
      }
    } else if(tcrt<no_ball_limit)
    {
      ball_present[k] = false;
    }
  }
// Serial.print("\n");    
/*
  if(strobo_left>0) {
    strobo_left--;
    if(strobo_left==0) {
       digitalWrite(stroboPin, LOW);
    }
  }

  if(reset_pressed==false && digitalRead(buttonPin) == HIGH) {
    reset_pressed==true;
    reset_game();
  } else if(digitalRead(buttonPin) == LOW)  {
    reset_pressed==false;    
  }
  */
  delay(20);    // 50 fps
}

void reset_game() {
  Serial.println(80);
}

