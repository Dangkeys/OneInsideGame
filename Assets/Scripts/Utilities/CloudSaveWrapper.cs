using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using UnityEngine;

namespace OneInside.Utils.CloudSave
{
    public static class CloudSaveWrapper
    {
        /// <summary>
        /// Saves a single data item to cloud storage
        /// </summary>
        /// <typeparam name="T">Type of data to save</typeparam>
        /// <param name="key">Unique identifier for the data</param>
        /// <param name="value">Data to save</param>
        /// <returns>Write lock for the saved data</returns>
        public static async Task<string> SaveData<T>(string key, T value)
        {
            try
            {
                var data = new Dictionary<string, object> { { key, value } };
                var result = await CloudSaveService.Instance.Data.Player.SaveAsync(data);
                return result[key];
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving data with key {key}: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Saves a single data item to cloud storage with write lock validation
        /// </summary>
        /// <typeparam name="T">Type of data to save</typeparam>
        /// <param name="key">Unique identifier for the data</param>
        /// <param name="value">Data to save</param>
        /// <param name="writeLock">Write lock for concurrency control</param>
        /// <returns>New write lock for the saved data</returns>
        public static async Task<string> SaveDataWithLock<T>(string key, T value, string writeLock)
        {
            try
            {
                var data = new Dictionary<string, SaveItem>
                {
                    { key, new SaveItem(value, writeLock) }
                };
                var result = await CloudSaveService.Instance.Data.Player.SaveAsync(data);
                return result[key];
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving data with key {key} and write lock: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Loads a single data item from cloud storage
        /// </summary>
        /// <typeparam name="T">Type of data to load</typeparam>
        /// <param name="key">Unique identifier for the data</param>
        /// <returns>Loaded data or default value if not found</returns>
        public static async Task<T> LoadData<T>(string key)
        {
            try
            {
                var results = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { key });
                if (results.TryGetValue(key, out var item))
                {
                    return item.Value.GetAs<T>();
                }
                return default;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading data with key {key}: {e.Message}");
                return default;
            }
        }

        /// <summary>
        /// Loads all data from cloud storage
        /// </summary>
        /// <returns>Dictionary containing all saved data</returns>
        public static async Task<Dictionary<string, Item>> LoadAllData()
        {
            try
            {
                return await CloudSaveService.Instance.Data.Player.LoadAllAsync();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading all data: {e.Message}");
                return new Dictionary<string, Item>();
            }
        }

        /// <summary>
        /// Deletes a single data item from cloud storage
        /// </summary>
        /// <param name="key">Unique identifier for the data to delete</param>
        /// <param name="writeLock">Optional write lock for concurrency control</param>
        public static async Task DeleteData(string key, string writeLock = null)
        {
            try
            {
                if (string.IsNullOrEmpty(writeLock))
                {
                    await CloudSaveService.Instance.Data.Player.DeleteAsync(key);
                }
                else
                {
                    var deleteOptions = new Unity.Services.CloudSave.Models.Data.Player.DeleteOptions { WriteLock = writeLock };
                    await CloudSaveService.Instance.Data.Player.DeleteAsync(key, deleteOptions);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error deleting data with key {key}: {e.Message}");
            }
        }

        /// <summary>
        /// Loads custom data from cloud storage
        /// </summary>
        /// <typeparam name="T">Type of data to load</typeparam>
        /// <param name="customId">Identifier for the custom data</param>
        /// <param name="key">Unique identifier for the data</param>
        /// <returns>Loaded data or default value if not found</returns>
        public static async Task<T> LoadCustomData<T>(string customId, string key)
        {
            try
            {
                var results = await CloudSaveService.Instance.Data.Custom.LoadAsync(customId, new HashSet<string> { key });
                if (results.TryGetValue(key, out var item))
                {
                    return item.Value.GetAs<T>();
                }
                return default;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading custom data with key {key} and customId {customId}: {e.Message}");
                return default;
            }
        }
    }
} 