using Microsoft.EntityFrameworkCore.ChangeTracking;
using OPOS_Projekat.Queue;
using OPOS_Projekat.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OPOS_Projekat.TaskScheduler
{
    [Serializable]
    public class Scheduler
    {
        public int maxTasks;
        public string algorithm;
        [NonSerialized]
        private readonly List<TaskLogic> orderedTasks = new();
        [NonSerialized]
        private readonly List<TaskLogic> tasksThatCantExecute = new();
        [NonSerialized]
        public readonly HashSet<TaskLogic> runningTasks = new();
        [NonSerialized]
        public ObservableCollection<TaskLogic> observableTasks = new();
        [NonSerialized]
        private readonly MyQueue taskQueue;
        bool preemptive = false;
        bool timeSlice = false;
        private readonly object _lockS = new();

        public Scheduler(int maxTasks, string algorithm)
        {
            this.maxTasks = maxTasks;
            this.algorithm = algorithm;
            if (algorithm.Equals("FIFO", StringComparison.OrdinalIgnoreCase))
            {
                taskQueue = new MyFifoQueue();
            }
            else if (algorithm.Equals("PRIORITY", StringComparison.OrdinalIgnoreCase))
            {
                taskQueue = new MyPriorityQueue();
            }
            else if (algorithm.Equals("PRIORITYPREEMPTIVE", StringComparison.OrdinalIgnoreCase))
            {
                taskQueue = new MyPriorityQueue();
                preemptive = true;
            }
            else if (algorithm.Equals("ROUNDROBIN", StringComparison.OrdinalIgnoreCase))
            {
                taskQueue = new MyFifoQueue();
                timeSlice = true;
            }
            else
            {
                throw new InvalidOperationException("invalid string");
            }
        }
        public int getQueueSize()
        {
            return this.taskQueue.count();
        }
        public STask Schedule(TaskSpecification taskSpecification)
        {
            //Based on the specification of a task that is a parameter of this function i can create
            //an TaskLogic object which i can then manipulate and have acces to full API
            TaskLogic taskLogic = new(action: taskSpecification.action, priority: taskSpecification.priority,
                onTaskContinueRequested: HandleTaskContinueRequested, onTaskFinished: HandleTaskFinished,
                onTaskPaused: HandleTaskPaused, onEnableExecution: HandleExecutionEnable, canExecute: taskSpecification.canExecute,
                allowedExecutionTime: taskSpecification.allowedExecutionTime, onElapsedTime: HandleOnElapsedTime,
                beginDate: taskSpecification.beginTime, endDate: taskSpecification.stopTime, onPreemptiveYield: HandlePreemptiveYield,
                onTimeOut: HandleOnTimeOut, timeSliceChecker: timeSlice);
            lock (observableTasks)
            {
                observableTasks.Add(taskLogic);
            }
            if (taskSpecification.beginTime != null)
            {
                lock (_lockS)
                {
                    orderedTasks.Add(taskLogic);
                    taskLogic.startBeginDateTimer();
                }
            }
            else
            {
                //calling the function to see if user specified that i can execute it or not
                bool checker = taskLogic.CheckExecutionState();
                lock (_lockS)
                {
                    if (checker)
                    {
                        if (runningTasks.Count < maxTasks)
                        {
                            //if there is free task "space" it can start executing immediatly
                            runningTasks.Add(taskLogic);
                            taskLogic.Start();
                        }
                        else
                        {
                            //if there are is no free task "space" it goes into waiting queue
                            taskQueue.enqueue(taskLogic);
                        }

                    }
                    //if it can not be executed then we put the task in the list of tasks that can not be executed and have to wait
                    //for the user to specify that it can work
                    else
                    {
                        tasksThatCantExecute.Add(taskLogic);
                    }
                }
            }

            STask task = new STask(taskLogic);
            return task;

        }
        public int getNumberOfRunningTasks()
        {
            return runningTasks.Count;
        }
        //we are specify-ing what will happen after task terminates
        private void HandleTaskFinished(TaskLogic taskLogic)
        {
            lock (_lockS)
            {
                runningTasks.Remove(taskLogic);

                if (taskQueue.count() > 0)
                {
                    TaskLogic dequedTaskLogic = taskQueue.dequeue();
                    runningTasks.Add(dequedTaskLogic);
                    dequedTaskLogic.Start();
                }
            }
        }
        //same function as the previous one but with this i am enabling myself to change the behaviour at any point in time
        //for this specified event
        private void HandleTaskPaused(TaskLogic taskLogic)
        {
            lock (_lockS)
            {
                runningTasks.Remove(taskLogic);
                if (taskQueue.count() > 0)
                {
                    TaskLogic dequedTaskLogic = taskQueue.dequeue();
                    runningTasks.Add(dequedTaskLogic);
                    dequedTaskLogic.Start();
                }
            }
        }
        //this function defines what happens when a task was paused but then unpaused, if there is "space" it will continue
        //but if there is not it will be put in queue once again
        private void HandleTaskContinueRequested(TaskLogic taskLogic)
        {
            lock (_lockS)
            {
                if (runningTasks.Count < maxTasks)
                {
                    runningTasks.Add(taskLogic);
                    taskLogic.Start();
                }
                else
                {
                    taskQueue.enqueue(taskLogic);
                }
            }
        }
        //if the user did not allow the task to perform at first but then later on he enabled its execution, this function will be
        //called and it will change its state and boolean value and after that if there is "space" it will start executing
        //else it will be placed in queue
        private void HandleExecutionEnable(TaskLogic taskLogic)
        {
            lock (_lockS)
            {
                tasksThatCantExecute.Remove(taskLogic);
                taskLogic.enableTask();
                if (runningTasks.Count < maxTasks)
                {
                    runningTasks.Add(taskLogic);
                    taskLogic.Start();
                }
                else
                {
                    taskQueue.enqueue(taskLogic);
                }
            }
        }
        private void HandleOnElapsedTime(TaskLogic taskLogic)
        {
            lock (_lockS)
            {
                orderedTasks.Remove(taskLogic);
                taskLogic.enableTask();
                if (runningTasks.Count < maxTasks)
                {
                    runningTasks.Add(taskLogic);
                    taskLogic.Start();
                }
                else
                {
                    taskQueue.enqueue(taskLogic);
                }

            }

        }
        private void HandlePreemptiveYield(TaskLogic taskLogic)
        {
            lock (_lockS)
            {
                if (!this.preemptive)
                    throw new InvalidOperationException("User did not specify that it has preemptive possibilities");
                if (runningTasks.Count == maxTasks && taskQueue.count() > 0)
                {
                    taskQueue.refresh();
                    TaskLogic tl = taskQueue.peek();

                    if (tl.Priority < taskLogic.Priority)
                    {
                        
                        tl = taskQueue.dequeue();
                        taskLogic.changeStateToYield();
                        runningTasks.Add(tl);
                        tl.Start();
                        runningTasks.Remove(taskLogic);
                        taskQueue.enqueue(taskLogic);
                    }
                }
            }
        }
        private void HandleOnTimeOut(TaskLogic taskLogic)
        {
            lock (_lockS)
            {
                if (!this.timeSlice)
                    throw new InvalidOperationException("User did not specify that it has RoundRobin possibilities");
                if (taskQueue.count() > 0)
                {
                    TaskLogic tl = taskQueue.dequeue();
                    runningTasks.Add(tl);
                    tl.Start();
                    runningTasks.Remove(taskLogic);
                    taskQueue.enqueue(taskLogic);
                    taskLogic.changeStateToTimedOut();
                    taskLogic.timeSliceTimer.Stop();
                    if(taskLogic.executionTimer!=null)
                        taskLogic.executionTimer.Stop();
                    if(taskLogic.stopTimer!=null)
                    taskLogic.stopTimer.Stop();
                }
                else
                {
                    taskLogic.changeStateToRunning();
                }
                //taskLogic.createNewTimeSliceTimer();


            }
        }

    }
}
