using OPOS_Projekat.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OPOS_Projekat.AutoSave
{
    public class AutoSaveMechanism
    {
        public Scheduler schedulerToSave;
        public static string pathToSerialization;
        public AutoSaveMechanism()
        {

        }
        public void setScheduler(Scheduler sh)
        {
            this.schedulerToSave = sh;
        }
        public static void deleteSerializationFiles()
        {
            DirectoryInfo directory = new DirectoryInfo(pathToSerialization);
            foreach (FileInfo file in directory.GetFiles())
            {
                file.Delete();
            }
        }
        public Scheduler Load()
        {
            Scheduler sh;
            using (FileStream stream = new FileStream(pathToSerialization + "\\scheduler.bin", FileMode.Open))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                sh = (Scheduler)formatter.Deserialize(stream);
                Console.WriteLine("");
            }
            Scheduler final = new Scheduler(sh.maxTasks, sh.algorithm);
            foreach (string filePath in Directory.EnumerateFiles(pathToSerialization).ToList())
            {
                if (!filePath.Contains("scheduler.bin"))
                {
                    using (FileStream stream = new FileStream(filePath, FileMode.Open))
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        TaskLogic bar = (TaskLogic)formatter.Deserialize(stream);
                        final.Schedule(new TaskSpecification(bar.action, bar.Priority, bar.canExecute, bar.allowedExecutiontime, bar.startTime, bar.endTime));
                    }
                }
            }
            return final;

        }
        public void Save()
        {
            while (true)
            {
                int counter = 0;
                deleteSerializationFiles();
                using (FileStream stream = new FileStream(pathToSerialization + "\\scheduler.bin", FileMode.Create))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, schedulerToSave);
                }
                foreach (TaskLogic tl in schedulerToSave.observableTasks)
                {

                    string name = "task" + counter++;
                    using (FileStream stream = new FileStream(pathToSerialization + "\\" + name, FileMode.Create))
                    {
                        lock (tl._lock)
                        {
                            BinaryFormatter formatter = new BinaryFormatter();
                            formatter.Serialize(stream, tl);
                        }
                    }
                }
                Thread.Sleep(2000);
            }
        }
    }
}
