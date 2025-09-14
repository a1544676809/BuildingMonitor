using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Assets.Codes.Entities
{
    [Serializable]
    public class SensorsInfo
    {
        public int sensorId;
        public string sensorName;
        public string manufacturer;
        public string modelName;
        public string serialNumber;
        public DateTime? installationDate;
        public DateTime? calibrationDate;
        public DateTime? lastMaintenanceDate;
        public int associatedBuildingId;
        public double initialBaselineX;
        public double initialBaselineY;
        public double initialBaselineZ;
        public double unity_baseline_X;
        public double unity_baseline_Y;
        public double unity_baseline_Z;
        public string sensorType;
        public int status;
        public string mac;//设备硬件地址
    }

    public enum SensorStatus
    {
        Running = 1, // 运行中
        Offline = 2, // 离线
        Abnormal = 3, // 异常
        Maintenance = 4, // 维护中
        LowBattery = 5, // 低电量
        Discarded = 6, // 已报废
        Pending = 7 // 待处理
    }

    public abstract class  SensorData
    {
        public int id;//数据ID
        public int sensorId;//传感器ID
        public DateTimeOffset timestamp;//数据时间戳,带时区信息
        public int status;//数据状态
    }

    [Serializable]
    public class StressSensorData : SensorData
    {
        public double rawReading;
        public double strainValue;
        public double stressValue;
        public double temperatureCelsius;
    }

    [Serializable]
    public class  DisplacementSensorData : SensorData
    {
        public double currentX;//传感器当前X坐标
        public double currentY;//传感器当前Y坐标
        public double currentZ;//传感器当前Z坐标
        public double settlement;//沉降量
        public double horizontalDisplacementX;//X方向水平位移量
        public double horizontalDisplacementY;//Y方向水平位移量
        public double totalHorizontalDisplacement;//总水平位移量
        public double tiltValue;//倾斜值
        public double tiltDirection;//倾斜方向
        public double deflectionValue;//挠度值
        public double totalDisplacement;//总位移量
    }
}
