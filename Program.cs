﻿using SharpPcap;
using System;
using System.IO;
using System.Reflection;
using System.Drawing;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace NetDog
{
    class Program
    {
        static void Main(string[] args)
        {
            //Config.DB.User = "SYSDBA";
            //Config.DB.Password = "masterkey";
            //Config.DB.Database = new Path(@".\Tools\Firebird\DB.FDB");
            //Config.DB.Charset = "UTF8";
            //Config.DB.Dialect = 3;
            //Config.DB.Save();

            //Config.NetDog.FirebirdClient = new Path(@".\Tools\Firebird\fbclient.dll");
            //Config.NetDog.Save();

            PropertyInfo[] properties = typeof(Config.DB).GetProperties(BindingFlags.Public | BindingFlags.Static);
            Path bla;

            foreach (PropertyInfo property in properties)
            {
                Console.WriteLine(property.Name);
                Console.WriteLine();
                if (property.PropertyType == typeof(Path))
                {
                    bla = Convert.ChangeType(property.GetValue(null, null), property.PropertyType);
                }
            }

            Console.ReadLine();
            CaptureDeviceList devices = CaptureDeviceList.Instance;

            if (devices.Count < 1)
            {
                Console.WriteLine("No devices were found on this machine");
                return;
            }

            Console.WriteLine("Select Device:");
            Console.WriteLine("");

            // Print out the available network devices
            Regex regex = new Regex(".*'(.*)'.*");
            for (int i = 0; i < devices.Count; i++)
            {
                Console.WriteLine("{0}: {1}", i, regex.firstMatch(devices[i].Description));
            }

            int index = -1;
            while (true)
            {
                try
                {
                    index = int.Parse(Console.ReadLine());
                    if (index >= 0 && index < devices.Count)
                    {
                        break;
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }

            ICaptureDevice device = devices[index];

            device.OnPacketArrival +=
                new SharpPcap.PacketArrivalEventHandler(device_OnPacketArrival);

            int readTimeoutMilliseconds = 1000;
            device.Open(DeviceMode.Promiscuous, readTimeoutMilliseconds);

            Console.WriteLine("-- Listening on {0}, hit 'Enter' to stop...",
                regex.firstMatch(device.Description));


            device.StartCapture();

            // Wait for 'Enter' from the user.
            Console.ReadLine();

            device.StopCapture();
            device.Close();
        }



        /// <summary>
        /// Prints the type and length of each received packet. Getting the Type
        /// of a package isn`t yet implemented.
        /// </summary>
        private static void device_OnPacketArrival(object sender, CaptureEventArgs args)
        {
            int len = args.Packet.Data.Length;
            string type = "";

            Console.WriteLine("{0}: {1}", type, len);
        }
    }
}
