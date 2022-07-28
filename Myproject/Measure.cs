using System;
using Microsoft.SPOT;
using System.Collections;

namespace Myproject
{
    public class Measure
    {
        public int version { get; set; }
        public String device_id { get; set; }
        public String iso_timestamp { get; set; }
        public ArrayList measurements { get; set; }
    }

    public class Measurements
    {
        public int sensor_id { get; set; }
        public String iso_timestamp { get; set; }
        public Double value { get; set; }
        public String status { get; set; }
    }
}
