using OPOS_Projekat.Graph;
using OPOS_Projekat.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPOS_Projekat.Resources
{
    [Serializable]
    public class Resource
    {
        public string name { get; set; } = "resurs";
        (TaskLogic, bool, int) owner = (null, false, 0);
        private static MyGraph _graph = new();
        private Queue<TaskLogic> waitingProcesses = new();
        public object _lock = "";
        private static Dictionary<string, Resource> keyValuePairs = new Dictionary<string, Resource>();
        public Resource() { }
        private Resource(string name)
        {
            this.name = name;
        }
        public static void createResource(string name)
        {
            Resource r = new Resource(name);
            if (!keyValuePairs.ContainsKey(name))
                keyValuePairs.Add(name, r);
            else
                throw new InvalidOperationException("Resource already exists");
        }
        public static Resource getResource(string name)
        {
            if (keyValuePairs.ContainsKey(name))
                return keyValuePairs[name];
            else
                throw new InvalidOperationException("Resource does not exist");

        }
        public void lockResource(TaskLogic process)
        {
            bool status = false; //if there is an owner
            lock (this)
            {
                //if there is no owner we set it and allow it to use
                if (!owner.Item2)
                {
                    owner.Item2 = true; //we set that this resource has an owner
                    owner.Item1 = process; //we set who is the owner
                    owner.Item3 = process.Priority; //and we set its real priority
                }
                else
                {
                    status = true;//there is an owner
                    bool hasCycle;
                    lock (_graph)
                    {
                        hasCycle = _graph.addTransition(process, owner.Item1); //we add the transition into the graph
                    }
                    Console.WriteLine(hasCycle);
                    if (hasCycle)
                    {
                        lock (_graph)
                        {
                            //if we detected a deadlock we throw an exception
                            _graph.removeTransition(process);
                        }
                        throw new Exception("Deadlock detected");
                    }
                    else
                    {
                        if (process.Priority <= this.owner.Item1.Priority)
                        {
                            //process.Priority;
                            this.owner.Item1.Priority = process.Priority - 1;

                        }
                        //else we enqueue this process as an waiting process
                        waitingProcesses.Enqueue(process);

                    }
                }
            }
            if (status)
            {
                lock (process.resourceLock)
                {
                    //and tell him to wait until it is his time
                    //process.CheckForPreemptiveYield();
                    //this.owner.Item1.CheckForPreemptiveYield();
                    //Monitor.Wait(process.resourceLock);
                    process.waitForResource();
                }
            }
        }
        public void unlockResource()
        {
            lock (this)
            {
                this.owner.Item2 = false;
                this.owner.Item1 = null;
                this.owner.Item3 = 0;
                if (waitingProcesses.Count > 0)
                {
                    TaskLogic waitingProcess = waitingProcesses.Dequeue();
                    this.owner.Item1 = waitingProcess;
                    this.owner.Item2 = true;
                    this.owner.Item3 = waitingProcess.Priority;
                    lock (_graph)
                    {
                        _graph.removeTransition(waitingProcess);
                    }
                    waitingProcess.continueAfterResource();
                    /*lock (waitingProcess.resourceLock)
                    {
                        Monitor.Pulse(waitingProcess.resourceLock);
                    }*/
                }
            }
        }
    }
}
