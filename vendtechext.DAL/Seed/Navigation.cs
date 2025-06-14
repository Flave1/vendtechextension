using System.Text.Json;

namespace vendtechext.DAL.Seed
{
    public class NavigationItem
    {
        public int Id { get; set; } //Unique Identifier
        public string DisplayName { get; set; }// Display Name
        public int Modules { get; set; }//Parent Sorting number
        public int SortNumber { get; set; }//Sub Sorting number
        public int Type {  get; set; } //B2C = 1, B2B = 2, General = 0
    }

    public class NavigationService
    {
        public List<NavigationItem> GetNavigationItems()
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "navigation.json");

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The navigation.json file was not found.", filePath);
            }

            var jsonContent = File.ReadAllText(filePath);

            var navigationItems = JsonSerializer.Deserialize<List<NavigationItem>>(jsonContent);

            // Check for duplicate Ids
            var duplicateIds = navigationItems
                .GroupBy(x => x.Id)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateIds.Any())
            {
                throw new Exception($"Duplicate Ids found in navigation.json: {string.Join(", ", duplicateIds)}");
            }

            return navigationItems ?? new List<NavigationItem>();
        }
    }
}
