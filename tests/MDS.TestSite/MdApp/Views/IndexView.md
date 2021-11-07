---
CodeBehind: IndexView.md.cs
Inherits: MDS.TestSuite.MdApp.Views.IndexView
ViewModel: MDS.TestSuite.MdApp.ViewModels.IndexViewModel
title: Markdown App - $(ViewModel.PageTitle)
Layout: wwwroot/TestSite.Layout.html
---

# $(ViewKey)

$(ViewBody:$(ViewKey))

```json
$(ViewModel)
```
