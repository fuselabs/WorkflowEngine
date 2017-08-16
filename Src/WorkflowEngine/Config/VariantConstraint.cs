// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Diagnostics;
using WorkflowEngine.Utils;

namespace WorkflowEngine.Config
{
    /// <summary>
    /// A single variant constraint
    /// Example for key: "flt", "mkt"
    /// Example for Valye: "flt6", "en-us"
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public class VariantConstraint
    {
        public VariantConstraint(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; }

        public string Value { get; }

        private string DebuggerDisplay => $"K:{Key},V:{Value},H:{GetHashCode()}";

        public override int GetHashCode()
        {
            return HashUtils.CombineHashCodes(Key.GetHashCode(), Value.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            var other = obj as VariantConstraint;
            if (other == null)
                return false;
            return Key == other.Key && Value == other.Value;
        }
    }
}
