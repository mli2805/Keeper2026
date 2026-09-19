# Copilot Instructions

## User Preferences
- All newly created files must use a namespace exactly matching the project name; do not include folder names in namespaces.

## General Guidelines
- Create new tests only when the task explicitly requests them; otherwise validate changes using existing tests and builds without adding tests.
- Prefer removing obsolete view-model properties and simplifying related state after UI controls that used them are deleted.

## Project-Specific Rules
- In Keeper2026 WPF forms, display all dates using `StringFormat='{}{0:d/MM/yyyy}'`.
- Forms in Keeper2026 without OxyPlot, integrated into the singleton MainMenuViewModel via the constructor, can be reopened normally; however, some forms with OxyPlot experience issues with reopening.
