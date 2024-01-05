using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace project_manage_system_backend.EAS
{
    public class TrelloEAS
    {
        private readonly HttpClient _httpClient;

        public TrelloEAS(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// post資料到遠端url上
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url">遠端網址</param>
        /// <param name="data">資料</param>
        /// <returns></returns>
        public async Task<bool> PostData<T>(string url, T data)
        {
            string content = JsonConvert.SerializeObject(data);
            var buffer = Encoding.UTF8.GetBytes(content);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(url, byteContent);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> PutData<T>(string url, T data)
        {
            string content = JsonConvert.SerializeObject(data);
            var buffer = Encoding.UTF8.GetBytes(content);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            HttpResponseMessage response = await _httpClient.PutAsync(url, byteContent);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// 使用Get取得回傳的Json Array
        /// </summary>
        /// <typeparam name="T">要接取的Json資料Dto</typeparam>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<List<T>> GetDataArray<T>(string url) where T : new()
        {
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException("Get request fail.");
            }
            string jsonInfo = await response.Content.ReadAsStringAsync();
            JArray json = JArray.Parse(jsonInfo);
            List<T> results = new List<T>();
            foreach (JObject list in json.OfType<JObject>())
            {
                T temp = new T();
                foreach (PropertyInfo element in typeof(T).GetProperties())
                {
                    element.SetValue(temp, (string)list.GetValue(element.Name));
                }
                results.Add(temp);
            }
            return results;
        }

        /// <summary>
        /// Get資料的status Code
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<bool> CheckStatusCode(string url)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// 使用Get取得回傳的Json
        /// </summary>
        /// <typeparam name="T">要接取的Json資料Dto</typeparam>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<T> GetDataSingle<T>(string url) where T : new()
        {
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException("Get request fail.");
            }
            string jsonInfo = await response.Content.ReadAsStringAsync();
            JObject json = JObject.Parse(jsonInfo);
            T temp = new T();
            foreach (PropertyInfo element in typeof(T).GetProperties())
            {
                element.SetValue(temp, (string)json.GetValue(element.Name));
            }
            return temp;
        }
    }
}
