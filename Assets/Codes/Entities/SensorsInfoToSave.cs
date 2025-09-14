using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Codes.Entities
{
    [Serializable]
    public class SensorsInfoToSave
    {
        public int sensorId;
        public double initialBaselineX;
        public double initialBaselineY;
        public double initialBaselineZ;
        public double unityBaselineX;
        public double unityBaselineY;
        public double unityBaselineZ;
    }
}
