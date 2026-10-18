# Copilot Instructions

## User Preferences
- All newly created files must use a namespace exactly matching the project name; do not include folder names in namespaces.

## General Guidelines
- Create new tests only when the task explicitly requests them; otherwise validate changes using existing tests and builds without adding tests.
- Prefer removing obsolete view-model properties and simplifying related state after UI controls that used them are deleted.

## Project-Specific Rules
- In Keeper2026 WPF forms, display all dates using `StringFormat='{}{0:d/MM/yyyy}'`.
- Forms in Keeper2026 without OxyPlot, integrated into the singleton MainMenuViewModel via the constructor, can be reopened normally; however, some forms with OxyPlot experience issues with reopening.
- For Keeper2026 utility-bill OCR parsing, retain partially recognized rows and months with at least one row; use a recurring service-name dictionary to correct 1–2 character OCR errors and infer consistent units by service. Printed text may be skewed relative to preprinted table borders.
- After parsing Keeper2026 OCR bills, create only missing monthly JSON files; preserve existing files on reparse. Monthly JSON stores the original JPEG SourceFile at the bill level, which is the original yyyyMM.jpeg filename, and has no Ready flag or completion locks. All months are editable, and JSON rows omit Period and SourceFile. The monthly editor uses vertical label/input footer rows, dates in labels, no diagnostic lists, selectable copyable cells, and service dropdown from saved bills and current edits.
- In Keeper2026's monthly OCR bill editor, autosave changed months when changing months or closing the form, with no manual save button. Invalid input or JSON write failures must prevent month changes and closing. All months are editable; numeric row sorting must work across month switches and the column total must be selectable/copyable.
- In Keeper2026's monthly bill editor, bulk row replacement (including RepeatMonth using BindableCollection.AddRange Reset) must retain row PropertyChanged subscriptions so column sums and dirty tracking update; removed rows must be unsubscribed.
