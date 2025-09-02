using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using vendtechext.BLL.Interfaces;
using vendtechext.Contracts;
using vendtechext.Helper;
using System.Data;

namespace vendtechext.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MigrationController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<MigrationController> _logger;

        public MigrationController(
            IConfiguration configuration,
            ILogger<MigrationController> logger)
        {

            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("migrate-vendors")]
        public async Task<IActionResult> MigrateVendors()
        {
            try
            {
                _logger.LogInformation("Starting vendor migration process...");
                
                var oldDbConnectionString = _configuration.GetConnectionString("VENDTECH_MAIN");
                if (string.IsNullOrEmpty(oldDbConnectionString))
                {
                    return BadRequest("VENDTECH_MAIN connection string not configured");
                }

                var migratedCount = 0;
                var errorCount = 0;
                var errors = new List<string>();

                                 // First, get all already migrated user IDs from the new database
                 var migratedUserIds = await GetMigratedUserIdsAsync();
                 
                 using (var connection = new SqlConnection(oldDbConnectionString))
                 {
                     await connection.OpenAsync();
                     
                     // Query to get all vendor users with their POS, Agency, Commission, and Password information from the old database
                     // Exclude users that have already been migrated
                     var query = @"
                         SELECT 
                             u.UserId,
                             u.Name,
                             u.SurName,
                             u.Email,
                             u.Phone,
                             u.Password,
                             u.Address,
                             u.CityId,
                             u.CountryId,
                             u.ProfilePic,
                             u.CompanyName,
                             u.VendorType,
                             p.CommissionPercentage,
                             u.Vendor,
                             u.Status,
                             u.CreatedAt,
                             p.SerialNumber as PosNumber,
                             p.PassCode,
                             p.IsNewPasscode,
                             a.AgencyId as OldAgencyId,
                             a.AgencyName,
                             c.CommissionId,
                             c.Percentage as CommissionPercentage
                         FROM Users u
                         LEFT JOIN POS p ON u.UserId = p.VendorId
                         LEFT JOIN Agency a ON u.AgentId = a.AgencyId
                         LEFT JOIN Commissions c ON p.CommissionPercentage = c.CommissionId
                         WHERE u.UserType = 17
                         AND u.Status = 1 
                         AND u.Email IS NOT NULL 
                         AND u.Email != ''
                         AND u.UserId NOT IN (" + (migratedUserIds.Any() ? string.Join(",", migratedUserIds) : "0") + ")";

                    var vendors = new List<OldVendorData>();
                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            
                            while (await reader.ReadAsync())
                            {
                                 vendors.Add(new OldVendorData
                                 {
                                     UserId = reader.GetInt64("UserId"),
                                     Name = reader.IsDBNull("Name") ? "" : reader.GetString("Name"),
                                     SurName = reader.IsDBNull("SurName") ? "" : reader.GetString("SurName"),
                                     Email = reader.GetString("Email"),
                                     Phone = reader.IsDBNull("Phone") ? "" : reader.GetString("Phone"),
                                     Password = reader.IsDBNull("Password") ? "" : reader.GetString("Password"),
                                     Address = reader.IsDBNull("Address") ? "" : reader.GetString("Address"),
                                     CityId = reader.IsDBNull("CityId") ? 0 : reader.GetInt32("CityId"),
                                     CountryId = reader.IsDBNull("CountryId") ? 0 : reader.GetInt32("CountryId"),
                                     ProfilePic = reader.IsDBNull("ProfilePic") ? "" : reader.GetString("ProfilePic"),
                                     CompanyName = reader.IsDBNull("CompanyName") ? "" : reader.GetString("CompanyName"),
                                     VendorType = reader.IsDBNull("VendorType") ? 0 : reader.GetInt32("VendorType"),
                                     Vendor = reader.IsDBNull("Vendor") ? "" : reader.GetString("Vendor"),
                                     Status = reader.GetInt32("Status"),
                                     CreatedAt = reader.GetDateTime("CreatedAt"),
                                     PosNumber = reader.IsDBNull("PosNumber") ? "" : reader.GetString("PosNumber"),
                                     PassCode = reader.IsDBNull("PassCode") ? "" : reader.GetString("PassCode"),
                                     IsNewPasscode = reader.IsDBNull("IsNewPasscode") ? false : reader.GetBoolean("IsNewPasscode"),
                                     OldAgencyId = reader.IsDBNull("OldAgencyId") ? (long?)null : reader.GetInt64("OldAgencyId"),
                                     AgencyName = reader.IsDBNull("AgencyName") ? "" : reader.GetString("AgencyName"),
                                     CommissionId = reader.IsDBNull("CommissionId") ? (int?)null : reader.GetInt32("CommissionId"),
                                     CommissionPercentage = reader.IsDBNull("CommissionPercentage") ? 0 : reader.GetInt32("CommissionPercentage")
                                 });
                            }
                        }
                    }

                    _logger.LogInformation($"Found {vendors.Count} vendors to migrate");

                                         // Process vendors sequentially to avoid DbContext threading issues
                     foreach (var vendor in vendors)
                     {
                         var result = await MigrateVendorAsync(vendor);
                         
                         if (result.Success)
                         {
                             migratedCount++;
                         }
                         else
                         {
                             errorCount++;
                             errors.Add(result.ErrorMessage);
                         }
                         
                         // Small delay between vendors to avoid overwhelming the system
                         await Task.Delay(100);
                     }
                }

                var response = new
                {
                    Success = true,
                    Message = $"Vendor migration completed. Migrated: {migratedCount}, Errors: {errorCount}",
                    MigratedCount = migratedCount,
                    ErrorCount = errorCount,
                    Errors = errors.Take(10).ToList() // Only return first 10 errors to avoid response size issues
                };

                _logger.LogInformation($"Vendor migration completed. Migrated: {migratedCount}, Errors: {errorCount}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during vendor migration");
                return StatusCode(500, new { Success = false, Message = "Internal server error during migration", Error = ex.Message });
            }
                 }

           private async Task<List<long>> GetMigratedUserIdsAsync()
          {
              try
              {
                  var migratedIds = new List<long>();
                  var newDbConnectionString = _configuration.GetConnectionString("DefaultConnection");
                  if (string.IsNullOrEmpty(newDbConnectionString))
                  {
                      _logger.LogWarning("DefaultConnection string not configured, assuming no users have been migrated yet");
                      return migratedIds;
                  }

                  using (var connection = new SqlConnection(newDbConnectionString))
                  {
                      await connection.OpenAsync();
                      
                      var query = @"
                          SELECT CAST(MigrationUniqueId AS BIGINT) as UserId
                          FROM Users 
                          WHERE MigrationUniqueId IS NOT NULL 
                          AND MigrationUniqueId != ''";
                      
                      using (var command = new SqlCommand(query, connection))
                      {
                          using (var reader = await command.ExecuteReaderAsync())
                          {
                              while (await reader.ReadAsync())
                              {
                                  if (!reader.IsDBNull("UserId"))
                                  {
                                      migratedIds.Add(reader.GetInt64("UserId"));
                                  }
                              }
                          }
                      }
                  }
                  
                  _logger.LogInformation($"Found {migratedIds.Count} already migrated users to exclude");
                  return migratedIds;
              }
              catch (Exception ex)
              {
                  _logger.LogError(ex, "Error retrieving migrated user IDs, proceeding without exclusion");
                  return new List<long>();
              }
          }

          private static bool IsPasswordValid(string password)
          {
              if (string.IsNullOrEmpty(password))
                  return false;
              
              // Check if password meets the new system's requirements
              // Based on Program.cs: RequiredLength = 6, RequireNonAlphanumeric = false
              // But we also need to check for uppercase requirement from the error message
              return password.Length >= 6 && 
                     password.Any(char.IsUpper) && 
                     password.Any(char.IsLower) && 
                     password.Any(char.IsDigit);
          }
          
          private static string GenerateTemporaryPassword(long userId)
          {
              return CREDENTIALS.VENDOR_PASSWORD;
          }

          private static string DecryptPassword(string encodedData)
         {
             try
             {
                 if (string.IsNullOrEmpty(encodedData))
                     return string.Empty;

                 System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
                 System.Text.Decoder utf8Decode = encoder.GetDecoder();
                 byte[] todecode_byte = Convert.FromBase64String(encodedData);
                 int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
                 char[] decoded_char = new char[charCount];
                 utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
                 string result = new String(decoded_char);
                 return result;
             }
             catch (Exception)
             {
                 // If decryption fails, return empty string
                 return string.Empty;
             }
         }

         private async Task<int> FindMatchingAgencyIdAsync(string agencyName)
         {
             try
             {
                 if (string.IsNullOrEmpty(agencyName))
                 {
                     return 1; // Default to agency ID 1 if no agency name
                 }
                if (agencyName == "VENDTECH")
                    return 7;
                 var newDbConnectionString = _configuration.GetConnectionString("VENDTECHENDUSER_DEV");
                 if (string.IsNullOrEmpty(newDbConnectionString))
                 {
                     _logger.LogWarning("VENDTECHENDUSER_DEV connection string not configured, using default agency ID 1");
                     return 1;
                 }

                 using (var connection = new SqlConnection(newDbConnectionString))
                 {
                     await connection.OpenAsync();
                     
                     var query = @"
                         SELECT Id 
                         FROM Agencies 
                         WHERE LOWER(AgencyName) = LOWER(@AgencyName) 
                         AND Deleted = 0 
                         AND Status = 1";
                     
                     using (var command = new SqlCommand(query, connection))
                     {
                         command.Parameters.AddWithValue("@AgencyName", agencyName);
                         
                         var result = await command.ExecuteScalarAsync();
                         if (result != null && result != DBNull.Value)
                         {
                             return Convert.ToInt32(result);
                         }
                     }
                 }
                 
                 _logger.LogWarning($"No matching agency found for '{agencyName}', using default agency ID 1");
                 return 1; // Default to agency ID 1 if no match found
             }
             catch (Exception ex)
             {
                 _logger.LogError(ex, $"Error finding matching agency for '{agencyName}', using default agency ID 1");
                 return 1; // Default to agency ID 1 on error
             }
         }

                  private async Task<MigrationResult> MigrateVendorAsync(OldVendorData oldVendor)
         {
             try
             {
                 // Create a new service scope for this operation to avoid DbContext threading issues
                 using (var scope = HttpContext.RequestServices.CreateScope())
                 {
                     var scopedUsersService = scope.ServiceProvider.GetRequiredService<IUsersService>();
                     
                     int agencyId = await FindMatchingAgencyIdAsync(oldVendor.AgencyName);
                     
                     var settings = AppConfiguration.GetSettings();
                     var commissions = settings.Commission;
                    
                     int commissionLevelId = 1;
                     
                    var commission = commissions.FirstOrDefault(d => d.Percentage ==  oldVendor.CommissionPercentage);
                     if (commission != null)
                     {
                        commissionLevelId = commission.Id;
                    }
                     string decryptedPassword = DecryptPassword(oldVendor.Password);
                     
                     // Ensure the decrypted password meets the new system's requirements
                     if (!string.IsNullOrEmpty(decryptedPassword) && !IsPasswordValid(decryptedPassword))
                     {
                         // If password doesn't meet requirements, generate a temporary one
                         decryptedPassword = GenerateTemporaryPassword(oldVendor.UserId);
                     }
                     
                     var vendorAccount = new VendorAccount
                      {
                          FirstName = oldVendor.Name,
                          LastName = oldVendor.SurName,
                          Email = oldVendor.Email,
                          Phone = oldVendor.Phone,
                          Address = oldVendor.Address,
                          CityId = 2,
                          CountryId = 2,
                          VendorName = !string.IsNullOrEmpty(oldVendor.CompanyName) ? oldVendor.CompanyName : oldVendor.Vendor,
                          Status = oldVendor.Status,
                          PosNumber = !string.IsNullOrEmpty(oldVendor.PosNumber) ? oldVendor.PosNumber : $"MIG_{oldVendor.UserId}", 
                          CommissionLevelId = commissionLevelId, 
                          AgencyId = agencyId, 
                          MigrationUniqueId = oldVendor.UserId.ToString(),
                          image = null,
                          imgUrl = oldVendor.ProfilePic,
                          Password = decryptedPassword,
                          IsNewPin = oldVendor.IsNewPasscode,
                          PinCode = oldVendor.PassCode,                     
                      };

                    
                     _logger.LogInformation($"Migrating vendor {oldVendor.UserId}: POS={oldVendor.PosNumber}, PassCode={oldVendor.PassCode}, IsNewPin={oldVendor.IsNewPasscode}, OldCommissionId={oldVendor.CommissionId}, NewCommissionLevelId={commissionLevelId}, OldAgencyName={oldVendor.AgencyName}, NewAgencyId={agencyId}, PasswordDecrypted={!string.IsNullOrEmpty(decryptedPassword)}, TemporaryPassword={decryptedPassword.StartsWith("Temp")}");
                     
                     // Call the scoped UsersService to create the vendor
                     var result = await scopedUsersService.CreateVendorAccount(vendorAccount);
                     
                     if (result.status == "success")
                     {
                         // TODO: If needed, set the PassCode and IsNewPin after vendor creation
                         // This might require calling a separate PIN service or updating the user account
                         // For now, we're just creating the vendor with basic information
                         
                         return new MigrationResult { Success = true, ErrorMessage = null };
                     }
                     else
                     {
                         return new MigrationResult { Success = false, ErrorMessage = result.message };
                     }
                 }
            }
            catch (Exception ex)
            {
                return new MigrationResult { Success = false, ErrorMessage = $"Error migrating vendor {oldVendor.UserId}: {ex.Message}" };
            }
         }
    }

              // Data model for old vendor data
     public class OldVendorData
     {
         public long UserId { get; set; }
         public string Name { get; set; }
         public string SurName { get; set; }
         public string Email { get; set; }
         public string Phone { get; set; }
         public string Address { get; set; }
         public int CityId { get; set; }
         public int CountryId { get; set; }
         public string ProfilePic { get; set; }
         public string CompanyName { get; set; }
         public int VendorType { get; set; }
         public string Vendor { get; set; }
         public int Status { get; set; }
         public DateTime CreatedAt { get; set; }
         public string PosNumber { get; set; }
         public string PassCode { get; set; }
         public bool IsNewPasscode { get; set; }
         public string Password { get; set; }
         public long? OldAgencyId { get; set; }
         public string AgencyName { get; set; }
         public int? CommissionId { get; set; }
         public int? CommissionPercentage { get; set; }
     }

    // Result model for migration operations
    public class MigrationResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }
}
