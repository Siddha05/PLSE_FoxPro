using PLSE_FoxPro.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace PLSE_FoxPro.TemplateSelectors
{
    internal class EventTypeTemplateSelector : DataTemplateSelector
    {
        public DataTemplate MessageTemplate { get; set; }
        public DataTemplate ExpertiseInWorkOverviewTemplate { get; set; }
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return item switch
            {
                Message m => MessageTemplate,
                ExpertisesInWorkOverview e => ExpertiseInWorkOverviewTemplate,
                _ => throw new NotImplementedException("Unknown type of parameter name")
            };
            //switch (item)
            //{
            //    case Message _:
            //        return MessageTemplate;
            //    case ExpertisesInWorkOverview _:
            //        return ExpertiseTemplate;
            //    case SimpleCnl _:
            //        return SimpleCnlTemplate;
            //    default:
            //        throw new ArgumentException($"Type {item.GetType().Name} not supported");
            //}
        }
    }
}
