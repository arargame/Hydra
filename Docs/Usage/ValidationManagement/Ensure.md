# Ensure Class - Usage Guide

The `Ensure` class helps you to check if the values are valid before using them.

It throws exceptions if something is missing or incorrect.
This makes your code safer and easier to debug.

---

## ✅ Methods

### 1. NotNull
Checks if the object is not null.

```csharp
var user = Ensure.NotNull(inputUser, nameof(inputUser));
```

- ❌ Throws: `ArgumentNullException` if `inputUser` is null.

---

### 2. NotNullOrWhiteSpace
Checks if the string is not null, empty, or just spaces.

```csharp
var name = Ensure.NotNullOrWhiteSpace(inputName, nameof(inputName));
```

- ❌ Throws: `ArgumentException` if string is null or empty.

---

### 3. IsTrue
Checks if a condition is true.

```csharp
Ensure.IsTrue(age > 18, nameof(age), "Age must be over 18");
```

- ❌ Throws: `ArgumentException` if condition is false.

---

### 4. InRange
Checks if a number is inside a valid range.

```csharp
Ensure.InRange(score, 0, 100, nameof(score));
```

- ❌ Throws: `ArgumentOutOfRangeException` if score is not between 0 and 100.

---

## ✅ Why Use It?

- Clean and readable code.
- Fails fast when something is wrong.
- Replaces repetitive `if` statements.

---

## 🛠 Tip

You can use `Ensure` at the beginning of functions to validate inputs quickly.

```csharp
public void CreateUser(string name, int age)
{
    Ensure.NotNullOrWhiteSpace(name, nameof(name));
    Ensure.InRange(age, 0, 120, nameof(age));
    
    // Continue safely...
}
```