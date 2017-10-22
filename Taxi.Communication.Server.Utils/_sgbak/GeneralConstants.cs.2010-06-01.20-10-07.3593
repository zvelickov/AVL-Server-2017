namespace Taxi.Communication.Server.Utils
{
    public static class GeneralConstants
    {
        private static string m_ShiftInOutSensor = "";
        public static string ShiftInOutSensor
        {
            get { return m_ShiftInOutSensor; }
            set
            {
                if (m_ShiftInOutSensor == "")
                    m_ShiftInOutSensor = value;
            }
        }

        private static int m_ShiftInOutSensorStateIn;
        private static bool m_ShiftInOutSensorStateInSeted = false;
        public static int ShiftInOutSensorStateIn
        {
            get { return m_ShiftInOutSensorStateIn; }
            set
            {
                if (!m_ShiftInOutSensorStateInSeted)
                {
                    m_ShiftInOutSensorStateIn = value;
                    m_ShiftInOutSensorStateInSeted = true;
                }
            }
        }

    }
}