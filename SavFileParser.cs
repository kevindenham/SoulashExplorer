namespace SavFileParser
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter the path to the world.sav file:");
            string filePath = Console.ReadLine();

            if (!File.Exists(filePath))
            {
                Console.WriteLine("File not found. Please check the path and try again.");
                return;
            }

            string lines = File.ReadAllText(filePath);
            var regions = new Dictionary<int, List<Resource>>();
            var regionSeenCounts = new Dictionary<int, int>(); // Tracks how many times each region is processed

            // Split by pipe
            var regionLines = lines.Split('|');
            foreach (var regionLine in regionLines)
            {
                if (regionLine.Contains("*") == false)
                {
                    continue;
                }

                // Split by star
                string[] regionParts = regionLine.Split('*');
                int regionId = int.Parse(regionParts[0]);

                // Increment the count for this region
                if (!regionSeenCounts.ContainsKey(regionId))
                {
                    regionSeenCounts[regionId] = 0;
                }
                regionSeenCounts[regionId]++;

                // Initialize the region's resource list if not already done
                if (!regions.ContainsKey(regionId))
                {
                    regions[regionId] = new List<Resource>();
                }

                // Process resources in the region
                for (int i = 1; i < regionParts.Length; i++)
                {
                    if (regionParts[i].Contains("&0&0"))
                    {
                        string[] resourceParts = regionParts[i].Split('&');

                        int quantity = int.Parse(resourceParts[1]);

                        if (regions[regionId].Any(x => x.Name == resourceParts[0]))
                        {
                            regions[regionId].First(x => x.Name == resourceParts[0]).TimesSeen++;
                            // Update highest seen in region if applicable
                            if (quantity > regions[regionId].First(x => x.Name == resourceParts[0]).HighestSeenInRegion)
                            {
                                regions[regionId].First(x => x.Name == resourceParts[0]).HighestSeenInRegion = quantity;
                            }
                        }
                        else
                        {
                            regions[regionId].Add(new Resource { Name = resourceParts[0], TimesSeen = 1, HighestSeenInRegion = quantity });
                        }
                    }
                }
            }

            // Output results
            foreach (var region in regions)
            {

                int totalRegionSeen = regionSeenCounts[region.Key]; // Total times this region was seen
                Console.WriteLine($"Region ID: {region.Key} - {totalRegionSeen}");
                foreach (var resource in region.Value)
                {
                    // Calculate the percentage of times the resource was seen in this region
                    double percentage = (double)resource.TimesSeen / totalRegionSeen * 100;
                    percentage = Math.Round(percentage, 2); // Round to two decimal places
                    Console.WriteLine($"{resource.Name} : {resource.TimesSeen} ({percentage}%) - Highest: {resource.HighestSeenInRegion}");
                }
                Console.WriteLine();
            }
        }
    }

    class Resource
    {
        public string Name { get; set; }
        public int TimesSeen { get; set; }
        public int HighestSeenInRegion { get; set; }
    }
}
