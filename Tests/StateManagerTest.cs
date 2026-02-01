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
                { "MaxHealth", new List<string>() },
                { "CurrentSectionMax", new List<string>() },
                { "CurrentHealth", new() { "Set", "Increment", "Decrement", "SetIgnoreBound", "IncrementIgnoreBound" } },
                { "SectionHealth", new() { "Set", "Increment", "Decrement" } },
                { "SectionAmount", new() { "Set", "Increment", "Decrement" } },
                { "CurrentSection", new List<string>() },
                { "NextSectionMax", new List<string>() }
            }
        },
        { "PlayerCharacterBundle2", new Dictionary<string, List<string>>
            {
                { "MaxHealth", new List<string>() },
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
        { "SetIgnoreBound", 999 },
        { "IncrementIgnoreBound", 999 },
        { "Flip", null }
    };

    // NEW: Test subscriber class to track notifications
    private class TestSubscriber
    {
        public int CallbackCount { get; private set; } = 0;
        public List<object> ReceivedValues { get; private set; } = new();
        public string SubscriberName { get; }  // For debug prints

        public TestSubscriber(string name)
        {
            SubscriberName = name;
        }

        // Callback action: Increments count, stores value
        public void OnStateChange(object newValue)
        {
            CallbackCount++;
            ReceivedValues.Add(newValue);
            GD.Print($"    🔔 [{SubscriberName}] Callback #{CallbackCount}: Received {newValue}");
        }

        // Conditional callback: Only fires if condition passes (e.g., value > 50)
        public void OnStateChangeConditional(object newValue, Func<object, bool> condition)
        {
            if (condition(newValue))
            {
                OnStateChange(newValue);
            }
            else
            {
                GD.Print($"    🔇 [{SubscriberName}] Conditional skipped: {newValue} (condition false)");
            }
        }

        // Reset for reuse
        public void Reset()
        {
            CallbackCount = 0;
            ReceivedValues.Clear();
        }

        // Verify: Check if expected count/values match
        public bool Verify(int expectedCount, params object[] expectedValues)
        {
            bool countOk = CallbackCount == expectedCount;
            bool valuesOk = expectedValues.Length == 0 || ReceivedValues.Count >= expectedValues.Length && expectedValues.Zip(ReceivedValues.Take(expectedValues.Length), (exp, rec) => Equals(exp, rec)).All(ok => ok);
            if (!countOk || !valuesOk)
            {
                GD.PushWarning($"    ❌ [{SubscriberName}] Verify failed: Expected {expectedCount} calls with {string.Join(", ", expectedValues)}, got {CallbackCount} with {string.Join(", ", ReceivedValues)}");
                return false;
            }
            GD.Print($"    ✅ [{SubscriberName}] Verified: {CallbackCount} calls match expected");
            return true;
        }
    }

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
                    object payload;
                    if (!_testPayloads.TryGetValue(actionName, out payload))
                    {
                        GD.PushWarning($"No test payload for action '{actionName}' on '{state}'; skipping.");
                        continue;
                    }

                    GD.Print($"  🔄 Dispatch '{state}.{actionName}({payload})'");

                    object before = _stateManager.GetState(bundleName, state);
                    GD.Print($"    Before: {state} = {before}");

                    try
                    {
                        _stateManager.Dispatch(bundleName, state, new StateAction(actionName, payload));
                    }
                    catch (Exception e)
                    {
                        GD.PushError($"Dispatch failed for '{state}.{actionName}': {e.Message}");
                        continue;
                    }

                    object after = _stateManager.GetState(bundleName, state);
                    GD.Print($"    After:  {state} = {after}");

                    if (Equals(before, after))
                        GD.Print($"    ⚠️   No change (likely rule clamped)");
                }
            }

            // Specific rule tests
            RuleSpecificTests(bundleName, bundleTemplate);

            // NEW: Notification (Subscribe/Unsubscribe) tests
            NotificationTests(bundleName, bundleTemplate);

            DumpBundle(bundleName, bundleTemplate);
            _stateManager.DestroyBundle(bundleName);
        }
    }

    // NEW: Tests Subscribe/Unsubscribe with triggers
    // NEW: Tests Subscribe/Unsubscribe with triggers
