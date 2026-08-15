using Caliburn.Micro;
using KeeperDomain;
using KeeperInfrastructure;
using KeeperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KeeperWpf;

[ExportViewModel]
public class ToDoViewModel(KeeperDataModel dataModel, TodoTaskRepository todoTaskRepository) : Screen
{
    public BindableCollection<TodoTaskModel> TodoTasks { get; set; } = [];
    public BindableCollection<TodoSubtaskModel> Subtasks { get; set; } = [];

    private TodoTaskModel? _selectedTask;
    public TodoTaskModel? SelectedTask
    {
        get => _selectedTask;
        set
        {
            if (Equals(value, _selectedTask)) return;
            _selectedTask = value;
            RefreshSubtasks();
            NotifyOfPropertyChange();
            NotifyTaskCommandStates();
        }
    }

    private TodoSubtaskModel? _selectedSubtask;
    public TodoSubtaskModel? SelectedSubtask
    {
        get => _selectedSubtask;
        set
        {
            if (Equals(value, _selectedSubtask)) return;
            _selectedSubtask = value;
            NotifyOfPropertyChange();
            NotifyOfPropertyChange(nameof(CanDeleteSubtask));
            NotifyOfPropertyChange(nameof(CanMoveSubtaskUp));
            NotifyOfPropertyChange(nameof(CanMoveSubtaskDown));
        }
    }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (value == _searchText) return;
            _searchText = value;
            NotifyOfPropertyChange();
            RefreshTasks();
        }
    }

    private bool _showCompleted = true;
    public bool ShowCompleted
    {
        get => _showCompleted;
        set
        {
            if (value == _showCompleted) return;
            _showCompleted = value;
            NotifyOfPropertyChange();
            RefreshTasks();
        }
    }

    private TodoTaskSortMode _selectedSortMode = TodoTaskSortMode.CreatedAt;
    public TodoTaskSortMode SelectedSortMode
    {
        get => _selectedSortMode;
        set
        {
            if (value == _selectedSortMode) return;
            _selectedSortMode = value;
            NotifyOfPropertyChange();
            RefreshTasks();
        }
    }

    public List<TodoTaskSortMode> SortModes { get; } = Enum.GetValues(typeof(TodoTaskSortMode)).OfType<TodoTaskSortMode>().ToList();
    public List<TodoImportance> ImportanceLevels { get; } = Enum.GetValues(typeof(TodoImportance)).OfType<TodoImportance>().ToList();

    public bool CanSaveTask => SelectedTask != null;
    public bool CanDeleteTask => SelectedTask != null;
    public bool CanAddSubtask => SelectedTask != null;
    public bool CanDeleteSubtask => SelectedTask != null && SelectedSubtask != null;
    public bool CanMoveSubtaskUp => SelectedSubtask != null && Subtasks.IndexOf(SelectedSubtask) > 0;
    public bool CanMoveSubtaskDown => SelectedSubtask != null && Subtasks.IndexOf(SelectedSubtask) >= 0 && Subtasks.IndexOf(SelectedSubtask) < Subtasks.Count - 1;

    public void Initialize()
    {
        dataModel.TodoTasks ??= [];
        RefreshTasks();
    }

    override protected void OnViewLoaded(object view)
    {
        base.OnViewLoaded(view);
        DisplayName = "ToDo";
        Initialize();
    }

    private void RefreshTasks(int? selectedTaskId = null)
    {
        selectedTaskId ??= SelectedTask?.Id;
        var tasks = dataModel.TodoTasks ?? [];
        var filtered = tasks.Where(t => ShowCompleted || !t.IsCompleted);

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = filtered.Where(t => t.SearchText.Contains(SearchText, StringComparison.CurrentCultureIgnoreCase));
        }

        filtered = SelectedSortMode switch
        {
            TodoTaskSortMode.CompletedAt => filtered.OrderBy(t => t.IsCompleted).ThenBy(t => t.CompletedAt ?? DateOnly.MaxValue).ThenBy(t => t.Title),
            TodoTaskSortMode.Importance => filtered.OrderBy(t => t.IsCompleted).ThenByDescending(t => t.Importance).ThenBy(t => t.Title),
            TodoTaskSortMode.Title => filtered.OrderBy(t => t.IsCompleted).ThenBy(t => t.Title),
            TodoTaskSortMode.CompletionStatus => filtered.OrderBy(t => t.IsCompleted).ThenByDescending(t => t.Importance).ThenBy(t => t.Title),
            _ => filtered.OrderBy(t => t.IsCompleted).ThenByDescending(t => t.CreatedAt).ThenByDescending(t => t.Importance).ThenBy(t => t.Title)
        };

        TodoTasks.Clear();
        TodoTasks.AddRange(filtered);
        SelectedTask = selectedTaskId.HasValue
            ? TodoTasks.FirstOrDefault(t => t.Id == selectedTaskId.Value) ?? TodoTasks.FirstOrDefault()
            : TodoTasks.FirstOrDefault();
    }

    private void RefreshSubtasks()
    {
        Subtasks.Clear();
        if (SelectedTask != null)
        {
            Subtasks.AddRange(SelectedTask.Subtasks);
        }
        SelectedSubtask = Subtasks.FirstOrDefault();
    }

    private void NotifyTaskCommandStates()
    {
        NotifyOfPropertyChange(nameof(CanSaveTask));
        NotifyOfPropertyChange(nameof(CanDeleteTask));
        NotifyOfPropertyChange(nameof(CanAddSubtask));
        NotifyOfPropertyChange(nameof(CanDeleteSubtask));
        NotifyOfPropertyChange(nameof(CanMoveSubtaskUp));
        NotifyOfPropertyChange(nameof(CanMoveSubtaskDown));
    }

    private void SyncSelectedTaskSubtasks()
    {
        if (SelectedTask == null) return;

        for (int i = 0; i < Subtasks.Count; i++)
        {
            Subtasks[i].Ordinal = i;
        }
        SelectedTask.Subtasks = [.. Subtasks];
        SelectedTask.NormalizeCompletion(DateOnly.FromDateTime(DateTime.Now));
        TodoTasks.Refresh();
        NotifyOfPropertyChange(nameof(SelectedTask));
        NotifyTaskCommandStates();
    }

    public async Task AddTask()
    {
        var newTask = new TodoTaskModel
        {
            Title = "Новая задача",
            CreatedAt = DateOnly.FromDateTime(DateTime.Now),
            Importance = TodoImportance.Normal
        };

        var savedTask = await todoTaskRepository.Add(newTask);
        dataModel.TodoTasks.Add(savedTask);
        RefreshTasks(savedTask.Id);
    }

    public async Task SaveTask()
    {
        if (SelectedTask == null) return;

        foreach (var subtask in Subtasks)
        {
            subtask.TodoTaskId = SelectedTask.Id;
        }
        SelectedTask.Subtasks = [.. Subtasks];
        SelectedTask.NormalizeCompletion(DateOnly.FromDateTime(DateTime.Now));

        var savedTask = SelectedTask.Id == 0
            ? await todoTaskRepository.Add(SelectedTask)
            : await todoTaskRepository.Update(SelectedTask);

        var idx = dataModel.TodoTasks.FindIndex(t => t.Id == savedTask.Id);
        if (idx >= 0)
        {
            dataModel.TodoTasks[idx] = savedTask;
        }
        else
        {
            dataModel.TodoTasks.Add(savedTask);
        }

        RefreshTasks(savedTask.Id);
    }

    public async Task DeleteTask()
    {
        if (SelectedTask == null) return;

        var taskId = SelectedTask.Id;
        if (taskId != 0)
        {
            await todoTaskRepository.Delete(taskId);
        }

        var taskInDataModel = dataModel.TodoTasks.FirstOrDefault(t => t.Id == taskId);
        if (taskInDataModel != null)
        {
            dataModel.TodoTasks.Remove(taskInDataModel);
        }

        RefreshTasks();
    }

    public void AddSubtask()
    {
        if (SelectedTask == null) return;

        var subtask = new TodoSubtaskModel
        {
            TodoTaskId = SelectedTask.Id,
            Title = "Новая подзадача"
        };

        Subtasks.Add(subtask);
        SelectedSubtask = subtask;
        SyncSelectedTaskSubtasks();
    }

    public void DeleteSubtask()
    {
        if (SelectedTask == null || SelectedSubtask == null) return;

        Subtasks.Remove(SelectedSubtask);
        SelectedSubtask = Subtasks.FirstOrDefault();
        SyncSelectedTaskSubtasks();
    }

    public void MoveSubtaskUp()
    {
        if (SelectedSubtask == null) return;

        var idx = Subtasks.IndexOf(SelectedSubtask);
        if (idx <= 0) return;

        Subtasks.Move(idx, idx - 1);
        SelectedSubtask = Subtasks[idx - 1];
        SyncSelectedTaskSubtasks();
    }

    public void MoveSubtaskDown()
    {
        if (SelectedSubtask == null) return;

        var idx = Subtasks.IndexOf(SelectedSubtask);
        if (idx < 0 || idx >= Subtasks.Count - 1) return;

        Subtasks.Move(idx, idx + 1);
        SelectedSubtask = Subtasks[idx + 1];
        SyncSelectedTaskSubtasks();
    }

    public void SubtaskCompletionChanged()
    {
        if (SelectedTask == null) return;

        SyncSelectedTaskSubtasks();
    }

    public void TaskCompletionChanged()
    {
        if (SelectedTask == null) return;

        SelectedTask.NormalizeCompletion(DateOnly.FromDateTime(DateTime.Now));
        NotifyOfPropertyChange(nameof(SelectedTask));
        TodoTasks.Refresh();
    }

    public async Task CloseView()
    {
        if (SelectedTask != null)
        {
            await SaveTask();
        }
        await TryCloseAsync();
    }
}

public enum TodoTaskSortMode
{
    CreatedAt,
    CompletedAt,
    Importance,
    Title,
    CompletionStatus
}
