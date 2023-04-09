using OPOS_Projekat.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPOS_Projekat.Queue
{
    public class MyFifoQueue :  MyQueue
    {
        private readonly Queue<TaskLogic> _queue = new();
        //ObservableCollection<TaskLogic>,
        public int count()
        {
            lock (_queue)
            {
                return _queue.Count;
            }
        }

        public TaskLogic dequeue()
        {
            lock (_queue)
            {
                return _queue.Dequeue();
            }
            
        }

        public void enqueue(TaskLogic task)
        {
            lock (_queue)
            {
                _queue.Enqueue(task);
            }
        }

        public TaskLogic peek()
        {
            lock (_queue)
            {
                return _queue.Peek();
            }
        }
        public void refresh()
        {
            List<TaskLogic> temp = new();
            for (int i = 0; i < _queue.Count; i++)
            {
                temp.Add(_queue.Dequeue());
            }
            _queue.Clear();
            foreach (TaskLogic task in temp)
            {
                _queue.Enqueue(task);
            }

        }

        /* override internal void Enqueue(TaskLogic taskLogic)
         {
             lock (_queue)
             {
                 _queue.Enqueue(taskLogic);
             }
         }
          override internal TaskLogic Dequeue()
         {
             lock (_queue)
             {
                 return _queue.Dequeue();
             }
         }

         override internal int Count()
         {
             lock (_queue)
             {
                 return _queue.Count;
             }
         }
         internal override TaskLogic Peek()
         {
             return this._queue.Peek();
         }*/
    }
}
