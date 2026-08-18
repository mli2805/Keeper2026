using KeeperInfrastructure;
using KeeperModels;
using KeeperWpf;

namespace Tests.Keeper;

[TestClass]
public sealed class ToDoViewModelTests
{
    [TestMethod]
    public async Task AddTwoSubtasks()
    {
        var factory = DbTestHelper.CreateIsolatedFactory();
        var todoTaskRepository = new TodoTaskRepository(factory);

        var tasks = await todoTaskRepository.GetAllTodoTasksWithSubtasks();
        if (tasks.Count == 0)
        {
            await todoTaskRepository.Add(new TodoTaskModel
            {
                Title = "Existing task for subtasks test",
                CreatedAt = DateOnly.FromDateTime(DateTime.Now)
            });

            tasks = await todoTaskRepository.GetAllTodoTasksWithSubtasks();
        }

        var existingTask = tasks.First();
        var initialSubtasksCount = existingTask.Subtasks.Count;
        var firstSubtaskTitle = $"First subtask {Guid.NewGuid():N}";
        var secondSubtaskTitle = $"Second subtask {Guid.NewGuid():N}";

        var dataModel = new KeeperDataModel
        {
            TodoTasks = tasks
        };

        var viewModel = new ToDoViewModel(dataModel, todoTaskRepository)
        {
            SelectedTask = dataModel.TodoTasks.First(t => t.Id == existingTask.Id)
        };

        viewModel.AddSubtask();
        Assert.IsNotNull(viewModel.SelectedSubtask);
        viewModel.SelectedSubtask.Title = firstSubtaskTitle;
        viewModel.MarkDirty();

        viewModel.AddSubtask();
        Assert.IsNotNull(viewModel.SelectedSubtask);
        viewModel.SelectedSubtask.Title = secondSubtaskTitle;
        viewModel.MarkDirty();

        await viewModel.SaveTask();
        await viewModel.CanCloseAsync();

        var tasksFromDb = await todoTaskRepository.GetAllTodoTasksWithSubtasks();
        var savedTask = tasksFromDb.First(t => t.Id == existingTask.Id);

        Assert.HasCount(initialSubtasksCount + 2, savedTask.Subtasks, "Two new subtasks should be saved to database");
        Assert.IsTrue(savedTask.Subtasks.Any(s => s.Title == firstSubtaskTitle), "First subtask should be saved to database");
        Assert.IsTrue(savedTask.Subtasks.Any(s => s.Title == secondSubtaskTitle), "Second subtask should be saved to database");
    }
}
