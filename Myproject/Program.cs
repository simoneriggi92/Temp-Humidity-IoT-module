using System;
using System.Collections;
using System.Threading;
using System.Text;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Touch;
using Microsoft.SPOT.Hardware;
using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.GHIElectronics;
using GHI.Glide;
using GHI.Glide.Display;
using GHI.Glide.UI;
using System.IO;
using System.Net;
using Json.NETMF;

namespace Myproject
{
    public partial class Program
    {
        public static bool timeSetted = false;
        public static DateTime initTime = new DateTime();
        String rootDirectory;
        String filepath;
        String confFilepath;
        String file = "values.json";
        String confFile = "configuration.json";
        ArrayList lastMeasures = new ArrayList();           //ArrayList to store the last measure 
        Hashtable measureSerializable = new Hashtable();    //HashTable to store the correct measurements that has to be serialized
        JsonTools jTools = new JsonTools();
        JsonSerializer serializer = new JsonSerializer();
        string jsonData;
        Char separator = '&';
        String[] substrings;
        String previousTemp = String.Empty;
        String previousLight = String.Empty;
        String previousHum = String.Empty;
        String previousSoilHum = String.Empty;

        int countTemp = 0;
        int countLight = 0;
        int countHum = 0;
        int countSoilHum = 0;

        Values myi2c = new Values();
        int i=0, threshold=35;
        Gadgeteer.SocketInterfaces.AnalogInput soil_hum_input;
        String soilHumValue;
       
        private static GHI.Glide.Display.Window window;
        private static TextBlock textBlock1, textBlock2, textBlock3, textBlock4, textBlock5, textBlock6,textBlock7,textBlock8;
        EthConnection eth;

        void ProgramStarted()
        {            

            if (sdCard.IsCardInserted)
            {
                //Get the root directory of SDCard
                rootDirectory = sdCard.StorageDevice.RootDirectory;
                //Set values.json path
                filepath = rootDirectory + @"\values.json";
                confFilepath = rootDirectory + @"\configuration.json";
            }
            else
                Debug.Print("SDCard: Unable to get root directory");

            createConfFile();

            //Set Ethernet Configuration
            eth = new EthConnection(ethernetJ11D);
            //Set Fez Time
            timeSetted = TimestampTools.SetTime();
            //Thread.Sleep(2000);
            initTime = DateTime.Now;
            Debug.Print("INIT TIME DEVICE: " + initTime);
            
            GT.Timer timer = new GT.Timer(60000);   // Create a timer
            timer.Tick += timer_Tick;               // Run the method timer_tick when the timer ticks
            timer.Start();                          // Start the time

            window = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.Mystring));

            GHI.Glide.UI.Image Image = (GHI.Glide.UI.Image)window.GetChildByName("contadino");
            Image.Bitmap = new Bitmap(Resources.GetBytes(Resources.BinaryResources.contadino), Bitmap.BitmapImageType.Gif);
            Image.Stretch = true;
            Image.Invalidate();

            GlideTouch.Initialize();

            textBlock1 = (TextBlock)window.GetChildByName("tb1");
            textBlock2 = (TextBlock)window.GetChildByName("tb2");
            textBlock3 = (TextBlock)window.GetChildByName("tb3");
            textBlock4 = (TextBlock)window.GetChildByName("tb4");
            textBlock5 = (TextBlock)window.GetChildByName("tb5");
            textBlock6 = (TextBlock)window.GetChildByName("tb6");
            textBlock7 = (TextBlock)window.GetChildByName("tb7");
            textBlock8 = (TextBlock)window.GetChildByName("tb8");

            Glide.MainWindow = window;

            Gadgeteer.Socket.SocketInterfaces.SetAnalogInputFactors(Gadgeteer.Socket.GetSocket(9, false, null, null), 100, 0.0, 12);
            
            soil_hum_input = breakout2.CreateAnalogInput(GT.Socket.Pin.Three);

            button.ButtonPressed += controller_ButtonPressed;       // Button down event
            button.ButtonReleased += controller_ButtonReleased;     // Button up event

