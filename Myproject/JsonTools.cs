using System;
using System.Globalization;
using Microsoft.SPOT;
using Microsoft.SPOT.Time;
using Json.NETMF;
using System.Collections;

namespace Myproject
{
    class JsonTools
    {
        Double maxTemp = 50;    //valori d'esempio
        Double minTemp = -10;   //valori d'esempio
        Double maxLux = 1000;   //valori d'esempio
        Double minLux = 50;     //valori d'esempio
        Double maxHum = 100;    //valori d'esempio
        Double minHum = 30;     //valori d'esempio
        Double maxSoil = 100;   //valori d'esempio
        Double minSoil = 10;    //valori d'esempio

        public JsonTools() { }

        // This method allows to convert a measure into a json string
        public String jsonSerializeMeasures(Hashtable measureSerializable)
        {
            Measure measure = new Measure();
            ArrayList arrayList = new ArrayList();
            JsonSerializer serializer = new JsonSerializer();

            foreach (DictionaryEntry pair in measureSerializable)   // The key is the sensor, instead the value is the sensor measure
            {
                Measurements measurements = new Measurements();

                measurements.iso_timestamp = getTimestamp();    // Measure timestamp
                measurements.value = Double.Parse(pair.Value.ToString());
                Double value_measure = measurements.value;

                if (pair.Key.ToString().Equals("temperature")){
                    measurements.sensor_id = 1;
                    if (value_measure == 0)
                        measurements.status = "FAIL";
                    else if (value_measure < minTemp || value_measure > maxTemp)
                        measurements.status = "OUTOFRANGE";
                    else
                        measurements.status = "OK";
                }
                else if (pair.Key.ToString().Equals("luminosity"))
                {
                    measurements.sensor_id = 2;
                    if (value_measure == 0)
                        measurements.status = "FAIL";
                    else if (value_measure < minLux || value_measure > maxLux)
                        measurements.status = "OUTOFRANGE";
                    else
                        measurements.status = "OK";
                }
                else if (pair.Key.ToString().Equals("humidity")){
                    measurements.sensor_id = 3;
                    if (value_measure == 0)
                        measurements.status = "FAIL";
                    else if (value_measure < minHum || value_measure > maxHum)
                        measurements.status = "OUTOFRANGE";
                    else
                        measurements.status = "OK";
                }
                else if (pair.Key.ToString().Equals("soil humidity"))
                {
                    measurements.sensor_id = 4;
                    if (value_measure == 0)
                        measurements.status = "FAIL";
                    else if (value_measure < minSoil || value_measure > maxSoil)
                        measurements.status = "OUTOFRANGE";
                    else
                        measurements.status = "OK";
                }                    

                arrayList.Add(measurements);
            }

            measure.version = 1;
            measure.device_id = "FEZ_29";
            measure.iso_timestamp = "0";     //timestamp della trasmissione: inizialmente settato a 0
            measure.measurements = arrayList;

            string jsonData = serializer.Serialize(measure);
            Debug.Print("Json Data: " + jsonData.ToString());
            return jsonData;
        }

        public String jsonSerializeConfiguration()
        {
            JsonSerializer serializer = new JsonSerializer();

            Sensor tempSensor = new Sensor();
            tempSensor.id = 1;
            tempSensor.name = "temperature";
            tempSensor.type = "temperature";
            
            Sensor luxSensor = new Sensor();
            luxSensor.id = 2;
            luxSensor.name = "luminosity";
            luxSensor.type = "luminosity";
            
            Sensor humSensor = new Sensor();
            humSensor.id = 3;
            humSensor.name = "humidity";
            humSensor.type = "humidity";
            
            Sensor soilSensor = new Sensor();
            soilSensor.id = 4;
            soilSensor.name = "soil humidity";
            soilSensor.type = "soil humidity";

            Sensor[] sensorsArray = new Sensor[4];
            sensorsArray[0] = tempSensor;
            sensorsArray[1] = humSensor;
            sensorsArray[2] = luxSensor;
            sensorsArray[3] = soilSensor;

            Configuration conf = new Configuration();
            conf.version = 1;
            conf.id = "FEZ_29";
            conf.name = "Greenhouse monitoring";
            conf.group = "FEZ 29";
            conf.type = "greenhouse";
            conf.sensors = sensorsArray;
            conf.description = "Sensors to measure the temperature, umidity and luminosity of a greenhouse";
            conf.location = "Corso Duca degli Abruzzi, 24, 10129, Torino TO, Italia";
            conf.latitude = 45.0624878;
            conf.longitude = 7.662327699999992;
            conf.@internal = true;

            String jsonConfiguration = serializer.Serialize(conf);
            Debug.Print("Json Configuration: " + jsonConfiguration.ToString());
            return jsonConfiguration;
        }

        /*Per convertire piu misure in Json
         *Creo array di Measurements[], creo oggetti Measeurements e aggiungo a questo array
         *poi alla fine faccio measure.measurements = array di Measurements[]
         */
        public static String getTimestamp()
        {
            DateTime dt = DateTime.Now;
            String timestamp = dt.ToString("yyyy-MM-ddTHH:mm:ss+02:00");
            return timestamp;
        }

        public static String getYearFirstMeasurement(String value)
        {
            JsonSerializer serializer = new JsonSerializer();
            
            Hashtable hashtable = serializer.Deserialize(value) as Hashtable;
            Measure m = new Measure();
            m.measurements = new ArrayList();
            ArrayList arrayList = hashtable["measurements"] as ArrayList;
            Measurements measurements = new Measurements();
            Hashtable ht = arrayList[0] as Hashtable;
            measurements.iso_timestamp = ht["iso_timestamp"].ToString();
            return measurements.iso_timestamp;
        }
    }
}





