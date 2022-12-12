using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using tomb3_ConfigTool.Controls;
using tomb3_ConfigTool.Utils;

namespace tomb3_ConfigTool.Models;

public class MainWindowViewModel : BaseLanguageViewModel
{
    private readonly Configuration _configuration;

    public IEnumerable<CategoryViewModel> Categories { get; private set; }

    public MainWindowViewModel()
    {
        _configuration = new Configuration();

        List<CategoryViewModel> categories = new();
        foreach (Category category in _configuration.Categories)
        {
            categories.Add(new CategoryViewModel(category));
            foreach (BaseProperty property in category.Properties)
            {
                property.PropertyChanged += EditorPropertyChanged;
            }
        }

        Categories = categories;
        SelectedCategory = Categories.FirstOrDefault();
    }

    private void EditorPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        IsEditorDirty = _configuration.IsDataDirty();
        IsEditorDefault = _configuration.IsDataDefault();
    }

    public void Load()
    {
        _configuration.Read();
        IsEditorDirty = false;
        IsEditorDefault = _configuration.IsDataDefault();
    }

    private CategoryViewModel _selectedCategory;
    public CategoryViewModel SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            _selectedCategory = value;
            NotifyPropertyChanged();
        }
    }

    private bool _isEditorDirty;
    public bool IsEditorDirty
    {
        get => _isEditorDirty;
        set
        {
            if (_isEditorDirty != value)
            {
                _isEditorDirty = value;
                NotifyPropertyChanged();
            }
        }
    }

    private bool _isEditorDefault;
    public bool IsEditorDefault
    {
        get => _isEditorDefault;
        set
        {
            if (_isEditorDefault != value)
            {
                _isEditorDefault = value;
                NotifyPropertyChanged();
            }
        }
    }

    private RelayCommand _importCommand;
    public ICommand ImportCommand
    {
        get => _importCommand ??= new RelayCommand(Import);
    }

    private void Import()
    {
        if (!ConfirmEditorSaveState())
        {
            return;
        }

        OpenFileDialog dialog = new()
        {
            Filter = ViewText["file_dialog_filter"] + Tomb3Config.ConfigFilterExtension
        };
        if (dialog.ShowDialog() ?? false)
        {
            try
            {
                _configuration.Import(dialog.FileName);
            }
            catch (Exception e)
            {
                MessageBoxUtils.ShowError(e.ToString(), ViewText["window_title_main"]);
            }
        }
    }

    private RelayCommand _exportCommand;
    public ICommand ExportCommand
    {
        get => _exportCommand ??= new RelayCommand(Export);
    }

    private void Export()
    {
        SaveFileDialog dialog = new()
        {
            Filter = ViewText["file_dialog_filter"] + Tomb3Config.ConfigFilterExtension
        };
        if (dialog.ShowDialog() ?? false)
        {
            try
            {
                _configuration.Export(dialog.FileName);
            }
            catch (Exception e)
            {
                MessageBoxUtils.ShowError(e.ToString(), ViewText["window_title_main"]);
            }
        }
    }

    private RelayCommand _reloadCommand;
    public ICommand ReloadCommand
    {
        get => _reloadCommand ??= new RelayCommand(Reload);
    }

    private void Reload()
    {
        if (ConfirmEditorReloadState())
        {
            Load();
        }
    }

    private RelayCommand _saveCommand;
    public ICommand SaveCommand
    {
        get => _saveCommand ??= new RelayCommand(Save, CanSave);
    }

    private void Save()
    {
        try
        {
            _configuration.Write();
            IsEditorDirty = false;
        }
        catch (Exception e)
        {
            MessageBoxUtils.ShowError(e.ToString(), ViewText["window_title_main"]);
        }
    }

    private bool CanSave()
    {
        return IsEditorDirty;
    }

    private RelayCommand _launchSetupCommand, _launchGameCommand, _launchGoldCommand;
    public ICommand LaunchSetupCommand
    {
        get => _launchSetupCommand ??= new RelayCommand(LaunchSetup);
    }

    public ICommand LaunchGameCommand
    {
        get => _launchGameCommand ??= new RelayCommand(LaunchGame);
    }

    public ICommand LaunchGoldCommand
    {
        get => _launchGoldCommand ??= new RelayCommand(LaunchGoldGame);
    }

    private void LaunchGame()
    {
        LaunchGameExe();
    }

    private void LaunchSetup()
    {
        LaunchGameExe(Tomb3Config.SetupArgs);
    }

    private void LaunchGoldGame()
    {
        LaunchGameExe(Tomb3Config.GoldArgs);
    }

    private void LaunchGameExe(string arguments = null)
    {
        if (!ConfirmEditorSaveState())
        {
            return;
        }

        try
        {
            ProcessUtils.Start(Path.GetFullPath(Tomb3Config.ExecutableName), arguments);
        }
        catch (Exception e)
        {
            MessageBoxUtils.ShowError(e.ToString(), ViewText["window_title_main"]);
        }
    }

    private RelayCommand<Window> _exitCommand;
    public ICommand ExitCommand
    {
        get => _exitCommand ??= new RelayCommand<Window>(Exit);
    }

    private void Exit(Window window)
    {
        if (ConfirmEditorSaveState())
        {
            IsEditorDirty = false;
            window.Close();
        }
    }

    public void Exit(CancelEventArgs e)
    {
        if (!ConfirmEditorSaveState())
        {
            e.Cancel = true;
        }
    }

    public bool ConfirmEditorSaveState()
    {
        if (IsEditorDirty)
        {
            switch (MessageBoxUtils.ShowYesNoCancel(ViewText["msgbox_unsaved_changes"], ViewText["window_title_main"]))
            {
                case MessageBoxResult.Yes:
                    Save();
                    break;
                case MessageBoxResult.Cancel:
                    return false;
            }
        }
        return true;
    }

    public bool ConfirmEditorReloadState()
    {
        return !IsEditorDirty
            || MessageBoxUtils.ShowYesNo(ViewText["msgbox_unsaved_changes_reload"], ViewText["window_title_main"]);
    }

    private RelayCommand _restoreDefaultsCommand;
    public ICommand RestoreDefaultsCommand
    {
        get => _restoreDefaultsCommand ??= new RelayCommand(RestoreDefaults, CanRestoreDefaults);
    }

    private void RestoreDefaults()
    {
        _configuration.RestoreDefaults();
    }

    private bool CanRestoreDefaults()
    {
        return !IsEditorDefault;
    }

    private RelayCommand _texturesCommand;
    public ICommand TexturesCommand
    {
        get => _texturesCommand ??= new RelayCommand(LaunchTextureTool);
    }

    private void LaunchTextureTool()
    {
        string launchPath = Path.GetFullPath(Tomb3Config.TextureExecutableName);
        if (!File.Exists(launchPath))
        {
            launchPath = Tomb3Config.TextureGitHubURL;
        }
        try
        {
            ProcessUtils.Start(launchPath);
        }
        catch (Exception e)
        {
            MessageBoxUtils.ShowError(e.ToString(), ViewText["window_title_main"]);
        }
    }

    private RelayCommand _gitHubCommand;
    public ICommand GitHubCommand
    {
        get => _gitHubCommand ??= new RelayCommand(GoToGitHub);
    }

    private void GoToGitHub()
    {
        ProcessUtils.Start(Tomb3Config.GitHubURL);
    }

    private RelayCommand _aboutCommand;
    public ICommand AboutCommand
    {
        get => _aboutCommand ??= new RelayCommand(ShowAboutDialog);
    }

    private void ShowAboutDialog()
    {
        new AboutWindow().ShowDialog();
    }
}
