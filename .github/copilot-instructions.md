# Copilot Instructions

## User Preferences
- All newly created files must use a namespace exactly matching the project name; do not include folder names in namespaces.

## Project-Specific Rules
- In Keeper2026 WPF forms, display all dates using `StringFormat='{}{0:d/MM/yyyy}'`.
- Forms in Keeper2026 without OxyPlot, integrated into the singleton MainMenuViewModel via the constructor, can be reopened normally; however, some forms with OxyPlot experience issues with reopening.
