using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EroJRPG.Scripts.StateManager;

public partial class StateManagerTest : Node
{
    private StateManager _stateManager;

    // Hardcoded from templates for easy testing
    private readonly Dictionary<string, string[]> _bundleStates = new()
    {
        { "CharacterBundle", new[] { "MaxHealth", "CurrentHealth", "ProportionalCurrentHealth" } },
        { "PlayerCharacterBundle", new[] { "MaxHealth", "CurrentSectionMax", "CurrentHealth", "SectionHealth", "SectionAmount", "CurrentSection", "NextSectionMax" } },
        { "PlayerCharacterBundle2", new[] { "MaxHealth", "MaxBarHealth", "HealthBars", "CurrentHealth", "CurrentHealthBar" } }
    };

    private readonly Dictionary<string, Dictionary<string, List<string>>> _bundleStateActions = new()
    {
        { "CharacterBundle", new Dictionary<string, List<string>>
            {
                { "MaxHealth", new() { "Set" } },
                { "CurrentHealth", new() { "Set", "Increment", "Decrement" } },
                { "ProportionalCurrentHealth", new() { "Set", "Increment", "Decrement" } }
            }
        },
        { "PlayerCharacterBundle", new Dictionary<string, List<string>>
            {
                { "MaxHealth", new() { "Set" } }, // ProductKeeper: derived, no actions - skip
                { "CurrentSectionMax", new List<string>() }, // derived
                { "CurrentHealth", new() { "Set", "Increment", "Decrement", "Set_ignore_bound", "Increment_ignore_bound" } },
                { "SectionHealth", new() { "Set", "Increment", "Decrement" } },
                { "SectionAmount", new() { "Set", "Increment", "Decrement" } },
                { "CurrentSection", new List<string>() }, // derived
                { "NextSectionMax", new List<string>() } // derived
            }
        },
        { "PlayerCharacterBundle2", new Dictionary<string, List<string>>
            {
                { "MaxHealth", new List<string>() }, // derived
                { "MaxBarHealth", new() { "Set", "Increment", "Decrement" } },
                { "HealthBars", new() { "Set", "Increment", "Decrement" } },
                { "CurrentHealth", new() { "Set", "Increment", "Decrement" } },
                { "CurrentHealthBar", new() { "Set", "Increment", "Decrement" } }
            }
        }
    };

    private readonly Dictionary<string, object> _testPayloads = new()
    {
        { "Set", 150 },
        { "Increment", 25 },
        { "Decrement", -10 },
        { "Set_ignore_bound", 999 },
        { "Increment_ignore_bound", 999 },
        { "Flip", null }
    };

    public override void _Ready()
    {
        // Fetch the autoloaded StateManager (adjust path if not AutoLoad)
        _stateManager = GetNode<StateManager>("/root/StateManager");
        if (_stateManager == null)
        {
            GD.PushError("StateManager not found as autoload! Add it to Project Settings > AutoLoad.");
            return;
        }

        GD.Print("=== FUNCTIONAL TESTS START ===");
        FunctionalTests();
        GD.Print("=== FUNCTIONAL TESTS END ===");

        GD.Print("=== PROFILING TESTS START ===");
        ProfilingTests();
        GD.Print("=== PROFILING TESTS END ===");
    }

    private void FunctionalTests()
    {
        foreach (var kvp in _bundleStates)
        {
            string bundleTemplate = kvp.Key;
            string bundleName = $"test_{bundleTemplate.ToLower()}";

            GD.Print($"\n🎯 TESTING BUNDLE: {bundleTemplate} as '{bundleName}'");

            // Create with defaults if available
            string defaults = bundleTemplate == "CharacterBundle" ? "TestDefaults" : "";
            _stateManager.CreateBundle(bundleTemplate, bundleName, defaults);

            DumpBundle(bundleName, bundleTemplate);

            // Test all actions per state
            foreach (var state in kvp.Value)
            {
                if (!_bundleStateActions[bundleTemplate].TryGetValue(state, out var actions) || !actions.Any())
                {
                    GD.Print($"  ⏭️  Skipping derived state '{state}' (no actions)");
                    continue;
                }

                foreach (string actionName in actions)
                {
                    object payload = _testPayloads[actionName];
                    GD.Print($"  🔄 Dispatch '{state}.{actionName}({payload})'");

                    // Before
                    object before = _stateManager.GetState(bundleName, state);
                    GD.Print($"    Before: {state} = {before}");

                    // Dispatch
                    try
                    {
                        _stateManager.Dispatch(bundleName, state, new StateAction(actionName, payload));
                    }
                    catch (Exception e)
                    {
                        GD.PushError($"Dispatch failed: {e.Message}");
                        continue;
                    }

                    // After
                    object after = _stateManager.GetState(bundleName, state);
                    GD.Print($"    After:  {state} = {after}");

                    // Check if changed (rules might clamp)
                    if (Equals(before, after))
                        GD.Print($"    ⚠️   No change (likely rule clamped)");
                }
            }

            // Specific rule tests
            RuleSpecificTests(bundleName, bundleTemplate);

            DumpBundle(bundleName, bundleTemplate);
            _stateManager.DestroyBundle(bundleName);
        }
    }