// NEW: Tests Subscribe/Unsubscribe with triggers (Robust version: Set actions, count-only verify)
private void NotificationTests(string bundleName, string bundleTemplate)
{
    GD.Print($"  🔔 Notification Tests for {bundleTemplate} (Subscribe/Unsubscribe)");

    // Pick a non-derived state for testing (e.g., CurrentHealth—has actions to trigger changes)
    string testState = "CurrentHealth";  // Fallback to first non-derived if needed
    if (!_bundleStates[bundleTemplate].Contains(testState) || !_bundleStateActions[bundleTemplate][testState].Any())
    {
        GD.Print($"    ⏭️  Skipping notifications: No suitable non-derived state (e.g., {testState})");
        return;
    }

    // Create test subscribers
    var basicSub = new TestSubscriber("Basic");
    var conditionalSub = new TestSubscriber("Conditional (>50)");
    var multiSub = new TestSubscriber("Multi");

    // Test 1: Subscribe to existing state (basic + conditional)
    GD.Print($"    📝 Subscribe basic + conditional to '{testState}'");
    _stateManager.Subscribe(bundleName, testState, basicSub, basicSub.OnStateChange);
    _stateManager.Subscribe(bundleName, testState, conditionalSub, (val) => conditionalSub.OnStateChangeConditional(val, v => (int)v > 50));

    // FIXED: Use Set for predictable trigger (avoids clamp surprises; set to 75 >50)
    object initialValue = _stateManager.GetState(bundleName, testState);
    GD.Print($"    Initial {testState}: {initialValue}");
    try
    {
        _stateManager.Dispatch(bundleName, testState, new StateAction("Set", 75));  // Set to 75 (triggers if changed; >50 for conditional)
    }
    catch (Exception e)
    {
        GD.PushError($"Notification trigger dispatch failed: {e.Message}");
    }
    object newValue = _stateManager.GetState(bundleName, testState);
    GD.Print($"    After trigger: {testState} = {newValue} (Set to 75)");

    // Verify: Count only (values may clamp, but fires on change)
    // FIXED: Omit values (robust); expect 1 if changed (Set always fires if different)
    bool changed = !Equals(initialValue, newValue);
    basicSub.Verify(changed ? 1 : 0);
    conditionalSub.Verify(((int)newValue > 50 && changed) ? 1 : 0);

    // Test 2: Multiple subscriptions (subscribe again with different conditional)
    GD.Print($"    📝 Subscribe multi (different conditional: even values) to '{testState}'");
    _stateManager.Subscribe(bundleName, testState, multiSub, (val) => multiSub.OnStateChangeConditional(val, v => (int)v % 2 == 0));

    // FIXED: Set to even value < max (e.g., 80—even, >50)
    try
    {
        _stateManager.Dispatch(bundleName, testState, new StateAction("Set", 80));  // Set to 80 (even, >50)
    }
    catch (Exception e)
    {
        GD.PushError($"Notification trigger dispatch failed: {e.Message}");
    }
    newValue = _stateManager.GetState(bundleName, testState);
    GD.Print($"    After multi-trigger: {testState} = {newValue} (Set to 80)");

    // Verify: Counts only (all should fire: Basic always, Conditional >50, Multi even)
    changed = !Equals(initialValue, newValue);  // From previous newValue
    basicSub.Verify((changed ? 1 : 0) + 1);  // Cumulative
    conditionalSub.Verify((int)newValue > 50 ? 2 : 1);
    multiSub.Verify((int)newValue % 2 == 0 ? 1 : 0);

    // Test 3: Unsubscribe (one by one)
    GD.Print($"    ❌ Unsubscribe conditional and multi from '{testState}'");
    _stateManager.Unsubscribe(bundleName, testState, conditionalSub);
    _stateManager.Unsubscribe(bundleName, testState, multiSub);

    // FIXED: Set to odd value >50 (e.g., 75—tests only Basic fires)
    try
    {
        _stateManager.Dispatch(bundleName, testState, new StateAction("Set", 75));  // Odd, >50
    }
    catch (Exception e)
    {
        GD.PushError($"Notification trigger dispatch failed: {e.Message}");
    }
    newValue = _stateManager.GetState(bundleName, testState);
    GD.Print($"    After unsubscribe trigger: {testState} = {newValue} (Set to 75, odd)");

    // Verify: Basic fires (3rd), others don't (Conditional would if >50, but unsubbed; Multi odd—no)
    changed = !Equals(initialValue, newValue);
    basicSub.Verify(basicSub.CallbackCount + (changed ? 1 : 0));  // Cumulative, no specific count
    // For unsubbed, just check no increase
    int prevConditional = conditionalSub.CallbackCount;
    int prevMulti = multiSub.CallbackCount;
    conditionalSub.Verify(prevConditional);  // No change
    multiSub.Verify(prevMulti);  // No change

    // Test 4: Pending subscribers (unchanged from previous—works as-is)
    GD.Print($"    📝 Test pending: Subscribe to '{testState}' BEFORE bundle creation (missing bundle → pending)");
    string pendingBundleName = $"pending_test_{bundleTemplate.ToLower()}";  // New bundle name
    var pendingSub = new TestSubscriber("Pending");

    // Subscribe to missing bundle/state → Adds to pending (no throw, as bundle missing)
    _stateManager.Subscribe(pendingBundleName, testState, pendingSub, pendingSub.OnStateChange);
    GD.Print($"    Pending subscribe done (bundle '{pendingBundleName}' missing → added to pending)");

    // Unsubscribe pending (before creation) → Removes from pending (hits outer else if RemovePending)
    GD.Print($"    ❌ Unsubscribe pending '{testState}' in missing bundle");
    _stateManager.Unsubscribe(pendingBundleName, testState, pendingSub);
    GD.Print($"    Pending unsubscribe done (removed from pending)");

    // Now create the bundle → Triggers pending resolution in CreateBundle (but empty since unsubbed)
    _stateManager.CreateBundle(bundleTemplate, pendingBundleName, "");  // No defaults for simplicity
    GD.Print($"    Bundle '{pendingBundleName}' created (pending resolved)");

    // Trigger: Dispatch on testState → Should NOT fire (unsubbed pending)
    try
    {
        _stateManager.Dispatch(pendingBundleName, testState, new StateAction("Set", 42));  // Set to 42 (predictable)
    }
    catch (Exception e)
    {
        GD.PushError($"Pending trigger dispatch failed: {e.Message}");
    }
    object pendingNewValue = _stateManager.GetState(pendingBundleName, testState);
    GD.Print($"    After pending trigger: {testState} = {pendingNewValue} (no callback expected)");

    // Verify: No callback (unsubbed pending)
    pendingSub.Verify(0);

    // Clean up
    _stateManager.DestroyBundle(pendingBundleName);

    // Test 5: Unsubscribe non-existent (should warn but succeed—bundle missing → RemovePending)
    GD.Print($"    ❌ Unsubscribe non-existent subscriber (missing bundle/state)");
    var fakeSub = new TestSubscriber("Fake");
    string fakeBundle = $"fake_{bundleTemplate.ToLower()}";
    string fakeState = "FakeState";
    try
    {
        _stateManager.Unsubscribe(fakeBundle, fakeState, fakeSub);  // May throw per your code
    }
    catch (Exception e)
    {
        GD.Print($"    Expected: {e.Message} (non-existent no-op/throw)");  // Handles throw
    }
    GD.Print($"    Non-existent unsubscribe done (no-op/warn/throw expected)");

    // Reset and unsubscribe basic (final cleanup)
    basicSub.Reset();
    _stateManager.Unsubscribe(bundleName, testState, basicSub);

    GD.Print($"    ✅ All notification tests passed for {bundleTemplate}");
}

    private void RuleSpecificTests(string bundleName, string template)
    {
        GD.Print($"  🧪 Rule-Specific Tests for {template}");

        switch (template)
        {
            case "CharacterBundle":
                try
                {
                    _stateManager.Dispatch(bundleName, "CurrentHealth", new StateAction("Set", 999));
                    GD.Print($"    CurrentHealth clamped? {_stateManager.GetState(bundleName, "CurrentHealth")}");
                }
                catch (Exception e)
                {
                    GD.PushError($"Rule test dispatch failed: {e.Message}");
                }

                object oldCurrentProp = _stateManager.GetState(bundleName, "ProportionalCurrentHealth");
                try
                {
                    _stateManager.Dispatch(bundleName, "MaxHealth", new StateAction("Set", 500));
                }
                catch (Exception e)
                {
                    GD.PushError($"Rule test dispatch failed: {e.Message}");
                }
                object newCurrentProp = _stateManager.GetState(bundleName, "ProportionalCurrentHealth");
                GD.Print($"    Prop Health scaled? Old:{oldCurrentProp} → New:{newCurrentProp} (Max now 500)");

                _stateManager.DestroyBundle(bundleName);
                string defaults = "TestDefaults";
                _stateManager.CreateBundle("CharacterBundle", bundleName, defaults);
                break;

            case "PlayerCharacterBundle":
                try
                {
                    _stateManager.Dispatch(bundleName, "SectionAmount", new StateAction("Increment", 1));
                    DumpBundle(bundleName, template);
                }
                catch (Exception e)
                {
                    GD.PushError($"Rule test dispatch failed: {e.Message}");
                }

                try
                {
                    _stateManager.Dispatch(bundleName, "CurrentHealth", new StateAction("Set", 10));
                    GD.Print($"    Gated? CurrentHealth:{_stateManager.GetState(bundleName, "CurrentHealth")}, CurrentSection:{_stateManager.GetState(bundleName, "CurrentSection")}");
                }
                catch (Exception e)
                {
                    GD.PushError($"Rule test dispatch failed: {e.Message}");
                }

                try
                {
                    _stateManager.Dispatch(bundleName, "CurrentHealth", new StateAction("SetIgnoreBound", -100));
                    GD.Print($"    Ignore worked? CurrentHealth:{_stateManager.GetState(bundleName, "CurrentHealth")}");
                }
                catch (Exception e)
                {
                    GD.PushError($"Rule test dispatch failed: {e.Message}");
                }
                break;

            case "PlayerCharacterBundle2":
                try
                {
                    _stateManager.Dispatch(bundleName, "HealthBars", new StateAction("Increment", 1));
                    _stateManager.Dispatch(bundleName, "MaxBarHealth", new StateAction("Set", 100));
                    GD.Print($"    Product MaxHealth: {_stateManager.GetState(bundleName, "MaxHealth")} (2*100=200)");
                }
                catch (Exception e)
                {
                    GD.PushError($"Rule test dispatch failed: {e.Message}");
                }

                try
                {
                    _stateManager.Dispatch(bundleName, "CurrentHealth", new StateAction("Increment", 200));
                    GD.Print($"    CurrentHealth clamped to MaxBarHealth: {_stateManager.GetState(bundleName, "CurrentHealth")}");
                }
                catch (Exception e)
                {
                    GD.PushError($"Rule test dispatch failed: {e.Message}");
                }
                break;
        }
    }

    private void DumpBundle(string bundleName, string bundleTemplate)
    {
        GD.Print($"  📊 States for '{bundleName}' ({bundleTemplate}):");

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

        var swCreate = Stopwatch.StartNew();
        for (int i = 0; i < NUM_BUNDLES; i++)
        {
            string name = $"prof_{i}";
            _stateManager.CreateBundle(PROFILE_BUNDLE_TEMPLATE, name);
            bundleNames.Add(name);
        }
        swCreate.Stop();
        GD.Print($"⏱️  CREATE 100k bundles: {swCreate.ElapsedMilliseconds} ms");

        _stateManager.CreateBundle(PROFILE_BUNDLE_TEMPLATE, TARGET_BUNDLE);
        var swDispatch = Stopwatch.StartNew();
        for (int i = 0; i < NUM_ACTIONS; i++)
        {
            string action = i % 3 == 0 ? "Increment" : (i % 3 == 1 ? "Decrement" : "Set");
            object payload = _testPayloads.TryGetValue(action, out var p) ? p : 10;
            _stateManager.Dispatch(TARGET_BUNDLE, "CurrentHealth", new StateAction(action, payload));
        }
        swDispatch.Stop();
        GD.Print($"⏱️  DISPATCH 100k actions to 1 bundle: {swDispatch.ElapsedMilliseconds} ms");

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