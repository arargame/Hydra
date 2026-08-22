# Theming

How the component kit stays independent of any CSS framework.

## The rule

`Hydra.RazorClassLibrary` components emit **semantic class names only** — never Bootstrap, Tailwind
or any other framework's utility classes, and never colour or spacing decisions:

```razor
<button class="hydra-btn hydra-btn--create">…</button>
<td class="hydra-grid__cell hydra-grid__cell--details">…</td>
```

The look comes entirely from the host application's stylesheet. Two applications can share the same
components and look nothing alike, without forking a single `.razor` file.

This is the same reasoning as keeping language-specific enum labels out of the core
(see [enum-and-value-display.md](enum-and-value-display.md)): anything baked into the shared library
is imposed on every consumer.

## The two stylesheets

| File | Role |
|---|---|
| `_content/Hydra.RazorClassLibrary/css/hydra-default.css` | Neutral, light default. Makes the components look reasonable with no theme at all. Entirely variable-driven. |
| the host's own theme (e.g. Tentacle's `wwwroot/css/tentacle-theme.css`) | Loaded **after** the default; overrides variables and adds application-specific detail. |

Order matters — the host theme must come second.

Most re-theming is just overriding the variables:

```css
:root {
    --hydra-primary:  #22D3EE;
    --hydra-create:   #10B981;
    --hydra-delete:   #ffb4ab;
    --hydra-surface:  #111827;
    --hydra-border:   #1E293B;
}
```

## Action colours travel as class names

The convention — create is green, edit is blue, delete is red, details is a fourth colour — is
expressed as `hydra-btn--create` / `--edit` / `--delete` / `--details`. The component decides *what
kind of action* a button is; the theme decides *what colour that means*. A project with a different
palette changes only its CSS.

Same for modals: `ConfirmDialogComponent` takes a semantic `Variant` (`"danger"`, `"primary"`,
`"success"`) and emits `hydra-modal--{Variant}`.

## Icons

Bootstrap **Icons** are still used (`bi bi-*`) alongside a `hydra-icon` hook. That package is a
standalone icon font with no dependency on Bootstrap's CSS, so dropping Bootstrap did not affect it.
The `hydra-icon` class exists so a host can restyle or replace icons from CSS.

## If styles do not appear

The RCL ships its stylesheet as a static web asset. Check that
`_content/Hydra.RazorClassLibrary/css/hydra-default.css` actually resolves in the browser — a 404
there leaves the components structurally correct but unstyled, which looks like a broken layout
rather than a missing file.
