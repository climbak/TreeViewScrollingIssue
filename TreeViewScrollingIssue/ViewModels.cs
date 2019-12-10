using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace TreeViewScrollingIssue
{
  public class ItemViewModel : PropertyChangedBase
  {
    public ItemViewModel(string name)
    {
      _children = new ObservableCollection<ItemViewModel>();
      _childrenView = CollectionViewSource.GetDefaultView(_children);
      Name = name;
    }

    public string Name { get; }

    private ObservableCollection<ItemViewModel> _children;

    public ObservableCollection<ItemViewModel> Children
    {
      get => _children;
      set => SetProperty(ref _children, value);

    }

    private ICollectionView _childrenView;

    public ICollectionView ChildrenView
    {
      get => _childrenView;
      set => SetProperty(ref _childrenView, value);
    }

    private bool _isExpanded = false;
    public bool IsExpanded
    {
      get => _isExpanded;
      set => SetProperty(ref _isExpanded, value);
    }

    private bool _isSelected = false;
    public bool IsSelected
    {
      get => _isSelected;
      set => SetProperty(ref _isSelected, value);
    }

    public void Expand()
    {
      IsExpanded = true;
      foreach (var item in Children)
        item.Expand();
    }
  }

  public class BasicItemViewModel : ItemViewModel
  {
    public BasicItemViewModel(string name)
    : base (name)
    {
    }
  }

  public class ItemWithAttributesViewModel : ItemViewModel
  {
    public ItemWithAttributesViewModel(string name)
    : base(name)
    {
      _attributes = new ObservableCollection<AttributeViewModel>();
      BindingOperations.EnableCollectionSynchronization(_attributes, _attributesLock);
      _attributesView = CollectionViewSource.GetDefaultView(_attributes);

    }

    private ObservableCollection<AttributeViewModel> _attributes;

    public ObservableCollection<AttributeViewModel> Attributes
    {
      get => _attributes;
      set => SetProperty(ref _attributes, value);
    }

    private ICollectionView _attributesView;

    public ICollectionView AttributesView
    {
      get => _attributesView;
      set => SetProperty(ref _attributesView, value);
    }

    private bool _loaded = false;
    public async Task LoadAttributes()
    {
      if (_loaded)
        return;

      await UpdateValues();
      _loaded = true;
    }

    public async Task UpdateValues()
    {
      await Task.Delay(250);
      var count = MyRandom.Random.Next(2, 5);
      for (int i = 0; i < count; ++i)
      {
        var attribute = new AttributeViewModel($"Field{i}");
        AddAttribute(attribute);
      }
    }

    private object _attributesLock = new object();
    public void AddAttribute(AttributeViewModel attribute)
    {
      lock (_attributesLock)
        Attributes.Add(attribute);
    }
  }

  public class AttributeViewModel : PropertyChangedBase
  {
    public AttributeViewModel(string name)
    {
      Name = name;
    }

    public string Name { get; }
  }

  public class WindowViewModel : PropertyChangedBase
  {
    private ObservableCollection<ItemViewModel> _items = new ObservableCollection<ItemViewModel>();
    public ObservableCollection<ItemViewModel> Items => _items;

    private ICollectionView _itemsView;

    public ICollectionView ItemsView
    {
      get
      {
        if (_itemsView == null)
          _itemsView = CollectionViewSource.GetDefaultView(_items);

        return _itemsView;
      }
    }

    private object _selectedTreeItem;
    public object SelectedTreeItem
    {
      get => _selectedTreeItem;
      set
      {
        if (SetProperty(ref _selectedTreeItem, value))
        {
          SelectedItem = _selectedTreeItem as BasicItemViewModel;
          SelectedItemWithAttributes = _selectedTreeItem as ItemWithAttributesViewModel;
        }
      }
    }

    private BasicItemViewModel _selectedItem;
    public BasicItemViewModel SelectedItem { get => _selectedItem; set => SetProperty(ref _selectedItem, value); }

    private ItemWithAttributesViewModel _selectedItemWithAttributes;
    public ItemWithAttributesViewModel SelectedItemWithAttributes
    {
      get => _selectedItemWithAttributes;
      set
      {
        if (SetProperty(ref _selectedItemWithAttributes, value))
          Util.NonAwaitCall(() => _selectedItemWithAttributes?.LoadAttributes());
      }
    }

    private AttributeViewModel _selectedAttribute;
    public AttributeViewModel SelectedAttribute { get => _selectedAttribute; set => SetProperty(ref _selectedAttribute, value); }

    private RelayCommand _fillCommand;
    public ICommand FillCommand => _fillCommand ?? (_fillCommand = new RelayCommand(o => OnFillTree(), o => true));
    public void OnFillTree()
    {
      var random = MyRandom.Random;
      var count = random.Next(2, 5);
      for (var i = 0; i < count; ++i)
      {
        var item = new BasicItemViewModel($"Workspace {i}");
        var subCount = random.Next(2, 5);
        for (var si = 0; si < subCount; ++si)
        {
          var subItem = new BasicItemViewModel($"Class {si}");
          var sub1Count = random.Next(1, 3);
          for (var si2 = 0; si2 < sub1Count; ++si2)
          {
            var subItem2 = new BasicItemViewModel($"Type {si2}");
            var sub2Count = random.Next(2, 5);
            for (var si3 = 0; si3 < sub2Count; ++si3)
            {
              var subItem3 = new ItemWithAttributesViewModel($"OID {si3}");
              subItem2.Children.Add(subItem3);
            }
            subItem.Children.Add(subItem2);
          }
          item.Children.Add(subItem);
        }
        _items.Add(item);
      }
    }
    
    private RelayCommand _expandCommand;
    public ICommand ExpandCommand => _expandCommand ?? (_expandCommand = new RelayCommand(o => OnExpand(), o => true));
    public void OnExpand()
    {
      foreach (var item in Items)
        item.Expand();
    }
  }

}
