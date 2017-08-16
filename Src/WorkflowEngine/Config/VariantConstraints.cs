// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WorkflowEngine.Utils;

namespace WorkflowEngine.Config
{
    /// <summary>
    /// A collection of variant constraints
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    internal class VariantConstraints
    {
        private readonly ISet<VariantConstraint> _variants;

        public VariantConstraints()
        {
            _variants = new HashSet<VariantConstraint>();
        }

        private string DebuggerDisplay => $"C:{string.Join(":", _variants.Select(v => v.GetHashCode()))},H:{GetHashCode()}";

        public void AddVariant(VariantConstraint vc)
        {
            _variants.Add(vc);
        }

        public int Size()
        {
            return _variants.Count;
        }

        public override bool Equals(object obj)
        {
            var other = obj as VariantConstraints;

            if (_variants.Count != other?._variants.Count)
                return false;

            foreach (var variant in _variants)
            {
                if (!other._variants.Contains(variant))
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return HashUtils.CombineHashCodes(_variants.Select(v => v.GetHashCode()).ToArray());
        }
    }
}
