//Deklarasi Variabel
int HeartBeatSensor = A0;
int LED = 3; //LED untuk tanda ketika terdeteksi detakan jantung
int Signal; //Variabel untuk menampung data mentah yang masuk. Nilai sinyal dapat berkisar dari 0-1024
int Threshold = 550; //--> Variabel untuk menentukan Sinyal mana yang akan "dihitung sebagai detak", dan mana yang akan diabaikan.
unsigned long previousMillisGetHR = 0; //Menyimpan kapan terakhir kali Millis (untuk mendapatkan Detak Jantung) diperbarui.
const long intervalGetHR = 10; //Interval pembacaan detak jantung (Heartbeat) = 10ms.
String inputString = ""; //String untuk menyimpan data yang masuk
bool stringComplete = false;
void setup() {
  pinMode(LED,OUTPUT); //Set Pin LED sebagai Output
  Serial.begin(115200); //Set kecepatan (baudrate) komunikasi serial
  inputString.reserve(200); //Menyimpan 200 byte untuk inputString
}

void loop() {
  //Mengirim data ke serial dengan interval 10 ms
  unsigned long currentMillisGetHR = millis();
  if (currentMillisGetHR - previousMillisGetHR >= intervalGetHR) {
    previousMillisGetHR = currentMillisGetHR;
    Signal = analogRead(HeartBeatSensor); //Baca nilai HeartBeatSensor dan menetapkan nilainya ke variabel "Signal".
    Serial.println(Signal); //--> Mengirim nilai signal ke serial
    if(Signal > Threshold){ //Jika nilai signal melebihi nilai threshold (550)
      digitalWrite(LED,HIGH); //LED akan menyala
    } else { //Jika nilai signal dibawah nilai threshold (550)
      digitalWrite(LED,LOW); //LED akan mati
    }
  }
  //Ubah nilai Threshold dari serial / vb application
  if (stringComplete) {
    Threshold = inputString.toInt();
    inputString = "";
    stringComplete = false;
  }
}

//Menerima serial data
void serialEvent() {
  while (Serial.available()) {
    //Mendapakan byte baru
    char inChar = (char)Serial.read();
    // Memasukkan ke inputString:
    inputString += inChar;
    if (inChar == '\n') {
      stringComplete = true;
    }
  }
}
