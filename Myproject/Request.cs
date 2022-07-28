using System;
using Microsoft.SPOT;
using System.Net;
using System.Text;
using System.IO;
using Json.NETMF;
using System.Collections;

namespace Myproject
{
    class Request
    {
        private HttpWebRequest request;
        // private WebRequest request;
        private Stream dataStream;

        public bool uploadMeasure(string data)
        {
            bool sended = false;
            try
            {
                Debug.Print("Data-->" + data);
                request = (System.Net.HttpWebRequest)WebRequest.Create("http://192.168.1.211:51881/SaveMeasure");
                string postData = data;
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                request.Method = "POST";
                request.ContentType = "application/json";
                request.ContentLength = byteArray.Length;

                using (dataStream = request.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                }

                WebResponse response = request.GetResponse();
                string res = ((HttpWebResponse)response).StatusDescription.ToString();
                //Debug.Print(res);
                //string[] substrings = res.Split(' ');
                //res = substrings[1];
                if (res.Equals(" OK"))
                    sended = true;
            }
            catch (Exception e)
            {
                Debug.Print("Errore--> " + e.Message);
            }
            return sended;
        }

        public bool checkMeasureStatus(string data)
        {
            bool acked = false;
            try
            {

                Debug.Print("Data-->" + data);
                request = (System.Net.HttpWebRequest)WebRequest.Create("http://192.168.1.211:51881/MeasureStatus");

                string postData = data;
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                request.Method = "POST";
                request.ContentType = "application/json";
                request.ContentLength = byteArray.Length;

                using (dataStream = request.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                }

                WebResponse response = request.GetResponse();
                string res = ((HttpWebResponse)response).StatusDescription.ToString();
                //string[] substrings = res.Split(' ');
                //res = substrings[1];
                // Display the status.
                Debug.Print("Status-->" + res);
                if (!res.Equals(" OK"))
                    return acked;

                // Get the stream containing content returned by the server.
                dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                string responseFromServer = reader.ReadToEnd();
                string[] substrings = responseFromServer.Split('"');
                responseFromServer = substrings[1];
                // Display the content.
                Debug.Print(responseFromServer);
                if (responseFromServer.Equals("1"))
                    acked = true;
            }
            catch (System.Net.Sockets.SocketException e)
            {
                Debug.Print("Errore--> " + e.Message);
            }
            return acked;
        }

        public String[] setSendTimestamp(string[] lines){
            
            // Deserializzo la stringa json per poter settare il timestamp della trasmissione
            JsonSerializer serializer = new JsonSerializer();
            string[] setted_lines = new string[lines.Length];
            int i = 0;
            string send_timestamp = JsonTools.getTimestamp();
            foreach (String line in lines)
            {
                Hashtable hashtable = serializer.Deserialize(line) as Hashtable;
                Measure m = new Measure();

                m.iso_timestamp = send_timestamp;
                m.device_id = hashtable["device_id"].ToString();
                m.version = int.Parse(hashtable["version"].ToString());
                m.measurements = new ArrayList();

                ArrayList arrayList = hashtable["measurements"] as ArrayList;

                foreach (var o in arrayList)
                {
                    Measurements measurements = new Measurements();
                    Hashtable ht = o as Hashtable;
                    measurements.iso_timestamp = ht["iso_timestamp"].ToString();
                    measurements.sensor_id = int.Parse(ht["sensor_id"].ToString());
                    measurements.status = ht["status"].ToString();
                    measurements.value = Double.Parse(ht["value"].ToString());
                    m.measurements.Add(measurements);
                }

                // Dopo aver deserializzato e settato il timestamp di trasmissione corretto
                // si serializza di nuovo l'oggetto per poterlo restituire
                String jsonData = serializer.Serialize(m);
                setted_lines[i] = jsonData + "&n";
                i++;
            }
            return setted_lines;
        }
    }
}
