using ExpressionAnalyzer.Models;
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

	public ModelingResult Simulate(TreeNode rootNode)
	{
		var (allTasks, operands, sequentialTime) = BuildTaskGraph(rootNode);

		var (executionStages, maxLevelWidth) = GroupTasksForMatrixSystem(allTasks, operands);

		Console.WriteLine("Етапи виконання");
		int stageNum = 0;
		foreach (var stage in executionStages)
		{
			Console.WriteLine($"Етап {stageNum++}: [{string.Join(", ", stage.Select(t => t.Id))}]");
		}

		var (parallelTime, ganttChart) = AssignTasksToProcessors(executionStages, operands);

		var result = CalculateMetrics(sequentialTime, parallelTime, ganttChart.Count(x => x.Any()));
		result.GanttChart = ganttChart;

		return result;
	}

	private (List<OperationTask> Tasks, HashSet<string> Operands, int SequentialTime) BuildTaskGraph(TreeNode root)
	{
		var tasks = new List<OperationTask>();
		var operands = new HashSet<string>();
		var nodeMap = new Dictionary<TreeNode, string>(); 
		int opCounter = 0;
		int sequentialTime = 0;

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

	private (List<List<OperationTask>> Stages, int MaxLevelWidth) GroupTasksForMatrixSystem(
		List<OperationTask> allTasks, HashSet<string> initialOperands)
	{
		var stages = new List<List<OperationTask>>();
		var taskLookup = allTasks.ToDictionary(t => t.Id);
		var processedTaskIds = new HashSet<string>(initialOperands);
		int maxLevelWidth = 0;

		while (processedTaskIds.Count < allTasks.Count + initialOperands.Count)
		{
			var readyTasks = allTasks
				.Where(t => !processedTaskIds.Contains(t.Id) &&
							t.DependencyIds.All(depId => processedTaskIds.Contains(depId)))
				.ToList();

			if (!readyTasks.Any())
			{
				break;
			}

			maxLevelWidth = Math.Max(maxLevelWidth, readyTasks.Count);

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

	private (int ParallelTime, List<ScheduledTask>[]) AssignTasksToProcessors(
		List<List<OperationTask>> executionStages, HashSet<string> operands)
	{
		var processorFreeTime = new int[_numProcessors]; 
		var taskLocations = new Dictionary<string, int>();
		var taskEndTimes = new Dictionary<string, int>();
		var ganttChart = Enumerable.Range(0, _numProcessors)
						   .Select(i => new List<ScheduledTask>())
						   .ToArray();

		int i = 0;
		foreach (var operand in operands)
		{
			int procId = i % _numProcessors;
			taskLocations[operand] = procId;
			taskEndTimes[operand] = 0; 
			i++;
		}

		foreach (var stage in executionStages)
		{
			var stageEndTimes = new Dictionary<string, int>();
			var stageLocations = new Dictionary<string, int>();

			for (int taskIndex = 0; taskIndex < stage.Count; taskIndex++)
			{
				var task = stage[taskIndex];
				int processorId = taskIndex % _numProcessors; 

				int maxReadyTime = 0;
				foreach (var depId in task.DependencyIds)
				{
					int sourceProcessor = taskLocations[depId];
					int sourceReadyTime = taskEndTimes[depId];
					int transferDelay = 0;

					if (sourceProcessor != processorId)
					{
						int distance = CalculateRingDistance(sourceProcessor, processorId, _numProcessors);
						transferDelay = distance * _sendReceiveHopTime;

						ganttChart[sourceProcessor].Add(new ScheduledTask($"s", sourceReadyTime, sourceReadyTime + transferDelay));
						ganttChart[processorId].Add(new ScheduledTask($"r", sourceReadyTime, sourceReadyTime + transferDelay));
					}

					int arrivalTime = sourceReadyTime + transferDelay;
					maxReadyTime = Math.Max(maxReadyTime, arrivalTime);
				}

				int startTime = Math.Max(processorFreeTime[processorId], maxReadyTime);
				int endTime = startTime + task.Duration;

				processorFreeTime[processorId] = endTime;
				stageLocations[task.Id] = processorId;
				stageEndTimes[task.Id] = endTime;

				task.ProcessorId = processorId;
				task.StartTime = startTime;
				task.EndTime = endTime;
				ganttChart[processorId].Add(new ScheduledTask(task.Id, startTime, endTime));
			}

			foreach (var (id, loc) in stageLocations) taskLocations[id] = loc;
			foreach (var (id, time) in stageEndTimes) taskEndTimes[id] = time;
		}

		int parallelTime = processorFreeTime.Max();
		return (parallelTime, ganttChart);
	}

	private int CalculateRingDistance(int p1, int p2, int n)
	{
		int diff = Math.Abs(p1 - p2);
		return Math.Min(diff, n - diff);
	}

	private ModelingResult CalculateMetrics(double sequentialTime, double parallelTime, int maxLevelWidth)
	{
		double speedup = sequentialTime / parallelTime;
		int activeProcessors = maxLevelWidth;
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