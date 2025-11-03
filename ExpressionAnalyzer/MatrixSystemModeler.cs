using ExpressionAnalyzer.Models;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

/// <summary>
/// Реалізує логіку моделювання для ЛР 5
/// </summary>
public class MatrixSystemModeler
{
	private readonly int _numProcessors = 7;
	private readonly Dictionary<string, int> _operationTimes = new Dictionary<string, int>
	{
		{ "+", 1 },
		{ "-", 1 },
		{ "*", 2 },
		{ "/", 4 },
		{ "S-R", 1 }
    };

	private readonly int _sendReceiveHopTime;

	public MatrixSystemModeler()
	{
		_sendReceiveHopTime = _operationTimes["S-R"];
	}

	/// <summary>
	/// Головний метод: виконує моделювання
	/// </summary>
	public ModelingResult Simulate(TreeNode rootNode)
	{
		// Крок 3: Побудова графу задачі
		var (allTasks, operands, sequentialTime) = BuildTaskGraph(rootNode);

		// Крок 4: Групування вершин за рівнем та типом операції
		var (executionStages, maxLevelWidth) = GroupTasksForMatrixSystem(allTasks, operands);

		Console.WriteLine("--- 📋 Етапи виконання (Рівень + Тип операції) ---");
		int stageNum = 0;
		foreach (var stage in executionStages)
		{
			Console.WriteLine($"Етап {stageNum++}: [{string.Join(", ", stage.Select(t => t.Id))}]");
		}

		// Крок 5: Розподілення тасок між процесорами
		var (parallelTime, ganttChart) = AssignTasksToProcessors(executionStages, operands);

		// Крок 6: Обчислення метрик
		var result = CalculateMetrics(sequentialTime, parallelTime, maxLevelWidth);
		result.GanttChart = ganttChart;

		return result;
	}

	/// <summary>
	/// Крок 3: Побудова графу задачі з дерева
	/// </summary>
	private (List<OperationTask> Tasks, HashSet<string> Operands, int SequentialTime) BuildTaskGraph(TreeNode root)
	{
		var tasks = new List<OperationTask>();
		var operands = new HashSet<string>();
		var nodeMap = new Dictionary<TreeNode, string>(); // Мапує вузол на ID його результату
		int opCounter = 0;
		int sequentialTime = 0;

		// Рекурсивна функція для обходу дерева
		void PostOrderTraverse(TreeNode node)
		{
			if (node == null) return;

			PostOrderTraverse(node.Left);
			PostOrderTraverse(node.Right);

			if (node.IsOperation())
			{
				opCounter++;
				string id = $"{node.Value}_{opCounter}";
				int duration = _operationTimes[node.Value];
				sequentialTime += duration;

				var task = new OperationTask(id, node.Value, duration);

				if (node.Left != null)
					task.DependencyIds.Add(nodeMap[node.Left]);
				if (node.Right != null)
					task.DependencyIds.Add(nodeMap[node.Right]);

				tasks.Add(task);
				nodeMap[node] = id;
			}
			else if (node.IsOperand())
			{
				operands.Add(node.Value);
				nodeMap[node] = node.Value;
			}
		}

		PostOrderTraverse(root);
		return (tasks, operands, sequentialTime);
	}

	/// <summary>
	/// Крок 4: Групування вершин (за рівнем, потім за типом)
	/// </summary>
	private (List<List<OperationTask>> Stages, int MaxLevelWidth) GroupTasksForMatrixSystem(
		List<OperationTask> allTasks, HashSet<string> initialOperands)
	{
		var stages = new List<List<OperationTask>>();
		var taskLookup = allTasks.ToDictionary(t => t.Id);
		var processedTaskIds = new HashSet<string>(initialOperands);
		int maxLevelWidth = 0;

		while (processedTaskIds.Count < allTasks.Count + initialOperands.Count)
		{
			// Знаходимо всі задачі, чиї залежності вже виконані
			var readyTasks = allTasks
				.Where(t => !processedTaskIds.Contains(t.Id) &&
							t.DependencyIds.All(depId => processedTaskIds.Contains(depId)))
				.ToList();

			if (!readyTasks.Any())
			{
				// Можлива помилка (цикл) або всі задачі виконані
				break;
			}

			maxLevelWidth = Math.Max(maxLevelWidth, readyTasks.Count);

			// Групуємо задачі поточного рівня за типом (вимога матричної системи)
			var groupsByType = readyTasks.GroupBy(t => t.Type);

			foreach (var group in groupsByType)
			{
				var stageTasks = group.ToList();
				stages.Add(stageTasks);
				foreach (var task in stageTasks)
				{
					processedTaskIds.Add(task.Id);
				}
			}
		}

		return (stages, maxLevelWidth);
	}

