using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using OPOS_Projekat.Interfaces;
using OPOS_Projekat.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace OPOS_Projekat.TaskScheduler

{
    [Serializable]
    public class TaskLogic : ICoopApi, INotifyPropertyChanged
    {
        public enum TaskStates
        {
            New, //justCreated
            CanNotExecute, //User did not specify for it to be able to execute

            Waiting, //Scheduled but did not get CPU time
            WaitingToResume,
            WaitingOnResource,
            NotTimeToExecute,
            Running, //Executing
            RunningWithPauseRequest, //User requested for it to wait
            RunningWithTerminationRequest, //User requested for it to terminate
            RunningWithTimeOutRequest,
            Yielded, //Task intentionally yielded because there is a task with more priority in the queue
            TimedOut, //Task used its time slice and needs to let other tasks to execute
            Paused, //Task is paused
            Terminated //Terminated
        }

        public IAction action;
        private static int timeSlice = 10000;//defined duration of time slice
        internal TaskStates _taskState; //current state of the task
        [NonSerialized]
        private readonly Thread thread; //Thread which will execute my Task
        public object _lock = new();//lock ""
        public bool canExecute = true;//did user specify that my task can execute
        private bool timeSliceChecker;//did he specify that we will use time slice
        [NonSerialized]
        private readonly Action<TaskLogic> onTaskFinished;//action that happens when task finished
        [NonSerialized]
        private readonly Action<TaskLogic> onTaskPaused;//action that happens when task paused
        [NonSerialized]
        private readonly Action<TaskLogic> onTaskContinueRequested;//action that happens when he resumes
        [NonSerialized]
        private readonly Action<TaskLogic> onEnableExecution;//action that happens when he is enabled to execute
        [NonSerialized]
        private readonly Action<TaskLogic> onElapsedTime;//action that happens when task is scheduled to start
        [NonSerialized]
        private readonly Action<TaskLogic> onPreemptiveYield;//action that happens when 
        [NonSerialized]
        private readonly Action<TaskLogic> onTimeOut;//action that happens when time slice expired
        [NonSerialized]
        private readonly Action<TaskLogic> onReleaseResource;
        [NonSerialized]
        private Action<double> onProgressMade;
        [NonSerialized]
        private int priority;
        public int Priority { get; set; }
        [NonSerialized]
        public System.Timers.Timer? executionTimer;//timer if the user limited its execution time
        [NonSerialized]
        private System.Timers.Timer? startTimer;//timer if the user specified when it should start
        [NonSerialized]
        public System.Timers.Timer? stopTimer;//timer if the user specified when it should end
        [NonSerialized]
        public System.Timers.Timer? timeSliceTimer;//timer if the user specified a time slice
        private static int id = 0;
        public int allowedExecutiontime;
        [NonSerialized]
        public DateTime? startTime;
        [NonSerialized]
        public DateTime? endTime;
        public int ID { get; set; }
        private double progress;
        public object resourceLock = new();
        bool isWaitingForResource = false;
        public TaskStates State
        {
            get => _taskState;
            private set
            {
                _taskState = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(IsStartable));
                NotifyPropertyChanged(nameof(IsPausableOrResumable));
                NotifyPropertyChanged(nameof(IsStoppable));
                NotifyPropertyChanged(nameof(IsCloseable));
            }

        }
        /*public bool IsCloseable
            {
            }*/
        public double Progress
        {
            get
            {
                return progress;
            }
            set
            {
                progress = value;
                NotifyPropertyChanged();
            }
        }
        public bool IsStoppable
        {
            get
            {
                return this._taskState == TaskStates.Running;
            }
        }
        public bool IsCloseable
        {
            get
            {
                return this._taskState == TaskStates.Terminated;
            }
        }
        public bool IsStartable
        {
            get
            {
                return this._taskState == TaskStates.CanNotExecute || this._taskState == TaskStates.Paused;
            }
        }
        public bool IsPausableOrResumable
        {
            get
            {
                return (this._taskState == TaskLogic.TaskStates.Running || this._taskState == TaskLogic.TaskStates.Paused);
            }
        }
        [field: NonSerialized]
        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyID = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyID));
        }



        //constructor
        public TaskLogic(IAction action, int priority, Action<TaskLogic> onTaskFinished, Action<TaskLogic> onTaskPaused,
            Action<TaskLogic> onTaskContinueRequested, Action<TaskLogic> onEnableExecution, Action<TaskLogic> onElapsedTime
            , bool canExecute, int allowedExecutionTime, DateTime? beginDate, DateTime? endDate, Action<TaskLogic> onPreemptiveYield,
            bool timeSliceChecker, Action<TaskLogic> onTimeOut)
        {
            this.action = action;
            //we initialize what will thread do
            thread = new(() =>
            {
                try
                {
                    //actual function that the user will specify
                    action.Run(this);
                }
                catch (Exception e)
                {
                    //throw e;
                    Console.WriteLine(e.Message);
                    if (e.Message.Equals("Deadlock detected"))
                        throw e;
                }
                finally
                {

                    //what happens at the end of a task
                    Finish();
                }
            });
            this.ID = action.getID();
            this.canExecute = canExecute;
            this.priority = priority;
            this.Priority = priority;
            this.onTaskFinished = onTaskFinished;
            this.onTaskPaused = onTaskPaused;
            this.onTaskContinueRequested = onTaskContinueRequested;
            this.onEnableExecution = onEnableExecution;
            this.onPreemptiveYield = onPreemptiveYield;
            this.onElapsedTime = onElapsedTime;
            this.timeSliceChecker = timeSliceChecker;
            this.onTimeOut = onTimeOut;
            this.allowedExecutiontime = allowedExecutionTime;
            this.startTime = beginDate;
            this.endTime = endDate;
            this.SetTimer(allowedExecutionTime, beginDate, endDate);
        }

        //Scheduler will check if User allowed for this task to execute or not
        public bool CheckExecutionState()
        {
            if (canExecute)
                State = TaskStates.Waiting;
            else
                State = TaskStates.CanNotExecute;
            return canExecute;

        }
        //when Scheduler deques the Task he will call this function
        public void Start()
        {
            lock (this._lock)
            {
                switch (State)
                {

                    case TaskStates.Waiting: //if the task is in waiting state he will start it
                        State = TaskStates.Running; //we are putting the task in a running state
                        if (executionTimer != null)
                            executionTimer.Start(); //starting the executionTimer the moment we start the execution of the task
                        if (stopTimer != null)
                            stopTimer.Start();
                        if (timeSliceTimer != null)
                            timeSliceTimer.Start();
                        thread.Start(); //and we are starting the thread which represents our task
                        break;
                    case TaskStates.WaitingOnResource:
                        State = TaskStates.Running;
                        State = TaskStates.Running; //we are putting the task in a running state
                        if (executionTimer != null)
                            executionTimer.Start(); //starting the executionTimer the moment we start the execution of the task
                        if (stopTimer != null)
                            stopTimer.Start();
                        if (timeSliceTimer != null)
                            timeSliceTimer.Start();
                        lock (this.resourceLock)
                        {
                            Monitor.Pulse(resourceLock);
                        }
                        break;
                    case TaskStates.CanNotExecute: // this state should not even happen but we are defining it whatsoever
                        throw new InvalidOperationException("User did not allow for it to start");
                    case TaskStates.Running: //task is already running
                        throw new InvalidOperationException("Task is already running");
                    case TaskStates.Terminated: //task already terminated
                        throw new InvalidOperationException("Task already terminated");
                    case TaskStates.WaitingToResume: //if the task was paused, then unapaused, sent to queue, now scheduler dequed it again
                        State = TaskStates.Running; //we put the state in a running state
                        Monitor.Pulse(this._lock); //we pulse it so it can resume after being locked
                        if (executionTimer != null)
                            executionTimer.Start();
                        if (stopTimer != null)
                            stopTimer.Start();
                        if (timeSliceTimer != null)
                            timeSliceTimer.Start();
                        break;
                    case TaskStates.Yielded:
                        State = TaskStates.Running;
                        lock (_lock)
                        {
                            Monitor.Pulse(_lock);
                            if (executionTimer != null)
                                executionTimer.Start();
                            if (stopTimer != null)
                                stopTimer.Start();
                        }
                        break;
                    case TaskStates.TimedOut:
                        State = TaskStates.Running;

                        Monitor.Pulse(_lock);
                        if (executionTimer != null)
                            executionTimer.Start();
                        if (stopTimer != null)
                            stopTimer.Start();
                        if (timeSliceTimer != null)
                            timeSliceTimer.Start();

                        //Monitor.Pulse(_lock);
                        break;
                    default:
                        break;

                }
            }

        }
        //what happens when a task is finished
        private void Finish()
        {
            State = TaskLogic.TaskStates.Terminated;
            lock (this)
            {
                Monitor.PulseAll(this); //wake all the waiting threads
            }
            if (this.stopTimer != null)
                this.stopTimer.Stop();
            if (this.executionTimer != null)
                this.executionTimer.Stop();
            if (this.timeSliceTimer != null)
                this.timeSliceTimer.Stop();
            onTaskFinished(this); //what will hapen after the task ended
        }
        //cooperative function that will check if task timed out based on the time slice
        public void CheckForTimeOut()
        {
            bool shouldTimeOut = false;
            lock (_lock)
            {
                switch (State)
                {
                    case TaskStates.Running:
                        break;
                    case TaskStates.RunningWithTimeOutRequest:
                        onTimeOut(this);
                        if (State.Equals(TaskStates.RunningWithTimeOutRequest))
                        {
                            shouldTimeOut = true;
                            State = TaskStates.TimedOut;

                            if (executionTimer != null)
                                executionTimer.Stop();
                            if (stopTimer != null)
                                stopTimer.Stop();
                            if (timeSliceTimer != null)
                                timeSliceTimer.Stop();
                        }
                        break;
                    case TaskStates.RunningWithTerminationRequest:
                        break;
                    default: throw new InvalidOperationException("Invalid job state");
                }
            }
            if (shouldTimeOut)
            {
                lock (_lock)
                {
                    Monitor.Wait(_lock);
                }
            }
            else if (this.timeSliceChecker && State.Equals(TaskStates.Running))
                timeSliceTimer.Start();
        }
        //cooperative function that will check if user paused the execution
        public void CheckForPause()
        {
            bool shouldPause = false; //boolean so we can know if the task was paused
            lock (this._lock)
            {
                switch (State)
                {
                    case TaskStates.New: //if the task was already created and someone called this function
                        throw new InvalidOperationException("Task has not been started yet"); //we then throw an exception
                    case TaskStates.Running: //if it is running, he continues to run still
                        break;
                    case TaskStates.Terminated: //we cannot pause a task that already terminated
                        throw new InvalidOperationException("Task already terminated");
                    case TaskStates.RunningWithPauseRequest: //if it was requested to pause, it is now ready to pause
                        State = TaskStates.Paused; //put the state in a paused state
                        onTaskPaused(this); //activate the reaction function
                        shouldPause = true; //signal that it needs to be paused
                        if (executionTimer != null)
                            executionTimer.Stop();
                        if (stopTimer != null)
                            stopTimer.Stop();
                        if (timeSliceTimer != null)
                            timeSliceTimer.Stop();
                        break;
                    case TaskStates.RunningWithTerminationRequest:
                        break;
                    case TaskStates.RunningWithTimeOutRequest:
                        break;
                    default:
                        break;
                }
                if (shouldPause)
                    Monitor.Wait(this._lock); //Task - Thread is now Paused
            }
        }
        //cooperative function that will check if user paused the execution
        public void CheckForTermination()
        {
            lock (_lock)
            {
                switch (State)
                {
                    case TaskStates.Waiting:
                        break;
                    case TaskStates.Running:
                        break;
                    case TaskStates.Terminated:
                        throw new InvalidOperationException("Task already finished");
                    case TaskStates.RunningWithTerminationRequest:
                        State = TaskStates.Terminated;
                        //onTaskFinished(this);
                        throw new Exception("Task terminated");
                    default:
                        break;

                }
            }
        }
        //in case user decided that scheduling algorithm will be priority with preemption this is yet another cooperative function
        //that will enable the task with lower priority to give the cpu to task with higher priorty
        public void CheckForPreemptiveYield()
        {
            bool shouldYield = false;
            lock (_lock)
            {

                switch (State)
                {
                    //this would only make sense if the task is already running and it needs to let cpu for another task
                    //that is already waiting
                    case TaskStates.Running:
                        onPreemptiveYield(this);
                        if (State.Equals(TaskStates.Yielded))
                            shouldYield = true;
                        break;
                    default: break;
                }
                if (shouldYield)
                {
                    Monitor.Wait(_lock);
                }
            }
        }
        //thread that is calling this function will block and wait till this thread notifies that it can continue
        internal void Wait()
        {
            lock (this)
            {
                Monitor.Wait(this);
            }
        }
        //this function is the user side of pausing a task
        public void RequestPause()
        {
            lock (_lock)
            {
                switch (State)
                {


                    case TaskStates.New:
                        throw new InvalidOperationException("Invalid job state");
                    case TaskStates.Running:
                        State = TaskStates.RunningWithPauseRequest;
                        break;
                    case TaskStates.RunningWithPauseRequest:
                        break;
                    case TaskStates.Paused:
                        break;
                    case TaskStates.Terminated:
                        break;
                    default:
                        break;

                }
            }
        }
        //this function is the user side of terminating a task
        public void RequestTermination()
        {
            switch (State)
            {

                case TaskStates.Running:
                    State = TaskStates.RunningWithTerminationRequest;
                    break;
                default:
                    State = TaskStates.RunningWithTerminationRequest;
                    break;
            }
        }
        //this function is used to context switch between two tasks
        public void RequestContextSwitch()
        {
            switch (State)
            {
                case TaskStates.Running:
                    State = TaskStates.RunningWithTimeOutRequest;
                    break;
                default:
                    break;
            }
        }
        //function used to continue execution of a task
        public void RequestResume()
        {
            lock (_lock)
            {
                switch (State)
                {
                    case TaskStates.New:
                        break;
                    case TaskStates.Running:
                        break;
                    case TaskStates.RunningWithPauseRequest:
                        State = TaskStates.Running;
                        break;
                    case TaskStates.Paused:
                        State = TaskStates.WaitingToResume;
                        onTaskContinueRequested(this);
                        break;
                    case TaskStates.Terminated:
                        break;
                    default:
                        break;
                }
            }
        }
        //function to enable execution of a task that was previously marked as not able to execute
        public void EnableExecution()
        {
            lock (_lock)
            {
                switch (State)
                {
                    case TaskStates.CanNotExecute:
                        State = TaskStates.Waiting;
                        onEnableExecution(this);
                        break;
                    default:
                        throw new InvalidOperationException("Invalid job state");

                }
            }
        }
        internal void enableTask()
        {

            State = TaskStates.Waiting;
            canExecute = true;

        }
        internal void changeStateToYield()
        {
            lock (_lock)
            {
                State = TaskStates.Yielded;
            }
        }
        internal void changeStateToTimedOut()
        {
            lock (_lock)
            {
                State = TaskStates.RunningWithTimeOutRequest;
            }
        }
        internal void changeStateToRunning()
        {
            lock (_lock)
            {
                State = TaskStates.Running;
            }
        }
        internal int getPriority()
        {
            return this.Priority;
        }
        //this function sets the executionTimer that defines the deadline until the task must be finished
        //when executionTimer reaches the set interval he calls the already defined function RequestTermination
        //that will signal the task that it must be terminated
        private void SetTimer(int allowedTime, DateTime? beginDate, DateTime? endDate)
        {
            if (allowedTime != 0)
            {
                executionTimer = new System.Timers.Timer(allowedTime); //setting up allowed time
                executionTimer.AutoReset = false; //false because we do not want for it to repeat periodically
                //the reaction to the event from the executionTimer
                executionTimer.Elapsed += (Object source, ElapsedEventArgs e) => { this.RequestTermination(); Console.WriteLine("Event triggered at " + e.SignalTime); };
            }
            else
                //if user specified that allowed time is 0 meaning he does not want to specify the deadline for this task
                //meaning the executionTimer is set null
                executionTimer = null;
            if (beginDate.HasValue)
            {
                DateTime temp = beginDate.Value;
                TimeSpan ts = temp - DateTime.Now;
                this.startTimer = new System.Timers.Timer(ts.TotalMilliseconds);
                this.startTimer.AutoReset = false;
                this.startTimer.Elapsed += (Object source, ElapsedEventArgs e) =>
                {
                    onElapsedTime(this); Console.WriteLine("Task starting at " + e.SignalTime);

                };
                this.State = TaskStates.NotTimeToExecute;
            }
            else beginDate = null;
            if (endDate.HasValue)
            {
                DateTime temp = endDate.Value;
                TimeSpan ts = temp - DateTime.Now;
                this.stopTimer = new System.Timers.Timer(ts.TotalMilliseconds);
                this.stopTimer.AutoReset = false;
                this.stopTimer.Elapsed += (Object source, ElapsedEventArgs e) => { this.RequestTermination(); Console.WriteLine("Event reached deadline at " + e.SignalTime); };

            }
            else endDate = null;
            if (this.timeSliceChecker)
            {
                timeSliceTimer = new System.Timers.Timer(timeSlice / this.priority);
                timeSliceTimer.Elapsed += (Object source, ElapsedEventArgs e) => { this.RequestContextSwitch(); Console.WriteLine("Task timed out at " + e.SignalTime); ; };
                this.timeSliceTimer.AutoReset = true;
            }
            else
                timeSliceTimer = null;


        }
        public void startBeginDateTimer()
        {
            if (this.startTimer != null)
                this.startTimer.Start();
        }


        public void SetProgress(double progress)
        {
            Progress = progress;
        }

        public void LockResource(Resource resource)
        {
            resource.lockResource(this);
        }
        public void waitForResource()
        {
            lock (_lock)
            {
                State = TaskStates.WaitingOnResource;
            }
            onTaskPaused(this);
            lock (resourceLock)
            {
                Monitor.Wait(resourceLock);
            }
        }
        public void continueAfterResource()
        {
            onTaskContinueRequested(this);
        }

        public void UnlockResource(Resource resource)
        {
            resource.unlockResource();
        }
        //function to create a new timer that will be used for time slicing feature
        internal void createNewTimeSliceTimer()
        {
            if (this.timeSliceChecker)
            {
                timeSliceTimer = new System.Timers.Timer(timeSlice / this.priority);
                timeSliceTimer.Elapsed += (Object source, ElapsedEventArgs e) => { this.RequestContextSwitch(); Console.WriteLine("Task timed out at " + e.SignalTime); ; };
                this.timeSliceTimer.AutoReset = false;
            }
        }
    }
}
