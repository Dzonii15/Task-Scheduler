using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OPOS_Projekat.TaskScheduler;
using OPOS_Projekat.Queue;
using System.Windows.Media.Animation;
using OPOS_Projekat.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using OPOS_Projekat.AutoSave;
using System.Threading;

namespace GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int numberOfTasks;
        string decision;
        public Scheduler scheduler = new(1, "FIFO");
        //HashSet<STask> currentTasks;
        public MainWindow(Scheduler sh, AutoSaveMechanism autoSave)
        {
            InitializeComponent();
            scheduler = sh;
            Tasks.ItemsSource = scheduler.observableTasks;
            new Thread(autoSave.Save).Start();

        }
        public MainWindow(int numberOfTasks, string decision)
        {
            AutoSaveMechanism autoSave = new();
            this.numberOfTasks = numberOfTasks;
            this.decision = decision;
            InitializeComponent();
            ObservableCollection<STask> test = new();
            scheduler = new Scheduler(numberOfTasks, decision);
            Tasks.ItemsSource = scheduler.observableTasks;
            autoSave.setScheduler(scheduler);
            new Thread(autoSave.Save).Start();

            //ObservableQueue<STask> kju = new();
            //ObservablePriorityQueue<STask> kjuNesto = new((STask a,STask b) => { return a.ID - b.ID; });
            // MyPriorityQueue taskLogics = new((TaskLogic a, TaskLogic b) => { return a.Priority - b.Priority; });
            /* TaskLogic taskLogic = new(action: new TaskTypeA(), priority: 4,
                 onTaskContinueRequested: null, onTaskFinished: null,
                 onTaskPaused: null, onEnableExecution: null, canExecute: true,
                 allowedExecutionTime: 0, onElapsedTime: null,
                 beginDate: null, endDate: null, onPreemptiveYield: null,
                 onTimeOut: null, timeSliceChecker: false) ;*/
            //WaitingTasks.ItemsSource = test2;
            //STask task = new STask(null);
            //STask task2 = new STask(null);
            //taskLogics.enqueue(taskLogic);
            /* Dispatcher.BeginInvoke(async () =>
             {
                 scheduler.Schedule(new TaskSpecification(new TaskTypeA(), 2, true, 0, null, null));
                 await Task.Delay(1000);

             });*/



            //kjuNesto.Enqueue(task);
            // RunningTasks.ItemsSource = taskLogics;
            //kjuNesto.Enqueue(task2);
            //test.Add(task);
            //test2.Add(task);
            //NotStartedTasks.ItemsSource= test3;
            //test3.Add(task2);


            //this.Show();



        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            TaskCreator ts = new TaskCreator(this.scheduler);
            ts.Show();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {

                TaskLogic task = (TaskLogic)button.DataContext;
                if (task.State == TaskLogic.TaskStates.CanNotExecute)
                {
                    task.EnableExecution();
                    // button.IsEnabled = false;
                }
                else if (task.State == TaskLogic.TaskStates.Paused)
                {
                    task.RequestResume();
                    // button.IsEnabled = false;
                }

            }
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {

                TaskLogic task = (TaskLogic)button.DataContext;
                task.RequestPause();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {

            if (sender is Button button)
            {

                TaskLogic task = (TaskLogic)button.DataContext;
                scheduler.observableTasks.Remove(task);
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {

            if (sender is Button button)
            {

                TaskLogic task = (TaskLogic)button.DataContext;
                task.RequestTermination();
            }
        }
    }
}
