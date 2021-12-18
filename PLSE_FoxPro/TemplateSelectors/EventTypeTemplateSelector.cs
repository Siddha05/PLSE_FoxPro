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
        public DataTemplate AnnualDatesOverview { get; set; }
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return item switch
            {
                Message m => MessageTemplate,
                ExpertisesInWorkOverview e => ExpertiseInWorkOverviewTemplate,
                AnnualDatesOverview a => AnnualDatesOverview,
                _ => throw new NotImplementedException("Unknown type of parameter item")
            };
        }
    }
}
