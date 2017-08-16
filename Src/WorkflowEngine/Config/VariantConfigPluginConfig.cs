// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using WorkflowEngine.Interfaces;

namespace WorkflowEngine.Config
{
    /// <summary>
    /// Applies Parallax-style variant constraints to a config source
    /// A variant is something that varies the value of a configuration
    /// A constraint is what constraints the value of this variant
    /// E.g for a variant constraint: FLT::FLT6, or MKT::EN-US
    ///
    /// Parses Variant Configuration of the format:
    /// CONFIG_VALUE||VARIANT1::VARIANT1_VALUE&&VARIANT2::VARIANT2_VALUE==CONFIGVALUE
    /// </summary>
    internal class VariantConfigPluginConfig : IPluginConfig
    {
        private const string OrOperator = "||";
        private const string AndOperator = "&&";
        private const string EqOperator = "==";
        private const string VariantSeparator = "::";

        /// <summary>
        /// Maps a configuration Key to a map that maps
        /// Variant Constraints to configuration values
        /// </summary>
        private static readonly IDictionary<string, IDictionary<VariantConstraints, string>> Cache = new Dictionary<string, IDictionary<VariantConstraints, string>>();

        private static readonly VariantConstraints DefaultConstraints = new VariantConstraints();

        /// <summary>
        /// The underlying config source
        /// </summary>
        private readonly IPluginConfig _pluginConfig;

        /// <summary>
        /// Variants of the current context
        /// </summary>
        private ISet<VariantConstraint> _variants = new HashSet<VariantConstraint>();

        public VariantConfigPluginConfig(IPluginConfig pluginConfig)
        {
            _pluginConfig = pluginConfig;
            Cache.Clear();
        }

        public void Initialize(IEnumerable<VariantConstraint> variants)
        {
            _variants = variants == null
                ? new HashSet<VariantConstraint>()
                : new HashSet<VariantConstraint>(variants);
        }

        public string Get(string configKey)
        {
            var rawConfigValue = _pluginConfig.Get(configKey);
            if (string.IsNullOrEmpty(rawConfigValue))
                return rawConfigValue;

            // If the raw value doesn't contain the OR operator, this means it
            // doesn't have variant constraints, just return the raw value
            if (!rawConfigValue.Contains(OrOperator))
                return rawConfigValue;

           if (!Cache.ContainsKey(configKey))
                PopulateCacheForKey(configKey, rawConfigValue);

            return GetValueFromCache(configKey);
        }

        private static void PopulateCacheForKey(string configKey, string rawConfigValue)
        {
            // This is a very manual way for parsing the AST, consider replacing this with a CFG parser
            // CFG parser is probably over-kill but it'll have better performance as our rules grow in size
            // Just in case we migrate to a proper parser, here's the grammar:
            //
            // CONFIG_VALUE = raw_value
            // CONFIG_VALUE = raw_value||VARIANT
            // VARIANT = VAIRANT||VARIANT
            // VARIANT = CONSTRAINT
            // CONSTRAINT = CONSTRAINT&&CONSTRAINT
            // CONSTRAINT = constraint_key::constraint_value==raw_value
            var variants = rawConfigValue.Split(new[] { OrOperator }, StringSplitOptions.RemoveEmptyEntries);

            // Always assume the first variant is the default one
            var defaultValue = variants[0];
            foreach (var variant in variants.Skip(1))
            {
                if (!variant.Contains(EqOperator))
                    throw new FormatException($"Cannot parse a variant config with no value. Variant: {variant}");

                var constraintsAndValue = variant.Split(new[] { EqOperator }, StringSplitOptions.RemoveEmptyEntries);
                if (constraintsAndValue.Length != 2)
                    throw new FormatException($"Variant: {variant} is malformed");

                var constraints = ParseConstraints(constraintsAndValue[0]);
                if (!Cache.ContainsKey(configKey))
                    Cache[configKey] = new Dictionary<VariantConstraints, string>();

                Cache[configKey][constraints] = constraintsAndValue[1];
            }

            // Always add the raw config value as the value for the default constraint
            Cache[configKey][DefaultConstraints] = defaultValue;
        }

        /// <summary>
        /// Generic method to compute the power set of a list
        /// We'll use it to compare the power set of input variant constraints
        /// WARNING: This function uses int bit masks, so it'll overflow for
        ///          input lists that are greater than 32 in size
        /// </summary>
        private static IEnumerable<List<VariantConstraint>> GetPowerSet(IReadOnlyList<VariantConstraint> input)
        {
            var n = input.Count;
            var powerSetCount = 1 << n;
            var powerSet = new List<List<VariantConstraint>>();
            for (var mask = 0; mask < powerSetCount; mask++)
            {
                var s = new List<VariantConstraint>();
                for (var i = 0; i < n; i++)
                {
                    if ((mask & (1 << i)) > 0)
                    {
                        s.Add(input[i]);
                    }
                }
                powerSet.Add(s);
            }

            return powerSet;
        }

        private static VariantConstraints ParseConstraints(string variant)
        {
            var variantParts = variant.Split(new[] { AndOperator }, StringSplitOptions.RemoveEmptyEntries);

            var variantConstraints = new VariantConstraints();
            foreach (var variantPart in variantParts)
            {
                var variantKeyAndValue = variantPart.Split(new[] { VariantSeparator }, StringSplitOptions.RemoveEmptyEntries);
                var variantPair = new VariantConstraint(variantKeyAndValue[0], variantKeyAndValue[1]);
                variantConstraints.AddVariant(variantPair);
            }
            return variantConstraints;
        }

        private string GetValueFromCache(string configKey)
        {
            var variantConstraints = GetVariantConstraints().OrderByDescending(s => s.Size());
            var variantConstraintsMap = Cache[configKey];

            foreach (var vc in variantConstraints)
            {
                if (variantConstraintsMap.ContainsKey(vc))
                    return variantConstraintsMap[vc];
            }
            throw new KeyNotFoundException($"Wasn't able to find the config key: {configKey}");
        }

        private IEnumerable<VariantConstraints> GetVariantConstraints()
        {
            var constraintsList = new List<VariantConstraints>();
            var variantConstraintsPowerSet = GetPowerSet(_variants.ToList());
            foreach (var variantConstraints in variantConstraintsPowerSet)
            {
                var constraints = new VariantConstraints();
                foreach (var variantConstraint in variantConstraints)
                {
                    constraints.AddVariant(variantConstraint);
                }
                constraintsList.Add(constraints);
            }
            return constraintsList;
        }
    }
}
