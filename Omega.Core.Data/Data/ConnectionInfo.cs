namespace Omega.Core.Data
{
    public class ConnectionInfo
    {
        public ConnectionInfo(string connectionName)
        {
            ConnectionName = connectionName;
        }
        public string ConnectionName { get; set; }
    }
}
