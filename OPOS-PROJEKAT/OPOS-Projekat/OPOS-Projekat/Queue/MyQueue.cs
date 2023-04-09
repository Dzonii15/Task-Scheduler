using OPOS_Projekat.TaskScheduler;
using OPOS_Projekat.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPOS_Projekat.Queue
{
    public interface MyQueue
    {
        public  void enqueue(TaskLogic task);
        public  int count();
        public  TaskLogic dequeue();
        public  TaskLogic peek();
        public void refresh();
    }
}
