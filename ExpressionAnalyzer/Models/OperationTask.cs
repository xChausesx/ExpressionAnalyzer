namespace ExpressionAnalyzer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class OperationTask
{
	public string Id { get; }
	public string Type { get; }
	public int Duration { get; }
	public List<string> DependencyIds { get; } = new List<string>();
	public int Level { get; set; } = -1;
	public int ProcessorId { get; set; }
	public int StartTime { get; set; }
	public int EndTime { get; set; }

	public OperationTask(string id, string type, int duration)
	{
		Id = id;
		Type = type;
		Duration = duration;
	}
}
public record ScheduledTask(string Name, int StartTime, int EndTime);