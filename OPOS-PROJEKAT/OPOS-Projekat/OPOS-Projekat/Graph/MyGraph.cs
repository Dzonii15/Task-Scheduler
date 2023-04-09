using OPOS_Projekat.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPOS_Projekat.Graph
{
    public class MyGraph
    {
        private readonly Dictionary<TaskLogic, TaskLogic> transitions = new();
        private readonly HashSet<TaskLogic> proceses = new HashSet<TaskLogic>();
        public bool addTransition(TaskLogic process,TaskLogic process2)
        {
            if(!transitions.ContainsKey(process))
                transitions.Add(process, process2);
            bool hasCycle = HasCycle(process);
            return hasCycle;
            //return true;
        }
        public void removeTransition(TaskLogic process)
        {
            transitions.Remove(process);
        }
        public bool HasCycle(TaskLogic process)
        {
            Stack<TaskLogic> recursionStack = new();
            HashSet<TaskLogic> belongsToCycle = new();
            HashSet<TaskLogic> visited = new();
            this.DFS(recursionStack,belongsToCycle,visited,process);
            return belongsToCycle.Count> 0;


        }
        private void DFS(Stack<TaskLogic> recursionStack, HashSet<TaskLogic> belongsToCycle,
            HashSet<TaskLogic> visited, TaskLogic start)
        {
            visited.Add(start);
            recursionStack.Push(start);
            TaskLogic transition;
            bool checker = transitions.TryGetValue(start,out transition);
            if (checker)
            {
                if (recursionStack.Contains(transition))
                    belongsToCycle.Add(transition);
                if (!visited.Contains(transition))
                    DFS(recursionStack, belongsToCycle, visited, transition);
            }

            recursionStack.Pop();
            return;
        }
    }
}
