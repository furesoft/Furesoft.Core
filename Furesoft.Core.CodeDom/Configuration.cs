// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Configuration;
using System.Reflection;

using Nova.CodeDOM;

namespace Nova
{
    /// <summary>
    /// Configuration class for Nova classes.
    /// </summary>
    public class Configuration : ConfigurationSection
    {
        /// <summary>
        /// Determines if non-default configuration settings should be logged.
        /// </summary>
        public static bool LogSettings = true;

        /// <summary>
        /// A key-value collection for all Nova settings (works like 'appSettings', but in a separate configSection).
        /// </summary>
        [ConfigurationProperty("", IsRequired = true, IsDefaultCollection = true)]
        public KeyValueConfigurationCollection Settings
        {
            get { return (KeyValueConfigurationCollection)this[""]; }
            set { this[""] = value; }
        }

        /// <summary>
        /// Load the settings from the config file.
        /// </summary>
        public static void LoadSettings()
        {
            string namespaceName = typeof(Configuration).Namespace;
            try
            {
                // Use reflection to set any static fields on any types.  Use the calling assembly so that this method
                // can be called for different assemblies (such as Nova.CodeDOM.dll and Nova.UI.dll).
                Assembly assembly = Assembly.GetCallingAssembly();
                Configuration config = (Configuration)ConfigurationManager.GetSection(namespaceName);
                if (config == null)
                    Log.DetailWriteLine("Nova.Configuration: WARNING: No '" + namespaceName + "' section found in .config file.");
                else
                {
                    foreach (string key in config.Settings.AllKeys)
                    {
                        // Split the key into a type name (with optional namespace prefix) and field name
                        int typeNameIndex = key.LastIndexOf('.');
                        if (typeNameIndex < 0)
                            LogMessage("Invalid key format '" + key + "' - must be 'typename.fieldname' or 'namespace.typename.fieldname'!", MessageSeverity.Error);
                        else
                        {
                            string typeName = key.Substring(0, typeNameIndex);
                            string fieldName = key.Substring(typeNameIndex + 1);

                            // Get the type using reflection - just ignore any types that aren't found, so that
                            // settings can exist in the file for different assemblies.  Also, we'll allow types
                            // under the Mono namespace through without prefixing the Nova namespace onto them.
                            string fullTypeName = (typeName.StartsWith("Mono.") ? typeName : namespaceName + "." + typeName);
                            Type type = assembly.GetType(fullTypeName);
                            if (type != null)
                            {
                                // Get the field from the type (assume it's public and static)
                                FieldInfo fieldInfo = type.GetField(fieldName, BindingFlags.Public | BindingFlags.Static);
                                if (fieldInfo == null)
                                    LogMessage("Type '" + typeName + "' doesn't have a public static field '" + fieldName + "'!", MessageSeverity.Error);
                                else
                                {
                                    // Set the static field to the value from the config file
                                    Type fieldType = fieldInfo.FieldType;
                                    object value = config.Settings[key].Value;
                                    if (fieldType.IsEnum && value is string)
                                    {
                                        if (fieldType == typeof(Log.Level))
                                        {
                                            Log.Level enumValue;
                                            if (Enum.TryParse((string)value, true, out enumValue))
                                                value = enumValue;
                                        }
                                    }
                                    fieldInfo.SetValue(null, Convert.ChangeType(value, fieldType));
                                    if (LogSettings)
                                        LogMessage(typeName + "." + fieldName + " = " + value, MessageSeverity.Information);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex, "loading");
            }
        }

        /// <summary>
        /// Log the specified text message with the specified severity level.
        /// </summary>
        public static void LogMessage(string message, MessageSeverity severity, string toolTip)
        {
            string prefix = (severity == MessageSeverity.Error ? "ERROR: " : (severity == MessageSeverity.Warning ? "Warning: " : ""));
            if (severity == MessageSeverity.Error || severity == MessageSeverity.Warning || Log.LogLevel >= Log.Level.Detailed)
                Log.WriteLine(prefix + "Configuration file: " + message, toolTip != null ? toolTip.TrimEnd() : null);
        }

        /// <summary>
        /// Log the specified text message with the specified severity level.
        /// </summary>
        public static void LogMessage(string message, MessageSeverity severity)
        {
            LogMessage(message, severity, null);
        }

        /// <summary>
        /// Log the specified exception and message.
        /// </summary>
        public static string LogException(Exception ex, string message)
        {
            return Log.Exception(ex, message + " configuration file");
        }
    }
}