	/// <summary>
	/// Крок 5: Розподіл операцій та моделювання
	/// </summary>
	private (int ParallelTime, List<ScheduledTask>[]) AssignTasksToProcessors(
		List<List<OperationTask>> executionStages, HashSet<string> operands)
	{
		// Стан системи
		var processorFreeTime = new int[_numProcessors]; // Коли процесор звільниться
		var taskLocations = new Dictionary<string, int>(); // Де (на якому P) лежить результат задачі
		var taskEndTimes = new Dictionary<string, int>();  // Коли результат буде готовий
		var ganttChart = Enumerable.Range(0, _numProcessors)
						   .Select(i => new List<ScheduledTask>())
						   .ToArray();

		// Ініціалізуємо початкові операнди (розподіляємо їх по процесорам)
		int i = 0;
		foreach (var operand in operands)
		{
			int procId = i % _numProcessors;
			taskLocations[operand] = procId;
			taskEndTimes[operand] = 0; // Доступні в час 0
			ganttChart[procId].Add(new ScheduledTask($"Var({operand})", 0, 0));
			i++;
		}

		// Моделюємо етап за етапом
		foreach (var stage in executionStages)
		{
			// Використовуємо тимчасові сховища, щоб задачі одного етапу
			// не впливали на розрахунки залежностей одна одної
			var stageEndTimes = new Dictionary<string, int>();
			var stageLocations = new Dictionary<string, int>();

			for (int taskIndex = 0; taskIndex < stage.Count; taskIndex++)
			{
				var task = stage[taskIndex];
				int processorId = taskIndex % _numProcessors; // Призначення процесора (round-robin)

				// 1. Знаходимо час, коли всі залежності будуть доступні на цьому процесорі
				int maxReadyTime = 0;
				foreach (var depId in task.DependencyIds)
				{
					int sourceProcessor = taskLocations[depId];
					int sourceReadyTime = taskEndTimes[depId];
					int transferDelay = 0;

					if (sourceProcessor != processorId)
					{
						// РОЗРАХУНОК ДЛЯ ТОПОЛОГІЇ "КІЛЬЦЕ"
						int distance = CalculateRingDistance(sourceProcessor, processorId, _numProcessors);
						transferDelay = distance * _sendReceiveHopTime;

						// Додаємо візуалізацію пересилки
						ganttChart[sourceProcessor].Add(new ScheduledTask($"Send({depId})", sourceReadyTime, sourceReadyTime + transferDelay));
						ganttChart[processorId].Add(new ScheduledTask($"Recv({depId})", sourceReadyTime, sourceReadyTime + transferDelay));
					}

					int arrivalTime = sourceReadyTime + transferDelay;
					maxReadyTime = Math.Max(maxReadyTime, arrivalTime);
				}

				// 2. Розраховуємо час старту та фінішу задачі
				int startTime = Math.Max(processorFreeTime[processorId], maxReadyTime);
				int endTime = startTime + task.Duration;

				// 3. Оновлюємо стан системи (для наступних етапів)
				processorFreeTime[processorId] = endTime;
				stageLocations[task.Id] = processorId;
				stageEndTimes[task.Id] = endTime;

				// 4. Зберігаємо результати для діаграми Ганта
				task.ProcessorId = processorId;
				task.StartTime = startTime;
				task.EndTime = endTime;
				ganttChart[processorId].Add(new ScheduledTask(task.Id, startTime, endTime));
			}

			// Оновлюємо глобальний стан після завершення всього етапу
			foreach (var (id, loc) in stageLocations) taskLocations[id] = loc;
			foreach (var (id, time) in stageEndTimes) taskEndTimes[id] = time;
		}

		int parallelTime = processorFreeTime.Max();
		return (parallelTime, ganttChart);
	}

	/// <summary>
	/// Розрахунок відстані для топології "Кільце"
	/// </summary>
	private int CalculateRingDistance(int p1, int p2, int n)
	{
		int diff = Math.Abs(p1 - p2);
		return Math.Min(diff, n - diff);
	}

	/// <summary>
	/// Крок 6: Обчислення метрик
	/// </summary>
	private ModelingResult CalculateMetrics(double sequentialTime, double parallelTime, int maxLevelWidth)
	{
		double speedup = sequentialTime / parallelTime;
		int activeProcessors = maxLevelWidth; // "Ідеальна" кількість
		int totalProcessors = _numProcessors;

		return new ModelingResult
		{
			SequentialTime = sequentialTime,
			ParallelTime = parallelTime,
			Speedup = speedup,
			ActiveProcessors = activeProcessors,
			TotalProcessors = totalProcessors,
			EfficiencyActive = speedup / activeProcessors,
			EfficiencyTotal = speedup / totalProcessors
		};
	}
}