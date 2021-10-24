---
copyright:
  start: 2021
  owner: Gateway Programming School, Inc.
view: MDS.TestSite.Mvc.IFormView
controller: MDS.TestSite.Mvc.FormController
---

## Sidebar Form

!form#frmOne({method=post},{action=form.md},{class=form-group})

!label({for=txtOne}):Search For:
!input#txtOne({class=form-element}):Default Value
!button#btnOne({type=submit},{class=btn btn-default},{onclick=>btnClicked}):Search

!/form

---