import { FormControl } from '@angular/forms';
import { passwordRequirements } from './password-requirements.validator';

describe('passwordRequirements', () => {
  const validate = (value: string) => passwordRequirements()(new FormControl(value));

  it('returns null for a password meeting all requirements', () => {
    expect(validate('Password123')).toBeNull();
  });

  it('returns null for an empty value (required is handled separately)', () => {
    expect(validate('')).toBeNull();
  });

  it('flags a password shorter than 8 characters', () => {
    expect(validate('short1A')).toEqual({ minlength: true });
  });

  it('flags a password with no uppercase letter', () => {
    expect(validate('alllowercase1')).toEqual({ uppercase: true });
  });

  it('flags a password with no lowercase letter', () => {
    expect(validate('ALLUPPERCASE1')).toEqual({ lowercase: true });
  });

  it('flags a password with no digit', () => {
    expect(validate('NoDigitsHere')).toEqual({ digit: true });
  });

  it('accumulates multiple errors when several requirements fail', () => {
    expect(validate('short')).toEqual({ minlength: true, uppercase: true, digit: true });
  });
});