    private void RuleSpecificTests(string bundleName, string template)
    {
        GD.Print($"  🧪 Rule-Specific Tests for {template}");

        switch (template)
        {
            case "CharacterBundle":
                // Test clamping (BoundedValueRule)
                _stateManager.Dispatch(bundleName, "CurrentHealth", new StateAction("Set", 999));
                GD.Print($"    CurrentHealth clamped? {_stateManager.GetState(bundleName, "CurrentHealth")}");

                // Test proportional scale (ProportionalBoundedValueRule bi-dir)
                object oldCurrentProp = _stateManager.GetState(bundleName, "ProportionalCurrentHealth");
                _stateManager.Dispatch(bundleName, "MaxHealth", new StateAction("Set", 500));
                object newCurrentProp = _stateManager.GetState(bundleName, "ProportionalCurrentHealth");
                GD.Print($"    Prop Health scaled? Old:{oldCurrentProp} → New:{newCurrentProp} (Max now 500)");

                // Reset (destroy first to avoid duplicate warning)
                _stateManager.DestroyBundle(bundleName);
                string defaults = "TestDefaults";
                _stateManager.CreateBundle("CharacterBundle", bundleName, defaults);
                break;

            case "PlayerCharacterBundle":
                // Test sectioning & gates
                _stateManager.Dispatch(bundleName, "SectionAmount", new StateAction("Increment", 1)); // MaxHealth +=50
                DumpBundle(bundleName, template); // Check derived updates

                // Damage to cross section
                _stateManager.Dispatch(bundleName, "CurrentHealth", new StateAction("Set", 10)); // Should gate at NextSectionMax?
                GD.Print($"    Gated? CurrentHealth:{_stateManager.GetState(bundleName, "CurrentHealth")}, CurrentSection:{_stateManager.GetState(bundleName, "CurrentSection")}");

                // Ignore bounds
                _stateManager.Dispatch(bundleName, "CurrentHealth", new StateAction("Set_ignore_bound", -100));
                GD.Print($"    Ignore worked? CurrentHealth:{_stateManager.GetState(bundleName, "CurrentHealth")}");
                break;

            case "PlayerCharacterBundle2":
                // Test product derived
                _stateManager.Dispatch(bundleName, "HealthBars", new StateAction("Increment", 1));
                _stateManager.Dispatch(bundleName, "MaxBarHealth", new StateAction("Set", 100));
                GD.Print($"    Product MaxHealth: {_stateManager.GetState(bundleName, "MaxHealth")} (2*100=200)");

                // Bar clamp
                _stateManager.Dispatch(bundleName, "CurrentHealth", new StateAction("Increment", 200));
                GD.Print($"    CurrentHealth clamped to MaxBarHealth: {_stateManager.GetState(bundleName, "CurrentHealth")}");
                break;
        }
    }

    private void DumpBundle(string bundleName, string bundleTemplate)
    {
        GD.Print($"  📊 States for '{bundleName}' ({bundleTemplate}):");

        // Quick lookup for states (assumes bundleName like "test_characterbundle" matches template)
        string[] states = _bundleStates.GetValueOrDefault(bundleTemplate, Array.Empty<string>());
        if (states.Length == 0)
        {
            GD.PushWarning($"No states found for template '{bundleTemplate}' in DumpBundle.");
            return;
        }

        foreach (string state in states)
        {
            try
            {
                object value = _stateManager.GetState(bundleName, state);
                GD.Print($"    {state}: {value}");
            }
            catch (Exception e)
            {
                GD.Print($"    {state}: <ERROR: {e.Message}>");
            }
        }
    }

    private void ProfilingTests()
    {
        const int NUM_BUNDLES = 100000;
        const string PROFILE_BUNDLE_TEMPLATE = "CharacterBundle";
        const string TARGET_BUNDLE = "profile_target";
        const int NUM_ACTIONS = 100000;

        var bundleNames = new List<string>(NUM_BUNDLES);

        // Create 100k bundles
        var swCreate = Stopwatch.StartNew();
        for (int i = 0; i < NUM_BUNDLES; i++)
        {
            string name = $"prof_{i}";
            _stateManager.CreateBundle(PROFILE_BUNDLE_TEMPLATE, name);
            bundleNames.Add(name);
        }
        swCreate.Stop();
        GD.Print($"⏱️  CREATE 100k bundles: {swCreate.ElapsedMilliseconds} ms");

        // Dispatch 100k actions to ONE bundle
        _stateManager.CreateBundle(PROFILE_BUNDLE_TEMPLATE, TARGET_BUNDLE); // Fresh target
        var swDispatch = Stopwatch.StartNew();
        for (int i = 0; i < NUM_ACTIONS; i++)
        {
            string action = i % 3 == 0 ? "Increment" : (i % 3 == 1 ? "Decrement" : "Set");
            object payload = _testPayloads[action];
            _stateManager.Dispatch(TARGET_BUNDLE, "CurrentHealth", new StateAction(action, payload));
        }
        swDispatch.Stop();
        GD.Print($"⏱️  DISPATCH 100k actions to 1 bundle: {swDispatch.ElapsedMilliseconds} ms");

        // Destroy all
        var swDestroy = Stopwatch.StartNew();
        foreach (string name in bundleNames)
        {
            _stateManager.DestroyBundle(name);
        }
        _stateManager.DestroyBundle(TARGET_BUNDLE);
        swDestroy.Stop();
        GD.Print($"⏱️  DESTROY 100k bundles: {swDestroy.ElapsedMilliseconds} ms");

        GD.Print($"💾 TOTAL RAM impact: Check Godot profiler!");
    }
}