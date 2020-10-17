using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using ResourceMonitorApi.dao;
using ResourceMonitorAPI.dao;

namespace ResourceMonitorAPI.utils {
    public class API {
        private HttpListener listener;
        private Thread listenerThread;
        private bool isRunning;
        private Dictionary<string, Type> tables;

        public API() {
            try {
                this.listener = new HttpListener();
                this.listener.IgnoreWriteExceptions = true;
            }   
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                this.listener = null;
            }
        }

        public bool Start() {
            this.isRunning = true;

            if (PlatformNotSupported()) {
                return false;
            }

            try {
                if (this.listener.IsListening) {
                    return true;
                }

                string prefix = "http://+:9002/";
                this.listener.Prefixes.Clear();
                this.listener.Prefixes.Add(prefix);
                this.listener.Start();

                if (this.listenerThread == null) {
                    this.listenerThread = new Thread(handleRequests);
                    this.listenerThread.Start();
                }

                tables = new Dictionary<string, Type>();
                using (var context = new DatabaseContext()) {
                    var properties = context.GetType().GetProperties();
                    foreach(var property in properties) {
                        if (property.PropertyType.IsGenericType) {
                            if (typeof(DbSet<>).IsAssignableFrom(property.PropertyType.GetGenericTypeDefinition())) {
                                var entityTypes = property.PropertyType.GetGenericArguments();
                                tables.Add(entityTypes[0].Name.ToLower(), Type.GetType(entityTypes[0].Name));
                            }
                        }
                    }
                }
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                return false;
            }

            return true;
        }

        private bool Stop() {
            this.isRunning = false;

            if (PlatformNotSupported()) {
                return false;
            }

            try {
                this.listenerThread.Abort();
                this.listener.Stop();
                this.listenerThread = null;
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                return false;
            }

            return true;
        }

        private void handleRequests() {
            while (this.listener.IsListening) {
                IAsyncResult context;
                context  = this.listener.BeginGetContext(new AsyncCallback(APICallback), this.listener);
                context.AsyncWaitHandle.WaitOne();
            }
        }

        private void APICallback(IAsyncResult result) {
            HttpListener listener = (HttpListener)result.AsyncState;
            if (listener == null || !listener.IsListening) {
                return;
            }

            HttpListenerContext httpContext;
            try {
                httpContext = listener.EndGetContext(result);
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                return;
            }

            HttpListenerRequest httpRequest = httpContext.Request;
            string requestString = httpRequest.RawUrl.Substring(1);
            Console.WriteLine(requestString);

            string body = "";
            using (Stream bodyStream = httpRequest.InputStream) {
                using (StreamReader reader = new StreamReader(bodyStream, httpRequest.ContentEncoding)) {
                    body = reader.ReadToEnd();
                }
            }

            string[] keys;
            try {
                keys = requestString.Split('/');
                foreach (string key in keys) {
                    Console.WriteLine(key);
                }
            }
            catch (Exception ex) {
                keys = new string[1];
                keys[0] = requestString;
            }

            string json = null;
            try {
                if (requestString.StartsWith("armazenamento")) {
                    json = processRequest(new ArmazenamentoDAO(), keys, body, httpRequest.HttpMethod);
                }
                else if (requestString.StartsWith("computador")) {
                    json = processRequest(new ComputadorDAO(), keys, body, httpRequest.HttpMethod);
                }
                else if (requestString.StartsWith("cpu")) {
                    json = processRequest(new CPUDAO(), keys, body, httpRequest.HttpMethod);
                }
                else if (requestString.StartsWith("gpu")) {
                    json = processRequest(new GPUDAO(), keys, body, httpRequest.HttpMethod);
                }
                else if (requestString.StartsWith("ram")) {
                    json = processRequest(new RAMDAO(), keys, body, httpRequest.HttpMethod);
                }
                else {
                    json = "{}";
                }
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                json = "{\"status\":\"error\"}";
            }

            SendJson(httpContext.Response, json);

            return;
        }

        private string processRequest<T>(DAO<T> dao, string[] keys, string body, string method) where T : class {
            string json = null;
            bool success;
            
            switch (method) {
                case "GET":
                    if (keys.Length == 1) {
                        List<T> objectList = dao.get();
                        json = JsonConvert.SerializeObject(objectList);
                    }
                    break;
                case "POST":
                    break;
                default:
                    break;
            }

            return json;
        }

        private void SendJson(HttpListenerResponse response, string content) {
            byte[] contentBytes = Encoding.UTF8.GetBytes(content);

            response.AddHeader("Cache-Control", "no-cache");
            response.AddHeader("Access-Control-Allow-Origin", "*");
            response.ContentLength64 = contentBytes.Length;
            response.ContentType = "application/json";

            try {
                Stream outputStream = response.OutputStream;
                outputStream.Write(contentBytes, 0, contentBytes.Length);
                outputStream.Close();
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }

            response.Close();             
        }

        private bool PlatformNotSupported() {
            if (this.listener == null) {
                return true;
            }
            return false;
        }
    }
}