using System;
using System.IO;

namespace GestureWheel.Supports
{
    internal static class EnvironmentSupport
    {
        #region Fields
        private static readonly string _storage = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\GestureWheel";
        #endregion

        #region Constants
        private const string settings = "Settings.json";
        #endregion

        #region Properties
        public static string Storage
        {
            get
            {
                if (!Directory.Exists(_storage))
                    Directory.CreateDirectory(_storage);

                return _storage;
            }
        }

        public static string Settings => Path.Combine(Storage, settings);
        #endregion
    }
}
