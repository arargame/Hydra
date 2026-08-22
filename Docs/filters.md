# Filters

How a query's filters are represented, combined, and turned into parameterised SQL.

## The model

```
IQueryableFilter          — anything that can render a WHERE fragment and owns parameters
 ├── Filter (IFilter)     — a leaf: one column, one operator (Equal, Contains, Between, In, …)
 └── JoinedFiltersGroup   — a composite: two operands joined by And/Or
```

`JoinedFiltersGroup` is itself an `IQueryableFilter`, and since its operands are typed
`IQueryableFilter` a group can contain another group. N filters therefore fold into a left-leaning
tree:

```
(((f0 And f1) And f2) And f3)
```

`SetFromColumns` builds exactly that and returns the root. There is **no upper bound** on the number
of filters.

> **History.** The operands used to be typed `IFilter` while the group implemented only
> `IQueryableFilter` — so a group could never be an operand of another group. The composite was not
> of the same type as its leaves, which is the classic way to break the Composite pattern. To reach a
> third filter an `AnotherFilter` field had been bolted on, and a fourth threw
> *"does not support to bind more than 3 filters"*. Typing the operands as `IQueryableFilter` removed
> the ceiling; `AnotherFilter` and `Bind()` are kept only for hand-built trees.

## The parameter-numbering invariant

This is the part that will silently corrupt a query if you get it wrong, so it is worth stating
precisely.

Two independent things must agree:

1. **The SQL text.** Leaf filters name their placeholders themselves:
   - `EqualFilter` / `ContainsFilter` → `@{StartParameterIndex}` (the *filter's* index)
   - `BetweenFilter` → `@{Parameters[0].Index}` and `@{Parameters[1].Index}` (the *parameters'* indices)

2. **The parameter dictionary.** `Table.SetQueryParameters` walks the **root** filter's flattened
   `Parameters` list and keys it by position:
   ```csharp
   for (int i = 0; i < Filter.Parameters.Count; i++)
       QueryParameters.Add($"@{i}", Filter.Parameters[i].Value);
   ```

So the invariant is: **every filter's `StartParameterIndex`, and every parameter's `Index`, must equal
that parameter's position in the root's flattened list.**

Two things keep it true:

- A group's `Parameters` is its operands' parameter lists concatenated **in operand order** — and
  they are the *same object references*, not copies. Renumbering at the root therefore reaches the
  leaves.
- `SetStartParameterIndex` is `virtual` on `QueryableFilter` and overridden by `JoinedFiltersGroup`
  to cascade: the left operand starts at the group's own index, the right operand starts where the
  left finished. Nested groups recurse naturally.

`FinishParameterIndex` is `StartParameterIndex + Parameters.Count`, which is correct for leaves and
groups alike.

If you add a filter type, make sure its `PrepareQueryString` derives placeholder names from
`StartParameterIndex` or from its own parameters' `Index` — never from a counter of its own.

## The base-constructor trap

`BaseObject`'s constructor calls the virtual `Initialize()`. In C# the base constructor runs
**before** the derived one, so at that moment a derived class's fields are still null.

`JoinedFiltersGroup.Initialize()` dereferences `LeftFilter` and `RightFilter`, so it threw
`NullReferenceException` on construction — meaning **every query with more than one active filter
failed**, which went unnoticed only because nothing in the app applied two filters at once until the
automatic Logs tab did.

`Initialize()` now returns early while the operands are null. It runs twice by design:

1. from the base constructor, with nothing set — returns immediately;
2. from the derived constructor, once the operands are assigned — this is the call that fills
   `Parameters` and numbers the tree.

Any other class deriving from `BaseObject` and overriding `Initialize()` is subject to the same rule:
**`Initialize()` must tolerate being called before the derived constructor body has run.**

## Where groups are built

- `QueryBuilder.SetTableFilter` — when the table has joins.
- `Table.SetFilter` — otherwise.

Both special-case a single filter (used directly, no group) and only build a group for two or more.
`SetFromColumns` returns an empty list for fewer than two filters, which matches those call sites.

## Verifying a change

The tree/numbering logic has no external dependencies, so it can be extracted into a standalone
console program and exercised directly. That is how the N-filter rewrite was checked: 2–8 filters,
with multi-parameter `BetweenFilter`s interleaved between single-parameter ones, asserting that the
`@N` placeholders appearing in the generated SQL are exactly `0..Parameters.Count-1`. Worth repeating
if you touch numbering.
