using System;
using Microsoft.SPOT;
using Json.NETMF;
using System.Collections;
using Microsoft.SPOT.Time;
using System.Net;


namespace Myproject
{
    class TimestampTools
    {
        public static bool SetTime()
        {
            try
            {
                TimeServiceSettings timeSettings = new TimeServiceSettings() { ForceSyncAtWakeUp = true };
                IPAddress[] address = Dns.GetHostEntry("time.google.com").AddressList;
                timeSettings.PrimaryServer = address[0].GetAddressBytes();
                TimeService.Settings = timeSettings;
                TimeService.SetTimeZoneOffset(120);
                TimeService.Start();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static string[] realignTimestamp(string[] lines, DateTime startTimestamp)
        {
            DateTime nowUpdate = EthConnection.updateTime;
            String timestampFirst = JsonTools.getYearFirstMeasurement(lines[0].ToString());

            String[] control = timestampFirst.Split('T');
            String[] dateControl = control[0].Split('-');
            String yearControl = dateControl[0];

            if (yearControl.Equals("2011"))
            {
                Debug.Print("Start Realign Timestamp Function!");
                Measure[] measure = new Measure[lines.Length];
                ArrayList differenceMin = new ArrayList();
                ArrayList differenceSec = new ArrayList();
                ArrayList differenceMls = new ArrayList();

                JsonSerializer serializer = new JsonSerializer();

                for (int i = 0; i < lines.Length && !lines[i].Equals(""); i++)
                {
                    string[] substrings = lines[i].Split('&');
                    Hashtable hashTable = serializer.Deserialize(substrings[0]) as Hashtable;
                    Measure m = new Measure();
                    m.measurements = new ArrayList();

                    m.iso_timestamp = hashTable["iso_timestamp"].ToString();
                    m.device_id = hashTable["device_id"].ToString();
                    m.version = int.Parse(hashTable["version"].ToString());

                    ArrayList arrayList = hashTable["measurements"] as ArrayList;

                    foreach (var o in arrayList){
                        Measurements measurements = new Measurements();
                        Hashtable ht = o as Hashtable;
                        measurements.iso_timestamp = ht["iso_timestamp"].ToString();
                        measurements.sensor_id = int.Parse(ht["sensor_id"].ToString());
                        measurements.status = ht["status"].ToString();
                        measurements.value = Double.Parse(ht["value"].ToString());
                        m.measurements.Add(measurements);
                    }

                    measure[i] = m;
                }

                for(int i = 0; i < measure.Length && !measure[i].Equals("") ; i++){
                    foreach (Measurements m in measure[i].measurements)
                    {
                        /*  
                         * Il timestamp ha il seguente formato: yyy-mm-ddThh:mm:ss+02:00
                         * Quindi prima splitto per + così tolgo il fuso; poi splitto per T 
                         * e poi ancora splitto per - la prima sottostringa (data) e per :
                         * la seconda sottostringa (dayTime)
                         * 
                         *      yyyy-MM-dd T hh:mm:ss + 02:00
                         *     |_____________________|  |___|
                         *           substring[0]     substring[1]
                         *     |__________|  |_______|
                         *         date       dayTime
                         *         
                         */

                        String[] substrings = m.iso_timestamp.Split('+');
                        String temp1 = substrings[0];
                        String[] temp2 = temp1.Split('T');
                        String[] date = temp2[0].Split('-');
                        String[] dayTime = temp2[1].Split(':');

                        String year = date[0];

                        if (year.Equals("2011"))
                        {
                            String h = dayTime[0];
                            String min = dayTime[1];
                            String sec = dayTime[2];

                            String mounth = date[1];
                            String day = date[2];

                            DateTime old_data = new DateTime(int.Parse(year), int.Parse(mounth), int.Parse(day), int.Parse(h), int.Parse(min), int.Parse(sec));
                            differenceMin.Add((old_data - startTimestamp).Minutes);
                            differenceSec.Add((old_data - startTimestamp).Seconds);
                            differenceMls.Add((old_data - startTimestamp).Milliseconds);
                            //Debug.Print("--- Init Time: " + startTimestamp.ToString("yyyy-MM-ddTHH:mm:ss"));
                            //Debug.Print("--- Old Time: " + old_data.ToString("yyyy-MM-ddTHH:mm:ss"));
                            //Debug.Print("--- Difference: " + (old_data - startTimestamp));
                        }
                    }
                }

                //inverto le differenze, più vicino è allo startTimestamp minore è lo scarto
                //la prima stringa di lines[] è quella ad avere più scarto

                int[] temp = new int[differenceMin.Count];
                int[] tempS = new int[differenceSec.Count];
                int[] tempM = new int[differenceMls.Count];
                int j = differenceMin.Count - 1;

                foreach (int item in differenceMin)
                {
                    temp[j] = item;
                    j--;
                }

                j = differenceSec.Count -1;
                foreach (int item in differenceSec)
                {
                    tempS[j] = item;
                    j--;
                }

                j = differenceMls.Count -1;
                foreach (int item in differenceMls)
                {
                    tempM[j] = item;
                    j--;
                }

                j = 0;
                for(int i = 0; i < measure.Length && !measure[i].Equals(""); i++){

                    foreach (Measurements m in measure[i].measurements)
                    {
                        string[] substrings = m.iso_timestamp.Split('-');
                        if (substrings[0].Equals("2011"))
                        {
                            //ricostruisco la data
                            string[] time = m.iso_timestamp.Split('T');
                            string[] time_details = time[1].Split(':');


                            //Debug.Print("---NOW: " + nowUpdate.ToString("yyyy-MM-ddTHH:mm:ss"));

                            //sottraggo minuti al time
                            DateTime now2 = nowUpdate.Add(new TimeSpan(0, 0, -temp[j], -tempS[j], -tempM[j]));
                            m.iso_timestamp = now2.ToString("yyyy-MM-ddTHH:mm:ss")+"+02:00";
                            j++;
                        }
                        lines[i] = serializer.Serialize(measure[i]) + "&n";
                    }
                }

                //foreach (String s in lines)
                //{
                //    Debug.Print("Stringa allineata" + s);
                //}
            }
            return lines;
        }
    }
}
