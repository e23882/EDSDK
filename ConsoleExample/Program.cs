using System;
using System.Collections.Generic;
using System.Threading;
using EDSDKLib;

namespace ConsoleExample
{
    class Program
    {
        static SDKHandler CameraHandler;
        static bool WaitForEvent;

        static void Main(string[] args)
        {
            try
            {
                CameraHandler = new SDKHandler();
                CameraHandler.SDKObjectEvent += handler_SDKObjectEvent;
                List<Camera> cameras = CameraHandler.GetCameraList();
                if (cameras.Count > 0)
                {
                    CameraHandler.OpenSession(cameras[0]);
                    Console.WriteLine("Opened session with camera: " + cameras[0].Info.szDeviceDescription);
                }
                else
                {
                    Console.WriteLine("No camera found. Please plug in camera");
                    CameraHandler.CameraAdded += handler_CameraAdded;
                    CallEvent();                    
                }

                CameraHandler.ImageSaveDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "RemotePhoto");
                CameraHandler.SetSetting(EDSDK.PropID_SaveTo, (uint)EDSDK.EdsSaveTo.Host);

                Console.WriteLine("Taking photo with current settings...");
                CameraHandler.TakePhoto();

                CallEvent();
                Console.WriteLine("Photo taken and saved");
            }
            catch (Exception ex) { Console.WriteLine("Error: " + ex.Message); }
            finally
            {
                CameraHandler.CloseSession();
                CameraHandler.Dispose();
                Console.WriteLine("Good bye! (press any key to close)");
                Console.ReadKey();
            }
        }

        static void CallEvent()
        {
            WaitForEvent = true;
            while (WaitForEvent)
            {
                EDSDK.EdsGetEvent();
                Thread.Sleep(200);
            }
        }

        static uint handler_SDKObjectEvent(uint inEvent, IntPtr inRef, IntPtr inContext)
        {
            if (inEvent == EDSDK.ObjectEvent_DirItemRequestTransfer || inEvent == EDSDK.ObjectEvent_DirItemCreated) WaitForEvent = false;
            return EDSDK.EDS_ERR_OK;
        }

        static void handler_CameraAdded()
        {
            List<Camera> cameras = CameraHandler.GetCameraList();
            if (cameras.Count > 0) CameraHandler.OpenSession(cameras[0]);
            Console.WriteLine("Opened session with camera: " + cameras[0].Info.szDeviceDescription);
            WaitForEvent = false;
        }
    }
}
