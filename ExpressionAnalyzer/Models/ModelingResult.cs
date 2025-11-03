using ExpressionAnalyzer.Models;

/// <summary>
/// Зберігає всі розраховані метрики (Крок 6)
/// </summary>
public class ModelingResult
{
	public double SequentialTime { get; set; }
	public double ParallelTime { get; set; }
	public double Speedup { get; set; }
	public int ActiveProcessors { get; set; } // "Ідеальна" кількість
	public int TotalProcessors { get; set; }
	public double EfficiencyActive { get; set; }
	public double EfficiencyTotal { get; set; }
	public List<ScheduledTask>[] GanttChart { get; set; }

	public void PrintMetrics()
	{
		Console.WriteLine("--- 📊 Розрахунок метрик ---");
		Console.WriteLine($"Час послідовного обчислення (T_s): {SequentialTime}");
		Console.WriteLine($"Час паралельного обчислення (T_p): {ParallelTime}");
		Console.WriteLine($"Загальна кількість процесорів (N): {TotalProcessors}");
		Console.WriteLine($"Кількість процесорів, що використовується (P_active): {ActiveProcessors}");
		Console.WriteLine($"Коефіцієнт прискорення (S): {Speedup:F2}");
		Console.WriteLine($"Коефіцієнт ефективності (E_active): {EfficiencyActive:F2}");
		Console.WriteLine($"Коефіцієнт ефективності (E_total): {EfficiencyTotal:F2}");
	}

	public void PrintGanttChart()
	{
		Console.WriteLine("\n--- 📊 Діаграма Ганта ---");
		for (int i = 0; i < GanttChart.Length; i++)
		{
			Console.Write($"P{i}: ");
			int lastEnd = 0;
			foreach (var task in GanttChart[i].OrderBy(t => t.StartTime))
			{
				// Додаємо час простою
				if (task.StartTime > lastEnd)
				{
					Console.Write($"[ {new string('-', task.StartTime - lastEnd)} ]");
				}
				// Додаємо саму задачу
				Console.Write($"[ {task.Name} ({task.EndTime - task.StartTime}) ]");
				lastEnd = task.EndTime;
			}
			Console.WriteLine();
		}
	}
}