# ValidationManager Usage

This helper class is used to manage common validation rules throughout the project.  
It helps centralize validation logic for consistency and reuse.

## ✅ Purpose

- Prevent duplicated validation code.
- Improve readability.
- Easily customize error messages.

## 📁 Location

`Hydra.ValidationManagement.ValidationManager`

## 🧰 Methods

### `ValueCannotBeNull(string propertyName)`
Returns a validation error if the given value is null or empty.

```csharp
ValidationManager.ValueCannotBeNull("FirstName");
```

---

### `CreateResult(string errorMessage, IEnumerable<string> memberNames)`
Creates a `ValidationResult` with custom message and affected property names.

```csharp
ValidationManager.CreateResult("Name is required", new[] { "Name" });
```

---

### `MustBeSelected(PropertyInfo propertyInfo)`
Ensures a value has been selected for UI drop-downs or selection fields.

```csharp
ValidationManager.MustBeSelected(typeof(User).GetProperty("Country"));
```

---

### `GreaterThan(PropertyInfo propertyInfo, int limitValue)`
Checks if a numeric value is greater than a limit.

```csharp
ValidationManager.GreaterThan(typeof(Order).GetProperty("Amount"), 100);
```

---

### `LessThan(PropertyInfo propertyInfo, int limitValue)`
Checks if a numeric value is less than a limit.

```csharp
ValidationManager.LessThan(typeof(Order).GetProperty("Quantity"), 50);
```

---

### `Range(PropertyInfo propertyInfo, int minValue, int maxValue)`
Validates whether a value is within a specific range.

```csharp
ValidationManager.Range(typeof(Product).GetProperty("Stock"), 1, 100);
```

---

### `IsDateInFuture(PropertyInfo propertyInfo, DateTime date)`
Ensures the given date is in the future.

```csharp
ValidationManager.IsDateInFuture(typeof(Appointment).GetProperty("StartDate"), appointment.StartDate);
```

---

### `IsDateInPast(PropertyInfo propertyInfo, DateTime date)`
Ensures the given date is in the past.

```csharp
ValidationManager.IsDateInPast(typeof(History).GetProperty("EndDate"), history.EndDate);
```

---

### `RegexMatch(string value, string pattern, string propertyName)`
Checks if a string matches a regex pattern.

```csharp
ValidationManager.RegexMatch("ABC-123", @"^[A-Z]{3}-\d{3}$", "Code");
```

---

### `IsValidEmail(string email, string propertyName)`
Validates if an email is in a valid format.

```csharp
ValidationManager.IsValidEmail("test@example.com", "Email");
```

---

### `IsValidPhoneNumber(string phoneNumber, string propertyName)`
Validates if a phone number is in a proper format.

```csharp
ValidationManager.IsValidPhoneNumber("+905551112233", "PhoneNumber");
```

## 📌 Tip

Consider creating custom validation attributes that internally use these methods for cleaner annotation-based validation.