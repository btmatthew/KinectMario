using Microsoft.Kinect;
using Microsoft.Kinect.VisualGestureBuilder;
using System;
using System.Collections.Generic;
using WindowsInput;
using WindowsInput.Native;
using System.Threading;



namespace Microsoft.Samples.Kinect.BodyBasics
{


    
    class Detector
    {
        private Boolean goRight = true;

        private readonly string databaseLocation = @"C:\Users\codingSociety\Desktop\KinectMario\DatabaseMario\marioGameAll4.gbd";

        /// <summary> Gesture frame source which should be tied to a body tracking ID </summary>
        private VisualGestureBuilderFrameSource vgbFrameSource = null;

        /// <summary> Gesture frame reader which will handle gesture events coming from the sensor </summary>
        private VisualGestureBuilderFrameReader vgbFrameReader = null;

        public Detector(KinectSensor kinectSensor)
        {
            this.vgbFrameSource = new VisualGestureBuilderFrameSource(kinectSensor, 0);
            // open the reader for the vgb frames
            this.vgbFrameReader = this.vgbFrameSource.OpenReader();
            if (this.vgbFrameReader != null)
            {
                this.vgbFrameReader.IsPaused = true;
                this.vgbFrameReader.FrameArrived += this.vgbFramereader_FrameArrived;
            }

            using (VisualGestureBuilderDatabase database = new VisualGestureBuilderDatabase(this.databaseLocation))
            {
                // we could load all available gestures in the database with a call to vgbFrameSource.AddGestures(database.AvailableGestures), 
                // but for this program, we only want to track one discrete gesture from the database, so we'll load it by name
                foreach (Gesture gesture in database.AvailableGestures)
                {
                        this.vgbFrameSource.AddGesture(gesture);
                }
            }

        }


        InputSimulator inputSimulator = new InputSimulator();

