using System;
using System.Collections.Generic;
using System.IO; 
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using Thorlabs.MotionControl.DeviceManagerCLI;
using Thorlabs.MotionControl.GenericMotorCLI;
using Thorlabs.MotionControl.GenericMotorCLI.ControlParameters;
using Thorlabs.MotionControl.GenericMotorCLI.AdvancedMotor;
using Thorlabs.MotionControl.GenericMotorCLI.KCubeMotor;
using Thorlabs.MotionControl.GenericMotorCLI.Settings;
using Thorlabs.MotionControl.KCube.DCServoCLI;


namespace KDC101Console
{
    class Program
    {
        static void Main(string[] args)
        {
            // Uncomment this line (and SimulationManager.Instance.UninitializeSimulations() at the end on Main)
            // If you are using a simulated device
            // SimulationManager.Instance.InitializeSimulations();

            // Enter the serial number for your device
            string serialNoX = "27265933";
            string serialNoY = "27267203";
            string serialNoZ = "27267208";

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
            KCubeDCServo deviceX = KCubeDCServo.CreateKCubeDCServo(serialNoX);
            KCubeDCServo deviceY = KCubeDCServo.CreateKCubeDCServo(serialNoY);
            KCubeDCServo deviceZ = KCubeDCServo.CreateKCubeDCServo(serialNoZ);

            // We tell the user that we are opening connection to the device. 
            Console.WriteLine("Opening device {0}", serialNoX);
            Console.WriteLine("Opening device {0}", serialNoY);
            Console.WriteLine("Opening device {0}", serialNoZ);

            // Connect to the device. 
            // Wait for the device settings to initialize. We ask the device to 
            // throw an exception if this takes more than 5000ms (5s) to complete. 
            deviceX.Connect(serialNoX);
            deviceX.WaitForSettingsInitialized(5000);

            deviceY.Connect(serialNoY);
            deviceY.WaitForSettingsInitialized(5000);

            deviceZ.Connect(serialNoZ);
            deviceZ.WaitForSettingsInitialized(5000);

            // This calls LoadMotorConfiguration on the device to initialize the 
            // DeviceUnitConverter object required for real world unit parameters.
            MotorConfiguration motorSettingsX = deviceX.LoadMotorConfiguration(deviceX.DeviceID,
            DeviceConfiguration.DeviceSettingsUseOptionType.UseFileSettings);

            MotorConfiguration motorSettingsY = deviceY.LoadMotorConfiguration(deviceY.DeviceID,
            DeviceConfiguration.DeviceSettingsUseOptionType.UseFileSettings);

            MotorConfiguration motorSettingsZ = deviceZ.LoadMotorConfiguration(deviceZ.DeviceID,
            DeviceConfiguration.DeviceSettingsUseOptionType.UseFileSettings);

            // This starts polling the device at intervals of 250ms (0.25s). 

            deviceX.StartPolling(1);
            deviceY.StartPolling(1);
            deviceZ.StartPolling(1);


            // We are now able to Enable the device otherwise any move is ignored. 
            // You should see a physical response from your controller. 
            deviceX.EnableDevice();
            Console.WriteLine("Device X Enabled");

            deviceY.EnableDevice();
            Console.WriteLine("Device Y Enabled");

            deviceY.EnableDevice();
            Console.WriteLine("Device Z Enabled");


            // Needs a delay to give time for the device to be enabled. 
            Thread.Sleep(500);

            // Home the stage/actuator.  

            Console.WriteLine("Actuator is Homing");
            deviceX.Home(60000);
            deviceY.Home(60000);
            deviceZ.Home(60000);

            deviceY.SetTriggerConfigParams(new KCubeTriggerConfigParams()
            {
                Trigger1Mode = KCubeTriggerConfigSettings.TriggerPortMode.TrigIN_RelativeMove,
                Trigger2Mode = KCubeTriggerConfigSettings.TriggerPortMode.TrigOUT_AtMaxVelocity,

                Trigger1Polarity = KCubeTriggerConfigSettings.TriggerPolarity.TriggerHigh,
                Trigger2Polarity = KCubeTriggerConfigSettings.TriggerPolarity.TriggerHigh
            });


            UInt32 status;
            string hex;

            for (int j = 0; j < 1000; j++)
            {
                status = deviceY.GetStatusBits();
                hex = status.ToString("X");
                Console.WriteLine(hex);
            }


            Decimal distance = -6.35M;
            deviceY.SetBacklash(0M);
            deviceY.SetVelocityParams(2.6M, 4M);
            deviceY.SetMoveRelativeDistance(distance);

            Thread.Sleep(500);

            for (int j = 0; j < 1000; j++)
            {
                status = deviceY.GetStatusBits();
                hex = status.ToString("X");
                Console.WriteLine(hex);
            }



            //deviceY.SetJogVelocityParams(2.6M, 4M);
            //deviceY.SetJogStepSize(1M);

            int i = 1;

            Console.WriteLine("Type \"stop\" to end program");
            while (!Console.ReadLine().Contains("stop"))
            {
                i = -1 * i;
                distance = i * distance;
                System.Threading.Thread.Sleep(100);
                deviceY.SetMoveRelativeDistance(distance);
                status = deviceY.GetStatusBits();
                Console.WriteLine(status);
                System.Threading.Thread.Sleep(4000);
            }
            deviceY.SetMoveRelativeDistance(6.35M);
            Thread.Sleep(5000);

            deviceY.SetTriggerConfigParams(new KCubeTriggerConfigParams()
            {
                Trigger1Mode = KCubeTriggerConfigSettings.TriggerPortMode.Disabled,
                Trigger2Mode = KCubeTriggerConfigSettings.TriggerPortMode.Disabled,

                Trigger1Polarity = KCubeTriggerConfigSettings.TriggerPolarity.TriggerHigh,
                Trigger2Polarity = KCubeTriggerConfigSettings.TriggerPolarity.TriggerHigh
            });


            //// Move the stage/actuator to 5mm (or degrees depending on the device 
            //// connected).
            //// device.SetRotationModes(RotationSettings.RotationModes.RotationalRange, RotationSettings.RotationDirections.Reverse);

            //decimal[] positions = { 3.0m, 1.5m, 1.0m
            //};
            //Console.WriteLine("Actuator is Moving");
            //for (int i = 0; i < 2; i++)
            //{
            //    foreach (decimal pos in positions)
            //    {
            //        device.MoveTo(pos, 10000);
            //        Thread.Sleep(1000);
            //        Console.WriteLine("Current position: " + device.Position);
            //    }
            //}

            //Stop polling device
            deviceX.StopPolling();
            deviceY.StopPolling();
            deviceZ.StopPolling();

            // Shut down controller using Disconnect() to close comms
            // Then the used library
            deviceX.ShutDown();
            deviceY.ShutDown();
            deviceZ.ShutDown();


            // Uncomment this line if you are using Simulations
            // SimulationManager.Instance.UninitializeSimulations();

            Console.WriteLine("Complete. Press any key to exit");
            Console.ReadKey();

        }

