using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPOS_Projekat.Interfaces
{
    
     public interface IAction
    {

         public void Run(ICoopApi coopApi);
        public int getID();
        
    }
}
