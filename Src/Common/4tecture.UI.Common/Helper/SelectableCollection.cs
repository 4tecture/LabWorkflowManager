using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace _4tecture.UI.Common.Helper
{
    public class SelectableCollection<TItem> : ObservableCollection<SelectableItem<TItem>> where TItem : class
    {
        public SelectableCollection()
        {
            this.CollectionChanged += SelectableCollection_CollectionChanged;
        }

        void SelectableCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var newItem in e.NewItems)
                {
                    var si = newItem as SelectableItem<TItem>;
                    if (si != null)
                    {
                        si.PropertyChanged += SelectionChangedHandler;

                        var npc = si.Item as INotifyPropertyChanged;
                        if (npc != null)
                        {
                            npc.PropertyChanged += ItemPropertyChangedHandler;
                        }

                        if (si.IsSelected && this.SelectionChanged != null)
                        {
                            this.SelectionChanged(this, null);
                        }
                    }
                }
            }
            if (e.OldItems != null)
            {
                foreach (var newItem in e.OldItems)
                {
                    var si = newItem as SelectableItem<TItem>;
                    if (si != null)
                    {
                        si.PropertyChanged -= SelectionChangedHandler;

                        var npc = si.Item as INotifyPropertyChanged;
                        if (npc != null)
                        {
                            npc.PropertyChanged -= ItemPropertyChangedHandler;
                        }

                        if (si.IsSelected && this.SelectionChanged != null)
                        {
                            this.SelectionChanged(this, null);
                        }
                    }
                }
            }
        }

        private void ItemPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if(this.ItemChanged != null)
            {
                this.ItemChanged(sender, e);
            }
        }

        private void SelectionChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (this.SelectionChanged != null && e.PropertyName.Equals("IsSelected"))
            {
                this.SelectionChanged(sender, null);
            }
        }

        public event EventHandler<PropertyChangedEventArgs> ItemChanged;
        public event EventHandler SelectionChanged;

        public void AddItem(TItem item)
        {
            this.Add(new SelectableItem<TItem>() { Item = item });
        }

        public IEnumerable<TItem> SelectedItems
        {
            get
            {
                return this.Where(i => i.IsSelected).Select(si => si.Item);
            }
            set
            {
                foreach(var si in this)
                {
                    if (value.Contains(si.Item))
                    {
                        si.IsSelected = true;
                    }
                    else
                    {
                        si.IsSelected = false;
                    }
                }
            }
        }

        public void Add(TItem item)
        {
            this.Add(new SelectableItem<TItem>() { Item = item });
        }

        private readonly object uiLockObject = new object();
        private bool selectedItemsCollectionNotificationPaused;
        private bool selectionChangedPaused;

        public void Init<TTargetItem>(IEnumerable<TItem> sourceItems, ObservableCollection<TTargetItem> selectedItemsCollection, Func<TItem, TTargetItem> convertToTargetItem, Func<TTargetItem, TItem> convertToSourceItem, Action selectionVerificationAction = null)
        {
            this.RefreshSelectableItems(sourceItems, selectedItemsCollection != null ? selectedItemsCollection.Select(convertToSourceItem) : null);

            this.SelectionChanged += (sender, eventArgs) =>
            {
                if (!selectionChangedPaused)
                {
                    selectedItemsCollectionNotificationPaused = true;
                    selectedItemsCollection.Clear();
                    foreach (var selectedItem in this.SelectedItems)
                    {
                        selectedItemsCollection.Add(convertToTargetItem(selectedItem));
                    }
                    
                    if (selectionVerificationAction != null)
                    {
                        selectionVerificationAction();
                    }
                    selectedItemsCollectionNotificationPaused = false;
                }
            };
            if (selectedItemsCollection != null)
            {
                selectedItemsCollection.CollectionChanged += (sender, eventArgs) =>
                {
                    if (!selectedItemsCollectionNotificationPaused)
                    {
                        selectionChangedPaused = true;
                        var selectedItemsTmp =
                            selectedItemsCollection.Select(convertToSourceItem)
                                .Where(o => this.InnerItems.Contains(o))
                                .ToList();
                        this.SelectedItems = selectedItemsTmp;

                        if (selectionVerificationAction != null)
                        {
                            selectionVerificationAction();
                        }

                        selectionChangedPaused = false;
                    }
                };
            }


        }

        public void RefreshSelectableItems(IEnumerable<TItem> sourceItems, IEnumerable<TItem> preSelectedElements = null)
        {
            this.Clear();

            foreach (var item in sourceItems)
            {
                this.Add(item);
            }

            if (preSelectedElements != null)
            {
                foreach (var selItem in this)
                {
                    if (preSelectedElements.Contains(selItem.Item))
                    {
                        selItem.IsSelected = true;
                    }
                }
            }
        }


        public IEnumerable<TItem> InnerItems
        {
            get { return this.Items.Select(o => o.Item); }
        }
        

        //void AvailableItemsSelectionChanged(object sender, EventArgs e)
        //{
        //    this.SelectedItemsCollection.CollectionChanged -= SelectedItemsCollectionCollectionChanged;
        //    this.SelectedItemsCollection.Clear();
        //    foreach (var selectedItem in this.SelectedItems)
        //    {
        //        this.SelectedItemsCollection.Add(selectedItem);
        //    }
        //    this.SelectedItemsCollection.CollectionChanged += SelectedItemsCollectionCollectionChanged;

        //    if(this.SelectionVerificationAction != null)
        //    {
        //        this.SelectionVerificationAction();
        //    }
        //}

        //void SelectedItemsCollectionCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        //{
        //    this.SelectionChanged -= AvailableItemsSelectionChanged;
        //    var selectedItemsTmp = this.SelectedItemsCollection.Where(o => this.InnerItems.Contains(o)).ToList();
        //    this.SelectedItems = selectedItemsTmp;
        //    this.SelectionChanged += AvailableItemsSelectionChanged;
        //    if (this.SelectionVerificationAction != null)
        //    {
        //        this.SelectionVerificationAction();
        //    }
        //}



        public static SelectableCollection<TItem> InitSelectableUIList<TTargetItem>(IEnumerable<TItem> sourceItems, ObservableCollection<TTargetItem> selectedItemsCollection, Func<TItem, TTargetItem> convertToTargetItem, Func<TTargetItem, TItem> convertToSourceItem, Action selectionVerificationAction = null)
        {
            var selectableCollection = new SelectableCollection<TItem>();
            BindingOperations.EnableCollectionSynchronization(selectableCollection, selectableCollection.uiLockObject);

            selectableCollection.Init(sourceItems, selectedItemsCollection, convertToTargetItem, convertToSourceItem, selectionVerificationAction);

            return selectableCollection;
        }
    }

    public class SelectableItem<TItem> : INotifyPropertyChanged where TItem : class
    {
        private bool isSelected;

        public bool IsSelected
        {
            get { return isSelected; }
            set { isSelected = value; this.RaisePropertyChanged("IsSelected"); }
        }

        private TItem item;

        public TItem Item
        {
            get { return item; }
            set { 
                if(this.item != null)
                {
                    var npc = this.item as INotifyPropertyChanged;
                    if(npc != null)
                    {
                        npc.PropertyChanged -= ItemPropertyChanged;
                    }
                }
                item = value;
                if (this.item != null)
                {
                    var npc = this.item as INotifyPropertyChanged;
                    if (npc != null)
                    {
                        npc.PropertyChanged += ItemPropertyChanged;
                    }
                }
                this.RaisePropertyChanged("IsSelected"); }
        }

        private void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.PropertyChanged(this.item, e);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propName)
        {
            if(this.PropertyChanged != null)
            {
                this.PropertyChanged(this.PropertyChanged, new PropertyChangedEventArgs(propName));
            }
        }

        public override bool Equals(object obj)
        {
            var other = obj as SelectableItem<TItem>;
            if (other == null)
            {
                return false;
            }
            return this.Item.Equals(other.Item);;
        }

        public override int GetHashCode()
        {
            return this.Item.GetHashCode();
        }
    }
}
