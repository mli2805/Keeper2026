using Caliburn.Micro;
using KeeperDomain;
using KeeperInfrastructure;
using KeeperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KeeperWpf;

[ExportViewModel]
public class ToDoViewModel(KeeperDataModel dataModel, TodoTaskRepository todoTaskRepository) : Screen
{
    public BindableCollection<TodoTaskModel> TodoTasks { get; set; } = [];
    public BindableCollection<TodoSubtaskModel> Subtasks { get; set; } = [];

    private string? _cleanTaskSnapshot;
    private TodoTaskModel? _cleanTaskModel;
    private Task _pendingSaveTask = Task.CompletedTask;

    private bool _hasUnsavedChanges;
    public bool HasUnsavedChanges
    {
        get => _hasUnsavedChanges;
        private set
        {
            if (value == _hasUnsavedChanges) return;
            _hasUnsavedChanges = value;
            NotifyOfPropertyChange();
            NotifyOfPropertyChange(nameof(CanSaveTask));
            NotifyOfPropertyChange(nameof(CanCancelTask));
        }
    }

    private TodoTaskModel? _selectedTask;
    public TodoTaskModel? SelectedTask
    {
        get => _selectedTask;
        set
        {
            if (Equals(value, _selectedTask)) return;
            SavePendingSelectionChanges();
            _selectedTask = value;
            RefreshSubtasks();
            ResetDirtyState();
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

    public List<TodoImportance> ImportanceLevels { get; } = Enum.GetValues(typeof(TodoImportance)).OfType<TodoImportance>().ToList();

    public bool CanSaveTask => SelectedTask != null && HasUnsavedChanges;
    public bool CanCancelTask => SelectedTask != null && HasUnsavedChanges;
    public bool CanDeleteTask => SelectedTask != null;
    public bool CanAddSubtask => SelectedTask != null;
    public bool CanDeleteSubtask => SelectedTask != null && SelectedSubtask != null;
    public bool CanMoveSubtaskUp => SelectedSubtask != null && Subtasks.IndexOf(SelectedSubtask) > 0;
    public bool CanMoveSubtaskDown => SelectedSubtask != null && Subtasks.IndexOf(SelectedSubtask) >= 0 && Subtasks.IndexOf(SelectedSubtask) < Subtasks.Count - 1;
    public bool CanToggleTaskCompletion => SelectedTask != null;
    public string TaskCompletionButtonCaption => SelectedTask?.IsCompleted == true ? "Открыть задачу" : "Завершить задачу";

    private void Initialize()
    {
        RefreshTasks();
    }

    protected override void OnViewLoaded(object view)
    {
        base.OnViewLoaded(view);
        DisplayName = "ToDo";
        Initialize();
    }

    private void RefreshTasks(int? selectedTaskId = null)
    {
        selectedTaskId ??= SelectedTask?.Id;
        var tasks = dataModel.TodoTasks;
        var filtered = tasks.Where(t => ShowCompleted || !t.IsCompleted);

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = filtered.Where(t => t.SearchText.Contains(SearchText, StringComparison.CurrentCultureIgnoreCase));
        }

        filtered = filtered
            .OrderBy(t => t.IsCompleted)
            .ThenByDescending(t => t.Importance)
            .ThenByDescending(t => t.CreatedAt)
            .ThenBy(t => t.Title);

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
        NotifyOfPropertyChange(nameof(CanCancelTask));
        NotifyOfPropertyChange(nameof(CanDeleteTask));
        NotifyOfPropertyChange(nameof(CanAddSubtask));
        NotifyOfPropertyChange(nameof(CanDeleteSubtask));
        NotifyOfPropertyChange(nameof(CanMoveSubtaskUp));
        NotifyOfPropertyChange(nameof(CanMoveSubtaskDown));
        NotifyOfPropertyChange(nameof(CanToggleTaskCompletion));
        NotifyOfPropertyChange(nameof(TaskCompletionButtonCaption));
    }

    public void MarkDirty()
    {
        if (SelectedTask == null) return;

        HasUnsavedChanges = !CurrentTaskMatchesSnapshot();
        TodoTasks.Refresh();
    }

    private void ResetDirtyState()
    {
        _cleanTaskSnapshot = SelectedTask == null ? null : BuildTaskSnapshot(SelectedTask, Subtasks);
        _cleanTaskModel = SelectedTask == null ? null : CloneTask(SelectedTask, Subtasks);
        HasUnsavedChanges = false;
    }

    private bool CurrentTaskMatchesSnapshot()
    {
        return SelectedTask != null && _cleanTaskSnapshot == BuildTaskSnapshot(SelectedTask, Subtasks);
    }

    private static string BuildTaskSnapshot(TodoTaskModel task, IEnumerable<TodoSubtaskModel> subtasks)
    {
        var builder = new StringBuilder();
        builder.Append(task.Id).Append('|')
            .Append(task.Title).Append('|')
            .Append(task.Importance).Append('|')
            .Append(task.IsCompleted).Append('|')
            .Append(task.CompletedAt?.ToString() ?? string.Empty);

        foreach (var subtask in subtasks)
        {
            builder.Append("||")
                .Append(subtask.Id).Append('|')
                .Append(subtask.TodoTaskId).Append('|')
                .Append(subtask.Ordinal).Append('|')
                .Append(subtask.Title).Append('|')
                .Append(subtask.IsCompleted);
        }

        return builder.ToString();
    }

    private void SavePendingSelectionChanges()
    {
        if (SelectedTask == null || !HasUnsavedChanges) return;

        var taskToSave = CloneTask(SelectedTask, Subtasks);
        _pendingSaveTask = SavePendingSelectionChangesAsync(taskToSave);
    }

    private async Task SavePendingSelectionChangesAsync(TodoTaskModel taskToSave)
    {
        await _pendingSaveTask;
        var savedTask = await SaveTaskCore(taskToSave, taskToSave.Subtasks);
        await Execute.OnUIThreadAsync(() =>
        {
            RefreshTasks(savedTask.Id);
            return Task.CompletedTask;
        });
    }

    private static TodoTaskModel CloneTask(TodoTaskModel task, IEnumerable<TodoSubtaskModel> subtasks)
    {
        return new TodoTaskModel
        {
            Id = task.Id,
            Title = task.Title,
            CreatedAt = task.CreatedAt,
            CompletedAt = task.CompletedAt,
            Importance = task.Importance,
            IsCompleted = task.IsCompleted,
            Subtasks = subtasks.Select(CloneSubtask).ToList()
        };
    }

    private static TodoSubtaskModel CloneSubtask(TodoSubtaskModel subtask)
    {
        return new TodoSubtaskModel
        {
            Id = subtask.Id,
            TodoTaskId = subtask.TodoTaskId,
            Ordinal = subtask.Ordinal,
            Title = subtask.Title,
            IsCompleted = subtask.IsCompleted
        };
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
        if (SelectedTask == null || !HasUnsavedChanges) return;

        var savedTask = await SaveTaskCore(SelectedTask, Subtasks);
        RefreshTasks(savedTask.Id);
        ResetDirtyState();
    }

    public void CancelTask()
    {
        if (SelectedTask == null || !HasUnsavedChanges || _cleanTaskModel == null) return;

        var taskToRestore = CloneTask(_cleanTaskModel, _cleanTaskModel.Subtasks);
        var idx = dataModel.TodoTasks.FindIndex(t => t.Id == taskToRestore.Id);
        if (idx >= 0)
        {
            dataModel.TodoTasks[idx] = taskToRestore;
        }

        HasUnsavedChanges = false;
        RefreshTasks(taskToRestore.Id);
        ResetDirtyState();
    }

    private async Task<TodoTaskModel> SaveTaskCore(TodoTaskModel task, IEnumerable<TodoSubtaskModel> subtasks)
    {
        var taskSubtasks = subtasks.ToList();

        foreach (var subtask in taskSubtasks)
        {
            subtask.TodoTaskId = task.Id;
        }
        task.Subtasks = [.. taskSubtasks];
        task.NormalizeCompletion(DateOnly.FromDateTime(DateTime.Now));

        var savedTask = task.Id == 0
            ? await todoTaskRepository.Add(task)
            : await todoTaskRepository.Update(task);

        var idx = dataModel.TodoTasks.FindIndex(t => t.Id == savedTask.Id);
        if (idx >= 0)
        {
            dataModel.TodoTasks[idx] = savedTask;
        }
        else
        {
            dataModel.TodoTasks.Add(savedTask);
        }

        return savedTask;
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
        MarkDirty();
    }

    public void DeleteSubtask()
    {
        if (SelectedTask == null || SelectedSubtask == null) return;

        Subtasks.Remove(SelectedSubtask);
        SelectedSubtask = Subtasks.FirstOrDefault();
        SyncSelectedTaskSubtasks();
        MarkDirty();
    }

    public void MoveSubtaskUp()
    {
        if (SelectedSubtask == null) return;

        var idx = Subtasks.IndexOf(SelectedSubtask);
        if (idx <= 0) return;

        Subtasks.Move(idx, idx - 1);
        SelectedSubtask = Subtasks[idx - 1];
        SyncSelectedTaskSubtasks();
        MarkDirty();
    }

    public void MoveSubtaskDown()
    {
        if (SelectedSubtask == null) return;

        var idx = Subtasks.IndexOf(SelectedSubtask);
        if (idx < 0 || idx >= Subtasks.Count - 1) return;

        Subtasks.Move(idx, idx + 1);
        SelectedSubtask = Subtasks[idx + 1];
        SyncSelectedTaskSubtasks();
        MarkDirty();
    }

    public void SubtaskCompletionChanged()
    {
        if (SelectedTask == null) return;

        SyncSelectedTaskSubtasks();
        MarkDirty();
    }

    public void ToggleTaskCompletion()
    {
        if (SelectedTask == null) return;

        SelectedTask.IsCompleted = !SelectedTask.IsCompleted;
        SelectedTask.NormalizeCompletion(DateOnly.FromDateTime(DateTime.Now));
        NotifyOfPropertyChange(nameof(SelectedTask));
        NotifyOfPropertyChange(nameof(TaskCompletionButtonCaption));
        TodoTasks.Refresh();
        MarkDirty();
    }

    public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken = default)
    {
        await _pendingSaveTask;
        await SaveTask();

        return await base.CanCloseAsync(cancellationToken);
    }
}

