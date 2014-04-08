using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Practices.Prism.ViewModel;

namespace _4tecture.UI.Common.ViewModels
{
    public abstract class NotificationObjectWithValidation : NotificationObject, INotifyDataErrorInfo
    {
        #region INotifyDataErrorInfo

        private readonly Dictionary<string, List<string>> errors = new Dictionary<string, List<string>>();

        protected void AddError(string propertyName, string error)
        {
            if (!this.errors.ContainsKey(propertyName) || (this.errors.ContainsKey(propertyName) && this.errors[propertyName] == null))
            {
                this.errors[propertyName] = new List<string>();
            }
            if(!this.errors[propertyName].Contains(error))
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
    }
}
