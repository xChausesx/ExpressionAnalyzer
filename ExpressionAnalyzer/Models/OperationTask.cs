namespace ExpressionAnalyzer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Представляє операцію в графі задачі (Крок 3)
/// </summary>
public class OperationTask
{
	public string Id { get; } // Унікальний ID, напр. "+_1"
	public string Type { get; } // Тип операції, напр. "+"
	public int Duration { get; }
	public List<string> DependencyIds { get; } = new List<string>(); // ID задач, від яких залежить ця
	public int Level { get; set; } = -1; // Рівень у графі

	// Поля для результатів планування
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

/// <summary>
/// Представляє блок на діаграмі Ганта (для візуалізації)
/// </summary>
public record ScheduledTask(string Name, int StartTime, int EndTime);