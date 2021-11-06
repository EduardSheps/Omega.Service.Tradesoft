using Omega.Core.Data;
using Omega.Core.Job;
using Quartz;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Omega.Job.Tradesoft
{
    [DisallowConcurrentExecution]
    public sealed class TradesoftClientOrderJob : ImplementingQuartz<TradesoftClientOrderJob>, IJob, INotifyPropertyChanged
    {
        //private readonly IConfigManager configManager;
        //private readonly IDisposableResource diposableResource;

        //public TradesoftClientOrderJob(IConfigManager configManager, IDisposableResource diposableResource)
        //{
        //    this.configManager = configManager ?? throw new ArgumentNullException(nameof(configManager));
        //    this.diposableResource = diposableResource ?? throw new ArgumentNullException(nameof(diposableResource));
        //}

        public TradesoftClientOrderJob()
        {
            CronSetting = "0/50 * * * * ?";
        }

        public Task Execute(IJobExecutionContext context)
        {

            ClientOrderGet();

            Console.Write($"Job {typeof(TradesoftClientOrderJob).Name} fired!" + Environment.NewLine);

            return Task.FromResult<object>(null);
        }

        private void SupplierOrderGet()
        {

            string connection = "Data Source=m-dts01;Initial Catalog=ADTS_TEST2;Integrated Security=True";

            var datetime = DateTime.Now.AddSeconds(-500);
            try
            {

                using (var conn = new SqlConnection(connection))
                {
                    using (var cmd = new SqlCommand())
                    {
                        conn.Open();
                        cmd.Connection = conn;
                        cmd.CommandType = System.Data.CommandType.Text;
                        cmd.CommandText = "SELECT MAX(time_edit) FROM dbo.tbr_A_ECOM_Exchange_Supplier_Order";
                        datetime = Convert.ToDateTime(cmd.ExecuteScalar());
                        conn.Close();
                    }
                }

                var client = new RestClient("http://testom.etsp.ru/");
                var request = new RestRequest(@"api/v1//supplier-orders", DataFormat.Json);
                request.AddParameter("token", "596710757d7f1c1064334b10a1c5146849c93e08");
                request.AddParameter("withSupplier-positions", "true");
                request.AddParameter("dateUpdatedStart", datetime.ToString("yyyy-MM-dd hh:mm:ss"));

                var response = client.Get(request);
                string result = result = response.Content;

                var xml = XDocument.Load(JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(result), new XmlDictionaryReaderQuotas()), LoadOptions.None);


                using (var conn = new SqlConnection(connection))
                {
                    using (var cmd = new SqlCommand())
                    {
                        conn.Open();
                        cmd.Connection = conn;
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.CommandText = "usp_A_Ecom_Customer_Orders_Insert_Buffer";
                        cmd.Parameters.Add(new SqlParameter("@xml", System.Data.SqlDbType.Xml, 0)).Value = xml.ToString();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                }

            }
            catch (Exception ex)
            {

            }

        }
        private void ClientOrderGet()
        {

            string connection = "Data Source=m-dts01;Initial Catalog=ADTS_TEST2;Integrated Security=True";

            var datetime = DateTime.Now.AddSeconds(-500);
            try
            {
                
                using (var conn = new SqlConnection(connection))
                {
                    using (var cmd = new SqlCommand())
                    {
                        conn.Open();
                        cmd.Connection = conn;
                        cmd.CommandType = System.Data.CommandType.Text;
                        cmd.CommandText = "SELECT MAX(time_edit) FROM dbo.tbr_A_ECOM_Exchange_CustomerOrder";
                        datetime = Convert.ToDateTime(cmd.ExecuteScalar());
                        conn.Close();
                    }
                }

                var client = new RestClient("http://testom.etsp.ru/");
                var request = new RestRequest(@"api/v1/customers/orders", DataFormat.Json);
                request.AddParameter("token", "596710757d7f1c1064334b10a1c5146849c93e08");
                request.AddParameter("withPositions", "true");
                request.AddParameter("dateUpdatedStart", datetime.ToString("yyyy-MM-dd hh:mm:ss"));

                var response = client.Get(request);
                string result = result = response.Content;

                var xml = XDocument.Load(JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(result), new XmlDictionaryReaderQuotas()), LoadOptions.None);


                using (var conn = new SqlConnection(connection))
                {
                    using (var cmd = new SqlCommand())
                    {
                        conn.Open();
                        cmd.Connection = conn;
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.CommandText = "usp_A_Ecom_Customer_Orders_Insert_Buffer";
                        cmd.Parameters.Add(new SqlParameter("@xml", System.Data.SqlDbType.Xml, 0)).Value = xml.ToString();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                }

            }
            catch (Exception ex)
            {
                
            }

        }
    }
}
