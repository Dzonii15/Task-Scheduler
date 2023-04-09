using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OPOS_Projekat.TaskScheduler
{
    [Serializable]
    public class STask 
    {
        public int ID { get; set; } = 1;
        private readonly TaskLogic? taskLogic;
        public double Priority{get;set;}=0;
        //private bool isStartable;
        
        //private bool isPausableOrResumable;
        
        public STask(TaskLogic? taskLogic)
        {
            if(taskLogic!=null)
            ID = taskLogic.ID;

            //id = taskLogic.ID;
            this.taskLogic = taskLogic;
            //this.taskLogic.setProgressHandler(HandleProgressMade); //Job
        }
        internal void Wait()
        {
            taskLogic.Wait();
        }
        public void RequestPause()
        {
            taskLogic.RequestPause();
        }
        internal void RequestResume() 
        {
            taskLogic.RequestResume();
        }
        public void EnableExecution()
        {
            taskLogic.EnableExecution();
        }
        internal void RequestTermination()
        {
            taskLogic.RequestTermination();
        }
        
    }
}
