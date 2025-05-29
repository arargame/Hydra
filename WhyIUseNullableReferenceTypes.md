
# Why I Use Nullable Reference Types in Older .NET Versions

In older versions of .NET (before nullable reference types became default in C# 8),  
we often used **nullable reference types** like:

```csharp
public string? Title { get; set; }
public Category? Category { get; set; }
```

---

## ❓ But Why Nullable?

### ✅ 1. Not Everything Is Always Set

Sometimes, we create objects that **do not yet have values**:

```csharp
var book = new Book(); // Title not known yet
```

If `Title` was not nullable, we would have to assign it a dummy value like `""`.

This causes confusion:
- `""` = something, but it's blank
- `null` = nothing has been set yet (and that's OK)

---

### ✅ 2. Object May Be Missing

Example:

```csharp
public class Order
{
    public Customer? Customer { get; set; }
}
```

Sometimes, an order is not assigned to a customer yet. So `Customer` can be `null`.

---

## 🧠 Accessing It Safely

We use the **null-conditional operator** to avoid exceptions:

```csharp
var customerName = order.Customer?.Name;
```

This means:
- If `Customer` is `null`, return `null`
- If `Customer` exists, return its `Name`

This avoids `NullReferenceException`.

---

## 🧩 Real World: Database Use

When saving to the database:

- `null` → means "not provided" (better for optional fields)
- `""` → means "blank" (could be confusing)

It's often **more meaningful** to store `null`.

---

## 🧼 Clean and Flexible Code

With nullable references:
- We don’t need to create dummy instances
- We write more realistic models
- We write safer access logic using `?.`

---

## ✅ Summary

| Concept | Why It Matters |
|--------|----------------|
| `null` for reference types | Gives flexibility during object creation |
| `?.` operator | Prevents runtime errors |
| DB compatibility | `null` is more semantically correct than `""` |
| Better modeling | Optional fields look like optional fields |

---

> Code should reflect reality:  
> sometimes things are missing, and that's perfectly OK.
