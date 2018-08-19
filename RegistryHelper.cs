using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public static class RegistryHelper
    {
        /// <summary>
        /// Get data from registry
        /// </summary>
        /// <param name="type">the resource. 0: LocalMachine. 1: CurrentUser</param>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <returns>the data of registry. null means there is no data.</returns>
        public static string GetRegistData(int type, string key, string name)
        {
            string strData = string.Empty;
            RegistryKey hkml = null;

            if (type == 0)
                hkml = Registry.LocalMachine;
            else if (type == 1)
                hkml = Registry.CurrentUser;
            else
                return strData;

            strData = hkml.OpenSubKey(key, false).GetValue(name).ToString();
            hkml.Close();

            return strData;
        }

        /// <summary>
        /// Set data to registry
        /// </summary>
        /// <param name="type">the resource. 0: LocalMachine. 1: CurrentUser</param>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void SetRegistData(int type, string key, string name, string value)
        {
            RegistryKey hklm = null;
            if (type == 0)
                hklm = Registry.LocalMachine;
            else if (type == 1)
                hklm = Registry.CurrentUser;
            else
                return;

            hklm.CreateSubKey(key).SetValue(name, value);
            hklm.Close();
        }

        /// <summary>
        /// Delete registry
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <param name="name"></param>
        public static void DeleteRegist(int type, string key, string name)
        {
            string[] aimnames = null;
            RegistryKey hkml = null;

            if (type == 0)
                hkml = Registry.LocalMachine;
            else if (type == 1)
                hkml = Registry.CurrentUser;
            else
                return;

            aimnames = hkml.OpenSubKey(key, true).GetSubKeyNames();
            foreach (string subKeyName in aimnames)
            {
                if (subKeyName == name)
                    hkml.OpenSubKey(key, true).DeleteSubKeyTree(name);
            }
            hkml.Close();
        }

        /// <summary>
        /// Check whether registry key is existed.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool IsRegKeyExist(int type, string key)
        {
            try
            {
                bool isExist = false;
                RegistryKey hkml = null;

                if (type == 0)
                    hkml = Registry.LocalMachine;
                else if (type == 1)
                    hkml = Registry.CurrentUser;
                else
                    return false;

                RegistryKey key_node = hkml.OpenSubKey(key, false);
                if (key_node != null)
                    isExist = true;
                hkml.Close();

                return isExist;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check whether registry name is existed.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool IsRegeditExist(int type, string key, string name)
        {
            try
            {
                bool isExist = false;
                string[] subkeyNames = null;
                RegistryKey hkml = null;

                if (type == 0)
                    hkml = Registry.LocalMachine;
                else if (type == 1)
                    hkml = Registry.CurrentUser;
                else
                    return false;

                subkeyNames = hkml.OpenSubKey(key, false).GetValueNames();
                foreach (string keyName in subkeyNames)
                {
                    if (keyName == name)
                    {
                        isExist = true;
                        break;
                    }
                }

                hkml.Close();
                return isExist;
            }
            catch
            {
                return false;
            }
        }
    }
}
