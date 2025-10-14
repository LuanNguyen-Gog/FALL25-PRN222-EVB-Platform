using System;
using System.Collections.Generic;

namespace Repositories.Models;

public partial class Setting
{
    public string Key { get; set; }

    public string Value { get; set; }

    public long? UpdatedBy { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User UpdatedByNavigation { get; set; }
}
