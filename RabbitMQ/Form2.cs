using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Timers;

namespace RabbitMQ
{
    public partial class Form2 : Form
    {
        private System.Timers.Timer refreshTimer;
        private DataGridView dataGridViewQueues;

        public Form2()
        {
            InitializeComponent();
            InitializeDataGridView();
            InitializeTimer();
            LoadQueuesAsync(); // Initial load
        }

        private void InitializeDataGridView()
        {
            dataGridViewQueues = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ScrollBars = ScrollBars.Both
            };

            this.Controls.Add(dataGridViewQueues);
        }

        private void InitializeTimer()
        {
            refreshTimer = new System.Timers.Timer(5000); // 5 seconds
            refreshTimer.Elapsed += async (s, e) => await LoadQueuesAsync();
            refreshTimer.AutoReset = true;
            refreshTimer.Start();
        }

        private async Task LoadQueuesAsync()
        {
            try
            {
                var queues = await GetRabbitMQQueues("http://localhost:15672/api/queues", "guest", "guest");
                 
                dataGridViewQueues.Invoke(new Action(() =>
                { 
                    dataGridViewQueues.Columns.Clear(); 
                    dataGridViewQueues.Columns.Add("Name", "Queue Name");
                    dataGridViewQueues.Columns.Add("Messages", "Messages");
                    dataGridViewQueues.Columns.Add("MessagesReady", "Ready");
                    dataGridViewQueues.Columns.Add("MessagesUnack", "Unacknowledged");
                    dataGridViewQueues.Columns.Add("Consumers", "Consumers");
                    dataGridViewQueues.Columns.Add("State", "State");
                    dataGridViewQueues.Columns.Add("Memory", "Memory (bytes)");
                    dataGridViewQueues.Columns.Add("IdleSince", "Idle Since");
                    dataGridViewQueues.Columns.Add("Type", "Type");
                    dataGridViewQueues.Columns.Add("Durable", "Durable");

                    // Clear existing rows
                    dataGridViewQueues.Rows.Clear();

                    foreach (var queue in queues)
                    {
                        dataGridViewQueues.Rows.Add(
                            queue.name,
                            queue.messages,
                            queue.messages_ready,
                            queue.messages_unacknowledged,
                            queue.consumers,
                            queue.state,
                            queue.memory,
                            queue.idle_since,
                            queue.type,
                            queue.durable
                        );
                    }

                    // Optional: Apply some formatting
                    dataGridViewQueues.Columns["Memory"].DefaultCellStyle.Format = "N0";
                    dataGridViewQueues.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private async Task<List<RabbitMQQueue>> GetRabbitMQQueues(string apiUrl, string username, string password)
        {
            using var client = new HttpClient();
            var byteArray = Encoding.ASCII.GetBytes($"{username}:{password}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            var response = await client.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<RabbitMQQueue>>(jsonString);
        }

        public class RabbitMQQueue
        {
            public string name { get; set; }
            public int messages { get; set; }
            public int messages_ready { get; set; }
            public int messages_unacknowledged { get; set; }
            public int consumers { get; set; }
            public string state { get; set; }
            public long memory { get; set; }
            public string idle_since { get; set; }
            public string type { get; set; }
            public bool durable { get; set; }
            public MessageStats message_stats { get; set; }

            // Add more properties as needed from the JSON
        }

        public class MessageStats
        {
            public int publish { get; set; }
            public int deliver { get; set; }
            public int ack { get; set; }
            // Add more message stats properties as needed
        }
    }
}