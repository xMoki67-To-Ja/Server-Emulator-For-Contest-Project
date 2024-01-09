using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System;
using System.Text.Json;

namespace EkoPolMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public class FridgeData
        {
            public bool IsRunning { get; set; }
            public TemperatureMainData TemperatureMain { get; set; }
        }

        public class TemperatureMainData
        {
            public double Value { get; set; }
            public bool Error { get; set; }
        }
        public string GetFridgeDataJson()
        {
            // Tworzymy losowy obiekt FridgeData
            var fridgeData = new FridgeData
            {
                IsRunning = new Random().Next(0, 2) == 1,
                TemperatureMain = new TemperatureMainData
                {
                    Value = new Random().Next(1000, 3000),
                    Error = new Random().Next(0, 2) == 1
                }
            };

            // Używamy klasy DataContractJsonSerializer do przekształcenia obiektu FridgeData w JSON
            var serializer = new DataContractJsonSerializer(typeof(FridgeData));
            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, fridgeData);
                return Encoding.Default.GetString(stream.ToArray());
            }
        }
        public void StartServer()
        {
            var listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8080/");

            listener.Start();
            Console.WriteLine("Server started...");

            while (true)
            {
                var context = listener.GetContext();
                var response = context.Response;

                // Zwracamy symulowane dane JSON, gdy otrzymamy żądanie GET na adresie /api/fridge
                if (context.Request.HttpMethod == "GET" && context.Request.Url.LocalPath == "/api/v1/school/status")
                {
                    var fridgeDataJson = GetFridgeDataJson();
                    var fridgeData = JsonSerializer.Deserialize<FridgeData>(fridgeDataJson);

                    var responseJson = JsonSerializer.Serialize(new
                    {
                        IS_RUNNING = fridgeData.IsRunning ? true : false,
                        TEMPERATURE_MAIN = new
                        {
                            value = fridgeData.TemperatureMain.Value,
                            error = fridgeData.TemperatureMain.Error,
                            
                        }
                    });

                    response.ContentType = "application/json";
                    response.ContentEncoding = Encoding.UTF8;
                    response.StatusCode = 200;

                    using (var stream = response.OutputStream)
                    {
                        var content = Encoding.UTF8.GetBytes(responseJson);
                        stream.Write(content, 0, content.Length);
                    }
                }
                else
                {
                    response.StatusCode = 404;
                }

                response.Close();
            }
        }
        public MainWindow()
        {
            InitializeComponent();
            StartServer();
        }
    }
}
