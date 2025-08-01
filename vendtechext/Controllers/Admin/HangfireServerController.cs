using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using vendtechext.BLL.Exceptions;
using Newtonsoft.Json;

namespace vendtechext.Controllers.Admin
{
    [ApiController]
    [Route("admin-hangfire-server/v1")]
    // [Authorize(Roles = APP_ROLES.SuperAdmin)] // Uncomment to restrict
    public class HangfireServerController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public HangfireServerController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET: /admin-hangfire-server/v1/servers
        [HttpGet("servers")]
        public IActionResult GetServers()
        {
            var servers = new List<object>();
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();
                using (var command = new SqlCommand("SELECT Id, Data, LastHeartbeat FROM [HangFire].[Server]", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var dataJson = reader["Data"].ToString();
                            dynamic data = null;
                            try
                            {
                                data = JsonConvert.DeserializeObject(dataJson);
                            }
                            catch { }

                            servers.Add(new
                            {
                                Id = reader["Id"].ToString(),
                                LastHeartbeat = reader["LastHeartbeat"].ToString(),
                                WorkerCount = data?.WorkerCount,
                                Queues = data?.Queues,
                                ServerName = data?.ServerName
                            });
                        }
                    }
                }
            }
            return Ok(servers);
        }

        // DELETE: /admin-hangfire-server/v1/server/{id}
        [HttpDelete("server/{id}")]
        public IActionResult RemoveServer(string id)
        {
            int rowsAffected = 0;
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();
                using (var command = new SqlCommand("DELETE FROM [HangFire].[Server] WHERE Id = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    rowsAffected = command.ExecuteNonQuery();
                }
            }
            if (rowsAffected == 0)
                throw new ServerTechnicalException($"Server with Id '{id}' not found.");
            return Ok(new { message = "Server removed successfully", id });
        }
    }
} 