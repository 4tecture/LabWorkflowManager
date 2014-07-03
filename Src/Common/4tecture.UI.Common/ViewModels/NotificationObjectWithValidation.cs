using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Practices.Prism.ViewModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Specialized;

namespace _4tecture.UI.Common.ViewModels
{
    public abstract class NotificationObjectWithValidation : NotificationObject, INotifyDataErrorInfo
    {
        protected NotificationObjectWithValidation()
        {
            this.PropertyChanged += (sender, args) =>
            {
                if (!args.PropertyName.Equals("IsDirty"))
                {
                    this.IsDirty = true;
                }
            };
        }

        #region INotifyDataErrorInfo

        private readonly Dictionary<string, List<string>> errors = new Dictionary<string, List<string>>();

        protected void AddError(string propertyName, string error)
        {
            if (!this.errors.ContainsKey(propertyName) || (this.errors.ContainsKey(propertyName) && this.errors[propertyName] == null))
            {
                this.errors[propertyName] = new List<string>();
            }
            if (!this.errors[propertyName].Contains(error))
            {
                this.errors[propertyName].Add(error);
            }
            if (this.ErrorsChanged != null)
            {
                this.ErrorsChanged(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }

        protected void RemoveError(string propertyName, string error = null)
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                if (this.errors.ContainsKey(propertyName) && this.errors[propertyName] != null && this.errors[propertyName].Contains(error))
                {
                    this.errors[propertyName].Remove(error);
                    if (this.errors[propertyName].Count == 0)
                    {
                        this.errors.Remove(propertyName);
                    }
                }
            }
            else
            {
                this.errors.Remove(propertyName);
            }

            if (this.ErrorsChanged != null)
            {
                this.ErrorsChanged(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }



        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public System.Collections.IEnumerable GetErrors(string propertyName)
        {
            if (this.errors.ContainsKey(propertyName))
            {
                return this.errors[propertyName];
            }
            else
            {
                return System.Linq.Enumerable.Empty<string>();
            }
        }

        protected bool HasErrorsInternal
        {
            get { return this.errors.Count > 0; }
        }
        public virtual bool HasErrors
        {
            get { return this.HasErrorsInternal; }
        }

        #endregion

        #region Helpers

        protected void VerifyStringNotEmpty(string s, string parametername, string error)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                this.AddError(parametername, error);
            }
            else
            {
                this.RemoveError(parametername);
            }
        }

        #endregion

        #region dirtyflag

        private bool isDirty;

        [IgnoreDataMember]
        [XmlIgnore]
        public bool IsDirty
        {
            get { return this.isDirty; }
            private set
            {
                this.isDirty = value;
                this.RaisePropertyChanged(() => this.IsDirty);
            }
        }

        public void ResetIsDirty()
        {
            this.IsDirty = false;
            foreach (var child in dirtyObservedChildren)
            {
                child.ResetIsDirty();
            }
        }

        private List<NotificationObjectWithValidation> dirtyObservedChildren = new List<NotificationObjectWithValidation>();
        public void AddIsDirtyObservableChildren(params NotificationObjectWithValidation[] children)
        {
            foreach (var child in children)
            {
                dirtyObservedChildren.Add(child);
                child.PropertyChanged += BubbleChildDirtyEvent;
            }
        }

        private void BubbleChildDirtyEvent(object sender, PropertyChangedEventArgs args)
        {

            if (args.PropertyName.Equals("IsDirty") && ((NotificationObjectWithValidation)sender).IsDirty)
            {
                this.IsDirty = true;
            }

        }

        public void AddIsDirtyObservableCollection(params IEnumerable[] collections)
        {
            foreach (var col in collections)
            {
                var oc = col as INotifyCollectionChanged;
                if (oc != null)
                {
                    oc.CollectionChanged += (sender, args) =>
                    {
                        if (args.NewItems != null)
                        {
                            foreach (var item in args.NewItems)
                            {
                                AddChangedHandlerToCollectionitem(item);
                            }
                        }
                        this.IsDirty = true;
                    };
                }
            }

            foreach (var col in collections)
            {
                foreach (var item in col)
                {
                    AddChangedHandlerToCollectionitem(item);
                }
            }
        }

        private void AddChangedHandlerToCollectionitem(object item)
        {
            var no = item as NotificationObjectWithValidation;
            if (no != null)
            {
                no.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName.Equals("IsDirty") && ((NotificationObjectWithValidation)sender).IsDirty)
                    {
                        this.IsDirty = true;
                    }
                };
            }
            else
            {
                var ic = item as INotifyPropertyChanged;
                if (ic != null)
                {
                    ic.PropertyChanged += (sender, args) => { this.IsDirty = true; };
                }
            }
        }

        #endregion
    }
}
