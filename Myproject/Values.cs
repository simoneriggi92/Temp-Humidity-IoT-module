using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace Myproject
{
    class Values
    { 
        int vallight;
        int valtemp;
        double real_valtemp, real_vallight;
        int valhum, real_valhum;
        String rounded_light = String.Empty;
        String rounded_temp = String.Empty;

        I2CDevice.Configuration conDeviceLight = new I2CDevice.Configuration(0x23, 100);
        I2CDevice.Configuration conDeviceTemp = new I2CDevice.Configuration(0x40, 100);
        I2CDevice myi2c;

        I2CDevice.I2CTransaction[] Actions = new I2CDevice.I2CTransaction[2];

        byte[] RegisterNumLight = new byte[1] { 0x10 };
        // create read buffer to read the register
        byte[] RegisterValueLight = new byte[2];

        byte[] RegisterNumTemp = new byte[1] { 0xE3 };
        // create read buffer to read the register
        byte[] RegisterValueTemp = new byte[2];

        byte[] RegisterNumHum = new byte[1] { 0xE5 };
        // create read buffer to read the register
        byte[] RegisterValueHum = new byte[2];

        public Values()
        {
            myi2c = new I2CDevice(conDeviceLight);

        }

        public void light_measure()
        {
            myi2c.Config = conDeviceLight;

            Actions[0] = I2CDevice.CreateWriteTransaction(RegisterNumLight);
            Actions[1] = I2CDevice.CreateReadTransaction(RegisterValueLight);

            if (myi2c.Execute(Actions, 1000) == 0)
                Debug.Print("Failed to perform I2C transaction");
            else
            {
                vallight = Int16.Parse((RegisterValueLight[0] << 8 | RegisterValueLight[1]).ToString());
                real_vallight = vallight / 1.2;
                rounded_light = real_vallight.ToString("F2");
                Debug.Print("Illuminance: " + rounded_light + " Lux");
            }
        }

        public void temp_measure()
        {
            myi2c.Config = conDeviceTemp;

            Actions[0] = I2CDevice.CreateWriteTransaction(RegisterNumTemp);
            Actions[1] = I2CDevice.CreateReadTransaction(RegisterValueTemp);

            if (myi2c.Execute(Actions, 1000) == 0)
                Debug.Print("Failed to perform I2C transaction");
            else
            {
                valtemp = Int16.Parse((RegisterValueTemp[0] << 8 | RegisterValueTemp[1]).ToString());
                real_valtemp = ((175.72 * valtemp) / (65536)) - 46.85;
                rounded_temp = real_valtemp.ToString("F2");
           
                Debug.Print("Temperature: " + rounded_temp + " °C");
            }
        }

        public void hum_measure()
        {
            myi2c.Config = conDeviceTemp;

            Actions[0] = I2CDevice.CreateWriteTransaction(RegisterNumHum);
            Actions[1] = I2CDevice.CreateReadTransaction(RegisterValueHum);

            if (myi2c.Execute(Actions, 1000) == 0)
                Debug.Print("Failed to perform I2C transaction");
            else
            {
                valhum = Int32.Parse((RegisterValueHum[0] << 8 | RegisterValueHum[1]).ToString());
                real_valhum = ((125 * valhum) / (65536)) - 6;
                Debug.Print("Relative Humidity: " + real_valhum + " %");
            }
        }

        public String lightvalue()
        {
            //return real_vallight.ToString("F2");
            return rounded_light;
        }

        public String tempvalue()
        {
            //return real_valtemp.ToString("F2");
            return rounded_temp;
        }

        public String humvalue()
        {
            return real_valhum.ToString();
        }

    }
}