            button2.ButtonPressed += controller_Button2Pressed;     // Button down event
            button2.ButtonReleased += controller_Button2Released;   // Button up event
            
        }

        private void createConfFile()
        {
            JsonTools jsonTools = new JsonTools();
            String configuration = jsonTools.jsonSerializeConfiguration();

            //Write device configuration into the configuration file
            try
            {
                if (sdCard.IsCardInserted)
                {
                    // Check if file exists
                    if (!File.Exists(confFilepath))
                    {
                        // Open or Create file
                        // The using statement also closes the FileStream.
                        using (FileStream fs = sdCard.StorageDevice.Open(confFile, FileMode.Append, FileAccess.Write))
                        {
                            using (StreamWriter writer = new StreamWriter(fs))
                                writer.WriteLine(configuration);
                        }
                    }
                    else
                    {
                        Debug.Print("File already exists!");
                    }
                }
                else
                    Debug.Print("Error saving data into SD card: SD Card is not inserted!");
            }
            catch (Exception)
            {
                Debug.Print("Error writing in file!");
            }
        }


        void timer_Tick(GT.Timer timer)
        {
            
            myi2c.light_measure();      //Get light value from sensor
            //Thread.Sleep(200);
            myi2c.temp_measure();       //Get teperature value from sensor
            //Thread.Sleep(200);
            myi2c.hum_measure();        //Get humidity value from sensor
            //Thread.Sleep(200);
            soilhumvalue();             //Get soil humidity value from sensor
            //Thread.Sleep(200);
                       

            if (previousTemp.Equals(String.Empty) && previousLight.Equals(String.Empty) && previousHum.Equals(String.Empty) && previousSoilHum.Equals(String.Empty))
            {
                Debug.Print("Prima misurazione!");

                //This is the fist measurement, so these measurements can be saved
                measureSerializable.Add("temperature", myi2c.tempvalue());      //id temp_sensor = 1
                measureSerializable.Add("luminosity", myi2c.lightvalue());      //id lum_sensor = 2
                measureSerializable.Add("humidity", myi2c.humvalue());          //id hum_sensor = 3
                measureSerializable.Add("soil humidity", getSoilHumValue());    //id soil_sensor = 4

                //Data Serialization
                jsonData = jTools.jsonSerializeMeasures(measureSerializable);

                //Store data into file
                saveIntoFile(jsonData);

                //Store serializated data into variables that become the previous measurements that the new values will compare with
                previousTemp = myi2c.tempvalue();
                previousLight = myi2c.lightvalue();
                previousHum = myi2c.humvalue();
                previousSoilHum = getSoilHumValue();

                measureSerializable.Clear();
            }
            else
            {
                // Temperature Measurement
                if(!myi2c.tempvalue().Equals(previousTemp)){
                    Debug.Print("--- CONFRONTO TRA (vecchio): " + previousTemp + " e (nuovo): " + myi2c.tempvalue() + " ---> DOVREBBERO ESSERE DIVERSI!!!");
                    previousTemp = myi2c.tempvalue();
                    measureSerializable.Add("temperature", myi2c.tempvalue());
                    countTemp = 0;
                }
                else
                {
                    Debug.Print("--- CONFRONTO TRA (vecchio): " + previousTemp + " e (nuovo): " + myi2c.tempvalue() + " ---> DOVREBBERO ESSERE UGUALI!!!");
                    previousTemp = myi2c.tempvalue();
                    countTemp++;

                    Debug.Print("COUNT TEMPERATURE :" + countTemp);
                    if (countTemp == 15)
                    {
                        measureSerializable.Add("temperature", myi2c.tempvalue());
                        countTemp = 0;
                    }
                }

                // Lux Measurement
                if (!myi2c.lightvalue().Equals(previousLight)){
                    Debug.Print("--- CONFRONTO TRA (vecchio): " + previousLight + " e (nuovo): " + myi2c.lightvalue() + " ---> DOVREBBERO ESSERE DIVERSI!!!");
                    previousLight = myi2c.lightvalue();
                    measureSerializable.Add("luminosity", myi2c.lightvalue());
                    countLight = 0;
                }
                else
                {
                    Debug.Print("--- CONFRONTO TRA (vecchio): " + previousLight + " e (nuovo): " + myi2c.lightvalue() + " ---> DOVREBBERO ESSERE UGUALI!!!");
                    previousLight = myi2c.lightvalue();
                    countLight++;
                    Debug.Print("COUNT LIGHT :" + countLight);
                    if (countLight == 15)
                    {
                        measureSerializable.Add("luminosity", myi2c.lightvalue());
                        countLight = 0;
                    }
                }

                // Humidity Measurement
                if (!myi2c.humvalue().Equals(previousHum))
                {
                    Debug.Print("--- CONFRONTO TRA (vecchio): " + previousHum + " e (nuovo): " + myi2c.humvalue() + " ---> DOVREBBERO ESSERE DIVERSI!!!");
                    measureSerializable.Add("humidity", myi2c.humvalue());
                    previousHum = myi2c.humvalue();
                    countHum = 0;
                }
                else
                {
                    Debug.Print("--- CONFRONTO TRA (vecchio): " + previousHum + " e (nuovo): " + myi2c.humvalue() + " ---> DOVREBBERO ESSERE UGUALI!!!");
                    countHum++;
                    previousHum = myi2c.humvalue();
                    Debug.Print("COUNT HUM :" + countHum);
                    if (countHum == 15)
                    {
                        measureSerializable.Add("humidity", myi2c.humvalue());
                        countHum = 0;
                    }
                }

                // Soil Humidity Measurement
                if (!getSoilHumValue().Equals(previousSoilHum))
                {
                    Debug.Print("--- CONFRONTO TRA (vecchio): " + previousSoilHum + " e (nuovo): " + getSoilHumValue() + " ---> DOVREBBERO ESSERE DIVERSI!!!");
                    measureSerializable.Add("soil humidity", getSoilHumValue());
                    countSoilHum = 0;
                    previousSoilHum = getSoilHumValue();
                }
                else
                {
                    Debug.Print("--- CONFRONTO TRA (vecchio): " + previousSoilHum + " e (nuovo): " + getSoilHumValue() + " ---> DOVREBBERO ESSERE UGUALI!!!");
                    countSoilHum++;
                    previousSoilHum = getSoilHumValue();
                    Debug.Print("COUNT SOIL HUM :" + countSoilHum);
                    if (countSoilHum == 15)
                    {
                        measureSerializable.Add("soil humidity", getSoilHumValue());
                        countSoilHum = 0;
                    }
                }

                //Data Serialization
                jsonData = jTools.jsonSerializeMeasures(measureSerializable);

                //Store serialized data into file
                saveIntoFile(jsonData);
                measureSerializable.Clear();
            }
            
            insert_card();
           
        }

        void controller_ButtonReleased(Gadgeteer.Modules.GHIElectronics.Button sender, Gadgeteer.Modules.GHIElectronics.Button.ButtonState state)
        {
            textBlock7.Text = ("Too dry soil!");
            window.FillRect(textBlock7.Rect);

            textBlock7.Invalidate();

            textBlock8.Text = ("Threshold: " + threshold);
            window.FillRect(textBlock8.Rect);

            textBlock8.Invalidate();
            
        }

        void controller_ButtonPressed(Gadgeteer.Modules.GHIElectronics.Button sender, Gadgeteer.Modules.GHIElectronics.Button.ButtonState state)
        {
            threshold--;
            textBlock7.Text = ("Too dry soil!");
            window.FillRect(textBlock7.Rect);
            
            textBlock7.Invalidate();

            textBlock8.Text = ("Threshold: " +threshold);
            window.FillRect(textBlock8.Rect);

            textBlock8.Invalidate();
            
        }

        void controller_Button2Released(Gadgeteer.Modules.GHIElectronics.Button sender, Gadgeteer.Modules.GHIElectronics.Button.ButtonState state)
        {
            textBlock7.Text = ("Too humid soil!");
            window.FillRect(textBlock6.Rect);

            textBlock7.Invalidate();

            textBlock8.Text = ("Threshold: " + threshold);
            window.FillRect(textBlock8.Rect);

            textBlock8.Invalidate();
        }

        void controller_Button2Pressed(Gadgeteer.Modules.GHIElectronics.Button sender, Gadgeteer.Modules.GHIElectronics.Button.ButtonState state)
        {
            threshold++;

            textBlock7.Text = ("Too humid soil!");
            window.FillRect(textBlock7.Rect);

            textBlock7.Invalidate();

            textBlock8.Text = ("Threshold: " + threshold);
            window.FillRect(textBlock8.Rect);

            textBlock8.Invalidate();
        }

        public void soilhumvalue()
        {
            double soil_hum_value = soil_hum_input.ReadVoltage();
            soilHumValue = soil_hum_value.ToString("F2");
            Debug.Print("Soil Hum: " + soilHumValue);
        }

        public String getSoilHumValue()
        {
            return soilHumValue;
        }

        public void insert_card()
        {
            try
            {
                if (sdCard.IsCardInserted)
                {
                    i++;
                    textBlock1.Text = ("Illuminance: " + myi2c.lightvalue() + " Lux");
                    textBlock2.Text = ("Temperature: " + myi2c.tempvalue() + " C");
                    textBlock3.Text = ("Relative humidity: " + myi2c.humvalue() + " %");
                    textBlock4.Text = ("o");
                    textBlock5.Text = ("");
                    textBlock6.Text = ("Soil humidity: " + getSoilHumValue());
                    textBlock7.Text = ("");
                    textBlock8.Text = ("");
                    window.FillRect(textBlock1.Rect);
                    window.FillRect(textBlock2.Rect);
                    window.FillRect(textBlock3.Rect);
                    window.FillRect(textBlock4.Rect);
                    window.FillRect(textBlock5.Rect);
                    window.FillRect(textBlock6.Rect);
                    window.FillRect(textBlock7.Rect);
                    window.FillRect(textBlock8.Rect);


                    textBlock1.Invalidate();
                    textBlock2.Invalidate();
                    textBlock3.Invalidate();
                    textBlock4.Invalidate();
                    textBlock5.Invalidate();
                    textBlock6.Invalidate();
                    textBlock7.Invalidate();
                    textBlock8.Invalidate();
                    
                    sdCard.Unmount();
                    Thread.Sleep(100);

                    if (!sdCard.IsCardMounted)
                    {
                        sdCard.Mount();
                        Thread.Sleep(100);
                    }
                }
                else
                {
                    textBlock1.Text = ("Illuminance: " + myi2c.lightvalue() + " Lux");
                    textBlock2.Text = ("Temperature: " + myi2c.tempvalue() + " C");
                    textBlock3.Text = ("Relative humidity: " + myi2c.humvalue() + " %");
                    textBlock4.Text = ("o");
                    textBlock5.Text = ("SD card not inserted");
                    textBlock6.Text = ("Soil humidity: " + getSoilHumValue());
                    textBlock7.Text = ("");
                    textBlock8.Text = ("");
                    window.FillRect(textBlock1.Rect);
                    window.FillRect(textBlock2.Rect);
                    window.FillRect(textBlock3.Rect);
                    window.FillRect(textBlock4.Rect);
                    window.FillRect(textBlock5.Rect);
                    window.FillRect(textBlock6.Rect);
                    window.FillRect(textBlock7.Rect);
                    window.FillRect(textBlock8.Rect);
                    textBlock1.Invalidate();
                    textBlock2.Invalidate();
                    textBlock3.Invalidate();
                    textBlock4.Invalidate();
                    textBlock5.Invalidate();
                    textBlock6.Invalidate();
                    textBlock7.Invalidate();
                    textBlock8.Invalidate();
                    i = 0;

                }
            }
            catch (NullReferenceException ex)
            {
                Debug.Print("SD card is not inserted");
            }
            
        }

        // Function to Store Serialized Data into file values.json
        private void saveIntoFile(String jsonData)
        {
            try
            {
                if (sdCard.IsCardInserted)
                {
                    // Open or Create file
                    // The using statement also closes the FileStream.
                    using (FileStream f = sdCard.StorageDevice.Open(file, FileMode.Append, FileAccess.Write))
                    {
                        using (StreamWriter writer = new StreamWriter(f))
                            writer.WriteLine(jsonData+"&n");
                    }
                }
                else
                    Debug.Print("Error saving data into SD card: SD Card is not inserted!");

            }
            catch (Exception e)
            {
                Debug.Print("Error writing in file!");
            }

            //Verifico quello che ho scritto
            readFromFile();
        }

        //Function to Read Serialized Data from file values.json
        public void readFromFile()
        {
            ArrayList valuesRead = new ArrayList();

            try
            {
                // Check if file exists
                if (!File.Exists(filepath))
                    Debug.Print("File doesn't exist!");
                else
                {
                    StreamReader sr = new StreamReader(filepath);
                    string line;
                    int num_lines = 0;
                    int k = 0;

                    while ((line = sr.ReadLine()) != null)
                    {
                        num_lines++;
                    }
                    string[] lines = new string[num_lines];

                    sr.Close();

                    StreamReader sr1 = new StreamReader(filepath);

                    while ((line = sr1.ReadLine()) != null)
                    {
                        lines[k] = line;
                        k++;
                    }

                    sr1.Close();
                    //se ho connessione
                    bool sended = false;
                    bool acked = false;
                    string[] temp_lines = new string[num_lines];
                    string[] timestamp_lines = new string[num_lines];
                    int first = 0;
                    int z = 0;
                    string json_array = "";
                    Request postRequest = new Request();

                    //se ho connessione
                    if (ethernetJ11D.IsNetworkConnected && ethernetJ11D.IsNetworkUp)
                    {
                        lines = TimestampTools.realignTimestamp(lines, initTime);

                        for (int i = 0; i < lines.Length && !lines[i].Equals(""); i++)
                        {
                             substrings = lines[i].Split(separator);
                             if (substrings[1].Equals("n"))
                             {
                                 timestamp_lines[z] = substrings[0];
                                 z++;
                             }
                        }

                        lines = postRequest.setSendTimestamp(timestamp_lines);
                       
                        for (int i = 0; i < lines.Length && !lines[i].Equals(""); i++)
                        {
                            substrings = lines[i].Split(separator);
                            if (substrings[1].Equals("n"))
                            {
                                Debug.Print("Ho letto: " + substrings[0]);
                                if (first == 0)
                                {
                                    if (i != lines.Length - 1)
                                        json_array = "[" + substrings[0] + ",";
                                    else
                                        json_array = "[" + substrings[0];
                                    first++;
                                }
                                else
                                {
                                    if (i != lines.Length - 1)
                                        json_array = json_array + substrings[0] + ",";
                                    else
                                        json_array = json_array + substrings[0];
                                }
                                string s_line = substrings[0] + "&s";
                                //abbiamo sostituito la stringa
                                temp_lines[i] = s_line;
                            }
                            else
                            {
                                //altrimenti mi salvo quella normale
                                temp_lines[i] = lines[i];
                            }

                        }
                        json_array = json_array + "]";
                        // Sending data to remote Web Service
                        
                        //sended = postRequest.uploadMeasure(json_array);   //da decommentare
                        Debug.Print("Ho mandato i dati al server remoto!");


                        //controllo lo stato delle mie misure
                        //mi scorro tutto il vettore di appoggio con le stringhe aggiornate &s e non
                        substrings = null;
                        string[] temp_lines2 = new string[temp_lines.Length];
                        for (int i = 0; i < temp_lines.Length && !temp_lines[i].Equals(""); i++)
                        {
                            substrings = temp_lines[i].Split(separator);
                            if (substrings[1].Equals("s"))
                            {
                                Debug.Print("Misura da verificare: " + substrings[0]);
                                //acked = postRequest.checkMeasureStatus(substrings[0]);    //da decommentare
                                if (acked)
                                {
                                    int n = 0;
                                    //lo elimino dall'array 
                                    for (int j = 0; j < temp_lines.Length; j++)
                                    {
                                        if (!temp_lines[j].Equals(substrings[0] + "&s"))
                                        {
                                            //temp_lines2 diventa l'array aggiornato da scrivere su file
                                            temp_lines2[n] = temp_lines[j];
                                            n++;
                                        }
                                    }
                                }
                                else
                                {
                                    //se non è stata mandata ad amazon, il vettore rimane quello aggiornato con &s
                                    temp_lines2 = temp_lines;
                                }
                                //controllo soltanto una stringa
                                //se acked = true, ho inviato una misura da controllare
                                //altrimenti acked = false, continua il ciclo
                                if (acked)
                                    break;
                            }
                        }
                        //se e solo se ho mandato le misure sosituisco vecchio array con quello nuovo
                        //altrimenti il file rimane lo stesso
                        if (sended)
                        {
                            //scrivo su file stringhe con &s meno quella di cui ho ricevuto l'ack
                            lines = temp_lines2;
                            using (StreamWriter writer = new StreamWriter(filepath, false))
                            {
                                for (int j = 0; j < lines.Length && !lines[j].Equals(""); j++)
                                {
                                    Debug.Print("S ----> " + lines[j]);
                                    writer.WriteLine(lines[j]);
                                }
                            }

                        }

                    }
                    else
                        Debug.Print("Errore! Connessione assente: controlla che il cavo sia collegato");
                }
            }
            catch (IOException)
            {
                // Let the user know what went wrong.
                Debug.Print("Error reading file!");
            }
        }
    }
}
