# Vendor Migration Controller

This controller handles the migration of vendors from the old VENDTECH_MAIN database to the new VENDTECHEXT_DEV database.

## Overview

The `MigrationController` provides an endpoint to migrate vendor accounts from the legacy database system to the new system while preserving all essential vendor information and maintaining data integrity.

## Features

- **Batch Processing**: Processes vendors in batches of 10 to avoid overwhelming the system
- **Async Operations**: All operations are asynchronous for better performance
- **Error Handling**: Comprehensive error handling and logging
- **Data Mapping**: Maps old database schema to new VendorAccount model
- **Migration Tracking**: Stores the old UserId in MigrationUniqueId for reference

 ## Database Schema Mapping
 
 ### Table Relationships
 The migration queries four main tables from the old VENDTECH_MAIN database:
 
 - **Users**: Main vendor information (UserType = 17 for vendors)
 - **POS**: POS device information linked via `Users.UserId = POS.VendorId`
 - **Agency**: Agency information linked via `Users.AgentId = Agency.AgencyId`
 - **Commissions**: Commission levels linked via `Users.VendorCommissionPercentage = Commissions.Percentage`
 
 **Note**: Commission levels are determined by using CommissionId from old database when available, otherwise matching the old `VendorCommissionPercentage` with the current system's commission settings.
 **Note**: Agency IDs are determined by matching agency names with the new VENDTECHCONSUMER_DEV database.
 
 ### Password Decryption
 The migration process automatically decrypts encrypted passwords from the old system:
 
 **Old Database (VENDTECH_MAIN.Users)**:
 - `Password` field contains Base64 encoded encrypted passwords
 - Uses custom UTF-8 decoding algorithm for decryption
 - Handles decryption failures gracefully
 
   **Decryption Process**:
  1. **Base64 Decoding**: Converts encoded string to byte array
  2. **UTF-8 Decoding**: Uses UTF-8 decoder to convert bytes to characters
  3. **Error Handling**: Returns empty string if decryption fails
  4. **Password Validation**: Checks if decrypted password meets new system requirements
  5. **Temporary Password Generation**: Creates compliant password if original doesn't meet requirements
  6. **Password Assignment**: Sets validated password in new VendorAccount
 
   **Security Note**: Decrypted passwords are logged only as boolean flags (PasswordDecrypted=true/false) for security purposes.
  
  **Password Requirements**: The new system requires passwords to have:
  - Minimum 6 characters length
  - At least one uppercase letter (A-Z)
  - At least one lowercase letter (a-z)
  - At least one digit (0-9)
  
  **Temporary Password Format**: If original password doesn't meet requirements, generates `Temp{UserId}@123`
 
 ### Agency Matching
 The migration process automatically matches agency names from the old system with agencies in the new VENDTECHCONSUMER_DEV database:
 
 **Old Database (VENDTECH_MAIN.Agency)**:
 - `AgencyId` (bigint)
 - `AgencyName` (nvarchar(500))
 - `AgentType` (int)
 - `Status` (int)
 - `CreatedAt` (datetime)
 - `CommissionId` (int)
 - `Representative` (bigint)
 
 **New Database (VENDTECHCONSUMER_DEV.Agencies)**:
 - `Id` (int)
 - `AgencyName` (nvarchar(max))
 - `UserId` (nvarchar(max))
 - `Description` (nvarchar(max))
 - `Status` (int)
 - `PosId` (uniqueidentifier)
 - `Deleted` (bit)
 - `CreatedAt` (datetime2)
 - `UpdatedAt` (datetime2)
 - `CreatedBy` (nvarchar(max))
 - `UpdatedBy` (nvarchar(max))
 
 **Matching Logic**:
 - Queries new database for agencies with matching `AgencyName`
 - Only considers active agencies (`Deleted = 0` and `Status = 1`)
 - Returns the matching `Id` from the new system
 - Falls back to Agency ID 1 if no match is found
 
 ### Commission Level Matching
 The migration process uses a two-tier approach for commission level matching:
 
 **Primary Method**: Uses CommissionId from old Commissions table when available
 - Direct mapping from old system's CommissionId to new system
 - Most accurate and reliable method
 
 **Fallback Method**: Matches vendor commission percentages with current system commission structure
 ```json
 Current Commission Settings:
 [
   {"id": 1, "percentage": 1.5},
   {"id": 2, "percentage": 0},
   {"id": 3, "percentage": 1},
   {"id": 4, "percentage": 0.5}
 ]
 ```
 
 **Matching Logic**:
 1. **Priority 1**: Use `Commissions.CommissionId` if available and valid
 2. **Priority 2**: Match `VendorCommissionPercentage` with current system percentages
 3. **Default**: Use CommissionLevelId = 1 if no match is found
 
 ### Old Database (VENDTECH_MAIN.Users + POS + Agency + Commissions)
 - `Users.UserId` → `MigrationUniqueId`
 - `Users.Name` → `FirstName`
 - `Users.SurName` → `LastName`
   - `Users.Email` → `Email`
  - `Users.Phone` → `Phone`
  - `Users.Password` → `Password` (decrypted)
  - `Users.Address` → `Address`
 - `Users.CityId` → `CityId`
 - `Users.CountryId` → `CountryId`
 - `Users.ProfilePic` → `imgUrl`
 - `Users.CompanyName` or `Users.Vendor` → `VendorName`
   - `Users.VendorCommissionPercentage` → `CommissionLevelId` (via Commissions table or percentage matching)
 - `Users.Status` → `Status`
 - `Users.AgentId` → `AgencyId` (via JOIN with Agency table and name matching)
 - `POS.SerialNumber` → `PosNumber` (via JOIN on Users.UserId = POS.VendorId)
 - `POS.PassCode` → Captured for potential PIN setup
 - `POS.IsNewPasscode` → Captured for PIN status tracking
   - `Agency.AgencyName` → Used for agency matching in new database
  - `Users.Password` → Decrypted and used as Password in new system
  - `Commissions.CommissionId` → Primary source for CommissionLevelId
  - `Commissions.Percentage` → Used for commission validation and fallback matching

