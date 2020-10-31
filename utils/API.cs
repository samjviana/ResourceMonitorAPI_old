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
using ResourceMonitorAPI.models;

namespace ResourceMonitorAPI.utils {
    public class API {
        private HttpListener listener;
        private Thread listenerThread;
        private bool isRunning;
        private Dictionary<string, Object> readings;

        public API() {
            try {
                this.listener = new HttpListener();
                this.listener.IgnoreWriteExceptions = true;
                readings = new Dictionary<string, Object>();
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
            //Console.WriteLine(requestString);

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

            string json = "";
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
                else if (requestString.StartsWith("leitura")) {
                    json = processRequest(keys, body, httpRequest.HttpMethod);
                }
                else if (requestString.StartsWith("versao")) {
                    json = processRequest();
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
                    else {
                        if (dao is ComputadorDAO) {
                            string nome = keys[1];
                            ComputadorDAO computadorDao = new ComputadorDAO();
                            Computador computador = computadorDao.getByNome(nome);
                            json = JsonConvert.SerializeObject(computador);
                        }
                    }
                    break;
                case "POST":
                    dynamic data = (T)JsonConvert.DeserializeObject<T>(body);
                    if (dao is ComputadorDAO) {
                        string nome = data.name;
                        ComputadorDAO computadorDao = new ComputadorDAO();
                        Computador computador = computadorDao.getByNome(nome);
                        if (computador != null) {
                            computador.cpus = data.cpus;
                            computador.gpus = data.gpus;
                            computador.storages = data.storages;
                            computador.ram = data.ram;
                            success = dao.update((dynamic)computador);
                        }
                        else {
                            success = dao.add(data);
                        }
                    }
                    json = "{\"status\":\"success\"}";
                    break;
                default:
                    break;
            }

            return json;
        }

        private string processRequest(string[] keys, string body, string method) {
            string json = null;
            ComputadorDAO dao = new ComputadorDAO();

            switch (method) {
                case "GET":
                    dynamic readingsObj = JsonConvert.DeserializeObject(readings[keys[1]].ToString());
                    switch (keys[2]) {
                        case "cpu":
                            int cpuId = Int32.Parse(keys[3]);
                            dynamic cpuObj = readingsObj.CPU[cpuId].Sensors;
                            json = JsonConvert.SerializeObject(new {
                                load = cpuObj.Load["CPU Total"].Value,
                                temperature = cpuObj.Temperature.Average,
                                clock = cpuObj.Clock.Average,
                                power = cpuObj.Power["CPU Package"].Value,
                            });
                            break;
                        case "gpu":
                            int gpuId = Int32.Parse(keys[3]);
                            dynamic gpuObj = readingsObj.GpuNvidia[gpuId].Sensors;
                            json = JsonConvert.SerializeObject(new {
                                load = gpuObj.Load.GPUCore.Value,
                                memoryload = gpuObj.Load.GPUMemory.Value,
                                temperature = gpuObj.Temperature.GPUCore.Value,
                                coreclock = gpuObj.Clock.GPUCore.Value,
                                memoryclock = gpuObj.Clock.GPUMemory.Value,
                            });
                            break;
                        case "ram":
                            dynamic ramObj = readingsObj.RAM[0].Sensors;
                            json = JsonConvert.SerializeObject(new {
                                load = ramObj.Load.Memory.Value,
                                used = ramObj.Data["Used Memory"].Value,
                                free = ramObj.Data["Available Memory"].Value,
                            });
                            break;
                        case "hdd":
                            List<dynamic> hddObj = JsonConvert.DeserializeObject<List<dynamic>>(readingsObj.HDD.ToString());
                            List<dynamic> storages = new List<dynamic>();
                            for (int i = 0; i < hddObj.Count; i++) {
                                storages.Add(new {
                                    load = hddObj[i].Sensors.Load["Used Space"].Value,
                                    read = hddObj[i].Sensors.Data["Host Reads"] == null ? -1 : hddObj[i].Sensors.Data["Host Reads"].Value,
                                    write = hddObj[i].Sensors.Data["Host Writes"] == null ? -1 : hddObj[i].Sensors.Data["Host Writes"].Value,
                                });
                            }
                            json = JsonConvert.SerializeObject(storages);
                            break;
                    }
                    break;
                case "POST":
                    dynamic data = JsonConvert.DeserializeObject(body);
                    Computador computador = dao.getByNome(String.Format("{0}", data.Name));
                    if (computador == null) {
                        return "{\"status\":\"error\"}";
                    } 

                    if (!readings.ContainsKey(computador.name)) {
                        readings.Add(computador.name, data.Hardware);
                    }
                    else {
                        readings[computador.name] = data.Hardware;
                    }
                    json = "{\"status\":\"success\"}";
                    break;
                default:
                    break;
            }

            return json;
        }

        private string processRequest() {
            string clientVersion  = "0.8.0";
            return "{\"ClientVersion\":\"" + clientVersion + "\"}";
        }

        private string processLeitura(string[] keys, string body, string method) {
            string json = null;
            ComputadorDAO dao = new ComputadorDAO();

            switch (method) {
                case "GET":
                    break;
                case "POST":
                    break;
                default:
                    break;
            }

            return json;
        }

        private void SendJson(HttpListenerResponse response, string content) {
            if (content == null) {
                content = "{}";
            }
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