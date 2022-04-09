using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Azure.Messaging.EventHubs.Consumer;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

using static ForestGuard.Pages.IndexModel;

namespace ForestGuard.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public class tempValues
        {
            public int? messageId { get; set; }
            public string? deviceId { get; set; }

            public int? Temperature { get; set; }

            public int? Humidity { get; set; }

        }

        List<int>? tempData = new List<int>();

        public List<tempValues>? Result { get; set; } = new List<tempValues>();

        public string? values { get; set; }

        public async void OnGet()
        {
           await ReceiveMessagesFromDeviceAsync();
        }

        private async Task ReceiveMessagesFromDeviceAsync()

        {

            var hubConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:44348/message").
                Build();

           await hubConnection.StartAsync();
                
            string connectionString = "Endpoint=sb://ihsuprodsouthafricanorthres005dednamespace.servicebus.windows.net/Endpoint=sb://ihsuprodsouthafricanorthres005dednamespace.servicebus.windows.net/;SharedAccessKeyName=iothubowner;SharedAccessKey=PkZ083+rNhOre3ekmeNBWWFKV0IvZLGK+sHKAltELJ0=;EntityPath=iothub-ehub-forestguar-16791339-e3c4c749f9";
            string eventhubName = "iothub-ehub-forestguar-16791339-e3c4c749f9";

            // Create the consumer using the default consumer group using a direct connection to the service.
            // Information on using the client with a proxy can be found in the README for this quick start, here:
            // https://github.com/Azure-Samples/azure-iot-samples-csharp/tree/main/iot-hub/Quickstarts/ReadD2cMessages/README.md#websocket-and-proxy-support
            await using var consumer = new EventHubConsumerClient(
                EventHubConsumerClient.DefaultConsumerGroupName,
                connectionString,eventhubName);

            Console.WriteLine("Listening for messages on all partitions.");

            try
            {
                // Begin reading events for all partitions, starting with the first event in each partition and waiting indefinitely for
                // events to become available. Reading can be canceled by breaking out of the loop when an event is processed or by
                // signaling the cancellation token.
                //
                // The "ReadEventsAsync" method on the consumer is a good starting point for consuming events for prototypes
                // and samples. For real-world production scenarios, it is strongly recommended that you consider using the
                // "EventProcessorClient" from the "Azure.Messaging.EventHubs.Processor" package.
                //
                // More information on the "EventProcessorClient" and its benefits can be found here:
                await foreach (PartitionEvent partitionEvent in consumer.ReadEventsAsync())
                {
                    //Console.WriteLine($"\nMessage received on partition {partitionEvent.Partition.PartitionId}:");

                    string data = Encoding.UTF8.GetString(partitionEvent.Data.Body.ToArray());
                    Console.WriteLine($"\tMessage body: {data}");

                    await hubConnection.InvokeAsync("Broadcast", "messagehub", data);

                    
                    //Console.WriteLine("\tApplication properties (set by device):");
                    //foreach (KeyValuePair<string, object> prop in partitionEvent.Data.Properties)
                    //{
                       // PrintProperties(prop);
                   // }

                    //Console.WriteLine("\tSystem properties (set by IoT Hub):");
                    //foreach (KeyValuePair<string, object> prop in partitionEvent.Data.SystemProperties)
                    //{
                        //PrintProperties(prop);
                    //}
                }
            }
            catch (Exception)
            {
                // This is expected when the token is signaled; it should not be considered an
                // error in this scenario.
                Console.WriteLine("ERROR");
            }
        }

        private static void PrintProperties(KeyValuePair<string, object> prop)
        {
            string? propValue = prop.Value is DateTime
                ? ((DateTime)prop.Value).ToString("O") // using a built-in date format here that includes milliseconds
                : prop.Value.ToString();

            Console.WriteLine($"\t\t{prop.Key}: {propValue}");
        }

    }

   public class MessageHub : Hub 
    {
        public  async Task Broadcast(string sender, tempValues values)
        {
          
            await Clients.All.SendAsync("ReceiveMessage", sender, values);
        }
    }

   
}