using System;
using Microsoft.SPOT;

namespace Myproject
{
    public class Configuration
    {
        public int version { get; set; }
        public String id { get; set; }
        public String name { get; set; }
        public String group { get; set; }
        public String type { get; set; }
        public Sensor[] sensors { get; set; }
        public String description { get; set; }
        public String location { get; set; }
        public Double latitude { get; set; }
        public Double longitude { get; set; }
        public bool @internal { get; set; }
    }
}
