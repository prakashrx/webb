using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace D3Demo;

public class DataCommands
{
    private static readonly Random _random = new Random();
    
    public async Task<List<DataPoint>> GetScatterData(DataArgs args)
    {
        await Task.Delay(100); // Simulate work
        
        var data = new List<DataPoint>();
        var count = args.Count ?? 50;
        
        for (int i = 0; i < count; i++)
        {
            data.Add(new DataPoint
            {
                X = _random.Next(0, 100),
                Y = _random.Next(0, 100),
                R = _random.Next(5, 20),
                Category = _random.Next(0, 3).ToString()
            });
        }
        
        return data;
    }
    
    public async Task<List<BarData>> GetBarData()
    {
        await Task.Delay(50);
        
        var categories = new[] { "Product A", "Product B", "Product C", "Product D", "Product E" };
        return categories.Select(cat => new BarData
        {
            Name = cat,
            Value = _random.Next(10, 100)
        }).ToList();
    }
    
    public async Task<List<TimeSeriesData>> GetTimeSeriesData()
    {
        await Task.Delay(50);
        
        var data = new List<TimeSeriesData>();
        var startDate = DateTime.Now.AddDays(-30);
        
        for (int i = 0; i < 30; i++)
        {
            data.Add(new TimeSeriesData
            {
                Date = startDate.AddDays(i).ToString("yyyy-MM-dd"),
                Value = 50 + _random.Next(-20, 20) + (i * 2)
            });
        }
        
        return data;
    }
}

public class DataArgs
{
    public int? Count { get; set; }
}

public class DataPoint
{
    public int X { get; set; }
    public int Y { get; set; }
    public int R { get; set; }
    public string Category { get; set; } = "";
}

public class BarData
{
    public string Name { get; set; } = "";
    public int Value { get; set; }
}

public class TimeSeriesData
{
    public string Date { get; set; } = "";
    public int Value { get; set; }
}