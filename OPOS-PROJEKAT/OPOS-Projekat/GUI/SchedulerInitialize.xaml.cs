using OPOS_Projekat.AutoSave;
using OPOS_Projekat.TaskScheduler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
    /// Interaction logic for SchedulerInitialize.xaml
    /// </summary>
    public partial class SchedulerInitialize : Window
    {
        public SchedulerInitialize()
        {
           
            AutoSaveMechanism autoSave = new AutoSaveMechanism();
            if (Directory.GetFiles(AutoSaveMechanism.pathToSerialization).Length != 0)
            {
                Scheduler sh = autoSave.Load();
                autoSave.setScheduler(sh);
                new MainWindow(sh, autoSave).Show();
                this.Close();
            }
            else
            {
                InitializeComponent();
                this.Show();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string NumberOf = (string)TB.Text;
            int number = Int32.Parse(NumberOf);
            string option;
            if (First.IsChecked == true)
                option = "FIFO";
            else if (Second.IsChecked == true)
                option = "PRIORITY";
            else if (Third.IsChecked == true)
                option = "PRIORITYPREEMPTIVE";
            else if (Fourth.IsChecked == true)
                option = "ROUNDROBIN";
            else
                throw new Exception("DID NOT CHOSE OPTION");
            new MainWindow(number, option).Show();
            this.Close();
        }
    }
}
