﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minimal.Domain.Core.Responces
{
    public class Response<T>
    {
        public T? Result { get; set; }

        public bool HasErrors { get; set; } = false;

        public bool Cached { get; set; } = false;

        public List<ValidationResult> ValidationErrors { get; set; } = new List<ValidationResult>();

        public void AddValidationError(string field, string error)
        {
            ValidationErrors.Add(new ValidationResult() { Field = field, Errors = new List<string> { error } });
        }

        public void AddValidationErrors(string field, List<string> errors)
        {
            ValidationErrors.Add(new ValidationResult() { Field = field, Errors = errors });
        }
    }
}
