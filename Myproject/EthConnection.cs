using System;
using Microsoft.SPOT;
using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.GHIElectronics;
using Microsoft.SPOT.Net.NetworkInformation;
using System.Net;
using System.Threading;

namespace Myproject
{
    class EthConnection
    {
        EthernetJ11D ethernetJ11D;
        public static DateTime updateTime = new DateTime();

        // This method is run when the mainboard is powered up or reset. 
        public EthConnection(EthernetJ11D ethernetJ11D)
        {
            this.ethernetJ11D = ethernetJ11D;
            Debug.Print("Program Started");

            SetupEthernet();

            ethernetJ11D.NetworkUp += OnNetworkUp;
            ethernetJ11D.NetworkDown += OnNetworkDown;
        }

        void OnNetworkDown(GTM.Module.NetworkModule sender, GTM.Module.NetworkModule.NetworkState state)
        {
            Debug.Print("Network down.");
        }

        void OnNetworkUp(GTM.Module.NetworkModule sender, GTM.Module.NetworkModule.NetworkState state)
        {
            Debug.Print("Network up.");

            if (!Program.timeSetted)
            {
                Program.timeSetted = TimestampTools.SetTime();
                Thread.Sleep(2000);
                updateTime = DateTime.Now;
            }
            //ListNetworkInterfaces();

            Debug.Print("INIT TIME DEVICE: " + Program.initTime);
            Debug.Print("UPDATE TIME DEVICE: " + updateTime);
        }

        void SetupEthernet()
        {
            ethernetJ11D.NetworkInterface.Open();
            ethernetJ11D.NetworkInterface.EnableDynamicDns();
            
            //If I use Chiara Home Wifi
            /*ethernetJ11D.UseStaticIP(
            "192.168.1.159",
                "255.255.255.0",
                "192.168.1.254");*/
            
            //If I use Simone Home Wifi
            ethernetJ11D.UseStaticIP(
            "192.168.1.200",
                "255.255.255.0",
                "192.168.1.1");
            
            //If I use Chiara's Hotspot
            /*ethernetJ11D.UseStaticIP(
                "192.168.1.200",
                "255.255.255.0",
                "192.168.1.254");*/
             

            //Campus
            /*ethernetJ11D.UseStaticIP(
                "192.168.22.90",
                "255.255.248.0",
                "192.168.21.254");*/
        }

        void ListNetworkInterfaces()
        {
            var settings = ethernetJ11D.NetworkSettings;

            Debug.Print("------------------------------------------------");
            //Debug.Print("MAC: " + ByteExtensions.ToHexString(settings.PhysicalAddress, "-"));
            Debug.Print("IP Address:   " + settings.IPAddress);
            Debug.Print("DHCP Enabled: " + settings.IsDhcpEnabled);
            Debug.Print("Subnet Mask:  " + settings.SubnetMask);
            Debug.Print("Gateway:      " + settings.GatewayAddress);
            Debug.Print("------------------------------------------------");
        }
    }
}
