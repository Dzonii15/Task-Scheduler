using OPOS_Projekat.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPOS_Projekat.Queue
{
    public class MyPriorityQueue : MyQueue
    {
        private PriorityQueue<TaskLogic, int> _queue = new();
        //Comparison<TaskLogic> comparison;
        //ObservableCollection<TaskLogic>
        public void enqueue(TaskLogic task)
        {
            lock (_queue)
            {
                _queue.Enqueue(task, task.getPriority());
            }
        }
        public TaskLogic dequeue()
        {
            lock (_queue)
            {
                return _queue.Dequeue();
            }
        }
        public int count()
        {
            lock (_queue)
            {
                return _queue.Count;
            }
        }
        public TaskLogic peek()
        {
            return this._queue.Peek();
        }
        public void refresh()
        {
            lock (_queue)
            {
                List<TaskLogic> temp = new();
                int n = _queue.Count;
                for (int i = 0; i < n; i++)
                {
                    temp.Add(_queue.Dequeue());
                }
                _queue.Clear();
                foreach (TaskLogic task in temp)
                {
                    _queue.Enqueue(task, task.getPriority());
                }
            }
        }

        /*  public void enqueue(TaskLogic task)
          {
              int index = Items.TakeWhile(x => comparison(x, task) < 0).Count();
              Insert(index, task);
          }

          public int count()
          {
              return this.Count;
          }

          public TaskLogic dequeue()
          {
              TaskLogic item = Items[Count - 1];
              RemoveAt(Count - 1);
              return item;
          }

          public TaskLogic peek()
          {
              return Items[Count - 1];
          }
        */
    }
}
