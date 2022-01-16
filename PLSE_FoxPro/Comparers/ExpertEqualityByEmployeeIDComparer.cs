using PLSE_FoxPro.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace PLSE_FoxPro.Comparers
{
    public class ExpertEqualityByEmployeeIDComparer : IEqualityComparer<Expert>
    {
        public bool Equals([AllowNull] Expert x, [AllowNull] Expert y) => x.Employee.ID == y.Employee.ID;

        public int GetHashCode([DisallowNull] Expert obj) =>obj.Employee.ID.GetHashCode();
    }
}
