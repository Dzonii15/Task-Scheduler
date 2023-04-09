using OPOS_Projekat.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPOS_Projekat.TaskScheduler
{
    public class TaskSpecification
    {
        public IAction action;
        internal int priority;
        internal bool canExecute;
        internal int allowedExecutionTime;
        internal DateTime? beginTime;
        internal DateTime? stopTime;
        public TaskSpecification(IAction action , int priority, bool canExecute, int allowedExecutionTime,
            DateTime? beginTime, DateTime? stopTime)
        {
            this.action = action;
            this.priority = priority;
            this.canExecute = canExecute;
            this.allowedExecutionTime = allowedExecutionTime;
            this.beginTime = beginTime;
            this.stopTime = stopTime;
        }
    }
}
