using OPOS_Projekat.Interfaces;
using OPOS_Projekat.TaskScheduler;
using OPOS_Projekat.TaskExamples;
using OPOS_Projekat.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OPOS_Projekat.Resources;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;

namespace OPOS_Projekat
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //testing FIFO
            /*Scheduler sh = new Scheduler(2, "FIFO");
            sh.Schedule(new TaskSpecification(new TaskTypeA(3), 1, true, 0, null, null));
            sh.Schedule(new TaskSpecification(new TaskTypeA(2), 1, true, 0, null, null));
            sh.Schedule(new TaskSpecification(new TaskTypeA(1), 1, true, 0, null, null));*/

            //testing Priority - lesser number bigger priority
            /*Scheduler sh1 = new Scheduler(1, "PRIORITY");
            sh1.Schedule(new TaskSpecification(new TaskTypeA(3), 1, true, 0, null, null));
            sh1.Schedule(new TaskSpecification(new TaskTypeA(3), 5, true, 0, null, null));
            sh1.Schedule(new TaskSpecification(new TaskTypeA(3), 2, true, 0, null, null));*/

            //testing Priority-Preemptive - CheckForPreemptiveYield()
            /*Scheduler sh2 = new Scheduler(1, "PRIORITYPREEMPTIVE");
            sh2.Schedule(new TaskSpecification(new TaskTypeA(5), 4, true, 0, null, null));
            sh2.Schedule(new TaskSpecification(new TaskTypeA(5), 0, true, 0, null, null));
            sh2.Schedule(new TaskSpecification(new TaskTypeA(5), 2, true, 0, null, null));
            sh2.Schedule(new TaskSpecification(new TaskTypeA(5), 7, true, 0, null, null));
            sh2.Schedule(new TaskSpecification(new TaskTypeA(5), 1, true, 0, null, null));
            sh2.Schedule(new TaskSpecification(new TaskTypeA(5), 10, true, 0, null, null));*/

            //testing Round-Robin - CheckForTimeOut()
            /*Scheduler sh3 = new Scheduler(1, "ROUNDROBIN");
            sh3.Schedule(new TaskSpecification(new TaskTypeA(25), 4, true, 0, null, null));
            sh3.Schedule(new TaskSpecification(new TaskTypeA(25), 5, true, 0, null, null));
            sh3.Schedule(new TaskSpecification(new TaskTypeA(25), 2, true, 0, null, null));*/

            //testing scheduling task without ability to start - canExecute field
            /*Scheduler sh4 = new Scheduler(3, "FIFO");
            var task1 = sh4.Schedule(new TaskSpecification(new TaskTypeA(25), 4, false, 0, null, null));
            sh4.Schedule(new TaskSpecification(new TaskTypeA(25), 5, true, 0, null, null));
            var task2 = sh4.Schedule(new TaskSpecification(new TaskTypeA(25), 2, false, 0, null, null));
            Thread.Sleep(1000);
            Console.WriteLine("Starting task - 1");
            task1.EnableExecution();
            Thread.Sleep(1000);
            Console.WriteLine("Starting task - 2");
            task2.EnableExecution();*/
            /*Scheduler sh = new Scheduler(1, "FIFO");
            var task1 = sh.Schedule(new TaskSpecification(new TaskTypeA(25,2), 4, true, 0, null, null));
            var task2 = sh.Schedule(new TaskSpecification(new TaskTypeA(25, 2), 4, true, 0, null, null));
            Thread.Sleep(1000);
            using (FileStream stream = new FileStream("data.bin", FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, sh);
            }
            foreach (TaskLogic tl in sh.observableTasks)
            {
                int counter = 0;
                string name = "task" + counter++;
                using (FileStream stream = new FileStream("data.bin" + name, FileMode.Create))
                {
                    lock (tl._lock)
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        formatter.Serialize(stream, tl);
                    }
                }
            }
            Thread.Sleep(5000);*/
            //testing start - pause - continue - terminate task CheckForPause(), CheckForTermination()

            /*Scheduler sh5 = new Scheduler(1, "FIFO");
            var task1 = sh5.Schedule(new TaskSpecification(new TaskTypeA(20,1), 4, true, 0, null, null));
            var task2 = sh5.Schedule(new TaskSpecification(new TaskTypeA(20,2), 4, true, 0, null, null));
           Thread.Sleep(2000);
            Console.WriteLine("Pausing task1");
            task1.RequestPause();
            Thread.Sleep(2000);
            Console.WriteLine("Pausing task2");
            task2.RequestPause();
            Thread.Sleep(2000);
            Console.WriteLine("UnPausing task1");
            task1.RequestResume();
            Thread.Sleep(2000);
            Console.WriteLine("UnPausing task2");
            task2.RequestResume();
            Thread.Sleep(2000);
            Console.WriteLine("Pausing task1");
            task1.RequestPause();
            Thread.Sleep(2000);
        Console.WriteLine("Pausing task2");
        task2.RequestPause();
        Console.WriteLine("UnPausing task1");
        task1.RequestResume();*/
            //Console.WriteLine("Terminating task1");
            //task1.RequestTermination();

            //testing waiting for task to be finished
            /*Scheduler sh6 = new Scheduler(1, "FIFO");
            var task1 = sh6.Schedule(new TaskSpecification(new TaskTypeA(10), 4, true, 0, null, null));
            Console.WriteLine("Waiting for task to be finished...");
            task1.Wait();
            Console.WriteLine("Task finished...");*/

            //testing specified duration time
            /*Scheduler sh7 = new Scheduler(1, "FIFO");
            sh7.Schedule(new TaskSpecification(new TaskTypeA(10), 4, true, 5000, null, null));*/

            //testing start time specification
            /*Scheduler sh8 = new Scheduler(1, "FIFO");
            var dt = DateTime.Now;
            var dt2 = dt.AddMilliseconds(10000);
            sh8.Schedule(new TaskSpecification(new TaskTypeA(10), 2, true, 0, dt2, null));
            Thread.Sleep(10000); //ends if main ends*/

            //testing stop time specification
            /*Scheduler sh9 = new Scheduler(1, "FIFO");
            var dt = DateTime.Now;
            var dt2 = dt.AddMilliseconds(2000);
            sh9.Schedule(new TaskSpecification(new TaskTypeA(10), 2, true, 0, null, dt2));*/

            //testing edge detection
            /*Scheduler sh10 = new Scheduler(1, "FIFO");
            List<string> input = new();
            List<string> output = new();
            input.Add("path");
            output.Add("path");
            input.Add("path");
            output.Add("path");
            sh10.Schedule(new TaskSpecification(new EdgeDetectionTask(1,input,output, 2, 2), 2, true, 0, null, null));*/

            //testing resources ACTIVATE CheckForYield()
            /*Scheduler sh = new Scheduler(1  ,"PRIORITYPREEMPTIVE");
            Resource.createResource("A");
            Resource.createResource("B");
            List<string> resursi= new List<string>();
            resursi.Add("A");
            List<string> resursi2 = new();
            resursi2.Add("B");
            //sh.Schedule(new TaskSpecification(new TaskTypeB(resursi,1), 3, true, 0, null, null));
            sh.Schedule(new TaskSpecification(new TaskTypeA(resursi,1), 3, true, 0, null, null));
            Thread.Sleep(2000);
            Console.WriteLine("B");
            sh.Schedule(new TaskSpecification(new TaskTypeA(resursi2,2), 2, true, 0, null, null));
            Thread.Sleep(2000);
            //Thread.Sleep(2000);
            Console.WriteLine("C");
            sh.Schedule(new TaskSpecification(new TaskTypeA(resursi, 3), 1, true, 0, null, null));*/


            //testing filesystem

            /* Scheduler sh = new Scheduler(1,"FIFO");
             string sourceFolder = "path";
             string targetFolder = "path";

             foreach (string filePath in Directory.EnumerateFiles(sourceFolder).ToList())
             {
                 string fileName = Path.GetFileName(filePath);
                 string targetPath = Path.Combine(targetFolder, fileName);
                 File.Copy(filePath, targetPath);
             }
             string outputFolder = "path";
            List<string> input = new();
            List<string> output = new();
             foreach (string filePath in Directory.EnumerateFiles(targetFolder).ToList())
             {
                 string fileName = Path.GetFileName(filePath);
                 string targetPath = Path.Combine(outputFolder, fileName);
                input.Add(filePath);
                output.Add(targetPath);
             }
            sh.Schedule(new TaskSpecification(new EdgeDetectionTask(1,input,output,3,1),2,true,0,null,null));*/


            //List<(string,string)> input = new List<(string,string)> ();
            //input.Add(("path", "path"));
            //input.Add(("path", "path"));
            //input.Add(("path", "path"));
            //input.Add(("path", "path"));
            //input.Add(("path", "path"));
            //input.Add(("path", "path"));
            //sh.Schedule(new TaskSpecification(new EdgeDetectionTask(input, 2,2), 2, true, 0, null, null));
            //STask t1 = sh.Schedule(new TaskSpecification(new TaskTypeA(),2,true,0,null,null));
            //STask t2 = sh.Schedule(new TaskSpecification(new TaskTypeA(), 3, true, 0, null, null));
            //STask t3 = sh.Schedule(new TaskSpecification(new TaskTypeA(), 4, true, 0, null, null));
            //sh.Schedule(new TaskSpecification(new TaskTypeA(),1,true,2000));
            //STask task2 = sh.Schedule(new TaskSpecification(new TaskTypeA(), 1, false,0));
            //Thread.Sleep(120000);
            //t1.RequestPause();
            //t1.Wait();
            //t2.Wait();
            //t1.Wait();
            /*Resource resurs = new Resource("prviResurs");
            Resource resurs2 = new Resource("drugiResurs");
            List<Resource> aLista = new();
            aLista.Add(resurs);
            aLista.Add(resurs2);
            sh.Schedule(new TaskSpecification(new TaskTypeA(aLista),6,true,0,null,null));
            //sh.Schedule(new TaskSpecification(new TaskTypeA(aLista), 1, true, 0, null, null));
            //sh.Schedule(new TaskSpecification(new TaskTypeA(aLista), 1, true, 0, null, null));
            sh.Schedule(new TaskSpecification(new TaskTypeB(aLista), 3, true, 0, null, null));
            sh.Schedule(new TaskSpecification(new TaskTypeA(aLista), 2, true, 0, null, null));
            sh.Schedule(new TaskSpecification(new TaskTypeA(aLista), 6, true, 0, null, null));*/
            //sh.Schedule(new TaskSpecification(new TaskTypeA(aLista), 1, true, 0, null, null));
            //sh.Schedule(new TaskSpecification(new TaskTypeA(aLista), 1, true, 0, null, null));
            //sh.Schedule(new TaskSpecification(new TaskTypeB(aLista), 3, true, 0, null, null));
            //sh.Schedule(new TaskSpecification(new TaskTypeA(aLista), 2, true, 0, null, null));
            /* string sourceFolder = "path";
            string targetFolder = "path";

            foreach (string filePath in Directory.EnumerateFiles(sourceFolder).ToList())
            {
                string fileName = Path.GetFileName(filePath);
                string targetPath = Path.Combine(targetFolder, fileName);
                File.Copy(filePath, targetPath);
            }/
           /List<string> files = new List<string>();
           files = Directory.EnumerateFiles("path").ToList();
           TaskScheduler ts = new TaskScheduler(1, new FifoSchedulingAlgorithm());

           ts.Schedule(new TaskSpecification(new EchoEffect(500,0.8f, "path", files)));
           Console.ReadLine();*/
            //sh.Schedule(new TaskSpecification(new TaskTypeA(),1,true,0,null,null));
            //sh.Schedule(new TaskSpecification(new TaskTypeA(), 1, true, 0, null, null));
            //sh.Schedule(new TaskSpecification(new TaskTypeA(), 1, true, 0, null, null));
            //Thread.Sleep(2000);
            //t1.RequestResume();
            //task2.EnableExecution();
            //task2.RequestPause();
            //Thread.Sleep(1500);
            //task2.RequestResume();
            //Thread.Sleep(500);
            // task2.RequestTermination();

        }
    }
}