        public static void HomeStage(KCubeDCServo device, AutoResetEvent waitEvent)
        {
            while (waitEvent.WaitOne())
            {
                waitEvent.Reset();
                device.Home(p => waitEvent.Set());
                waitEvent.Reset();
            }
        }


        public static void MoveStage(KCubeDCServo device, decimal position, AutoResetEvent waitEvent)
        {
            while (waitEvent.WaitOne())
            {
                waitEvent.Reset();
                device.MoveTo(position, p => waitEvent.Set());
                waitEvent.Reset();
            }
        }


        public static void JogForward(KCubeDCServo device, AutoResetEvent waitEvent)
        {
            while (waitEvent.WaitOne())
            {
                waitEvent.Reset();
                device.MoveJog(MotorDirection.Forward, p => waitEvent.Set());
                waitEvent.Reset();
            }
        }


        public static void JogBackward(KCubeDCServo device, AutoResetEvent waitEvent)
        {
            while (waitEvent.WaitOne())
            {
                waitEvent.Reset();
                device.MoveJog(MotorDirection.Backward, p => waitEvent.Set());
                waitEvent.Reset();
            }
        }




        public static void StagePosition(KCubeDCServo deviceX, KCubeDCServo deviceY, KCubeDCServo deviceZ, BinaryWriter bwX, BinaryWriter bwY, BinaryWriter bwZ, BinaryWriter bwT)
        {
            int start = Environment.TickCount;

            while (true)
            {
                int current = Environment.TickCount;
                double positionX = (double)deviceX.Position;
                double positionY = (double)deviceY.Position;
                double positionZ = (double)deviceZ.Position;
                int time = current - start;
                bwX.Write(positionX);
                bwY.Write(positionY);
                bwZ.Write(positionZ);
                bwT.Write(time);
                Thread.Sleep(1);
            }
        }
    }
}