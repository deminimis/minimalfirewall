The Applications view implements the first iteration proposed in [issue #176](https://github.com/deminimis/minimalfirewall/issues/176).

## Implementation map

- `ApplicationEntry.cs` groups normalized executable paths using case-insensitive identity. It keeps every individual rule, including disabled rules, and separates rule summaries from observations.
- `ApplicationsControl.cs` provides search, filters, executable details, scoped rule inspection, reported blocked attempts, and existing rule actions. It preserves the selected executable, focused column, and scroll position when the list updates.
- `MainViewModel.cs` reuses the data service's unaggregated rule snapshot and retains the last 500 reported blocked attempts for the session. `DataModels.cs` adds the report timestamp.
- `MainForm.cs` and its designer add navigation and reuse existing refresh, theme, icon, and background-operation services. The Rules view's delayed unload cannot clear a visible Applications snapshot.
- `ThemedTabControl.cs` draws keyboard focus for the application details tabs and the main navigation.
- `tests/ApplicationsChecks` exercises identity, filtering, scope preservation, observation semantics, and UI refresh state. The Windows workflow builds the application and runs these checks without invoking firewall mutation actions.

## Behavior and scope

Search matches the executable name or full path. Different executable paths remain separate even when their filenames or rule names match. Paths arrive normalized from the existing data service and event listener.

The explicit allow/block filters include disabled rules; each rule's enabled state remains visible. A summary describes explicit executable rules, never an effective global Allowed/Blocked state. Global rules, package identities, policies, and other firewall constraints can also affect traffic. Advanced Rules, auditing, and live monitoring remain accessible through their existing tabs.

Reported blocked attempts come from the existing pending-notification stream. Its existing filtering, duplicate suppression, snoozing, and automatic decisions remain unchanged. This is a bounded session history of reports, not a complete Windows firewall audit or a live connection log. Reports remain available after a dashboard decision. An absent report does not imply that no connection was blocked.

Publisher names appear only when existing auto-allow rule metadata supplies them. This metadata does not establish the current file's signature status. No new signature checks or trust decisions run in this view.

Allow/Block uses the existing application-rule operation. The direction selector defaults to outbound. Before queuing an action, confirmation identifies the full executable path and explains that all profiles, protocols, ports, addresses, and services using that executable are covered. Existing rules in the selected direction, including scoped and third-party rules, are replaced. Allow can also remove Minimal Firewall general block rules in either direction. These are existing operation semantics, disclosed explicitly by the new view.

Edit opens the existing advanced rule editor for the individual selected rule. It replaces that rule only, using the existing queue. Existing firewall rule creation, persistence, and enforcement implementations are unchanged.

## Validation

The [Windows verification run](https://github.com/malys/minimalfirewall/actions/runs/43255897115) passes compilation, model checks, UI refresh checks, and search/filter/direction mnemonic checks. Only pre-existing nullable warnings remain in unrelated source files.

Committed captures show standalone controls from that run, using an empty baseline and synthetic application data, at 100% layout scale:

- [Before: existing Rules control, empty](screenshots/issue-176/before-rules-empty.png)
- [After: Applications, light](screenshots/issue-176/applications-light.png)
- [After: Applications, dark](screenshots/issue-176/applications-dark.png)
- [After: reported blocks, dark](screenshots/issue-176/reported-blocks-dark.png)

Use Windows with the .NET 10 SDK and Visual Studio MSBuild, which supports COM reference generation:

```powershell
msbuild tests/ApplicationsChecks/ApplicationsChecks.csproj /restore /p:Configuration=Release /p:Platform=x64 /verbosity:minimal
dotnet tests/ApplicationsChecks/bin/x64/Release/net10.0-windows8.0/win-x64/ApplicationsChecks.dll
```

The checks produce an empty existing Rules control capture and populated Applications captures using synthetic data, in both themes at 100%, 125%, 150%, and 200% layout scaling. GitHub Actions uploads these as `applications-screenshots`. Layout scaling in the harness does not replace a real Windows display-DPI check.

Manual acceptance checks on a disposable Windows installation:

1. Open Applications and search by executable name and path. Confirm two paths sharing a filename remain distinct.
2. Inspect multiple allow/block rules with different directions, profiles, ports, addresses, and enabled states. Confirm every scope remains visible.
3. Trigger a reported blocked connection. Confirm its timestamp and endpoint appear separately from explicit block rules. Process its dashboard notification and confirm the report remains in Applications.
4. Select an executable, scroll vertically and horizontally, and refresh. Confirm selection, focused column, and viewport persist. Switch from Rules to Applications and wait more than 30 seconds; confirm the list remains populated.
5. Use Tab, Shift+Tab, arrow keys, Ctrl+Tab, and label mnemonics to search, filter, inspect rules, choose direction, and open/cancel action confirmations. Confirm visible focus and readable selection in both themes.
6. At real Windows display scales of 100%, 125%, 150%, and 200%, resize the window and columns. Confirm paths are readable or horizontally scrollable, details remain reachable, and action controls do not clip.
7. Inspect and cancel Allow/Block confirmations. On a disposable rule fixture, apply each direction and verify the resulting individual rules in Applications and Rules. Edit one scoped rule and confirm sibling rules remain intact.
8. Restart and confirm existing rule persistence and firewall enforcement behave as before.

Linux builds stop at the project's existing `ResolveComReference` step (`MSB4803`). The Windows workflow is the compilation and UI-check gate; real display-DPI, full keyboard navigation, and firewall integration require the manual checks above.
