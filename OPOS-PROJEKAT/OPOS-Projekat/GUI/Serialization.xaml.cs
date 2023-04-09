using OPOS_Projekat.AutoSave;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for Serialization.xaml
    /// </summary>
    public partial class Serialization : Window
    {
        public Serialization()
        {
            AutoSaveMechanism.pathToSerialization = "path";
            InitializeComponent();
        }

        private void YESBUTTON_Click(object sender, RoutedEventArgs e)
        {
            new SchedulerInitialize();
            this.Close();
            
        }

        private void NOBUTTON_Click(object sender, RoutedEventArgs e)
        {
            AutoSaveMechanism.deleteSerializationFiles();
            new SchedulerInitialize();
            this.Close();

        }
    }
}
