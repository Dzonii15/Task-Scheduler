using OPOS_Projekat.Interfaces;
using OPOS_Projekat.TaskScheduler;

namespace SchedulerTest
{
    [TestClass]
    public class SchedulerTestClass
    {
        [TestMethod]
        public void waitingQueue()
        {
            Scheduler scheduler = new Scheduler(2,"FIFO");
            scheduler.Schedule(new TaskSpecification(new TaskTypeA(50), 1, true, 0, null, null));
            scheduler.Schedule(new TaskSpecification(new TaskTypeA(50), 1, true, 0, null, null));
            scheduler.Schedule(new TaskSpecification(new TaskTypeA(50), 1, true, 0, null, null));
            Assert.AreEqual(1,scheduler.getQueueSize());
        }
        [TestMethod]
        public void runningTasks()
        {
            Scheduler scheduler = new Scheduler(2, "FIFO");
            scheduler.Schedule(new TaskSpecification(new TaskTypeA(50), 1, true, 0, null, null));
            scheduler.Schedule(new TaskSpecification(new TaskTypeA(50), 1, true, 0, null, null));
            scheduler.Schedule(new TaskSpecification(new TaskTypeA(50), 1, true, 0, null, null));
            scheduler.Schedule(new TaskSpecification(new TaskTypeA(50), 1, true, 0, null, null));
            scheduler.Schedule(new TaskSpecification(new TaskTypeA(50), 1, true, 0, null, null));
            scheduler.Schedule(new TaskSpecification(new TaskTypeA(50), 1, true, 0, null, null));
            scheduler.Schedule(new TaskSpecification(new TaskTypeA(50), 1, true, 0, null, null));
            Assert.AreEqual(2, scheduler.getNumberOfRunningTasks());
        }
        [TestMethod]
        public void enabledExecution()
        {
            Scheduler scheduler = new Scheduler(2, "FIFO");
            var task1 = scheduler.Schedule(new TaskSpecification(new TaskTypeA(50), 1, false, 0, null, null));
            Assert.AreEqual(0, scheduler.getQueueSize());
            task1.EnableExecution();
            Thread.Sleep(1000);
            Assert.AreEqual(1, scheduler.getNumberOfRunningTasks());
        }
        [TestMethod]
        public void executionDuration()
        {
            Scheduler scheduler = new Scheduler(2, "FIFO");
            var task1 = scheduler.Schedule(new TaskSpecification(new TaskTypeA(50), 1, false, 5000, null, null));
            Thread.Sleep(6000);
            Assert.AreEqual(0, scheduler.getNumberOfRunningTasks());
        }
        [TestMethod]
        public void startExecutionTime()
        {
            Scheduler scheduler = new Scheduler(1, "FIFO");
            var dt = DateTime.Now;
            var dt2 = dt.AddMilliseconds(3000);
            scheduler.Schedule(new TaskSpecification(new TaskTypeA(10), 2, true, 0, dt2, null));
            Thread.Sleep(4000);
            Assert.AreEqual(1, scheduler.getNumberOfRunningTasks());
        }
        [TestMethod]
        public void stopExecutionTime()
        {
            Scheduler scheduler = new Scheduler(1, "FIFO");
            var dt = DateTime.Now;
            var dt2 = dt.AddMilliseconds(3000);
            scheduler.Schedule(new TaskSpecification(new TaskTypeA(10), 2, true, 0, null, dt2));
            Thread.Sleep(4000);
            Assert.AreEqual(0, scheduler.getNumberOfRunningTasks());
        }
    }

}