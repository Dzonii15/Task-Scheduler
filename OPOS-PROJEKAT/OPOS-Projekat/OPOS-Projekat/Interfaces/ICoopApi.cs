using OPOS_Projekat.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPOS_Projekat.Interfaces
{
    //provides needed functions for cooperative pausing, termination and so on
    //the class that implements this has to have acces to the possible states
    public interface ICoopApi
    {
        public void CheckForPause();
        public void CheckForTermination();
        public void CheckForPreemptiveYield();
        public void CheckForTimeOut();
        public void SetProgress(double progress);
        public void LockResource(Resource resource);
        public void UnlockResource(Resource resource);
    }
}
