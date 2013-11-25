using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
