using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace TreeViewScrollingIssue
{
  public static class MyRandom
  {
    public static Random Random = new Random();
  }
  public abstract class PropertyChangedBase : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string name = "")
    {
      var changed = !EqualityComparer<T>.Default.Equals(field, value);
      if (changed)
      {
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
      }
      return changed;
    }
  }

  public class RelayCommand : ICommand
  {
    private Action<object> execute;
    private Func<object, bool> canExecute;

    public event EventHandler CanExecuteChanged
    {
      add => CommandManager.RequerySuggested += value;
      remove => CommandManager.RequerySuggested -= value;
    }

    public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
    {
      this.execute = execute;
      this.canExecute = canExecute;
    }

    public bool CanExecute(object parameter)
    {
      return canExecute == null || canExecute(parameter);
    }

    public void Execute(object parameter)
    {
      execute(parameter);
    }
  }

  internal class BindableSelectedItemBehavior : Behavior<TreeView>
  {
    #region SelectedItem Property

    public object SelectedItem
    {
      get => (object)GetValue(SelectedItemProperty);
      set => SetValue(SelectedItemProperty, value);
    }

    public static readonly DependencyProperty SelectedItemProperty =
      DependencyProperty.Register("SelectedItem", typeof(object), typeof(BindableSelectedItemBehavior), new UIPropertyMetadata(null, OnSelectedItemChanged));

    private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
      var item = e.NewValue as TreeViewItem;
      if (item != null)
      {
        item.SetValue(TreeViewItem.IsSelectedProperty, true);
      }
    }

    #endregion

    protected override void OnAttached()
    {
      base.OnAttached();

      this.AssociatedObject.SelectedItemChanged += OnTreeViewSelectedItemChanged;
    }

    protected override void OnDetaching()
    {
      base.OnDetaching();

      if (this.AssociatedObject != null)
      {
        this.AssociatedObject.SelectedItemChanged -= OnTreeViewSelectedItemChanged;
      }
    }

    private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
      this.SelectedItem = e.NewValue;
    }
  }

  internal class Util
  {
    public static async void NonAwaitCall(Func<Task> function)
    {
      try
      {
        if (function != null)
        {
          var task = function();
          if (task != null)
            await task;
        }
      }
      catch (Exception)
      {
        // ignored
      }
    }
  }
}
