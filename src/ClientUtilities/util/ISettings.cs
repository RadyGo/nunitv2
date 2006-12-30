using System;

namespace NUnit.Util
{
	/// <summary>
	/// The ISettings interface is used to access all user
	/// settings and options.
	/// </summary>
	public interface ISettings
	{
		/// <summary>
		/// Load a setting from the storage.
		/// </summary>
		/// <param name="settingName">Name of the setting to load</param>
		/// <returns>Value of the setting or null</returns>
		object GetSetting( string settingName );

		/// <summary>
		/// Load a setting from the storage or return a default value
		/// </summary>
		/// <param name="settingName">Name of the setting to load</param>
		/// <param name="settingName">Value to return if the setting is missing</param>
		/// <returns>Value of the setting or the default value</returns>
		object GetSetting( string settingName, object defaultValue );

		/// <summary>
		/// Load an integer setting from the storage or return a default value
		/// </summary>
		/// <param name="settingName">Name of the setting to load</param>
		/// <param name="defaultValue">Value to return if the setting is missing</param>
		/// <returns>Value of the setting or the default value</returns>
		int GetSetting( string settingName, int defaultValue );

		/// <summary>
		/// Load a boolean setting or return a default value
		/// </summary>
		/// <param name="settingName">Name of setting to load</param>
		/// <param name="defaultValue">Value to return if the setting is missing</param>
		/// <returns>Value of the setting or the default value</returns>
		bool GetSetting( string settingName, bool defaultValue );

		/// <summary>
		/// Load a string setting from the storage or return a default value
		/// </summary>
		/// <param name="settingName">Name of the setting to load</param>
		/// <param name="defaultValue">Value to return if the setting is missing</param>
		/// <returns>Value of the setting or the default value</returns>
		string GetSetting( string settingName, string defaultValue );

		/// <summary>
		/// Load an enum setting from the storage or return a default value
		/// </summary>
		/// <param name="settingName">Name of the setting to load</param>
		/// <param name="defaultValue">Value to return if the setting is missing</param>
		/// <returns>Value of the setting or the default value</returns>
		System.Enum GetSetting( string settingName, System.Enum defaultValue );

		/// <summary>
		/// Remove a setting from the storage
		/// </summary>
		/// <param name="settingName">Name of the setting to remove</param>
		void RemoveSetting( string settingName );

		/// <summary>
		/// Save a setting in the storage
		/// </summary>
		/// <param name="settingName">Name of the setting to save</param>
		/// <param name="settingValue">Value to be saved</param>
		void SaveSetting( string settingName, object settingValue );
	}
}