### New Database (VENDTECHEXT_DEV)
- All fields are properly mapped to the new `VendorAccount` model
- Personal information stored in `AppUser` table
- Business information stored in vendor-specific tables

## Configuration

### Connection Strings

 Add the following connection strings to your `appsettings.json`:
 
 ```json
 {
   "ConnectionStrings": {
     "VENDTECH_MAIN": "Server=your_server;Database=VENDTECH_MAIN;User Id=your_user;Password=your_password;MultipleActiveResultSets=True;TrustServerCertificate=true;",
     "VENDTECHENDUSER_DEV": "Server=your_server;Database=VENDTECHCONSUMER_DEV;User Id=your_user;Password=your_password;MultipleActiveResultSets=True;TrustServerCertificate=true;"
   }
 }
 ```

### Required Packages

Ensure the following NuGet package is installed:

```xml
<PackageReference Include="Microsoft.Data.SqlClient" Version="5.1.5" />
```

## API Endpoint

### POST /api/migration/migrate-vendors

**Description**: Migrates all vendor accounts from the old database to the new system.

**Request**: No request body required

**Response**:
```json
{
  "success": true,
  "message": "Vendor migration completed. Migrated: 150, Errors: 2",
  "migratedCount": 150,
  "errorCount": 2,
  "errors": [
    "Error migrating vendor 12345: Email already exists",
    "Error migrating vendor 67890: Invalid country ID"
  ]
}
```

  ## Migration Process
  
    1. **Duplicate Prevention**: Queries the new database to get all already migrated user IDs from MigrationUniqueId field
    2. **Data Extraction**: Queries the old VENDTECH_MAIN.Users table joined with POS, Agency, and Commissions tables for vendor records, POS information, agency details, commission levels, and encrypted passwords, excluding already migrated users
    3. **Data Validation**: Ensures required fields are present and valid
    4. **Data Transformation**: Maps old schema to new VendorAccount model, including POS SerialNumber as PosNumber, commission percentage matching, agency name matching, and password decryption
    5. **Vendor Creation**: Calls UsersService.CreateVendorAccount for each vendor using scoped services
    6. **Sequential Processing**: Processes vendors sequentially to avoid DbContext threading issues
    7. **Error Handling**: Captures and reports any errors during migration
    8. **Progress Tracking**: Logs migration progress and results, including POS, Commission, Agency matching, and Password decryption information

## Usage Example

```bash
# Trigger vendor migration
curl -X POST "https://your-api-url/api/migration/migrate-vendors"
```

   ## Important Notes
  
  - **Duplicate Prevention**: Automatically excludes users that have already been migrated by checking MigrationUniqueId in the new database. This allows safe re-running of the migration process.
  - **Threading Safety**: Uses scoped services and sequential processing to avoid DbContext threading conflicts that can occur with parallel operations.
  - **Agency Assignment**: Automatically matches agency names from old system with new VENDTECHCONSUMER_DEV database to determine correct AgencyId. Falls back to Agency ID 1 if no match is found.
 - **POS Numbers**: Uses actual POS SerialNumber from the old database when available, otherwise generates temporary "MIG_{OldUserId}" numbers.
 - **PIN Codes**: PassCode and IsNewPasscode are captured from the POS table but not automatically set during migration (requires additional PIN service integration).
 - **Commission Levels**: Matches old `VendorCommissionPercentage` with current system commission settings to determine `CommissionLevelId`. Defaults to level 1 if no match is found.
 - **Images**: Profile pictures are preserved as URLs but not re-uploaded during migration.
 - **Status**: Vendor status is preserved from the old system.
 - **Passwords**: Encrypted passwords from old system are automatically decrypted and preserved in new system.

## Error Handling

The migration process handles various error scenarios:

- **Database Connection Issues**: Returns appropriate error messages
- **Data Validation Errors**: Logs and reports validation failures
- **Service Errors**: Captures errors from UsersService operations
- **Network Timeouts**: Implements retry logic and batch processing

## Logging

All migration operations are logged with appropriate log levels:

- **Information**: Migration start, progress, and completion
- **Warning**: Non-critical issues during migration
- **Error**: Critical errors and failures

## Security Considerations

- Ensure database credentials are properly secured
- Consider implementing authentication for the migration endpoint
- Validate all input data before processing
- Implement rate limiting if needed

## Performance Considerations

- **Batch Size**: Configurable batch size (default: 10)
- **Delays**: 1-second delay between batches to avoid overwhelming the system
- **Async Operations**: All database operations are asynchronous
- **Connection Management**: Proper connection disposal and management

## Troubleshooting

### Common Issues

1. **Connection String Errors**: Verify VENDTECH_MAIN connection string configuration
2. **Permission Errors**: Ensure database user has read access to Users table
3. **Validation Errors**: Check that required fields are present in old database
4. **Service Errors**: Verify UsersService is properly configured and accessible

### Debug Information

Enable detailed logging to troubleshoot migration issues:

```json
{
  "Logging": {
    "LogLevel": {
      "vendtechext.Controllers.MigrationController": "Debug"
    }
  }
}
```
