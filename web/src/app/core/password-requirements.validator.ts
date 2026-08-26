import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/** Mirrors RegisterRequestValidator on the API: min 8 chars, at least one
 * uppercase, one lowercase, and one digit (see design.md - Context). */
export function passwordRequirements(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value as string;
    if (!value) {
      return null;
    }

    const errors: ValidationErrors = {};
    if (value.length < 8) {
      errors['minlength'] = true;
    }
    if (!/[A-Z]/.test(value)) {
      errors['uppercase'] = true;
    }
    if (!/[a-z]/.test(value)) {
      errors['lowercase'] = true;
    }
    if (!/[0-9]/.test(value)) {
      errors['digit'] = true;
    }

    return Object.keys(errors).length > 0 ? errors : null;
  };
}
