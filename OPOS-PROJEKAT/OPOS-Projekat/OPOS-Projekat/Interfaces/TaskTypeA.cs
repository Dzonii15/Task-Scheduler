using OPOS_Projekat.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPOS_Projekat.Interfaces
{
    [Serializable]
    public class TaskTypeA : IAction
    {
        public int ID { get; set; } = 0;
        public int NumberOfIterations { get; set; } = 10;
        public TaskTypeA(int numberOfIterations, int id)
        {
            ID = id;
            NumberOfIterations = numberOfIterations;
        }
        private int currentIteration = 0;

        public List<Resource> listOfResources { get; set; } = new();
        public TaskTypeA() { listOfResources = new(); }
        public TaskTypeA(List<string> listOfResources, int ID)
        {
            this.ID= ID;
            foreach (string res in listOfResources)
            {
                Resource r = Resource.getResource(res);
                this.listOfResources.Add(r);
            }
        }
        public void Run(ICoopApi coopApi)
        {

            Resource a = listOfResources.ElementAt(0);
            Thread.Sleep(1000);
            coopApi.LockResource(a);
            // coopApi.CheckForPreemptiveYield();
            for (; currentIteration <= NumberOfIterations; currentIteration++)
            {
                coopApi.CheckForPreemptiveYield();
                Console.WriteLine("Task " + ID + "-Iteration:" + currentIteration);
                Thread.Sleep(500);
                double progress = (double)currentIteration / NumberOfIterations;
                coopApi.SetProgress(progress);
                coopApi.CheckForPause();
                //coopApi.CheckForTimeOut();
                coopApi.CheckForTermination();
            }
            Console.WriteLine("Task " + ID + " finished succesfully");
            coopApi.UnlockResource(a);
            Console.WriteLine("Resource Unlocked at TaskType A");

        }

        public int getID()
        {
            return ID;
        }
    }
}
