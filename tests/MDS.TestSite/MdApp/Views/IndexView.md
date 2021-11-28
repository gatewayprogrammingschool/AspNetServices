---
CodeBehind: IndexView.md.cs
Inherits: MDS.TestSuite.MdApp.Views.IndexView
ViewModel: MDS.TestSuite.MdApp.ViewModels.IndexViewModel
title: Markdown App - $(ViewModel.PageTitle)
Layout: wwwroot/TestSite.Layout.html
---

# Search Example

!form({method=POST},{action=/mdapp/})
!span.H2:Search
!input.form-control#q({type=text})
!button.btn.btn-default({type=submit}):Go
!/form

$(ViewBody:$(ViewKey))

```json
$(ViewModel)
```
