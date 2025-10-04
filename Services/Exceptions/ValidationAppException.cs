using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Exceptions
{
    public sealed class ValidationAppException : Exception
    {
        public IDictionary<string, string[]> Errors { get; }
        public ValidationAppException(IDictionary<string, string[]> errors, string? message = null)
            : base(message ?? "Validation failed") => Errors = errors;
    }
}
