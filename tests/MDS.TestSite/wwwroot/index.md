---
title: Page title
copyright: 2021 - Gateway Programming School, Inc.
viewmodel: Namespace.ViewModel
view: IIndexView
a: 1
b: true
c: 'false'
MarkdownSidebar: form.md
---

#include(https://raw.githubusercontent.com/MarkdownServer/AspNetServices/main/README.md)
#include(link1.md)

# Markdown File

This is a paragraph.

---

## Quote

> Quoted Text

---

## Unordered List

* [Site Link](link1.md)
* [Github](https://github.com)
* [[Wiki Link]]

---

## Ordered List

1. One
2. Two
3. Three

---

## Table

| A   | B   | C   |
|-|-|-|
| 10% | ABC | No  |
| 90% | ZYX | Yes |
| $(a) | $(b) | $(c) |

---

## Code Highlighting

```csharp
public class CSharpClass
{
	public string Property { get; set; }
}
```