        private void vgbFramereader_FrameArrived(object sender, VisualGestureBuilderFrameArrivedEventArgs e)
        {
            //VisualGestureBuilderFrameReference frameReference = e.FrameReference.AcquireFrame;
            using (VisualGestureBuilderFrame frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {

                    IReadOnlyDictionary<Gesture, DiscreteGestureResult> discreteResults = frame.DiscreteGestureResults;

                    if (discreteResults != null)
                    {
                        // we only have one gesture in this source object, but you can get multiple gestures
                        foreach (Gesture gesture in this.vgbFrameSource.Gestures)
                        {
                            if (gesture.GestureType == GestureType.Discrete)
                            {

                                DiscreteGestureResult result = null;
                                discreteResults.TryGetValue(gesture, out result);
                                
                                switch (gesture.Name)
                                {
                                    case "jump":
                                        if (result != null)
                                        {
                                            if (result.Confidence > 0.3)
                                            {
                                                Thread childThread = new Thread(jump);
                                                
                                                    childThread.Start();
                                                
                                                
                                            }
                                        }
                                        break;
                                    case "run":
                                        if (result != null)
                                        {
                                            if (result.Confidence > 0.4)
                                            {
                                                
                                                if (this.goRight)
                                                {
                                                    VirtualKeyCode keyCode = VirtualKeyCode.VK_D;
                                                    inputSimulator.Keyboard.KeyDown(keyCode); // Hold the key down

                                                    Console.WriteLine(" Run Right" + result.Confidence);
                                                }
                                                else
                                                {
                                                    VirtualKeyCode keyCode = VirtualKeyCode.VK_A;
                                                    inputSimulator.Keyboard.KeyDown(keyCode); // Hold the key down
                                                    
                                                    Console.WriteLine(" Run Left" + result.Confidence);
                                                }
                                            }
                                            else if(result.Confidence<0.25)
                                            {
                                                if (this.goRight)
                                                {
                                                    VirtualKeyCode keyCode = VirtualKeyCode.VK_D;
                                                    inputSimulator.Keyboard.KeyUp(keyCode); // Release the key

                                                    Console.WriteLine(" Run Right" + result.Confidence);
                                                }
                                                else
                                                {
                                                    VirtualKeyCode keyCode = VirtualKeyCode.VK_A;
                                                    inputSimulator.Keyboard.KeyUp(keyCode); // Release the key

                                                    Console.WriteLine(" Run Left" + result.Confidence);
                                                }
                                            }
                                        }
                                        break;
                                    
                                    case "crawl":
                                        if (result != null)
                                        {
                                            if (result.Confidence > 0.65)
                                            {
                                                Console.WriteLine("In Main: Creating the Child thread");
                                                Thread childThread = new Thread(crouch);
                                                childThread.Start();

                                                Console.WriteLine(" down " + result.Confidence);
                                            }
                                        }
                                        break;
                                   case "leftArm":
                                        if (result != null)
                                        {
                                            if (result.Confidence > 0.75)
                                            {
                                                Console.WriteLine(" Left " + result.Confidence);
                                                this.goRight = false;
                                                run();
                                            }
                                        }
                                        break;
                                    case "rightArm":
                                        if (result != null)
                                        {
                                            if (result.Confidence > 0.75)
                                            {
                                                Console.WriteLine(" Right " + result.Confidence);
                                                this.goRight = true;
                                                run();
                                            }
                                        }
                                        break;
                                    case "shooting":
                                        if (result != null)
                                        {
                                            if (result.Confidence > 0.75)
                                            {
                                                Console.WriteLine("In Main: Creating the Child thread");
                                                Thread childThread = new Thread(shoot);
                                                childThread.Start();

                                                Console.WriteLine(" shooting " + result.Confidence);
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }


        public void shoot()
        {
            inputSimulator.Keyboard.KeyDown(VirtualKeyCode.VK_X); // Hold the key down
            Thread.Sleep(100);
            inputSimulator.Keyboard.KeyUp(VirtualKeyCode.VK_X); // Release the key
        }

        public void crouch()
        {
            inputSimulator.Keyboard.KeyDown(VirtualKeyCode.VK_S); // Hold the key down
            Thread.Sleep(100);
            inputSimulator.Keyboard.KeyUp(VirtualKeyCode.VK_S); // Release the key

        }
        private void jump()
        {

                inputSimulator.Keyboard.KeyDown(VirtualKeyCode.VK_Z); // Hold the key down
                Thread.Sleep(500);
                run();
                
                inputSimulator.Keyboard.KeyUp(VirtualKeyCode.VK_Z); // Release the key
            }

        private void run()
        {
            if (this.goRight)
            {
                VirtualKeyCode keyCode = VirtualKeyCode.VK_D;
                inputSimulator.Keyboard.KeyDown(keyCode); // Hold the key down  
                Thread.Sleep(100);
                inputSimulator.Keyboard.KeyUp(VirtualKeyCode.VK_D); // Release the key

            }
            else
            {
                VirtualKeyCode keyCode = VirtualKeyCode.VK_A;
                inputSimulator.Keyboard.KeyDown(keyCode); // Hold the key down
                Thread.Sleep(100);
                inputSimulator.Keyboard.KeyUp(VirtualKeyCode.VK_A); // Release the key
            }

        }

        /// <summary>
        /// Disposes all unmanaged resources for the class
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the VisualGestureBuilderFrameSource and VisualGestureBuilderFrameReader objects
        /// </summary>
        /// <param name="disposing">True if Dispose was called directly, false if the GC handles the disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.goRight = true;

                VirtualKeyCode keyCode = VirtualKeyCode.VK_D;
                inputSimulator.Keyboard.KeyUp(keyCode);

                VirtualKeyCode keyCode1 = VirtualKeyCode.VK_A;
                inputSimulator.Keyboard.KeyUp(keyCode1);


                if (this.vgbFrameReader != null)
                {
                    this.vgbFrameReader.FrameArrived -= this.vgbFramereader_FrameArrived;
                    this.vgbFrameReader.Dispose();
                    this.vgbFrameReader = null;
                }

                if (this.vgbFrameSource != null)
                {
                    this.vgbFrameSource.Dispose();
                    this.vgbFrameSource = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the detector is currently paused
        /// If the body tracking ID associated with the detector is not valid, then the detector should be paused
        /// </summary>
        public bool IsPaused
        {
            get
            {
                return this.vgbFrameReader.IsPaused;
            }

            set
            {
                if (this.vgbFrameReader.IsPaused != value)
                {
                    this.vgbFrameReader.IsPaused = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the body tracking ID associated with the current detector
        /// The tracking ID can change whenever a body comes in/out of scope
        /// </summary>
        public ulong TrackingId
        {
            get
            {
                return this.vgbFrameSource.TrackingId;
            }

            set
            {
                if (this.vgbFrameSource.TrackingId != value)
                {
                    this.vgbFrameSource.TrackingId = value;
                }
            }
        }
    }
}


