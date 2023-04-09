using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OPOS_Projekat
{
    public class CurrentTypes
    {
        public static Type[] getCurrentTypes()
        {
            return Assembly.GetExecutingAssembly().GetTypes();
        }
    }
}
