using System;
using ResourceMonitorAPI.models;
using ResourceMonitorAPI.utils;

namespace ResourceMonitorAPI
{
    class Program
    {
        static void Main(string[] args)
        {
            var api = new API();
            api.Start();
        }
    }
}
