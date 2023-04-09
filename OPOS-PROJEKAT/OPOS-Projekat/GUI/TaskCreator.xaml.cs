using OPOS_Projekat;
using OPOS_Projekat.Interfaces;
using OPOS_Projekat.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GUI
{
    /// <summary>
    /// Interaction logic for TaskCreator.xaml
    /// </summary>
    public partial class TaskCreator : Window
    {
        Scheduler sh;
        public TaskCreator(Scheduler sh)
        {
            InitializeComponent();
            //Get all types in current assembly
            Type[] types = CurrentTypes.getCurrentTypes();
            List<Type> tips = new();

            foreach (Type type in types)
            {
                if (type.GetInterfaces().Contains(typeof(IAction)))
                {
                    tips.Add(type);
                    //derivedTypes.
                }
            }
            IEnumerable<Type> implementingTypes = tips;
            AvailableTask.ItemsSource = implementingTypes;
            this.sh = sh;
        }

        private void CONFIRMBUTTON_Click(object sender, RoutedEventArgs e)
        {
            //Parsing the priority of the task created
            int priority = 0;
            try
            {
                priority = Int32.Parse(PriorityTextBlock.Text);
            }
            catch
            {
                //If user did not specify the priority
                Environment.Exit(-1);
            }
            //if the task will start executing immediatly or not
            bool canExecute;
            if (trueRadioButton.IsChecked == true)
                canExecute = true;
            else canExecute = false;

            int executionTime = 0;
            if (AllowedTimeCheckBox.IsChecked == true)
                executionTime = Int32.Parse(ExecutionTimeTextBox.Text);
            DateTime startDate = new();
            bool checker = false;
            if (StartDateCB.IsChecked == true)
            {
                checker = true;
                if (StartDatePicker.SelectedDate != null)
                {

                    startDate = (DateTime)StartDatePicker.SelectedDate;
                    if (startDate.Year == 2001 && startDate.Month == 12 && startDate.Day == 19)
                        checker = false;
                }
                else Environment.Exit(-1);
                int hh;
                int mm;
                int ss;
                var parsed = StartTime.Text.Split("-");
                hh = Int32.Parse(parsed[0]);
                mm = Int32.Parse(parsed[1]);
                ss = Int32.Parse(parsed[2]);
                startDate = startDate.AddSeconds(ss);
                startDate = startDate.AddMinutes(mm);
                startDate = startDate.AddHours(hh);

            }


            DateTime endDate = new();
            bool checker2 = false;
            if (EndDateCB.IsChecked == true)
            {
                checker2 = true;
                if (EndDatePicker.SelectedDate != null)
                {
                    endDate = (DateTime)EndDatePicker.SelectedDate;
                    startDate = (DateTime)StartDatePicker.SelectedDate;
                    if (startDate.Year == 2001 && startDate.Month == 12 && startDate.Day == 19)
                        checker2 = false;
                }
                else Environment.Exit(-1);
                var parsed2 = EndTime.Text.Split("-");
                var hh2 = Int32.Parse(parsed2[0]);
                var mm2 = Int32.Parse(parsed2[1]);
                var ss2 = Int32.Parse(parsed2[2]);
                endDate = endDate.AddSeconds(ss2);
                endDate = endDate.AddMinutes(mm2);
                endDate = endDate.AddHours(hh2);
            }
            Type taskType = (Type)AvailableTask.SelectedItem;
            IAction task = (IAction)Activator.CreateInstance(taskType);

            DateTime? trueBeginDate;
            if (checker)
                trueBeginDate = startDate;
            else
                trueBeginDate = null;
            DateTime? trueEndDate;
            if (checker2)
                trueEndDate = endDate;
            else
                trueEndDate = null;


            TaskSpecification ts = new(task, priority, canExecute, executionTime, trueBeginDate, trueEndDate);
            ts.action = (IAction)JsonSerializer.Deserialize(JsonSer.Text,task.GetType(),jsonSerializerOptions);
            //Dispatcher.BeginInvoke(async () =>
            // {
            //   sh.Schedule(ts);


            // });
            sh.Schedule(ts);
            this.Close();
        }

        private void AvailableTask_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Type TaskType = (Type)AvailableTask.SelectedItem;
            object userTask = Activator.CreateInstance(TaskType);
            JsonSer.Text = JsonSerializer.Serialize(userTask, userTask.GetType(), jsonSerializerOptions);
        }
        private static readonly JsonSerializerOptions jsonSerializerOptions = new() { WriteIndented = true, IncludeFields = true };
    }
}
