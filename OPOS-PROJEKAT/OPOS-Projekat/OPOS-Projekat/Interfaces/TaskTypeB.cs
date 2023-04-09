using OPOS_Projekat.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPOS_Projekat.Interfaces
{
    public class TaskTypeB : IAction
    {
        public int ID { get; set; } = 0;
        List<Resource> listOfResources = new();
        public TaskTypeB() { }
        public TaskTypeB(List<string> listOfResources,int ID) {
            this.ID = ID;
            foreach (string res in listOfResources)
            {
                Resource r = Resource.getResource(res);
                this.listOfResources.Add(r);
            }
        }
        public void Run(ICoopApi coopApi)
        {

            Resource a = listOfResources.ElementAt(0);
            Resource b = listOfResources.ElementAt(1);
            Console.WriteLine("Trying to lock resource 1");
            coopApi.LockResource(a);
            //Thread.Sleep(1000);
            Console.WriteLine("Trying to lock resource 0");
            coopApi.LockResource(b);
            Console.WriteLine("Resource Locked at TaskType B");
            for (int i = 1; i <= 10; i++)
            {
                Console.WriteLine("Task B " + ID + "-Iteration:" + i);
                Thread.Sleep(500);
                double progress = (double)i / 10;
                coopApi.SetProgress(progress);
                //coopApi.CheckForPause();
                //coopApi.CheckForTimeOut();
                //coopApi.CheckForTermination();

            }
            coopApi.UnlockResource(b);
            coopApi.UnlockResource(a);
            Console.WriteLine("Resource Unlocked at TaskType A");

        }

        public int getID()
        {
            return this.ID;
        }
    }
}
