using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Thorlabs.MotionControl.DeviceManagerCLI;
using Thorlabs.MotionControl.GenericMotorCLI.Settings;
using Thorlabs.MotionControl.GenericMotorCLI;
using Thorlabs.MotionControl.KCube.DCServoCLI;

namespace KDC101Console
{
    class Program
    {
        static void Main(string[] args)
        {
            // Uncomment this line (and SimulationManager.Instance.UninitializeSimulations() at the end on Main)
            // If you are using a simulated device
            SimulationManager.Instance.InitializeSimulations();

            // Enter the serial number for your device
            string serialNo = "27000001";   // Z; 

            //Console.WriteLine("Enter the axis of moving direction (X/Y/Z): ");
            //string strAxis = Console.ReadLine(); 

            //// check if the input is valid 
            //if (!string.IsNullOrEmpty(strAxis) && strAxis.Length == 1)
            //{
            //    char charAxis = strAxis[0]; 
            //    switch (charAxis)
            //    {
            //        case 'X':
            //        case 'x':
            //            serialNo = "27265933";
            //            break;
            //        case 'Y':
            //        case 'y':
            //            serialNo = "27267203";
            //            break;
            //        case 'Z':
            //        case 'z':
            //            serialNo = "27267208";
            //            break;
            //    }
            //}
            //else
            //{                
            //    Console.WriteLine("Invalid input! Will use the default device (Z axis, 27267208)");
            //}
            // string serialNo = "27265933";   // X
            // string serialNo = "27267203";   // Y
            // string serialNo = "27267208";   // Z

            DeviceManagerCLI.BuildDeviceList();


            // This creates an instance of KCubeDCServo class, passing in the Serial 
            //Number parameter.  
            KCubeDCServo device = KCubeDCServo.CreateKCubeDCServo(serialNo);

            // We tell the user that we are opening connection to the device. 
            Console.WriteLine("Opening device {0}", serialNo);

            // This connects to the device. 
            device.Connect(serialNo);

            // Wait for the device settings to initialize. We ask the device to 
            // throw an exception if this takes more than 5000ms (5s) to complete. 
            device.WaitForSettingsInitialized(5000);

            // This calls LoadMotorConfiguration on the device to initialize the 
            // DeviceUnitConverter object required for real world unit parameters.
            MotorConfiguration motorSettings = device.LoadMotorConfiguration(device.DeviceID,
            DeviceConfiguration.DeviceSettingsUseOptionType.UseFileSettings);

            // This starts polling the device at intervals of 250ms (0.25s). 

            device.StartPolling(250);

            // We are now able to Enable the device otherwise any move is ignored. 
            // You should see a physical response from your controller. 
            device.EnableDevice();
            Console.WriteLine("Device Enabled");


            // Needs a delay to give time for the device to be enabled. 
            Thread.Sleep(500);

            // Home the stage/actuator.  

            Console.WriteLine("Actuator is Homing");
            device.Home(60000);

            // Move the stage/actuator to 5mm (or degrees depending on the device 
            // connected).
            // device.SetRotationModes(RotationSettings.RotationModes.RotationalRange, RotationSettings.RotationDirections.Reverse);

            decimal[] positions = { 3.0m, 1.5m, 1.0m
            };
            Console.WriteLine("Actuator is Moving");
            for (int i = 0; i < 2; i++)
            {
                foreach (decimal pos in positions)
                {
                    device.MoveTo(pos, 10000);
                    Thread.Sleep(1000);
                    Console.WriteLine("Current position: " + device.Position);
                }
            }

            //Stop polling device
            device.StopPolling();

            // Shut down controller using Disconnect() to close comms
            // Then the used library
            device.ShutDown();
            

            // Uncomment this line if you are using Simulations
            SimulationManager.Instance.UninitializeSimulations();

            Console.WriteLine("Complete. Press any key to exit");
            Console.ReadKey();

        }
    }
}