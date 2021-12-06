using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace PLSE_FoxPro.Models
{
    public enum PermissionProfile
    {
        [Description("администратор")]
        Admin,
        [Description("начальник")]
        Boss,
        [Description("зам. начальника")]
        Subboss,
        [Description("бухгалтер")]
        Accountant,
        [Description("эксперт")]
        Expert,
        [Description("лаборант")]
        Laboratorian,
        [Description("делопроизводитель")]
        Clerk,
        [Description("инспектор по кадрам")]
        Staffinspector,
        [Description("начальник снабжения")]
        Provisionboss,
        [Description("без прав")]
        Rightless,
        [Description("заведующий отделом")]
        HeadOfDepartament
    }
}
