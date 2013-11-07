using _4tecture.UI.Common.DependencyInjection;
using Microsoft.Practices.Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;

namespace _4tecture.UI.Common.Helper
{
    public static class DataTemplateHelper
    {
        public static void RegisterAndCreateTemplate<TViewModel, TView>(IDependencyContainer container, string regionname = null)
        {
            container.RegisterType<TViewModel>();
            container.RegisterType<TView>();
            var dataTemplate = DataTemplateHelper.CreateTemplate<TViewModel, TView>();
            Application.Current.Resources.Add(dataTemplate.DataTemplateKey, dataTemplate);

            if (!string.IsNullOrWhiteSpace(regionname))
            {
                var regionmanager = container.TryResolve<IRegionManager>();
                if (regionmanager != null)
                {
                    regionmanager.RegisterViewWithRegion(regionname, typeof(TViewModel));
                }
                else
                {
                    throw new Exception("RegionManager must be initialize to register views.");
                }
            }
        }

        public static DataTemplate CreateTemplate<TViewModel, TView>()
        {
            return CreateTemplate(typeof(TViewModel), typeof(TView));
        }
        public static DataTemplate CreateTemplate(Type viewModelType, Type viewType)
        {
            const string xamlTemplate = "<DataTemplate DataType=\"{{x:Type vm:{0}}}\"><v:{1} /></DataTemplate>";
            var xaml = String.Format(xamlTemplate, viewModelType.Name, viewType.Name, viewModelType.Namespace, viewType.Namespace);

            var context = new ParserContext();

            context.XamlTypeMapper = new XamlTypeMapper(new string[0]);
            context.XamlTypeMapper.AddMappingProcessingInstruction("vm", viewModelType.Namespace, viewModelType.Assembly.FullName);
            context.XamlTypeMapper.AddMappingProcessingInstruction("v", viewType.Namespace, viewType.Assembly.FullName);

            context.XmlnsDictionary.Add("", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
            context.XmlnsDictionary.Add("x", "http://schemas.microsoft.com/winfx/2006/xaml");
            context.XmlnsDictionary.Add("vm", "vm");
            context.XmlnsDictionary.Add("v", "v");

            var template = (DataTemplate)XamlReader.Parse(xaml, context);
            return template;
        }
    }
}
