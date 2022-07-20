using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Enrollment
{
    public abstract class Model
    {
        public Model()
        {
            // Code ...
        }

        /*
         * Override: get the data of the model
         */
        public abstract Model[] getData();

        /*
         * Override get the filename of data to be saved
         */
        public abstract string getFileName();

        /*
         * Get class name
         */
        public abstract string getClassName();
    }
}
