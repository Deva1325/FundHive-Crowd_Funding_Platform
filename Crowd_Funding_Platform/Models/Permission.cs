using System;
using System.Collections.Generic;

namespace Crowd_Funding_Platform.Models;

public partial class Permission
{
    public int Permissionid { get; set; }

    public int Tabid { get; set; }

    public bool? Isactive { get; set; }

    public bool Iscreatorapproved { get; set; }

    public bool Isadmin { get; set; }

    public virtual TblTab Tab { get; set; } = null!;
}
